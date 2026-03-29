#nullable enable

using EddiEvents;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiVoiceAttackAdapter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal class VoiceAttackEventHandling
    {
        private const string RuntimeEventType = "va_runtime";
        private const string DispatchEventName = "dispatch_event";

        private readonly ConcurrentDictionary<string, TaskQueue<Event>> taskQueues = new();
        private readonly CancellationTokenSource consumerCancellationTS = new();

        public VoiceAttackEventHandling()
        {
            Logging.Debug( "Started VoiceAttack event handler" );
        }

        // We'll maintain a referenceable list of variables that we've set from events
        private readonly ConcurrentDictionary<string, VoiceAttackVariable> currentVariables = new();

        public void Handle ( Event @event )
        {
            if ( consumerCancellationTS.IsCancellationRequested || @event.type == null )
            {
                return;
            }

            var commandName = $"((EDDI {@event.type.ToLowerInvariant()}))";
            var taskQueue = taskQueues.GetOrAdd( @event.type, new TaskQueue<Event>( commandName ) );
            if ( taskQueue.TryAdd( @event ) )
            {
                taskQueue.StartOrRestart( () => dequeueEventsAsync( taskQueue ), consumerCancellationTS.Token );
            }
            Logging.Debug( $"Command '{commandName}' enqueued for runtime payload dispatch.", @event );
        }

        private async Task dequeueEventsAsync ( TaskQueue<Event> eventQueue )
        {
            try
            {
                foreach ( var @event in eventQueue.GetConsumingEnumerable( consumerCancellationTS.Token ) )
                {
                    await ExecuteEventAsync( eventQueue.commandName, @event ).ConfigureAwait( false );
                }
            }
            catch ( OperationCanceledException )
            {
                // Task canceled. Nothing to do here.
            }
            finally
            {
                // If there are still events, start a new consumer
                if ( !eventQueue.IsCompleted && eventQueue.Count > 0 && !eventQueue.isRunning )
                {
                    eventQueue.StartOrRestart( () => dequeueEventsAsync( eventQueue ), consumerCancellationTS.Token );
                }
            }
        }

        private async Task ExecuteEventAsync ( string commandName, Event @event )
        {
            try
            {
                if ( @event.type == null )
                {
                    return;
                }

                Logging.Debug( $"Passing event {@event.type} to VoiceAttack runtime payload channel", @event );

                var payload = BuildRuntimePayload( commandName, @event );
                var eventData = new EventData
                {
                    EventType = RuntimeEventType,
                    EventName = DispatchEventName,
                    EventPayload = payload
                };

                var dispatched = await RuntimeEventDispatcher
                    .DispatchAsync( eventData, consumerCancellationTS.Token )
                    .ConfigureAwait( false );

                if ( !dispatched )
                {
                    Logging.Warn( "VoiceAttack runtime payload dispatch skipped: no dispatcher registered" );
                }

                await Task.Yield();
            }
            catch ( OperationCanceledException )
            {
                // expected during shutdown
            }
            catch ( Exception ex )
            {
                Logging.Error( $"VoiceAttack failed to handle {@event.type} event payload dispatch.", ex );
            }
        }

        private Dictionary<string, object> BuildRuntimePayload( string commandName, Event @event )
        {
            var eventType = @event.type ?? string.Empty;

            var clearVariables = currentVariables.Values
                .Where( v => v.eventType == eventType && v.value != null )
                .Select( ToVariablePayloadForClear )
                .ToList();

            var eventVariables = VoiceAttackVariables.Convert(
                new MetaVariables( @event.GetType(), @event ).Results,
                "EDDI",
                eventType );

            foreach ( var variable in eventVariables )
            {
                currentVariables[ variable.key ] = variable;
            }

            var setVariables = eventVariables
                .Select( ToVariablePayload )
                .ToList();

            Logging.Debug( $"Prepared VoiceAttack runtime payload for EDDI event {eventType}", eventVariables );

            return new Dictionary<string, object>
            {
                { RuntimePayloadKeys.DispatchPayload.CommandName, commandName },
                { RuntimePayloadKeys.DispatchPayload.EventType, eventType },
                { RuntimePayloadKeys.DispatchPayload.ClearVariables, clearVariables },
                { RuntimePayloadKeys.DispatchPayload.SetVariables, setVariables }
            };
        }

        private static Dictionary<string, object?> ToVariablePayload( VoiceAttackVariable variable )
        {
            return new Dictionary<string, object?>
            {
                { RuntimePayloadKeys.VariablePayload.Key, variable.key },
                { RuntimePayloadKeys.VariablePayload.Type, ResolveVariableTypeName( variable.variableType ) },
                { RuntimePayloadKeys.VariablePayload.Value, variable.value }
            };
        }

        private static Dictionary<string, object?> ToVariablePayloadForClear( VoiceAttackVariable variable )
        {
            return new Dictionary<string, object?>
            {
                { RuntimePayloadKeys.VariablePayload.Key, variable.key },
                { RuntimePayloadKeys.VariablePayload.Type, ResolveVariableTypeName( variable.variableType ) },
                { RuntimePayloadKeys.VariablePayload.Value, null }
            };
        }

        private static string ResolveVariableTypeName( Type variableType )
        {
            if ( variableType == typeof( bool ) || variableType == typeof( bool? ) )
            {
                return "bool";
            }

            if ( variableType == typeof( DateTime ) || variableType == typeof( DateTime? ) )
            {
                return "date";
            }

            if ( variableType == typeof( int ) || variableType == typeof( int? ) )
            {
                return "int";
            }

            if ( variableType == typeof( decimal ) || variableType == typeof( decimal? ) ||
                 variableType == typeof( double ) || variableType == typeof( float ) ||
                 variableType == typeof( long ) || variableType == typeof( ulong ) )
            {
                return "decimal";
            }

            return "text";
        }

        public async Task StopEventHandlingAsync ()
        {
            // Cancel event queue threads and wait for them to complete
            consumerCancellationTS.Cancel();
            await Task.WhenAny(
                Task.Run( async () =>
                {
                    while ( taskQueues.Values.Any( q => q.isRunning ) )
                    {
                        await Task.Delay( TimeSpan.FromMilliseconds( 25 ) ).ConfigureAwait(false);
                    }
                } ),
                Task.Delay( 500 )
            );
            foreach ( var q in taskQueues.Values )
            {
                try
                {
                    q.CompleteAdding();
                    q.Dispose();
                }
                catch ( Exception )
                {
                    // We are stopping event handling. Nothing to do here.
                }
            }
        }
    }

    internal class TaskQueue<T> ( string commandName ) : BlockingCollection<T>
    {
        public string commandName { get; } = commandName;

        public bool isRunning => consumerTask != null &&
                                 consumerTask.Status != TaskStatus.Canceled &&
                                 consumerTask.Status != TaskStatus.Faulted &&
                                 consumerTask.Status != TaskStatus.RanToCompletion;

        private Task? consumerTask { get; set; }

        public void StartOrRestart ( Func<Task> action, CancellationToken cancellationToken )
        {
            if ( !isRunning )
            {
                consumerTask = Task.Factory.StartNew( action,
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                ).Unwrap(); // Unwrap the task to handle exceptions properly
            }
        }
    }
}
