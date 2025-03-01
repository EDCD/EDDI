using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal class VoiceAttackEventHandler
    {
        private dynamic VaProxy => VoiceAttackPlugin.VaProxy;
        private readonly ConcurrentDictionary<string, TaskQueue<Event>> taskQueues = new ConcurrentDictionary<string, TaskQueue<Event>>();
        private readonly CancellationTokenSource consumerCancellationTS = new CancellationTokenSource(); // This must be static so that it is visible to child threads and tasks

        public VoiceAttackEventHandler()
        {
            Logging.Debug( "Started VoiceAttack event handler" );
        }

        // We'll maintain a referenceable list of variables that we've set from events
        private readonly ConcurrentDictionary<string, VoiceAttackVariable> currentVariables = new ConcurrentDictionary<string, VoiceAttackVariable>();

        // If running VoiceAttack version 1.7.4 or later then we should use the more modern API endpoints
        private bool useLegacyVACommandAPI => EDDI.Instance.vaVersion?.CompareTo( new System.Version( 1, 7, 4 ) ) <= 0;

        public void Handle ( Event theEvent )
        {
            if ( theEvent is null || consumerCancellationTS.IsCancellationRequested ) { return; }

            if ( taskQueues.TryGetValue( theEvent.type, out var taskQueue ) && !taskQueue.IsAddingCompleted )
            {
                // Add our event to an existing blocking collection for that event type.
                taskQueue.Add( theEvent );
            }
            else
            {
                taskQueue = new TaskQueue<Event> { theEvent };
                taskQueues.TryAdd( theEvent.type, taskQueue );
            }
            taskQueue.StartOrRestart( () => dequeueEvents( taskQueue ), consumerCancellationTS.Token );
        }

        private async void dequeueEvents ( BlockingCollection<Event> eventQueue )
        {
            try
            {
                foreach ( var @event in eventQueue.GetConsumingEnumerable( consumerCancellationTS.Token ) )
                {
                    try
                    {
                        if ( @event.type != null )
                        {
                            Logging.Debug( $"Passing event {@event.type} to VoiceAttack", @event );
                            await Task.Run( () => updateValuesOnEvent( @event ) );
                            var isCommandFound = TryTriggerVACommands( @event );
                            // We need to wait until each event is no longer active before moving to the next from the same
                            // queue / event type so that variables aren't overwritten before VoiceAttack can respond.
                            // Other queues / event types will be able to continue processing events while we wait.
                            var isCommandExecuting = true;
                            while ( isCommandFound && isCommandExecuting )
                            {
                                await Task.Delay( 25 );
                                isCommandExecuting = useLegacyVACommandAPI 
                                    ? VaProxy.CommandActive( "((EDDI " + @event.type.ToLowerInvariant() + "))" ) 
                                    : VaProxy.Command.Active( "((EDDI " + @event.type.ToLowerInvariant() + "))" );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( $"VoiceAttack failed to handle {@event.type} event.", ex );
                    }

                    if ( eventQueue.Count == 0 )
                    {
                        // If the event queue is empty then we can complete and clean up the associated consumer task.
                        break;
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                // Task canceled. Nothing to do here.
            }
        }

        private void updateValuesOnEvent ( Event @event )
        {
            try
            {
                Logging.Debug( $"Processing EDDI event {@event.type}:", @event );
                var startTime = DateTime.UtcNow;
                VaProxy.SetText( "EDDI event", @event.type );

                // Retrieve and clear variables from prior iterations of the same event
                clearPriorEventValues( @event.type );

                // Update monitor variables
                foreach ( var monitor in EDDI.Instance.monitors )
                {
                    foreach ( var kv in monitor.GetVariables() )
                    {
                        var variableName = kv.Key;
                        var variableType = kv.Value.Item1;
                        var variableValue = kv.Value.Item2;

                        if ( variableValue is StarSystem starSystem )
                        {
                            VoiceAttackVariables.setStarSystemValues( starSystem, variableName, VaProxy );
                        }
                        else
                        {
                            var values = new MetaVariables(variableType, variableValue)
                                .Results
                                .AsVoiceAttackVariables("EDDI", variableName);
                            foreach ( var var in values )
                            {
                                var.Set( VaProxy );
                                currentVariables[ var.key ] = var;
                            }
                        }
                    }
                }

                // Prepare and update this event's variable values
                // Save the updated state of our event variables
                var eventVariables = new MetaVariables(@event.GetType(), @event)
                    .Results
                    .AsVoiceAttackVariables("EDDI", @event.type);
                foreach ( var var in eventVariables )
                {
                    var.Set( VaProxy );
                    currentVariables[var.key] = var;
                }
                Logging.Debug( $"Set VoiceAttack variables for EDDI event {@event.type}", eventVariables );
                Logging.Debug( $"Processed EDDI event {@event.type} in {( DateTime.UtcNow - startTime ).Milliseconds} milliseconds:", @event );
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
                    variable.Set( VaProxy );
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to clear event variables in VoiceAttack", ex );
            }
        }

        private bool TryTriggerVACommands ( Event @event )
        {
            string commandName = "((EDDI " + @event.type.ToLowerInvariant() + "))";
            try
            {
                // Fire local command if present  
                Logging.Debug( "Searching for command " + commandName );
                var commandExists = useLegacyVACommandAPI
                    ? VaProxy.CommandExists( commandName )
                    : VaProxy.Command.Exists( commandName );
                if ( commandExists ) 
                {
                    Logging.Debug( "Found command " + commandName );
                    if ( useLegacyVACommandAPI )
                    {
                        VaProxy.ExecuteCommand( commandName );
                    }
                    else
                    {
                        VaProxy.Command.Execute( commandName );
                    }
                    Logging.Info( "Executed command " + commandName );
                    return true;
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
            foreach ( var q in taskQueues.Values ) { q.CompleteAdding(); q.Dispose(); }
        }
    }

    internal class TaskQueue<T> : BlockingCollection<T>
    {
        public bool isRunning => consumerTask != null &&
                                 consumerTask.Status != TaskStatus.Canceled &&
                                 consumerTask.Status != TaskStatus.Faulted &&
                                 consumerTask.Status != TaskStatus.RanToCompletion;

        private Task consumerTask { get; set; }

        public void StartOrRestart ( Action action, CancellationToken cancellationToken )
        {
            if ( !isRunning )
            {
                consumerTask = Task.Factory.StartNew(
                    action,
                    cancellationToken,
                    TaskCreationOptions.PreferFairness,
                    TaskScheduler.Default
                );
            }
        }
    }
}
