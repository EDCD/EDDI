using Eddi;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiNavigationService;
using EddiSpeechResponder;
using EddiSpeechService;
using EddiStarMapService;
using EddiUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal static class VoiceAttackInvokationHandler
    {
        private static readonly Random random = new();
        private static readonly AsyncLocal<Dictionary<string, object>> invocationPayload = new();

        public static void HandleInvokedCommand ( string context, IReadOnlyDictionary<string, object> parameters = null )
        {
            // This thread is invoked from VoiceAttack and may by invoked with the system default culture
            // so make sure that we're using our assigned culture.
            App.ApplyAnyOverrideCulture();

            Logging.Debug( $"Invoked with context '{context}'" );

            invocationPayload.Value = parameters == null
                ? new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase )
                : new Dictionary<string, object>( parameters, StringComparer.OrdinalIgnoreCase );

            try
            {
                switch ( context?.ToLowerInvariant() )
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
                        InvokeSayAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "speech":
                        InvokeSpeech();
                        break;
                    case "system comment":
                        InvokeStarMapSystemCommentAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "initialize eddi":
                        InvokeInitializeEddiAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "configuration":
                    case "configurationminimize":
                    case "configurationmaximize":
                    case "configurationrestore":
                    case "configurationclose":
                        // UI commands always execute here (only runs in EDDI.exe when FromVA=true)
                        InvokeConfiguration( context );
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
                        InvokeTransmitAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "missionsroute":
                    case "route":
                        InvokeRouteDetailsAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "inara":
                        InvokeInaraProfileDetailsAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                        break;
                    case "volume":
                        InvokeVolume();
                        break;
                }
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to invoke context '{context}'", e );
                RuntimeWriteToLog( $"Failed to invoke context '{context}'", "red" );
            }
            finally
            {
                invocationPayload.Value = null;
            }
        }

        private static async Task InvokeInitializeEddiAsync ()
        {
            try
            {
                if ( EDDI.Instance.FromVA )
                {
                    RuntimeWriteToLog( "The EDDI plugin is fully operational.", "green" );
                    return;
                }

                await VoiceAttackResponderModeHandler
                    .SetResponderModeAsync( true, App.VoiceAttackVersion )
                    .ConfigureAwait( false );

                RuntimeWriteToLog( "VoiceAttack responder mode initialized.", "green" );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Initialize EDDI command failed", ex );
                RuntimeWriteToLog( "EDDI initialization failed. See logs for details.", "red" );
            }
        }

        private static void InvokeVolume ()
        {
            var volumeInt = RuntimeGetInt( "Volume" );
            var config = ConfigService.Instance.speechServiceConfiguration;

            if ( config is null ) { return; }

            // Fix any inputs outside of the expected range
            if ( volumeInt == null )
            {
                volumeInt = new SpeechServiceConfiguration().Volume;
            } // Default volume
            else if ( volumeInt < 0 )
            {
                volumeInt = 0;
            } // Must be zero or greater
            else if ( volumeInt > 100 )
            {
                volumeInt = 100;
            } // Must be 100 or less

            // Update our speech configuration settings
            config.Volume = (int)volumeInt;
            ConfigService.Instance.speechServiceConfiguration = config;

            // Refresh the UI with the new volume (only in standalone mode)
            // In plugin mode, the UI is running in a separate process and will refresh on its own
            if ( !EDDI.Instance.FromVA )
            {
                if ( Application.Current?.Dispatcher != null )
                {
                    Application.Current?.Dispatcher?.InvokeAsync( () =>
                    {
                        var mainWindow = (MainWindow)Application.Current?.MainWindow;
                        if ( mainWindow == null )
                        {
                            return;
                        }

                        foreach ( var tab in mainWindow.MainTabControl.Items )
                        {
                            if ( tab is System.Windows.Controls.TabItem item && item.Content is TextToSpeechTab tts )
                            {
                                tts.ConfigureTTS();
                            }
                        }
                    } ).Task.SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
        }

        private static async Task InvokeInaraProfileDetailsAsync ()
        {
            var commanderName = RuntimeGetText( "Name" );
            if ( commanderName == null )
            {
                return;
            }

            try
            {
                var inaraService = new EddiInaraService.InaraService();
                var result = await inaraService.GetCommanderProfileAsync( commanderName ).ConfigureAwait( false );
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
            if ( Application.Current?.Dispatcher != null )
            {
                var windowIsNull =
                    Application.Current?.Dispatcher?.Invoke( () => Application.Current?.MainWindow == null );
                if ( windowIsNull == true && context != "configuration" )
                {
                    RuntimeWriteToLog( "The EDDI configuration window is not open.", "orange" );
                    return;
                }
            }

            switch ( context )
            {
                case "configuration":
                    if ( Application.Current?.Dispatcher != null )
                    {
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            try
                            {
                                if ( Application.Current?.MainWindow?.Visibility is Visibility.Collapsed or Visibility.Hidden )
                                {
                                    Application.Current.MainWindow?.Show();
                                }
                                else
                                {
                                    // Tell the configuration UI to restore its window if minimized
                                    setWindowState( WindowState.Minimized, true, false );
                                    RuntimeWriteToLog( "The EDDI configuration window is already open.",
                                        "orange" );
                                }
                            }
                            catch ( Exception ex )
                            {
                                Logging.Warn( "Show configuration window failed", ex );
                            }
                        } ).Task.SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
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
                    if ( Application.Current?.Dispatcher != null )
                    {
                        Application.Current?.Dispatcher?.InvokeAsync( async () =>
                            {
                                Application.Current?.MainWindow?.Hide();
                            } )
                            .Task.SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    }

                    break;
                default:
                    RuntimeWriteToLog( $"Plugin context '{context}' not recognized.", "orange" );
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
                RuntimeWriteToLog( "The EDDI window state cannot be changed at this time.", "orange" );
            }
            else
            {
                if ( Application.Current?.Dispatcher != null )
                {
                    Application.Current?.Dispatcher?.InvokeAsync( () =>
                    {
                        var mainwindow = (MainWindow)Application.Current?.MainWindow;
                        if ( mainwindow == null ) { return; }
                        var handler = mainwindow.VaWindowStateChange ?? mainwindow.OnVaWindowStateChange;
                        handler( newState, minimizeCheck );
                    } ).Task.SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                }
            }
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile ()
        {
            EDDI.Instance.refreshProfileAsync( true ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
        }

        private static void OpenOrStoreURI ( string systemUri )
        {
            if ( RuntimeGetBoolean( "EDDI open uri in browser" ) != false )
            {
                Logging.Debug( "Starting process with uri " + systemUri );
                HandleUri( systemUri );
            }

            Logging.Debug( "Writing URI to `{TXT:EDDI uri}`: " + systemUri );
            RuntimeSetText( "EDDI uri", systemUri );
        }

        private static void InvokeInaraSystem ()
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.GameState.CurrentStarSystem == null )
                {
                    Logging.Debug( "No information on current system" );
                    return;
                }

                var systemUri =
                    $"https://inara.cz/elite/starsystem/?search={EDDI.Instance.GameState.CurrentStarSystem.systemAddress}";
                OpenOrStoreURI( systemUri );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send system data to Inara", e );
            }

            Logging.Debug( "Leaving" );
        }

        private static void InvokeInaraStation ()
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.GameState.CurrentStarSystem == null )
                {
                    Logging.Debug( "No information on current station" );
                    return;
                }

                if ( EDDI.Instance.GameState.CurrentStation == null )
                {
                    // Missing current star system information
                    Logging.Debug( "No information on current station" );
                    return;
                }

                var stationUri = $"https://inara.cz/elite/station/?search={EDDI.Instance.GameState.CurrentStation.marketId}";
                OpenOrStoreURI( stationUri );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send station data to Inara", e );
            }

            Logging.Debug( "Leaving" );
        }

        private static void InvokeInaraFleetCarrier ()
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.GameState.FleetCarrier == null )
                {
                    Logging.Debug( "No information on fleet carrier" );
                    return;
                }

                var carrierUri =
                    $"https://inara.cz/elite/cmdr-fleetcarrier/?search={EDDI.Instance.GameState.FleetCarrier.callsign}";
                OpenOrStoreURI( carrierUri );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send fleet carrier data to Inara", e );
            }

            Logging.Debug( "Leaving" );
        }

        private static void InvokeInaraProfile ()
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

                var cmdrUri = $"https://inara.cz/elite/cmdr/{inaraID}/";
                OpenOrStoreURI( cmdrUri );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send Inara commander data to Inara", e );
            }

            Logging.Debug( "Leaving" );
        }

        private static void InvokeCoriolis ( bool beta = false )
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.GameState.CurrentShip == null )
                {
                    Logging.Debug( "No information on ship" );
                    return;
                }

                var shipUri = EDDI.Instance.GameState.CurrentShip.CoriolisUri( beta );
                OpenOrStoreURI( shipUri );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to send ship data to coriolis", e );
            }

            Logging.Debug( "Leaving" );
        }

        private static void InvokeEDShipyard ()
        {
            Logging.Debug( "Entered" );
            try
            {
                if ( EDDI.Instance.GameState.CurrentShip == null )
                {
                    Logging.Debug( "No information on ship" );
                    return;
                }

                var shipUri = EDDI.Instance.GameState.CurrentShip.EDShipyardUri();
                OpenOrStoreURI( shipUri );
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
            var useClipboard = RuntimeGetBoolean( "EDDI use clipboard" );
            if ( useClipboard is true )
            {
                var thread = new Thread( () =>
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
                var proc = new ProcessStartInfo( Net.GetDefaultBrowserPath(), "\"" + uri + "\"" )
                {
                    UseShellExecute = true
                };
                Process.Start( proc );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        private static async Task InvokeSayAsync ()
        {
            try
            {
                var script = RuntimeGetText( "Script" );
                if ( script == null )
                {
                    return;
                }

                int? priority = RuntimeGetInt( "Priority" ) ?? 3;

                var voice = RuntimeGetText( "Voice" );

                var speech = SpeechFromScript( script );

                Ship ship = null;
                if ( EDDI.Instance.GameState.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.GameState.CurrentShip;
                }

                await SpeechService.Instance.SayAsync( ship, speech, (int)priority, voice, false, null )
                    .ConfigureAwait( false );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to run EDDI's internal speech system (say)", e );
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary> 
        private static async Task InvokeTransmitAsync ()
        {
            try
            {
                var script = RuntimeGetText( "Script" );
                if ( script == null )
                {
                    return;
                }

                int? priority = RuntimeGetInt( "Priority" ) ?? 3;

                var voice = RuntimeGetText( "Voice" );

                var speech = SpeechFromScript( script );

                Ship ship = null;
                if ( EDDI.Instance.GameState.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.GameState.CurrentShip;
                }

                await SpeechService.Instance.SayAsync( ship, speech, (int)priority, voice, true, null )
                    .ConfigureAwait( false );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to run EDDI's internal speech system (transmit)", e );
            }
        }

        /// <summary>
        /// Stop talking
        /// </summary>
        private static void InvokeShutUp ()
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
        private static void InvokeSpeech ()
        {
            try
            {
                var script = RuntimeGetText( "Script" );
                if ( script == null )
                {
                    return;
                }

                var priority = RuntimeGetInt( "Priority" );

                var voice = RuntimeGetText( "Voice" );

                var speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder( "Speech responder" );
                if ( speechResponder == null )
                {
                    Logging.Warn( "Unable to find speech responder" );
                }

                Ship ship = null;
                if ( EDDI.Instance.GameState.Vehicle == Constants.VEHICLE_SHIP )
                {
                    ship = EDDI.Instance.GameState.CurrentShip;
                }

                // sayOutLoud must be true to match the behavior described by the wiki for the `disablespeechresponder` command
                // i.e. "not talk unless specifically asked for information"
                speechResponder?.SayAsync( ship, script, null, priority, voice, true, true );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to run internal speech system", e );
            }
        }

        private static void InvokeDisableSpeechResponder ()
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

        private static void InvokeEnableSpeechResponder ()
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

        private static void InvokeSetSpeechResponderPersonality ()
        {
            var personality = RuntimeGetText( "Personality" );
            try
            {
                var speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder( "Speech responder" );
                speechResponder?.TrySetPersonality( personality );
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to set speech responder personality", e );
            }
        }

        private static void InvokeSetState ()
        {
            try
            {
                var name = RuntimeGetText( "State variable" );
                if ( string.IsNullOrEmpty( name ) )
                {
                    Logging.Info( "No value in the VoiceAttack text variable 'State variable'; nothing to set" );
                    return;
                }

                // State variable names are lower-case
                var stateVariableName = name.ToLowerInvariant().Replace( " ", "_" );

                var strValue = RuntimeGetText( "State variable text value" );
                if ( !string.IsNullOrEmpty( strValue ) )
                {
                    EDDI.Instance.State[ stateVariableName ] = strValue;
                    return;
                }

                var intValue = RuntimeGetInt( "State variable int value" );
                if ( intValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = intValue;
                    return;
                }

                var boolValue = RuntimeGetBoolean( "State variable bool value" );
                if ( boolValue != null )
                {
                    EDDI.Instance.State[ stateVariableName ] = boolValue;
                    return;
                }

                var decValue = RuntimeGetDecimal( "State variable decimal value" );
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

        private static string RuntimeGetText( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                return null;
            }

            if ( !TryGetPayloadValue( key, out var value ) )
            {
                return null;
            }

            return value?.ToString();
        }

        private static int? RuntimeGetInt( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                return null;
            }

            if ( !TryGetPayloadValue( key, out var value ) || value == null )
            {
                return null;
            }

            return int.TryParse( value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed )
                ? parsed
                : null;
        }

        private static bool? RuntimeGetBoolean( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                return null;
            }

            if ( !TryGetPayloadValue( key, out var value ) || value == null )
            {
                return null;
            }

            return bool.TryParse( value.ToString(), out var parsed )
                ? parsed
                : null;
        }

        private static decimal? RuntimeGetDecimal( string key )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                return null;
            }

            if ( !TryGetPayloadValue( key, out var value ) || value == null )
            {
                return null;
            }

            return decimal.TryParse( value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed )
                ? parsed
                : null;
        }

        private static void RuntimeSetText( string key, string value )
            => DispatchRuntimeAction( "set_text", key, value );

        private static void RuntimeSetInt( string key, int? value )
            => DispatchRuntimeAction( "set_int", key, value );

        private static void RuntimeSetDecimal( string key, decimal? value )
            => DispatchRuntimeAction( "set_decimal", key, value );

        private static void RuntimeSetBoolean( string key, bool? value )
            => DispatchRuntimeAction( "set_boolean", key, value );

        private static void RuntimeWriteToLog( string message, string color )
        {
            var payload = new Dictionary<string, object>
            {
                { "action", "write_log" },
                { "message", message ?? string.Empty },
                { "color", color ?? "white" }
            };

            DispatchRuntimeEventPayload( payload );
        }

        private static bool TryGetPayloadValue( string key, out object value )
        {
            value = null;

            var payload = invocationPayload.Value;
            if ( payload == null )
            {
                return false;
            }

            if ( payload.TryGetValue( key, out var directValue ) )
            {
                value = directValue;
                return true;
            }

            return false;
        }

        private static void DispatchRuntimeAction( string action, string key, object value )
        {
            var payload = new Dictionary<string, object>
            {
                { "action", action },
                { "key", key ?? string.Empty },
                { "value", value ?? string.Empty }
            };

            DispatchRuntimeEventPayload( payload );
        }

        private static void DispatchRuntimeEventPayload( Dictionary<string, object> payload )
        {
            try
            {
                var eventData = new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = payload
                };

                RuntimeEventDispatcher.DispatchAsync( eventData )
                    .GetAwaiter()
                    .GetResult();
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to dispatch runtime action payload", ex );
            }
        }

        public static string SpeechFromScript ( string script )
        {
            if ( script == null )
            {
                return null;
            }

            // Variable replacement
            var ship = EDDI.Instance.GameState.CurrentShip;
            if ( ship != null )
            {
                script = script.Replace( "$=", ship.phoneticname );
            }

            var cmdrScript = string.IsNullOrEmpty( ConfigService.Instance.commanderConfiguration.commanderName )
                ? EddiCore.Properties.Resources.Commander
                : ConfigService.Instance.commanderConfiguration.phoneticName;
            script = script.Replace( "$-", cmdrScript );

            // Multiple choice selection
            var sb = new StringBuilder();

            // Step 1 - resolve any options in square brackets
            var matchResult = GeneratedRegex.VoiceAttackCommandPermutationsRegex().Match( script );
            while ( matchResult.Success )
            {
                if ( matchResult.Value.StartsWith( '[' ) )
                {
                    // Remove the brackets and pick one of the options
                    var result = matchResult.Value.Substring( 1, matchResult.Value.Length - 2 );
                    var options = result.Split( ';' );
                    sb.Append( options[ random.Next( 0, options.Length ) ] );
                }
                else
                {
                    // Pass it right along
                    sb.Append( matchResult.Groups[ 0 ].Value );
                }

                matchResult = matchResult.NextMatch();
            }

            var res = sb.ToString();

            // Step 2 - resolve phrases separated by semicolons
            if ( res.Contains( ';' ) )
            {
                // Pick one of the options
                var options = res.Split( ';' );
                res = options[ random.Next( 0, options.Length ) ];
            }

            // Step 3 - pass it through the script resolver
            res = new EddiSpeechResponder.ScriptResolverService.ScriptResolver( null ).resolveFromValue( res, true );

            return res ?? "";
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        private static async Task InvokeStarMapSystemCommentAsync ()
        {
            try
            {
                var comment = RuntimeGetText( "EDDI system comment" );
                if ( comment == null )
                {
                    return;
                }

                if ( EDDI.Instance.GameState.CurrentStarSystem != null )
                {
                    // Store locally
                    var systemAddress = EDDI.Instance.GameState.CurrentStarSystem.systemAddress;
                    var currentSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( systemAddress )
                        .ConfigureAwait( false );
                    currentSystem.comment = comment == "" ? null : comment;
                    await EDDI.Instance.DataProvider.SaveStarSystemAsync( currentSystem ).ConfigureAwait( false );

                    // Store in EDSM
                    var edsmService = new StarMapService( null, true );
                    await edsmService.sendStarMapCommentAsync( systemAddress, comment ).ConfigureAwait( false );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to store system comment", e );
            }
        }

        private static void InvokeJumpDetails ()
        {
            try
            {
                var type = RuntimeGetText( "Type variable" );
                if ( !string.IsNullOrEmpty( type ) )
                {
                    var detail = EDDI.Instance.GameState.CurrentShip?.JumpDetails( type );
                    RuntimeSetDecimal( "Ship jump detail distance", detail?.distance );
                    RuntimeSetInt( "Ship jump detail jumps", detail?.jumps );
                    RuntimeSetText( "Type variable", null );
                }
            }
            catch ( Exception e )
            {
                VoiceAttackVariables.setStatus( "Failed to get jump details", e );
            }
        }

        private static async Task InvokeRouteDetailsAsync ()
        {
            try
            {
                var type = RuntimeGetText( "Type variable" );
                var string0 = RuntimeGetText( "System variable" );
                var string1 = RuntimeGetText( "System variable 2" ) ??
                              RuntimeGetText( "Station variable" );
                var numeric = RuntimeGetDecimal( "Numeric variable" );
                var boolean = RuntimeGetBoolean( "Boolean variable" );

                RuntimeSetText( "Type variable", null );
                RuntimeSetText( "System variable", null );
                RuntimeSetText( "System variable 2", null );
                RuntimeSetText( "Station variable", null );
                RuntimeSetDecimal( "Numeric variable", null );
                RuntimeSetBoolean( "Boolean variable" , null );

                if ( Enum.TryParse( type, true, out QueryType result ) )
                {
                    var @event = await NavigationService.Instance
                        .NavQueryAsync( result, string0, string1, numeric, boolean ).ConfigureAwait( false );
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