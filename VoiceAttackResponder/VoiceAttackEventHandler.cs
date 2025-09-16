using EddiCore;
using EddiEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal class VoiceAttackEventHandler
    {
        private readonly ConcurrentDictionary<string, TaskQueue<Event>> taskQueues = new ConcurrentDictionary<string, TaskQueue<Event>>();
        private readonly CancellationTokenSource consumerCancellationTS = new CancellationTokenSource(); // This must be static so that it is visible to child threads and tasks

        public VoiceAttackEventHandler()
        {
            Logging.Debug( "Started VoiceAttack event handler" );
        }

        // We'll maintain a referenceable list of variables that we've set from events
        private readonly ConcurrentDictionary<string, VoiceAttackVariable> currentVariables = new ConcurrentDictionary<string, VoiceAttackVariable>();

        public void Handle ( Event theEvent )
        {
            if ( theEvent is null || consumerCancellationTS.IsCancellationRequested ) { return; }
            var taskQueue = taskQueues.GetOrAdd(theEvent.type, _ => new TaskQueue<Event>());
            taskQueue.StartOrRestart( async () => await dequeueEvents( taskQueue ), consumerCancellationTS.Token );
        }

        private async Task dequeueEvents ( TaskQueue<Event> eventQueue )
        {
            try
            {
                foreach ( var @event in eventQueue.GetConsumingEnumerable( consumerCancellationTS.Token ) )
                {
                    await ExecuteEventAsync(@event);
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
                    eventQueue.StartOrRestart( async () => await dequeueEvents( eventQueue ), consumerCancellationTS.Token );
                }
            }
        }

        private async Task ExecuteEventAsync(Event @event)
        {
            try
            {
                if ( @event.type != null )
                {
                    Logging.Debug( $"Passing event {@event.type} to VoiceAttack", @event );
                    updateValuesOnEvent( @event );
                    if ( TryTriggerVACommands( @event ) )
                    {
                        // We need to wait until each event is no longer active before moving to the next from the same
                        // queue / event type so that variables aren't overwritten before VoiceAttack can respond.
                        // Other queues / event types will be able to continue processing events while we wait.
                        await VoiceAttackPlugin.WaitForCommandExecutionAsync( $"((EDDI {@event.type.ToLowerInvariant()}))" );                                
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( $"VoiceAttack failed to handle {@event.type} event.", ex );
            }
        }

        private void updateValuesOnEvent ( Event @event )
        {
            try
            {
                VoiceAttackPlugin.SetText( "EDDI event", @event.type );

                // Retrieve and clear variables from prior iterations of the same event
                clearPriorEventValues( @event.type );

                // Update monitor variables
                foreach ( var monitor in EDDI.Instance.monitors )
                {
                    foreach ( var kv in monitor.GetVariables() ?? new Dictionary<string, Tuple<Type, object>>() )
                    {
                        var variableName = kv.Key;
                        var variableType = kv.Value.Item1;
                        var variableValue = kv.Value.Item2;

                        var values = VoiceAttackVariables.Convert(
                            new MetaVariables( variableType, variableValue ).Results, "EDDI", variableName );
                        foreach ( var var in values )
                        {
                            var.Set();
                            currentVariables[ var.key ] = var;
                        }
                    }
                }

                // Prepare and update this event's variable values
                // Save the updated state of our event variables
                var eventVariables = VoiceAttackVariables.Convert( new MetaVariables(@event.GetType(), @event).Results, "EDDI", @event.type);
                foreach ( var var in eventVariables )
                {
                    var.Set();
                    currentVariables[var.key] = var;
                }
                Logging.Debug( $"Set VoiceAttack variables for EDDI event {@event.type}", eventVariables );
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to set event variables in VoiceAttack", ex );
            }
        }

        private void clearPriorEventValues ( string eventType )
        {
            try
            {
                // We clear variable values by swapping the values to null and then instructing VA to set them again
                foreach ( var variable in currentVariables.Values
                             .Where( v => v.eventType == eventType && v.value != null ) )
                {
                    variable.value = null;
                    variable.Set();
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to clear event variables in VoiceAttack", ex );
            }
        }

        private bool TryTriggerVACommands ( Event @event )
        {
            var commandName = $"((EDDI {@event.type.ToLowerInvariant()}))";
            try
            {
                // Fire local command if present  
                if ( VoiceAttackPlugin.CommandExists( commandName ) ) 
                {
                    VoiceAttackPlugin.ExecuteCommand( commandName );
                    Logging.Info( $"Executed command '{commandName}'" );
                    return true;
                }
                else
                {
                    Logging.Debug( $"Command '{commandName}' not found." );
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to trigger local VoiceAttack command " + commandName, ex );
            }
            return false;
        }

        public void StopEventHandling ()
        {
            // Cancel event queue threads and wait for them to complete
            consumerCancellationTS?.Cancel();
            Task.WhenAny(
                Task.Run( async () =>
                {
                    while ( taskQueues.Values.Any( q => q.isRunning ) )
                    {
                        await Task.Delay( TimeSpan.FromMilliseconds( 25 ) );
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

    internal class TaskQueue<T> : BlockingCollection<T>
    {
        public bool isRunning => consumerTask != null &&
                                 consumerTask.Status != TaskStatus.Canceled &&
                                 consumerTask.Status != TaskStatus.Faulted &&
                                 consumerTask.Status != TaskStatus.RanToCompletion;

        private Task consumerTask { get; set; }

        public void StartOrRestart ( Func<Task> action, CancellationToken cancellationToken )
        {
            if ( !isRunning )
            {
                consumerTask = Task.Factory.StartNew(
                    async () => await action(),
                    cancellationToken,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default
                ).Unwrap(); // Unwrap the task to handle exceptions properly
            }
        }
    }
}
