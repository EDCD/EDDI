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
        private static EddiVoiceAttackAdapter.Client.VoiceAttackEventHandler? _eventHandler;
        private static PropertyChangedEventHandler? _propertyChangedHandler;
        private static bool _initialized;

        /// <summary>
        /// Initialize responder mode subscriptions and event forwarding for VoiceAttack integration.
        /// Called from EDDI startup when FromVA=true and IPC connection is established.
        /// </summary>
        internal static async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                Logging.Info("Initializing EDDI VoiceAttack responder mode");

                // Push initial state to VA (standard variables and configuration)
                VoiceAttackVariables.initializeStandardValues();

                // Subscribe to EDDI events and property changes
                EDDI.Instance.PropertyChanged += OnEddiPropertyChanged;
                EDDI.Instance.State.CollectionChanged += OnEddiStateCollectionChanged;
                EDDI.Instance.State.PropertyChanged += OnEddiStatePropertyChanged;
                SpeechService.Instance.SpeechManager.PropertyChanged += OnSpeechPropertyChanged;
                CompanionAppService.Instance.StateChanged += OnCapiStateChanged;
                EddiConfigService.ConfigService.Instance.PropertyChanged += VoiceAttackVariables.updateConfigurationValues;

                // Initialize event handler for EDDI event dispatch through IPC
                try
                {
                    _eventHandler = new EddiVoiceAttackAdapter.Client.VoiceAttackEventHandler();
                    _propertyChangedHandler = DispatchEddiEventAsync;
                    EDDI.Instance.PropertyChanged += _propertyChangedHandler;
                    Logging.Debug("Event handler initialized for EDDI event dispatch to VoiceAttack");
                }
                catch (Exception ex)
                {
                    Logging.Warn($"Failed to initialize event handler: {ex.Message}");
                }

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

                RuntimeWriteToLog("The EDDI plugin is fully operational.", "green");
                VoiceAttackVariables.setStatus("Operational");

                // Fire initialization event
                EDDI.Instance.enqueueEvent(new VAInitializedEvent(DateTime.UtcNow));

                _initialized = true;
                Logging.Info("EDDI VoiceAttack responder mode initialization complete");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize VoiceAttack responder mode", e);
                RuntimeWriteToLog("Unable to fully initialize EDDI VoiceAttack integration. Some functions may not work.", "red");
            }
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

                _eventHandler = null;
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
            if (_eventHandler == null || string.IsNullOrEmpty(e?.PropertyName))
            {
                return;
            }

            // Dispatch event through handler asynchronously (fire-and-forget with error handling)
            var task = _eventHandler.DispatchEventAsync(
                eventType: "EddiPropertyChanged",
                eventName: e.PropertyName,
                eventPayload: new System.Collections.Generic.Dictionary<string, object>
                {
                    { "property", e.PropertyName }
                }
            );

            task.SafeFireAndForget(ex => Logging.Error($"Failed to dispatch EDDI property event to VoiceAttack: {ex.Message}", ex));
        }

        private static void RuntimeWriteToLog( string message, string color )
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
    }
}
