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
        private readonly BlockingCollection<Event> _eventQueue = [ ];
        private Task _eventConsumerThread;

        internal EddiEventPipeline (
            Func<Event, Task<bool>> processEventAsync,
            Func<IEnumerable<IEddiMonitor>> getActiveMonitors,
            Func<IEnumerable<IEddiResponder>> getActiveResponders,
            Func<string, IEddiResponder> obtainResponder,
            Func<bool> isUnitTesting,
            Func<System.Version> getGameVersion,
            System.Version minGameVersion,
            CancellationToken appCancellationToken )
        {
            _processEventAsync = processEventAsync;
            _getActiveMonitors = getActiveMonitors;
            _getActiveResponders = getActiveResponders;
            _obtainResponder = obtainResponder;
            _isUnitTesting = isUnitTesting;
            _getGameVersion = getGameVersion;
            _minGameVersion = minGameVersion;
            _appCancellationToken = appCancellationToken;
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
            if ( !_eventQueue.IsAddingCompleted )
            {
                _eventQueue.CompleteAdding();
            }
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

        internal async Task HandleEventAsync ( Event @event )
        {
            if ( @event is null ) { return; }

            // Event handling is disabled when running a legacy game version.
            var gameVersion = _getGameVersion();
            if ( gameVersion != null && gameVersion < _minGameVersion && @event is not FileHeaderEvent ) { return; }

            try
            {
                Logging.Debug( $"Handling event: {@event.type}", @event );

                // We have some additional processing to do for a number of events
                var passEvent = await _processEventAsync( @event ).ConfigureAwait( false );

                // Additional processing is over, send to the event monitors and responders if required
                if ( passEvent )
                {
                    await OnEventAsync( @event ).ConfigureAwait( false );
                }

                LastEventOfType[ @event.type ] = @event;
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
