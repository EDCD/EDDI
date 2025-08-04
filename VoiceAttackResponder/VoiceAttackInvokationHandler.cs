using Eddi;
using EddiConfigService;
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
        private static readonly Random random = new Random();

        public static void HandleInvokedCommand(string context)
        {
            // This thread is invoked from VoiceAttack and may by invoked with the system default culture
            // so make sure that we're using our assigned culture.
            App.ApplyAnyOverrideCulture();

            Logging.Debug( $"Invoked with context '{context}'" );

            try
            {
                switch (context?.ToLowerInvariant())
                {
                    case "coriolis":
                        InvokeCoriolis();
                        break;
                    case "coriolisbeta":
                        InvokeCoriolis( true );
                        break;
                    case "inaracarrier":
                        InvokeInaraFleetCarrier();
                        break;
                    case "inaraprofile":
                        InvokeInaraProfile();
                        break;
                    case "inarasystem":
                    case "eddbsystem": // Redirect to Inara
                        InvokeInaraSystem();
                        break;
                    case "inarastation":
                    case "eddbstation": // Redirect to Inara
                        InvokeInaraStation();
                        break;
                    case "edshipyard":
                        InvokeEDShipyard();
                        break;
                    case "profile":
                        InvokeUpdateProfile();
                        break;
                    case "say":
                        InvokeSay();
                        break;
                    case "speech":
                        InvokeSpeech();
                        break;
                    case "system comment":
                        InvokeStarMapSystemComment();
                        break;
                    case "initialize eddi":
                        if (App.FromVA && Application.Current != null)
                        {
                            VoiceAttackPlugin.WriteToLog("The EDDI plugin is fully operational.", "green");
                        }
                        else
                        {
                            VoiceAttackPlugin.VA_Init1(null);  // Attempt initialization again to see if it works this time...
                        }
                        break;
                    case "configuration":
                    case "configurationminimize":
                    case "configurationmaximize":
                    case "configurationrestore":
                    case "configurationclose":
                        // Ignore any attempt to access the EDDI UI if VA
                        // doesn't own the EDDI instance.
                        if (App.FromVA && Application.Current != null)
                        {
                            InvokeConfiguration(context);
                        }
                        else
                        {
                            VoiceAttackPlugin.WriteToLog("The EDDI plugin is not fully initialized.", "red");
                        }
                        break;
                    case "shutup":
                        InvokeShutUp();
                        break;
                    case "setstate":
                        InvokeSetState();
                        break;
                    case "disablespeechresponder":
                        InvokeDisableSpeechResponder();
                        break;
                    case "enablespeechresponder":
                        InvokeEnableSpeechResponder();
                        break;
                    case "setspeechresponderpersonality":
                        InvokeSetSpeechResponderPersonality();
                        break;
                    case "jumpdetails":
                        InvokeJumpDetails();
                        break;
                    case "transmit":
                        InvokeTransmit();
                        break;
                    case "missionsroute":
                    case "route":
                        InvokeRouteDetails();
                        break;
                    case "inara":
                        InvokeInaraProfileDetails();
                        break;
                    case "volume":
                        InvokeVolume();
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Error( $"Failed to invoke context '{context}'", e);
                VoiceAttackPlugin.WriteToLog( $"Failed to invoke context '{context}'", "red");
            }
        }

        private static void InvokeVolume ()
        {
            int? volumeInt = VoiceAttackPlugin.GetInt("Volume");

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

        private static void InvokeInaraProfileDetails ()
        {
            string commanderName = VoiceAttackPlugin.GetText("Name");
            if ( commanderName == null )
            {
                return;
            }
            try
            {
                EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                var result = inaraService.GetCommanderProfileAsync(commanderName).GetAwaiter().GetResult();
                if ( result != null )
                {
                    OpenOrStoreURI( result.url );
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

        private static void InvokeConfiguration ( string context )
        {
            if ( Application.Current?.Dispatcher != null
                && ( Application.Current?.Dispatcher?.Invoke( () => Application.Current.MainWindow == null ) ?? false )
                && context != "configuration" )
            {
                VoiceAttackPlugin.WriteToLog( "The EDDI configuration window is not open.", "orange" );
                return;
            }

            switch ( context )
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
                                    setWindowState( WindowState.Minimized, true, false );
                                    VoiceAttackPlugin.WriteToLog( "The EDDI configuration window is already open.", "orange" );
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
                    setWindowState( WindowState.Minimized );
                    break;
                case "configurationmaximize":
                    setWindowState( WindowState.Maximized );
                    break;
                case "configurationrestore":
                    setWindowState( WindowState.Normal );
                    break;
                case "configurationclose":
                    Application.Current?.Dispatcher?.InvokeAsync( () => Application.Current?.MainWindow?.Hide() );
                    break;
                default:
                    VoiceAttackPlugin.WriteToLog( $"Plugin context '{context}' not recognized.", "orange" );
                    break;
            }
        }

        // Set main window minimize, maximize and normal states. Ignore and warn
        // if the main window is blocked waiting for a modal dialog to close.
        private static void setWindowState ( WindowState newState, bool minimizeCheck = false, bool warn = true )
        {
            if ( EDDI.Instance.SpeechResponderModalWait && warn )
            {
                System.Media.SystemSounds.Beep.Play();
                VoiceAttackPlugin.WriteToLog( "The EDDI window state cannot be changed at this time.", "orange" );
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

        private static void OpenOrStoreURI ( string systemUri )
        {
            if ( VoiceAttackPlugin.GetBoolean( "EDDI open uri in browser" ) != false )
            {
                Logging.Debug( "Starting process with uri " + systemUri );
                HandleUri( systemUri );
            }
            Logging.Debug( "Writing URI to `{TXT:EDDI uri}`: " + systemUri );
            VoiceAttackPlugin.SetText( "EDDI uri", systemUri );
        }

        public static void InvokeInaraSystem ()
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
                OpenOrStoreURI( systemUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send system data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraStation ()
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
                OpenOrStoreURI( stationUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send station data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraFleetCarrier ()
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
                OpenOrStoreURI( carrierUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send fleet carrier data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeInaraProfile ()
        {
            Logging.Debug( "Entered" );
            try
            {
                var inaraID = ConfigService.Instance.inaraConfiguration.inaraID;
                if ( inaraID is null )
                {
                    Logging.Debug( "No information on Inara commander" );
                    return;
                }
                string cmdrUri = $"https://inara.cz/elite/cmdr/{inaraID}/";
                OpenOrStoreURI( cmdrUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send Inara commander data to Inara", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeCoriolis ( bool beta = false )
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
                OpenOrStoreURI( shipUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send ship data to coriolis", e );
            }
            Logging.Debug( "Leaving" );
        }

        public static void InvokeEDShipyard ()
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
                OpenOrStoreURI( shipUri );
                VoiceAttackVariables.setStatus( "Operational" );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send ship data to coriolis", e );
            }
            Logging.Debug( "Leaving" );
        }

        /// <summary>
        /// Handle a URI, either sending it to the default web browser or putting it on the clipboard
        /// </summary>
        private static void HandleUri ( string uri )
        {
            bool? useClipboard = VoiceAttackPlugin.GetBoolean("EDDI use clipboard");
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
        public static void InvokeSay ()
        {
            try
            {
                string script = VoiceAttackPlugin.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = VoiceAttackPlugin.GetInt("Priority") ?? 3;

                string voice = VoiceAttackPlugin.GetText("Voice");

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
                VoiceAttackVariables.setStatus( "Failed to run EDDI's internal speech system (say)", e );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary> 
        public static void InvokeTransmit ()
        {
            try
            {
                string script = VoiceAttackPlugin.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = VoiceAttackPlugin.GetInt("Priority") ?? 3;

                string voice = VoiceAttackPlugin.GetText("Voice");

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
                VoiceAttackVariables.setStatus( "Failed to run EDDI's internal speech system (transmit)", e );
            }
        }

        /// <summary>
        /// Stop talking
        /// </summary>
        public static void InvokeShutUp ()
        {
            try
            {
                SpeechService.Instance.ShutUp();
                SpeechService.Instance.StopAudio();
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to shut up", e );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSpeech ()
        {
            try
            {
                string script = VoiceAttackPlugin.GetText("Script");
                if ( script == null )
                {
                    return;
                }

                int? priority = VoiceAttackPlugin.GetInt("Priority");

                string voice = VoiceAttackPlugin.GetText("Voice");

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
                VoiceAttackVariables.setStatus( "Failed to run internal speech system", e );
            }
        }

        public static void InvokeDisableSpeechResponder ()
        {
            try
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = true;
                SpeechService.Instance.speechQueue.DequeueAllSpeech();
                SpeechService.Instance.StopCurrentSpeech();
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to disable speech responder", e );
            }
        }

        public static void InvokeEnableSpeechResponder ()
        {
            try
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = false;
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to enable speech responder", e );
            }
        }

        public static void InvokeSetSpeechResponderPersonality ()
        {
            string personality = VoiceAttackPlugin.GetText("Personality");
            try
            {
                var speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                speechResponder?.SetPersonality( personality );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to set speech responder personality", e );
            }
        }

        public static void InvokeSetState ()
        {
            try
            {
                string name = VoiceAttackPlugin.GetText("State variable");
                if ( string.IsNullOrEmpty( name ) )
                {
                    Logging.Info( "No value in the VoiceAttack text variable 'State variable'; nothing to set" );
                    return;
                }

                // State variable names are lower-case
                string stateVariableName = name.ToLowerInvariant().Replace(" ", "_");

                string strValue = VoiceAttackPlugin.GetText(name);
                if ( strValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = strValue;
                    return;
                }

                int? intValue = VoiceAttackPlugin.GetInt(name);
                if ( intValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = intValue;
                    return;
                }

                bool? boolValue = VoiceAttackPlugin.GetBoolean(name);
                if ( boolValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = boolValue;
                    return;
                }

                decimal? decValue = VoiceAttackPlugin.GetDecimal(name);
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
                VoiceAttackVariables.setStatus( "Failed to set state", e );
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
            if ( string.IsNullOrEmpty( ConfigService.Instance.commanderConfiguration.commanderName ) )
            {
                cmdrScript = EddiCore.Properties.Resources.Commander;
            }
            else
            {
                cmdrScript = ConfigService.Instance.commanderConfiguration.phoneticName;
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
        public static void InvokeStarMapSystemComment ()
        {
            try
            {
                string comment = VoiceAttackPlugin.GetText("EDDI system comment");
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
                VoiceAttackVariables.setStatus( "Failed to store system comment", e );
            }
        }

        public static void InvokeJumpDetails ()
        {
            try
            {
                string type = VoiceAttackPlugin.GetText("Type variable");
                if ( !string.IsNullOrEmpty( type ) )
                {
                    var detail = EDDI.Instance.CurrentShip?.JumpDetails(type);
                    VoiceAttackPlugin.SetDecimal( "Ship jump detail distance", detail?.distance );
                    VoiceAttackPlugin.SetInt( "Ship jump detail jumps", detail?.jumps );
                    VoiceAttackPlugin.SetText( "Type variable", null );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to get jump details", e );
            }
        }

        public static void InvokeRouteDetails ()
        {
            try
            {
                string type = VoiceAttackPlugin.GetText("Type variable");
                string string0 = VoiceAttackPlugin.GetText("System variable");
                string string1 = VoiceAttackPlugin.GetText("System variable 2") ?? VoiceAttackPlugin.GetText("Station variable");
                decimal? numeric = VoiceAttackPlugin.GetDecimal("Numeric variable");
                bool? boolean = VoiceAttackPlugin.GetBoolean ("Boolean variable");

                VoiceAttackPlugin.SetText( "Type variable", null );
                VoiceAttackPlugin.SetText( "System variable", null );
                VoiceAttackPlugin.SetText( "System variable 2", null );
                VoiceAttackPlugin.SetText( "Station variable", null );
                VoiceAttackPlugin.SetDecimal( "Numeric variable", null );
                VoiceAttackPlugin.SetBoolean( "Boolean variable", null );

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
                VoiceAttackVariables.setStatus( "Failed to get route", e );
            }
        }

    }
}
