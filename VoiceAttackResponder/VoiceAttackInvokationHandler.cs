using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using EddiSpeechResponder;
using EddiSpeechService;
using EddiStarMapService;
using EddiUI;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal class VoiceAttackInvokationHandler
    {
        private static dynamic VaProxy => VoiceAttackPlugin.VaProxy;
        private static readonly Random random = new Random();

        public static void HandleInvokedCommand(dynamic vaProxy)
        {
            try
            {
                switch (((string)vaProxy.Context)?.ToLowerInvariant())
                {
                    case "coriolis":
                        InvokeCoriolis( ref vaProxy );
                        break;
                    case "coriolisbeta":
                        InvokeCoriolis( ref vaProxy, true );
                        break;
                    case "inaracarrier":
                        InvokeInaraFleetCarrier( ref vaProxy );
                        break;
                    case "inaraprofile":
                        InvokeInaraProfile( ref vaProxy );
                        break;
                    case "inarasystem":
                    case "eddbsystem": // Redirect to Inara
                        InvokeInaraSystem( ref vaProxy );
                        break;
                    case "inarastation":
                    case "eddbstation": // Redirect to Inara
                        InvokeInaraStation( ref vaProxy );
                        break;
                    case "edshipyard":
                        InvokeEDShipyard(ref vaProxy);
                        break;
                    case "profile":
                        InvokeUpdateProfile();
                        break;
                    case "say":
                        InvokeSay(ref vaProxy);
                        break;
                    case "speech":
                        InvokeSpeech(ref vaProxy);
                        break;
                    case "system comment":
                        InvokeStarMapSystemComment(ref vaProxy);
                        break;
                    case "initialize eddi":
                        if (VoiceAttackPlugin.VaProxy != null && Application.Current != null)
                        {
                            vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                        }
                        else
                        {
                            VoiceAttackPlugin.VA_Init1(vaProxy);  // Attempt initialization again to see if it works this time...
                        }
                        break;
                    case "configuration":
                    case "configurationminimize":
                    case "configurationmaximize":
                    case "configurationrestore":
                    case "configurationclose":
                        // Ignore any attempt to access the EDDI UI if VA
                        // doesn't own the EDDI instance.
                        if (VoiceAttackPlugin.VaProxy != null && Application.Current != null)
                        {
                            InvokeConfiguration(ref vaProxy);
                        }
                        else
                        {
                            vaProxy.WriteToLog("The EDDI plugin is not fully initialized.", "red");
                        }
                        break;
                    case "shutup":
                        InvokeShutUp(ref vaProxy);
                        break;
                    case "setstate":
                        InvokeSetState(ref vaProxy);
                        break;
                    case "disablespeechresponder":
                        InvokeDisableSpeechResponder(ref vaProxy);
                        break;
                    case "enablespeechresponder":
                        InvokeEnableSpeechResponder(ref vaProxy);
                        break;
                    case "setspeechresponderpersonality":
                        InvokeSetSpeechResponderPersonality(ref vaProxy);
                        break;
                    case "jumpdetails":
                        InvokeJumpDetails(ref vaProxy);
                        break;
                    case "transmit":
                        InvokeTransmit(ref vaProxy);
                        break;
                    case "missionsroute":
                    case "route":
                        InvokeRouteDetails(ref vaProxy);
                        break;
                    case "inara":
                        InvokeInaraProfileDetails(ref vaProxy);
                        break;
                    case "volume":
                        InvokeVolume(ref vaProxy);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to invoke context " + vaProxy.Context, e);
                vaProxy.WriteToLog("Failed to invoke context " + vaProxy.Context, "red");
            }
        }

        private static void InvokeVolume ( ref dynamic vaProxy )
        {
            int? volumeInt = vaProxy.GetInt("Volume");

            if ( SpeechService.Instance.Configuration == null )
            { return; }

            // Fix any inputs outside of the expected range
            if ( volumeInt == null )
            { volumeInt = new SpeechServiceConfiguration().Volume; } // Default volume
            else if ( volumeInt < 0 )
            { volumeInt = 0; } // Must be zero or greater
            else if ( volumeInt > 100 )
            { volumeInt = 100; } // Must be 100 or less

            // Update our speech configuration settings
            SpeechService.Instance.Configuration.Volume = (int)volumeInt;
            SpeechService.Instance.Configuration.ToFile();

            // Refresh the UI with the new volume
            Application.Current.Dispatcher.InvokeAsync( () =>
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                if ( mainWindow == null )
                { return; }
                foreach ( var tab in mainWindow.MainTabControl.Items )
                {
                    if ( tab is System.Windows.Controls.TabItem tabItem && tabItem.Content is TextToSpeechTab tts )
                    {
                        tts.ConfigureTTS();
                    }
                }
            } );
        }

        private static void InvokeInaraProfileDetails ( ref dynamic vaProxy )
        {
            string commanderName = vaProxy.GetText("Name");
            if ( commanderName == null )
            {
                return;
            }
            try
            {
                EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                var result = inaraService.GetCommanderProfile(commanderName);
                if ( result != null )
                {
                    OpenOrStoreURI( ref vaProxy, result.url );
                }
                else
                {
                    Logging.Debug( "No information on commander " + commanderName );
                }
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to obtain Inara details on commander " + commanderName, ex );
            }
        }

        private static void InvokeConfiguration ( ref dynamic vaProxy )
        {
            string config = (string)vaProxy.Context;

            if ( Application.Current?.Dispatcher != null
                && ( Application.Current?.Dispatcher?.Invoke( () => Application.Current.MainWindow == null ) ?? false )
                && config != "configuration" )
            {
                vaProxy.WriteToLog( "The EDDI configuration window is not open.", "orange" );
                return;
            }

            switch ( config )
            {
                case "configuration":
                    if ( Application.Current?.Dispatcher != null )
                    {
                        Application.Current.Dispatcher.InvokeAsync( () =>
                        {
                            try
                            {
                                if ( Application.Current?.MainWindow?.Visibility == Visibility.Collapsed
                                    || Application.Current?.MainWindow?.Visibility == Visibility.Hidden )
                                {
                                    Application.Current.MainWindow?.Show();
                                }
                                else
                                {
                                    // Tell the configuration UI to restore its window if minimized
                                    setWindowState( VaProxy, WindowState.Minimized, true, false );
                                    VaProxy.WriteToLog( "The EDDI configuration window is already open.", "orange" );
                                }
                            }
                            catch ( Exception ex )
                            {
                                Logging.Warn( "Show configuration window failed", ex );
                            }
                        } );
                    }
                    break;
                case "configurationminimize":
                    setWindowState( vaProxy, WindowState.Minimized );
                    break;
                case "configurationmaximize":
                    setWindowState( vaProxy, WindowState.Maximized );
                    break;
                case "configurationrestore":
                    setWindowState( vaProxy, WindowState.Normal );
                    break;
                case "configurationclose":
                    Application.Current?.Dispatcher?.InvokeAsync( () => Application.Current?.MainWindow?.Hide() );
                    break;
                default:
                    vaProxy.WriteToLog( "Plugin context \"" + (string)vaProxy.Context + "\" not recognized.", "orange" );
                    break;
            }
        }

        // Set main window minimize, maximize and normal states. Ignore and warn
        // if the main window is blocked waiting for a modal dialog to close.
        private static void setWindowState ( dynamic vaProxy, WindowState newState, bool minimizeCheck = false, bool warn = true )
        {
            if ( EDDI.Instance.SpeechResponderModalWait && warn )
            {
                System.Media.SystemSounds.Beep.Play();
                vaProxy.WriteToLog( "The EDDI window state cannot be changed at this time.", "orange" );
            }
            else
            {
                Application.Current?.Dispatcher?.InvokeAsync( () =>
                {
                    MainWindow mainwindow = (MainWindow)Application.Current?.MainWindow;
                    mainwindow?.Dispatcher?.Invoke( mainwindow.VaWindowStateChange, newState, minimizeCheck );
                } );
            }
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile ()
        {
            Task.Run( async () =>
            {
                await EDDI.Instance.refreshProfileAsync( true );
            } );
        }

        private static void OpenOrStoreURI ( ref dynamic vaProxy, string systemUri )
        {
            if ( vaProxy.GetBoolean( "EDDI open uri in browser" ) != false )
            {
                Logging.Debug( "Starting process with uri " + systemUri );
                HandleUri( ref vaProxy, systemUri );
            }
            Logging.Debug( "Writing URI to `{TXT:EDDI uri}`: " + systemUri );
            vaProxy.SetText( "EDDI uri", systemUri );
        }

        public static void InvokeInaraSystem ( ref dynamic vaProxy )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.CurrentStarSystem == null )
                {
                    Logging.Debug( "No information on current system" );
                    return;
                }
                string systemUri = $"https://inara.cz/elite/starsystem/?search={EDDI.Instance.CurrentStarSystem.systemAddress}";
                OpenOrStoreURI( ref vaProxy, systemUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send system data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraStation ( ref dynamic vaProxy )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.CurrentStarSystem == null )
                {
                    Logging.Debug( "No information on current station" );
                    return;
                }
                if ( EDDI.Instance.CurrentStation == null )
                {
                    // Missing current star system information
                    Logging.Debug( "No information on current station" );
                    return;
                }
                string stationUri = $"https://inara.cz/elite/station/?search={EDDI.Instance.CurrentStation.marketId}";
                OpenOrStoreURI( ref vaProxy, stationUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send station data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraFleetCarrier ( ref dynamic vaProxy )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.FleetCarrier == null )
                {
                    Logging.Debug( "No information on fleet carrier" );
                    return;
                }
                string carrierUri = $"https://inara.cz/elite/cmdr-fleetcarrier/?search={EDDI.Instance.FleetCarrier.callsign}";
                OpenOrStoreURI( ref vaProxy, carrierUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send fleet carrier data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraProfile ( ref dynamic vaProxy )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.Cmdr?.InaraID == null )
                {
                    Logging.Debug( "No information on Inara commander" );
                    return;
                }
                string cmdrUri = $"https://inara.cz/elite/cmdr/{EDDI.Instance.Cmdr.InaraID}/";
                OpenOrStoreURI( ref vaProxy, cmdrUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send Inara commander data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeCoriolis ( ref dynamic vaProxy, bool beta = false )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.CurrentShip == null )
                {
                    Logging.Debug( "No information on ship" );
                    return;
                }

                var shipUri = EDDI.Instance.CurrentShip.CoriolisUri(beta);
                OpenOrStoreURI( ref vaProxy, shipUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send ship data to coriolis", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeEDShipyard ( ref dynamic vaProxy )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.CurrentShip == null )
                {
                    Logging.Debug( "No information on ship" );
                    return;
                }

                string shipUri = EDDI.Instance.CurrentShip.EDShipyardUri();
                OpenOrStoreURI( ref vaProxy, shipUri );
                VoiceAttackVariables.setStatus( vaProxy, "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to send ship data to coriolis", e );
            }
            Logging.Debug( "Leaving" );
        }

        /// <summary>
        /// Handle a URI, either sending it to the default web browser or putting it on the clipboard
        /// </summary>
        private static void HandleUri ( ref dynamic vaProxy, string uri )
        {
            bool? useClipboard = vaProxy.GetBoolean("EDDI use clipboard");
            if ( useClipboard != null && useClipboard == true )
            {
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetData( DataFormats.Text, uri );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Warn( "Failed to set clipboard", ex );
                    }
                } );
                thread.SetApartmentState( ApartmentState.STA );
                thread.Start();
                thread.Join();
            }
            else
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), "\"" + uri + "\"")
                {
                    UseShellExecute = false
                };
                Process.Start( proc );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSay ( ref dynamic vaProxy )
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                Ship ship = null;
                if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.CurrentShip;
                }

                SpeechService.Instance.Say( ship, speech, (int)priority, voice, false, null, true );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to run EDDI's internal speech system (say)", e );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary> 
        public static void InvokeTransmit ( ref dynamic vaProxy )
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                Ship ship = null;
                if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.CurrentShip;
                }

                SpeechService.Instance.Say( ship, speech, (int)priority, voice, true, null, true );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to run EDDI's internal speech system (transmit)", e );
            }
        }

        /// <summary>
        /// Stop talking
        /// </summary>
        public static void InvokeShutUp ( ref dynamic vaProxy )
        {
            try
            {
                SpeechService.Instance.ShutUp();
                SpeechService.Instance.StopAudio();
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to shut up", e );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSpeech ( ref dynamic vaProxy )
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority");

                string voice = vaProxy.GetText("Voice");

                var speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                if ( speechResponder == null )
                {
                    Logging.Warn( "Unable to find speech responder" );
                }

                Ship ship = null;
                if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.CurrentShip;
                }

                // sayOutLoud must be true to match the behavior described by the wiki for the `disablespeechresponder` command
                // i.e. "not talk unless specifically asked for information"
                speechResponder?.Say( ship, script, null, priority, voice, true, true );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to run internal speech system", e );
            }
        }

        public static void InvokeDisableSpeechResponder ( ref dynamic vaProxy )
        {
            try
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = true;
                SpeechService.Instance.speechQueue.DequeueAllSpeech();
                SpeechService.Instance.StopCurrentSpeech();
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to disable speech responder", e );
            }
        }

        public static void InvokeEnableSpeechResponder ( ref dynamic vaProxy )
        {
            try
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = false;
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to enable speech responder", e );
            }
        }

        public static void InvokeSetSpeechResponderPersonality ( ref dynamic vaProxy )
        {
            string personality = vaProxy.GetText("Personality");
            try
            {
                var speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                speechResponder?.SetPersonality( personality );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to set speech responder personality", e );
            }
        }

        public static void InvokeSetState ( ref dynamic vaProxy )
        {
            try
            {
                string name = vaProxy.GetText("State variable");
                if ( string.IsNullOrEmpty( name ) )
                {
                    Logging.Info( "No value in the VoiceAttack text variable 'State variable'; nothing to set" );
                    return;
                }

                // State variable names are lower-case
                string stateVariableName = name.ToLowerInvariant().Replace(" ", "_");

                string strValue = vaProxy.GetText(name);
                if ( strValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = strValue;
                    return;
                }

                int? shortValue = vaProxy.GetSmallInt(name);
                if ( shortValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = shortValue;
                    return;
                }

                int? intValue = vaProxy.GetInt(name);
                if ( intValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = intValue;
                    return;
                }

                bool? boolValue = vaProxy.GetBoolean(name);
                if ( boolValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = boolValue;
                    return;
                }

                decimal? decValue = vaProxy.GetDecimal(name);
                if ( decValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = decValue;
                    return;
                }

                // Nothing above, so set the item to null
                EDDI.Instance.State[ stateVariableName ] = null;
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to set state", e );
            }
        }

        public static string SpeechFromScript ( string script )
        {
            if ( script == null )
            { return null; }

            // Variable replacement
            Ship ship = EDDI.Instance.CurrentShip;
            if ( ship != null )
            {
                script = script.Replace( "$=", ship.phoneticname );
            }

            string cmdrScript;
            if ( string.IsNullOrEmpty( EDDI.Instance.Cmdr?.name ) )
            {
                cmdrScript = EddiCore.Properties.Resources.Commander;
            }
            else
            {
                cmdrScript = EDDI.Instance.Cmdr.phoneticname;
            }
            script = script.Replace( "$-", cmdrScript );

            // Multiple choice selection
            StringBuilder sb = new StringBuilder();

            // Step 1 - resolve any options in square brackets
            Match matchResult = Regex.Match(script, @"\[[^\]]*\]|[^\[\]]+");
            while ( matchResult.Success )
            {
                if ( matchResult.Value.StartsWith( "[" ) )
                {
                    // Remove the brackets and pick one of the options
                    string result = matchResult.Value.Substring(1, matchResult.Value.Length - 2);
                    string[] options = result.Split(';');
                    sb.Append( options[ random.Next( 0, options.Length ) ] );
                }
                else
                {
                    // Pass it right along
                    sb.Append( matchResult.Groups[ 0 ].Value );
                }
                matchResult = matchResult.NextMatch();
            }
            string res = sb.ToString();

            // Step 2 - resolve phrases separated by semicolons
            if ( res.Contains( ";" ) )
            {
                // Pick one of the options
                string[] options = res.Split(';');
                res = options[ random.Next( 0, options.Length ) ];
            }

            // Step 3 - pass it through the script resolver
            res = new EddiSpeechResponder.ScriptResolverService.ScriptResolver( null ).resolveFromValue( res, true );

            return res ?? "";
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        public static void InvokeStarMapSystemComment ( ref dynamic vaProxy )
        {
            try
            {
                string comment = vaProxy.GetText("EDDI system comment");
                if ( comment == null )
                {
                    return;
                }

                if ( EDDI.Instance.CurrentStarSystem != null )
                {
                    // Store locally
                    var systemAddress = EDDI.Instance.CurrentStarSystem.systemAddress;
                    var currentSystem = EDDI.Instance.DataProvider.GetOrFetchStarSystem(systemAddress);
                    currentSystem.comment = comment == "" ? null : comment;
                    EDDI.Instance.DataProvider.SaveStarSystem( currentSystem );

                    // Store in EDSM
                    var edsmService = new StarMapService(null, true);
                    edsmService.sendStarMapComment( systemAddress, comment );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to store system comment", e );
            }
        }

        public static void InvokeJumpDetails ( ref dynamic vaProxy )
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                if ( !string.IsNullOrEmpty( type ) )
                {
                    var detail = EDDI.Instance.CurrentShip?.JumpDetails(type);
                    vaProxy.SetDecimal( "Ship jump detail distance", detail?.distance );
                    vaProxy.SetInt( "Ship jump detail jumps", detail?.jumps );
                    vaProxy.SetText( "Type variable", null );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to get jump details", e );
            }
        }

        public static void InvokeRouteDetails ( ref dynamic vaProxy )
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                string string0 = vaProxy.GetText("System variable");
                string string1 = vaProxy.GetText("System variable 2") ?? vaProxy.GetText("Station variable");
                decimal? numeric = vaProxy.GetDecimal("Numeric variable");
                bool? boolean = vaProxy.GetBoolean ("Boolean variable");

                vaProxy.SetText( "Type variable", null );
                vaProxy.SetText( "System variable", null );
                vaProxy.SetText( "System variable 2", null );
                vaProxy.SetText( "Station variable", null );
                vaProxy.SetDecimal( "Numeric variable", null );
                vaProxy.SetBoolean( "Boolean variable", null );

                if ( Enum.TryParse( type, true, out QueryType result ) )
                {
                    var @event = NavigationService.Instance.NavQuery(result, string0, string1, numeric, boolean);
                    if ( @event != null )
                    {
                        EDDI.Instance?.enqueueEvent( @event );
                    }
                }
                else
                {
                    Logging.Warn( $"The search query '{type}' is unrecognized." );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( vaProxy, "Failed to get route", e );
            }
        }

    }
}
