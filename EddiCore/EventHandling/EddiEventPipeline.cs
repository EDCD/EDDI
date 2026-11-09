using EddiEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.EventHandling
{
    internal sealed class EddiEventPipeline
    {
        private readonly Func<Event, Task<bool>> _processEventAsync;
        private readonly Func<IEnumerable<IEddiMonitor>> _getActiveMonitors;
        private readonly Func<IEnumerable<IEddiResponder>> _getActiveResponders;
        private readonly Func<string, IEddiResponder> _obtainResponder;
        private readonly Func<bool> _isUnitTesting;
        private readonly Func<System.Version> _getGameVersion;
        private readonly System.Version _minGameVersion;
        private readonly CancellationToken _appCancellationToken;
        private readonly IEventScheduler _eventScheduler;
        private readonly BlockingCollection<Event> _eventQueue = [ ];
        private Task _eventConsumerThread;
        private readonly Queue<FriendsEvent> pendingFriends = new();
        private readonly Queue<CargoTransferEvent> pendingCargoTransfers = new();
        private readonly ConcurrentDictionary<CrewPaidWageEvent, bool> crewWageReadiness = new();
        private readonly ConcurrentDictionary<BodyMappedEvent, PendingBodyMapping> pendingBodyMappings = new();
        private SurfaceSignalsEvent lastAcceptedSaaSignals;

        // Timer callbacks only change readiness. Dispatch remains on the event consumer.
        private sealed class PendingBodyMapping
        {
            internal const int Waiting = 0;
            internal const int TimedOut = 1;
            internal const int Released = 2;
            private int state;
            internal int State => Volatile.Read( ref state );
            internal bool MarkTimedOut () =>
                Interlocked.CompareExchange( ref state, TimedOut, Waiting ) == Waiting;
            internal bool Release () => Interlocked.Exchange( ref state, Released ) != Released;
        }
        private bool commanderReady;
        private int shipShutdownPending;
        private int stopped;

        internal EddiEventPipeline (
            Func<Event, Task<bool>> processEventAsync,
            Func<IEnumerable<IEddiMonitor>> getActiveMonitors,
            Func<IEnumerable<IEddiResponder>> getActiveResponders,
            Func<string, IEddiResponder> obtainResponder,
            Func<bool> isUnitTesting,
            Func<System.Version> getGameVersion,
            System.Version minGameVersion,
            CancellationToken appCancellationToken,
            IEventScheduler eventScheduler = null )
        {
            _processEventAsync = processEventAsync;
            _getActiveMonitors = getActiveMonitors;
            _getActiveResponders = getActiveResponders;
            _obtainResponder = obtainResponder;
            _isUnitTesting = isUnitTesting;
            _getGameVersion = getGameVersion;
            _minGameVersion = minGameVersion;
            _appCancellationToken = appCancellationToken;
            _eventScheduler = eventScheduler ?? new EventScheduler( Enqueue );
        }

        internal ConcurrentDictionary<string, Event> LastEventOfType { get; } = [ ];

        internal void Enqueue ( Event @event )
        {
            if ( @event is null ) { return; }

            if ( !_eventQueue.IsAddingCompleted )
            {
                _eventQueue.Add( @event );
            }

            // Start (or restart) our event handler thread (as long as we are not unit testing)
            if ( !_appCancellationToken.IsCancellationRequested &&
                 ( _eventConsumerThread is null || _eventConsumerThread?.Status >= TaskStatus.RanToCompletion ) &&
                 !_isUnitTesting() )
            {
                _eventConsumerThread?.Dispose();
                _eventConsumerThread = Task.Run( DequeueEventsAsync, _appCancellationToken );
            }
        }

        internal bool HasQueuedSignalDetectedEvents () => _eventQueue.Any( e => e is SignalDetectedEvent );

        internal void Stop ()
        {
            if ( Interlocked.Exchange( ref stopped, 1 ) != 0 ) { return; }

            if ( !_eventQueue.IsAddingCompleted )
            {
                _eventQueue.CompleteAdding();
            }
            _eventScheduler.Dispose();
        }

        private async Task DequeueEventsAsync ()
        {
            try
            {
                foreach ( var @event in _eventQueue.GetConsumingEnumerable( _appCancellationToken ) )
                {
                    await HandleEventAsync( @event ).ConfigureAwait( false );
                    await Task.Yield();
                }
            }
            catch ( TaskCanceledException )
            {
                Stop();
            }
        }

        internal Task HandleEventAsync ( Event @event ) => HandleEventAsync( @event, false );

        private async Task HandleEventAsync ( Event @event, bool releaseCargoTransfer )
        {
            if ( @event is null ) { return; }

            // Event handling is disabled when running a legacy game version.
            var gameVersion = _getGameVersion();
            if ( gameVersion != null && gameVersion < _minGameVersion && @event is not FileHeaderEvent ) { return; }

            if ( @event is FileHeaderEvent )
            {
                commanderReady = false;
                pendingFriends.Clear();
                pendingCargoTransfers.Clear();
            }

            if ( @event is CargoTransferEvent cargoTransferEvent && !releaseCargoTransfer )
            {
                if ( cargoTransferEvent.fromLoad ) { return; }
                pendingCargoTransfers.Enqueue( cargoTransferEvent );
                return;
            }

            if ( @event is FriendsEvent friend )
            {
                if ( friend.fromLoad ) { return; }
                if ( !commanderReady )
                {
                    pendingFriends.Enqueue( friend );
                    return;
                }
            }

            if ( @event is CrewPaidWageEvent crewPaidWageEvent )
            {
                if ( crewPaidWageEvent.fromLoad ) { return; }

                // Delay `Crew paid wage` events to occur after events where the commander receives a payment.
                if ( crewWageReadiness.TryGetValue( crewPaidWageEvent, out var ready ) && ready )
                {
                    crewWageReadiness.TryRemove( crewPaidWageEvent, out _ );
                }
                else
                {
                    if ( crewWageReadiness.TryAdd( crewPaidWageEvent, false ) )
                    {
                        _eventScheduler.Schedule( TimeSpan.FromSeconds( 5 ), () =>
                        {
                            crewWageReadiness[ crewPaidWageEvent ] = true;
                            return crewPaidWageEvent;
                        } );
                    }
                    return;
                }
            }

            if ( @event is ShipShutdownEvent { partialshutdown: false } shipShutdownEvent )
            {
                if ( Interlocked.CompareExchange( ref shipShutdownPending, 1, 0 ) != 0 ) { return; }

                var schedulingStarted = DateTime.UtcNow;
                _eventScheduler.Schedule( TimeSpan.FromSeconds( 30 ), () =>
                {
                    Interlocked.Exchange( ref shipShutdownPending, 0 );
                    return new ShipShutdownRebootEvent(
                        shipShutdownEvent.timestamp + ( DateTime.UtcNow - schedulingStarted ) );
                } );
            }

            try
            {
                Logging.Debug( $"Handling event: {@event.type}", @event );

                if ( @event is BodyMappedEvent scheduledMapping &&
                     pendingBodyMappings.TryGetValue( scheduledMapping, out var pendingMapping ) )
                {
                    if ( pendingMapping.State == PendingBodyMapping.Waiting ) { return; }
                    pendingBodyMappings.TryRemove( scheduledMapping, out _ );
                    if ( pendingMapping.Release() )
                    {
                        await DispatchBodyMappingAsync( scheduledMapping ).ConfigureAwait( false );
                    }
                    return;
                }

                // Finish old-system reports before a location change updates their context.
                if ( @event is JumpedEvent or LocationEvent or CommanderLoadingEvent or FileHeaderEvent )
                {
                    await ReleaseBodyMappingsAsync( _ => true ).ConfigureAwait( false );
                    lastAcceptedSaaSignals = null;
                }

                // We have some additional processing to do for a number of events
                var passEvent = await _processEventAsync( @event ).ConfigureAwait( false );

                if ( passEvent && @event is BodyMappedEvent { fromLoad: false, bodyId: not null } mapping &&
                     !( lastAcceptedSaaSignals != null && MatchesMapping( mapping, lastAcceptedSaaSignals ) ) )
                {
                    // Core processing has updated the mapped body. Wait for SAA signals before
                    // notifying monitors/responders, even when the journal spans multiple reads.
                    var pending = new PendingBodyMapping();
                    pendingBodyMappings[ mapping ] = pending;
                    _eventScheduler.Schedule( TimeSpan.FromSeconds( 1 ), () =>
                    {
                        if ( pending.MarkTimedOut() ) { return mapping; }
                        pendingBodyMappings.TryRemove( mapping, out _ );
                        return null;
                    } );
                    return;
                }

                // Additional processing is over, send to the event monitors and responders if required
                if ( passEvent )
                {
                    await OnEventAsync( @event ).ConfigureAwait( false );
                }

                LastEventOfType[ @event.type ] = @event;
                if ( passEvent && @event is SurfaceSignalsEvent { fromLoad: false } signals )
                {
                    if ( signals.detectionType == "SAA" ) { lastAcceptedSaaSignals = signals; }
                    await ReleaseBodyMappingsAsync( mapping => MatchesMapping( mapping, signals ) )
                        .ConfigureAwait( false );
                }
                if ( @event is CommanderLoadingEvent )
                {
                    commanderReady = true;
                    while ( pendingFriends.Count > 0 )
                    {
                        await HandleEventAsync( pendingFriends.Dequeue() ).ConfigureAwait( false );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( $"EDDI core failed to handle {@event.type} event {@event.raw}.", ex );

                // Even if an error occurs, we still need to pass the raw data
                // to the EDDN responder to maintain its integrity and to the Inara / EDSM responders to keep external services up-to-date.
                await _obtainResponder( "EDDN Responder" ).HandleAsync( @event ).ConfigureAwait( false );
                await _obtainResponder( "EDSM Responder" ).HandleAsync( @event ).ConfigureAwait( false );
                await _obtainResponder( "Inara Responder" ).HandleAsync( @event ).ConfigureAwait( false );
            }

            if ( @event is CargoEvent )
            {
                while ( pendingCargoTransfers.Count > 0 )
                {
                    await HandleEventAsync( pendingCargoTransfers.Dequeue(), true ).ConfigureAwait( false );
                }
            }
        }

        private static bool MatchesMapping ( BodyMappedEvent mapping, SurfaceSignalsEvent signals ) =>
            !signals.fromLoad && signals.detectionType == "SAA" &&
            signals.systemAddress == mapping.systemAddress && signals.bodyId == mapping.bodyId &&
            signals.timestamp >= mapping.timestamp;

        private async Task ReleaseBodyMappingsAsync ( Func<BodyMappedEvent, bool> matches )
        {
            foreach ( var entry in pendingBodyMappings.OrderBy( entry => entry.Key.timestamp ) )
            {
                if ( matches( entry.Key ) && entry.Value.Release() )
                {
                    await DispatchBodyMappingAsync( entry.Key ).ConfigureAwait( false );
                }
            }
        }

        private async Task DispatchBodyMappingAsync ( BodyMappedEvent mapping )
        {
            await OnEventAsync( mapping ).ConfigureAwait( false );
            LastEventOfType[ mapping.type ] = mapping;
        }

        private async Task OnEventAsync ( Event @event )
        {
            try
            {
                // We send the event to all monitors to ensure that their info is up-to-date.
                await PassToMonitorPreHandlersAsync( @event ).ConfigureAwait( false );

                // Now we pass the data to the responders. Responders must not change global states.
                await PassToRespondersAsync( @event ).ConfigureAwait( false );

                // We also pass the event to all active monitors in case they have asynchronous follow-on work.
                await PassToMonitorPostHandlersAsync( @event ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to pass event to all monitors and responders", ex );
            }
        }

        private async Task PassToMonitorPreHandlersAsync ( Event @event )
        {
            // All changes to state must be handled here.
            var monitorTasks = new List<Task>();
            foreach ( var monitor in _getActiveMonitors() )
            {
                var monitorTask = monitor.PreHandleAsync( @event );
                monitorTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{monitor.MonitorName()} failed to handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }

        private async Task PassToRespondersAsync ( Event @event )
        {
            // Wait for all to complete.
            var responderTasks = new List<Task>();
            foreach ( var responder in _getActiveResponders() )
            {
                var responderTask = responder.HandleAsync( @event );
                responderTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{responder.ResponderName()} failed to handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                responderTasks.Add( responderTask );
            }

            try
            {
                await Task.WhenAll( responderTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }

        private async Task PassToMonitorPostHandlersAsync ( Event @event )
        {
            // Pass back to monitors for follow-on work, wait for all to complete.
            var monitorTasks = new List<Task>();
            foreach ( var monitor in _getActiveMonitors() )
            {
                var monitorTask = monitor.PostHandleAsync( @event );
                monitorTask.ContinueWith( task =>
                    {
                        if ( task.IsFaulted )
                        {
                            Logging.Error(
                                $"{monitor.MonitorName()} failed to post-handle {@event.type} event {@event.raw}",
                                task.Exception );
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously )
                    .SafeFireAndForget( e => Logging.Error( e.Message, e ) );
                monitorTasks.Add( monitorTask );
            }

            try
            {
                await Task.WhenAll( monitorTasks.ToArray() );
            }
            catch ( TaskCanceledException )
            {
                // Task(s) cancelled. Nothing to do here.
            }
        }
    }
}

