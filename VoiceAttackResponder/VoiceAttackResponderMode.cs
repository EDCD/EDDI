#nullable enable

using EddiCompanionAppService;
using EddiCore;
using EddiCore.Upgrader;
using EddiEvents;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// Manages responder mode initialization when EDDI is running under VoiceAttack.
    /// All event subscriptions, variable synchronization, and IPC communication for VA integration
    /// are centralized here and only active when FromVA=true.
    /// </summary>
    internal static class VoiceAttackResponderMode
    {
        private static PropertyChangedEventHandler? _propertyChangedHandler;
        private static bool _initialized;
        private static readonly SemaphoreSlim standardVariableReplayLock = new(1, 1);

        // Test Hooks
        internal static Action InitializeStandardValues = VoiceAttackVariables.initializeStandardValues;
        internal static Action<string, Exception?> SetStatus = VoiceAttackVariables.setStatus;
        internal static Action<VAInitializedEvent> EnqueueVAInitializedEvent = e => EDDI.Instance.enqueueEvent( e );

        /// <summary>
        /// Initialize responder mode subscriptions and event forwarding for VoiceAttack integration.
        /// Called from EDDI startup when FromVA=true and IPC connection is established.
        /// </summary>
        internal static Task InitializeAsync()
        {
            if (_initialized)
            {
                return Task.CompletedTask;
            }

            try
            {
                Logging.Info("Initializing EDDI VoiceAttack responder mode");

                // Subscribe to EDDI events and property changes. Associated variables will be forwarded to VoiceAttack via IPC after connection is established.
                EDDI.Instance.PropertyChanged += OnEddiPropertyChanged;
                EDDI.Instance.State.CollectionChanged += OnEddiStateCollectionChanged;
                EDDI.Instance.State.PropertyChanged += OnEddiStatePropertyChanged;
                SpeechService.Instance.SpeechManager.PropertyChanged += OnSpeechPropertyChanged;
                CompanionAppService.Instance.StateChanged += OnCapiStateChanged;
                EddiConfigService.ConfigService.Instance.PropertyChanged += VoiceAttackVariables.updateConfigurationValues;

                _propertyChangedHandler = DispatchEddiEventAsync;
                EDDI.Instance.PropertyChanged += _propertyChangedHandler;
                Logging.Debug("EDDI property-change forwarding registered for VoiceAttack IPC broadcast");

                // Check for available upgrades and notify user
                if (EddiUpgrader.UpgradeAvailable)
                {
                    RuntimeWriteToLog(
                        $"EDDI version {EddiUpgrader.UpgradeVersion} is now available. " +
                        "Please shut down VoiceAttack and run EDDI standalone to upgrade",
                        "orange");

                    var msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.SayAsync(null, msg, 0)
                        .SafeFireAndForget(ex => Logging.Error(ex.Message, ex));
                }
                
                _initialized = true;
                Logging.Info("EDDI VoiceAttack responder mode initialization complete");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize VoiceAttack responder mode", e);
                RuntimeWriteToLog("Unable to fully initialize EDDI VoiceAttack integration. Some functions may not work.", "red");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Shutdown responder mode and unsubscribe from all events.
        /// Called from VoiceAttackPlugin.VA_Exit1 during plugin shutdown.
        /// </summary>
        internal static void Shutdown()
        {
            if (!_initialized)
            {
                return;
            }

            try
            {
                Logging.Info("Shutting down EDDI VoiceAttack responder mode");

                // Unsubscribe from all events
                EDDI.Instance.PropertyChanged -= OnEddiPropertyChanged;
                if (_propertyChangedHandler != null)
                {
                    EDDI.Instance.PropertyChanged -= _propertyChangedHandler;
                    _propertyChangedHandler = null;
                }
                EDDI.Instance.State.CollectionChanged -= OnEddiStateCollectionChanged;
                EDDI.Instance.State.PropertyChanged -= OnEddiStatePropertyChanged;
                SpeechService.Instance.SpeechManager.PropertyChanged -= OnSpeechPropertyChanged;
                CompanionAppService.Instance.StateChanged -= OnCapiStateChanged;
                EddiConfigService.ConfigService.Instance.PropertyChanged -= VoiceAttackVariables.updateConfigurationValues;

                _initialized = false;

                Logging.Info("EDDI VoiceAttack responder mode shutdown complete");
            }
            catch (Exception ex)
            {
                Logging.Error("Error during VoiceAttack responder mode shutdown", ex);
            }
        }

        private static void OnCapiStateChanged(CompanionAppService.State oldState, CompanionAppService.State newState)
        {
            VoiceAttackVariables.setCAPIState(newState == CompanionAppService.State.Authorized);
        }

        private static void OnEddiStatePropertyChanged(object? s, PropertyChangedEventArgs e)
        {
            VoiceAttackVariables.setDictionaryValues(EDDI.Instance.State, "state");
        }

        private static void OnEddiStateCollectionChanged(object? s, NotifyCollectionChangedEventArgs e)
        {
            VoiceAttackVariables.setDictionaryValues(EDDI.Instance.State, "state");
        }

        private static void OnEddiPropertyChanged(object? s, PropertyChangedEventArgs e)
        {
            VoiceAttackVariables.updateStandardValues(e);
        }

        private static void OnSpeechPropertyChanged(object? s, PropertyChangedEventArgs e)
        {
            VoiceAttackVariables.setSpeechState(e);
        }

        /// <summary>
        /// Dispatches EDDI property change events to VoiceAttack via IPC.
        /// Runs asynchronously without blocking the UI thread.
        /// </summary>
        private static void DispatchEddiEventAsync(object? sender, PropertyChangedEventArgs? e)
        {
            if (string.IsNullOrEmpty(e?.PropertyName))
            {
                return;
            }

            DispatchEddiEventCoreAsync(e.PropertyName)
                .SafeFireAndForget(ex => Logging.Error($"Failed to dispatch EDDI property event to VoiceAttack: {ex.Message}", ex));
        }

        private static async Task DispatchEddiEventCoreAsync(string propertyName)
        {
            var eventData = new EventData
            {
                EventType = "EddiPropertyChanged",
                EventName = propertyName,
                EventPayload = new Dictionary<string, object>
                {
                    { "property", propertyName }
                }
            };

            var dispatched = await RuntimeEventDispatcher.DispatchAsync(eventData).ConfigureAwait(false);
            if (!dispatched)
            {
                Logging.Debug($"Property change '{propertyName}' could not be dispatched because no runtime IPC dispatcher is registered");
            }
        }

        internal static async Task ReplayStandardValuesAsync (
            string reason,
            CancellationToken cancellationToken = default )
        {
            await standardVariableReplayLock.WaitAsync( cancellationToken ).ConfigureAwait( false );

            try
            {
                Logging.Info( $"Replaying VoiceAttack standard variables ({reason})" );

                InitializeStandardValues();
                SetStatus( "Operational", null );

                // This should now fire after VA IPC is actually connected.
                EnqueueVAInitializedEvent( new VAInitializedEvent( DateTime.UtcNow ) );

                Logging.Info( $"VoiceAttack standard variable replay complete ({reason})" );
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to replay VoiceAttack standard variables", ex );
                throw;
            }
            finally
            {
                standardVariableReplayLock.Release();
            }
        }

        private static void RuntimeWriteToLog( string? message, string? color )
        {
            try
            {
                var eventData = new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = new Dictionary<string, object>
                    {
                        { "action", "write_log" },
                        { "message", message ?? string.Empty },
                        { "color", color ?? "white" }
                    }
                };

                RuntimeEventDispatcher.DispatchAsync( eventData )
                    .GetAwaiter()
                    .GetResult();
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to dispatch runtime write-log payload", ex );
            }
        }

        internal static void ResetTestHooks ()
        {
            InitializeStandardValues = VoiceAttackVariables.initializeStandardValues;
            SetStatus = VoiceAttackVariables.setStatus;
            EnqueueVAInitializedEvent = e => EDDI.Instance.enqueueEvent( e );
        }
    }
}
