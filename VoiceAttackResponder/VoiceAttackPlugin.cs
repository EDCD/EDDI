using Eddi;
using EddiCompanionAppService;
using EddiCore;
using EddiCore.Upgrader;
using EddiEvents;
using EddiSpeechService;
using JetBrains.Annotations;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EddiVoiceAttackResponder
{
    [UsedImplicitly]
    public class VoiceAttackPlugin
    {
        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static string VA_DisplayName()
        {
            return Constants.EDDI_NAME + " " + Constants.EDDI_VERSION;
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static string VA_DisplayInfo()
        {
            return Constants.EDDI_NAME + "\r\nVersion " + Constants.EDDI_VERSION;
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        internal static dynamic VaProxy;
        internal static System.Version VaVersion
        {
            get
            {
                lock ( vaProxyLock )
                {
                    return VaProxy?.VAVersion as System.Version;
                }
            }
        }

        internal static readonly object vaProxyLock = new();

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Init1(dynamic vaProxy)
        {
            // Initialize and launch an EDDI instance without opening the main window
            // VoiceAttack commands will be used to manipulate the window state.

            if ( vaProxy != null )
            {
                lock ( vaProxyLock )
                {
                    VaProxy = vaProxy;
                    App.VoiceAttackVersion = VaProxy.VAVersion as System.Version;
                }
            }

            while ( App.AlreadyRunning() )
            {
                WriteToLog( "An instance of the EDDI application is already running.", "red" );
                var result = MessageBox.Show("An instance of EDDI is already running. Please close\r\n" +
                                             "the open EDDI application and click OK to continue. " +
                                             "If you click CANCEL, the EDDI VoiceAttack plugin will not be fully initialized.",
                        "EDDI Instance Exists",
                        MessageBoxButton.OKCancel, MessageBoxImage.Information);

                // Any response will require the mutex to be reset
                App.eddiMutex.Close();

                if ( MessageBoxResult.Cancel == result )
                {
                    WriteToLog( "EDDI initialization cancelled by user.", "red" );
                    return;
                }
            }

            App.vaStartup = () =>
            {
                try
                {
                    Logging.Info("Initialising EDDI VoiceAttack plugin");

                    // Set initial values for standard variables
                    VoiceAttackVariables.initializeStandardValues();

                    // Add notifiers for changes in variables we want to react to 
                    // (we can only use event handlers with classes which are always constructed - nullable objects will be updated via responder events)
                    EDDI.Instance.PropertyChanged += OnEddiPropertyChanged;
                    EDDI.Instance.State.CollectionChanged += OnEddiStateCollectionChanged;
                    EDDI.Instance.State.PropertyChanged += OnEddiStatePropertyChanged;
                    SpeechService.Instance.SpeechManager.PropertyChanged += OnSpeechPropertyChanged;
                    CompanionAppService.Instance.StateChanged += OnCapiStateChanged;

                    EddiConfigService.ConfigService.Instance.PropertyChanged += VoiceAttackVariables.updateConfigurationValues;

                    // Display instance information if available
                    if ( EddiUpgrader.UpgradeAvailable )
                    {
                        WriteToLog( $"EDDI version {EddiUpgrader.UpgradeVersion} is now available. Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange" );
                        var msg = Properties.VoiceAttack.run_eddi_standalone;
                        SpeechService.Instance.SayAsync( null, msg, 0 )
                            .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    }

                    WriteToLog("The EDDI plugin is fully operational.", "green");
                    VoiceAttackVariables.setStatus( "Operational");

                    // Fire an event once the VA plugin is initialized
                    EDDI.Instance.enqueueEvent(new VAInitializedEvent(DateTime.UtcNow));

                    lock ( vaProxyLock )
                    {
                        Logging.Info( $"VoiceAttack version: {App.VoiceAttackVersion}" );
                    }
                    Logging.Info("EDDI VoiceAttack plugin initialization complete");
                }
                catch (Exception e)
                {
                    Logging.Error("Failed to initialize VoiceAttack plugin", e);
                    WriteToLog("Unable to fully initialize EDDI. Some functions may not work.", "red");
                }
            };

            var appThread = new Thread(App.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();
        }

        private static void OnCapiStateChanged ( CompanionAppService.State oldState, CompanionAppService.State newState )
        {
            VoiceAttackVariables.setCAPIState( newState == CompanionAppService.State.Authorized );
        }

        private static void OnEddiStatePropertyChanged ( object s, PropertyChangedEventArgs e )
        {
            VoiceAttackVariables.setDictionaryValues( EDDI.Instance.State, "state" );
        }

        private static void OnEddiStateCollectionChanged ( object s, NotifyCollectionChangedEventArgs e )
        {
            VoiceAttackVariables.setDictionaryValues( EDDI.Instance.State, "state" );
        }

        private static void OnEddiPropertyChanged ( object s, PropertyChangedEventArgs e )
        {
            VoiceAttackVariables.updateStandardValues( e );
        }
        
        private static void OnSpeechPropertyChanged ( object s, PropertyChangedEventArgs e )
        {
            VoiceAttackVariables.setSpeechState( e );
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Exit1( dynamic _ )
        {
            Logging.Info("EDDI VoiceAttack plugin exiting");

            // Unsubscribe from events
            EDDI.Instance.PropertyChanged -= OnEddiPropertyChanged;
            EDDI.Instance.State.CollectionChanged -=OnEddiStateCollectionChanged;
            EDDI.Instance.State.PropertyChanged -= OnEddiStatePropertyChanged;
            SpeechService.Instance.SpeechManager.PropertyChanged -= OnSpeechPropertyChanged;
            CompanionAppService.Instance.StateChanged -= OnCapiStateChanged;
            EddiConfigService.ConfigService.Instance.PropertyChanged -= VoiceAttackVariables.updateConfigurationValues;

            // Stop monitors and responders
            EDDI.Instance.Stop();

            // Give background tasks a moment to gracefully shut down
            Thread.Sleep( 500 );

            // Force application shutdown to ensure all threads terminate
            Application.Current?.Dispatcher.InvokeAsync( () =>
            {
                Application.Current.Shutdown();
            } );
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_StopCommand()
        { }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Invoke1(dynamic vaProxy)
        {
            lock ( vaProxyLock )
            {
                VaProxy = vaProxy;
            }
            VoiceAttackInvokationHandler.HandleInvokedCommand(vaProxy.Context);
        }

        private static bool IsVaVersionSameOrNewer ( System.Version minVersion )
        {
            lock ( vaProxyLock )
            {
                return ( VaVersion )?.CompareTo( minVersion ) >= 0;
            }
        }

        #region Command Interactions

        // If running VoiceAttack version 1.7.4 or later then we should use the more modern command API endpoints
        private static readonly System.Version commandApiVaVersion = new( 1, 7, 4 );
        
        public static async Task WaitForCommandExecutionAsync ( string commandName )
        {
            var isCommandExecuting = true;
            while ( isCommandExecuting )
            {
                await Task.Delay( 25 ).ConfigureAwait(false);
                lock ( vaProxyLock )
                {
                    isCommandExecuting = IsVaVersionSameOrNewer( commandApiVaVersion )
                        ? VaProxy.Command.Active( commandName )
                        : VaProxy.CommandActive( commandName );
                }
            }
        }

        public static bool CommandExists ( string commandName )
        {
            lock ( vaProxyLock )
            {
                return IsVaVersionSameOrNewer( commandApiVaVersion )
                    ? VaProxy.Command.Exists( commandName )
                    : VaProxy.CommandExists( commandName );
            }
        }

        public static void ExecuteCommand ( string commandName )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( commandApiVaVersion ) )
                {
                    VaProxy.Command.Execute( commandName );
                }
                else
                {
                    // Use the legacy endpoint
                    VaProxy.ExecuteCommand( commandName );
                }
            }
        }

        #endregion

        #region Log Interactions

        public static void WriteToLog ( string message, string color )
        {
            lock ( vaProxyLock )
            {
                VaProxy.WriteToLog( message, color );
            }
        }

        #endregion

        #region Variable Interactions

        // If running VoiceAttack version 1.10.4 or later then we should use the more modern variable API endpoints
        private static readonly System.Version variableApiVaVersion = new( 1, 10, 4 );

        public static bool? GetBoolean ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy.GetBoolean( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy.GetBoolean( key );
            }
        }

        public static DateTime? GetDate ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy.GetDate( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy.GetDate( key );
            }
        }

        public static decimal? GetDecimal ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy.GetDecimal( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy.GetDecimal( key );
            }
        }

        public static int? GetInt ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy.GetInt( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy.GetInt( key );
            }
        }

        public static string GetText ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy.GetText( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy.GetText( key );
            }
        }

        public static void SetBoolean ( string key, bool? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy.SetBoolean( key, value, saveToProfile );
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                VaProxy.SetBoolean( key, value );
            }
        }

        public static void SetDate ( string key, DateTime? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy.SetDate( key, value, saveToProfile );
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                VaProxy.SetDate( key, value );
            }
        }

        public static void SetDecimal ( string key, decimal? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy.SetDecimal( key, value, saveToProfile );
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                VaProxy.SetDecimal( key, value );
            }
        }

        public static void SetInt ( string key, int? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy.SetInt( key, value, saveToProfile );
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                VaProxy.SetInt( key, value );
            }
        }

        public static void SetSmallInt ( string key, short? value )
        {
            lock ( vaProxyLock )
            {
                // SmallInt values are deprecated in VoiceAttack version 2 and later. 
                // We should only use them if the VA version is less than 2.0.0
                if ( IsVaVersionSameOrNewer( new System.Version( 2, 0, 0 ) ) )
                {
                    return;
                }

                // Use the legacy endpoint
                VaProxy.SetSmallInt( key, value );
            }
        }

        public static void SetText ( string key, string value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy.SetText( key, value, saveToProfile );
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                VaProxy.SetText( key, value );
            }
        }

        #endregion
    }
}
