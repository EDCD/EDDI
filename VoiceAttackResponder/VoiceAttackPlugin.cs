using Eddi;
using EddiCompanionAppService;
using EddiCore;
using EddiCore.Upgrader;
using EddiEvents;
using EddiSpeechService;
using JetBrains.Annotations;
using System;
using System.Threading;
using System.Windows;
using Utilities;

namespace EddiVoiceAttackResponder
{
    [UsedImplicitly]
    public class VoiceAttackPlugin
    {
        // ReSharper disable once UnusedMember.Global - VA Interface Member
        public static string VA_DisplayName()
        {
            return Constants.EDDI_NAME + " " + Constants.EDDI_VERSION;
        }

        // ReSharper disable once UnusedMember.Global - VA Interface Member
        public static string VA_DisplayInfo()
        {
            return Constants.EDDI_NAME + "\r\nVersion " + Constants.EDDI_VERSION;
        }

        // ReSharper disable once UnusedMember.Global - VA Interface Member
        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        internal static dynamic VaProxy;
        internal static readonly object vaProxyLock = new object();

        private static VoiceAttackEventHandler voiceAttackEventHandler;

        // ReSharper disable once MemberCanBePrivate.Global - VA Interface Member
        public static void VA_Init1(dynamic vaProxy)
        {
            // Initialize and launch an EDDI instance without opening the main window
            // VoiceAttack commands will be used to manipulate the window state.

            if (App.AlreadyRunning()) { return; }

            lock ( vaProxyLock )
            {
                VaProxy = vaProxy;
                App.VaProxy = vaProxy;
            }

            App.vaStartup = () =>
            {
                try
                {
                    Logging.Info("Initialising EDDI VoiceAttack plugin");

                    // Set initial values for standard variables
                    VoiceAttackVariables.initializeStandardValues();

                    // Set up our event responder.
                    voiceAttackEventHandler = new VoiceAttackEventHandler();
                    VoiceAttackResponder.RaiseEvent += (s, @event) => { voiceAttackEventHandler.Handle(@event); };

                    // Add notifiers for changes in variables we want to react to 
                    // (we can only use event handlers with classes which are always constructed - nullable objects will be updated via responder events)
                    EDDI.Instance.PropertyChanged += (s, e) => VoiceAttackVariables.updateStandardValues( e, VaProxy);
                    EDDI.Instance.State.CollectionChanged += (s, e) =>
                    {
                        VoiceAttackVariables.setDictionaryValues( EDDI.Instance.State, "state", VaProxy );
                    };
                    EDDI.Instance.State.PropertyChanged += ( s, e ) =>
                    {
                        VoiceAttackVariables.setDictionaryValues( EDDI.Instance.State, "state", VaProxy );
                    };
                    SpeechService.Instance.PropertyChanged += (s, e) =>
                    {
                        VoiceAttackVariables.setSpeechState( e);
                    };
                    CompanionAppService.Instance.StateChanged += (oldState, newState) =>
                    {
                        VoiceAttackVariables.setCAPIState( newState == CompanionAppService.State.Authorized, VaProxy );
                    };

                    EddiConfigService.ConfigService.Instance.PropertyChanged += VoiceAttackVariables.updateConfigurationValues;

                    // Display instance information if available
                    if (EddiUpgrader.UpgradeRequired)
                    {
                        VaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "red");
                        string msg = Properties.VoiceAttack.run_eddi_standalone;
                        SpeechService.Instance.Say(null, msg, 0);
                    }
                    else if (EddiUpgrader.UpgradeAvailable)
                    {
                        VaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange");
                        string msg = Properties.VoiceAttack.run_eddi_standalone;
                        SpeechService.Instance.Say(null, msg, 0);
                    }

                    if (EddiUpgrader.Motd != null)
                    {
                        VaProxy.WriteToLog("Message from EDDI: " + EddiUpgrader.Motd, "black");
                        string msg = String.Format(EddiCore.Properties.Resources.msg_from_eddi, EddiUpgrader.Motd);
                        SpeechService.Instance.Say(null, msg, 0);
                    }

                    VaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                    VoiceAttackVariables.setStatus( VaProxy, "Operational");

                    // Fire an event once the VA plugin is initialized
                    EDDI.Instance.enqueueEvent(new VAInitializedEvent(DateTime.UtcNow));

                    // Set a variable indicating the version of VoiceAttack in use
                    if ( vaProxy.VAVersion is System.Version version )
                    {
                        EDDI.Instance.vaVersion = version;
                    }

                    Logging.Info("EDDI VoiceAttack plugin initialization complete");
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to initialize VoiceAttack plugin", e);
                    vaProxy.WriteToLog("Unable to fully initialize EDDI. Some functions may not work.", "red");
                }
            };

            var appThread = new Thread(App.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Style", "IDE0060:Remove unused parameter", Justification = "VA API" )]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global - VoiceAttack API
        public static void VA_Exit1(dynamic vaProxy)
        {
            lock ( vaProxyLock )
            {
                VaProxy = vaProxy;
                App.VaProxy = vaProxy;
            }

            Logging.Info("EDDI VoiceAttack plugin exiting");

            // Cancel event queue threads and wait for them to complete
            voiceAttackEventHandler.StopEventHandling();

            // Stop all monitors and responders
            EDDI.Instance.Stop();

            // Release the mutex
            if ( !App.eddiMutex.SafeWaitHandle.IsClosed )
            {
                App.eddiMutex.ReleaseMutex();
            }

            // Finish the shutdown
            Application.Current.Dispatcher.Invoke( () =>
            {
                Application.Current.Shutdown();
            } );
        }

        [UsedImplicitly]
        public static void VA_StopCommand()
        { }

        [UsedImplicitly]
        public static void VA_Invoke1(dynamic vaProxy)
        {
            lock ( vaProxyLock )
            {
                VaProxy = vaProxy;
                App.VaProxy = vaProxy;
            }

            Logging.Debug("Invoked with context " + (string)vaProxy.Context);

            // This thread is invoked from VoiceAttack and may by invoked with the system default culture
            // so make sure that we're using our assigned culture.
            App.ApplyAnyOverrideCulture();

            VoiceAttackInvokationHandler.HandleInvokedCommand(vaProxy);
        }
    }
}
