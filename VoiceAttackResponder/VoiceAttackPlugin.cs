using Eddi;
using EddiCargoMonitor;
using EddiEvents;
using EddiDataProviderService;
using EddiDataDefinitions;
using EddiMaterialMonitor;
using EddiMissionMonitor;
using EddiShipMonitor;
using EddiSpeechResponder;
using EddiSpeechService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public class VoiceAttackPlugin : VoiceAttackVariables
    {
        public static string VA_DisplayName()
        {
            return Constants.EDDI_NAME + " " + Constants.EDDI_VERSION;
        }

        public static string VA_DisplayInfo()
        {
            return Constants.EDDI_NAME + "\r\nVersion " + Constants.EDDI_VERSION;
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        private static readonly Random random = new Random();

        public static BlockingCollection<Event> EventQueue = new BlockingCollection<Event>();
        public static Thread eventThread = null;
        public static Thread updaterThread = null;

        private static readonly object vaProxyLock = new object();

        public static void VA_Init1(dynamic vaProxy)
        {
            Logging.Info("Initialising EDDI VoiceAttack plugin");

            try
            {
                GetEddiInstance(ref vaProxy);
                Logging.incrementLogs();
                App.StartRollbar();
                App.ApplyAnyOverrideCulture();
                EDDI.Instance.Start();

                // Add notifiers for events we want to react to 
                EDDI.Instance.State.CollectionChanged += (s, e) => setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);
                SpeechService.Instance.PropertyChanged += (s, e) => setSpeaking(SpeechService.eddiSpeaking, ref vaProxy);
                VoiceAttackResponder.RaiseEvent += (s, theEvent) => updateValuesOnEvent(theEvent, ref vaProxy);

                // Display instance information if available
                if (EDDI.Instance.UpgradeRequired)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "red");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }
                else if (EDDI.Instance.UpgradeAvailable)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }

                if (EDDI.Instance.Motd != null)
                {
                    vaProxy.WriteToLog("Message from EDDI: " + EDDI.Instance.Motd, "black");
                    string msg = String.Format(Eddi.Properties.EddiResources.msg_from_eddi, EDDI.Instance.Motd);
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }

                // Set the initial values from the main EDDI objects
                setStandardValues(ref vaProxy);

                vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                setStatus(ref vaProxy, "Operational");

                // Fire an event once the VA plugin is initialized
                Event @event = new VAInitializedEvent(DateTime.UtcNow);

                if (initEventEnabled(@event.type))
                {
                    EDDI.Instance.eventHandler(@event);
                }

                // Set a variable indicating whether EDDI is speaking
                try
                {
                    setSpeaking(SpeechService.eddiSpeaking, ref vaProxy);
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to set initial speaking status", ex);
                }
                Logging.Info("EDDI VoiceAttack plugin initialization complete");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize VoiceAttack plugin", e);
                vaProxy.WriteToLog("Unable to fully initialize EDDI. Some functions may not work.", "red");
            }
        }

        public static void updateValuesOnEvent(Event theEvent, ref dynamic vaProxy)
        {
            try
            {
                lock (vaProxyLock)
                {
                    vaProxy.SetText("EDDI event", theEvent.type);

                    // Event-specific values  
                    List<string> setKeys = new List<string>();
                    // We start off setting the keys which are official and known  
                    setEventValues(vaProxy, theEvent, setKeys);
                    // Now we carry out a generic walk through the event object to create whatever we find  
                    setEventExtendedValues(ref vaProxy, "EDDI " + theEvent.type.ToLowerInvariant(), JsonConvert.DeserializeObject(JsonConvert.SerializeObject(theEvent)), setKeys);

                    // Update all standard values  
                    setStandardValues(ref vaProxy);

                    // Fire local command if present  
                    string commandName = "((EDDI " + theEvent.type.ToLowerInvariant() + "))";
                    Logging.Debug("Searching for command " + commandName);
                    if (vaProxy.CommandExists(commandName))
                    {
                        Logging.Debug("Found command " + commandName);
                        vaProxy.ExecuteCommand(commandName);
                        Logging.Info("Executed command " + commandName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to handle event in VoiceAttack", ex);
            }
        }

        private static bool initEventEnabled(string name)
        {
            Script script = null;
            SpeechResponderConfiguration config = SpeechResponderConfiguration.FromFile();

            if (config != null)
            {
                Personality personality = Personality.FromName(config.Personality);
                personality.Scripts.TryGetValue(name, out script);
            }

            return script == null ? false : script.Enabled;
        }

        private static Mutex eddiMutex = null;
        private static bool eddiInstance = false;
        private static void GetEddiInstance(ref dynamic vaProxy)
        {
            if (eddiInstance)
            {
                return;
            }

            bool firstOwner = false;

            while (!firstOwner)
            {
                eddiMutex = new Mutex(true, Constants.EDDI_SYSTEM_MUTEX_NAME, out firstOwner);

                if (!firstOwner)
                {
                    vaProxy.WriteToLog("An instance of the EDDI application is already running.", "red");

                    MessageBoxResult result =
                    MessageBox.Show("An instance of EDDI is already running. Please close\r\n" +
                                    "the open EDDI application and click OK to continue. " +
                                    "If you click CANCEL, the EDDI VoiceAttack plugin will not be fully initialized.",
                                    "EDDI Instance Exists",
                                    MessageBoxButton.OKCancel, MessageBoxImage.Information);

                    // Any response will require the mutex to be reset
                    eddiMutex.Close();

                    if (MessageBoxResult.Cancel == result)
                    {
                        throw new Exception("EDDI initialization cancelled by user.");
                    }
                }
            }

            eddiInstance = true;
        }

        public static void VA_Exit1(dynamic vaProxy)
        {
            Logging.Info("EDDI VoiceAttack plugin exiting");

            if (configWindow != null)
            {
                try
                {
                    configWindow.Dispatcher.BeginInvoke((Action)configWindow.Close);
                }
                catch (Exception ex)
                {
                    Logging.Debug("EDDI configuration UI close from VA failed." + ex + ".");
                }
            }

            if (updaterThread != null)
            {
                updaterThread.Abort();
            }

            SpeechService.Instance.ShutUp();

            if (eddiInstance)
            {
                EDDI.Instance.Stop();
            }
        }

        public static void VA_StopCommand()
        {
        }

        public static void VA_Invoke1(dynamic vaProxy)
        {
            Logging.Debug("Invoked with context " + (string)vaProxy.Context);

            try
            {
                switch ((string)vaProxy.Context)
                {
                    case "coriolis":
                        InvokeCoriolis(ref vaProxy);
                        break;
                    case "eddbsystem":
                        InvokeEDDBSystem(ref vaProxy);
                        break;
                    case "eddbstation":
                        InvokeEDDBStation(ref vaProxy);
                        break;
                    case "edshipyard":
                        InvokeEDShipyard(ref vaProxy);
                        break;
                    case "profile":
                        InvokeUpdateProfile(ref vaProxy);
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
                        if (eddiInstance)
                        {
                            vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                        }
                        else
                        {
                            VA_Init1(vaProxy);  // Attempt initialization again to see if it works this time...
                        }
                        break;
                    case "configuration":
                    case "configurationminimize":
                    case "configurationmaximize":
                    case "configurationrestore":
                    case "configurationclose":
                        // Ignore any attempt to access the EDDI UI if VA
                        // doesn't own the EDDI instance.
                        if (eddiInstance)
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
                    case "transmit":
                        InvokeTransmit(ref vaProxy);
                        break;
                    case "missionsroute":
                        InvokeMissionsRoute(ref vaProxy);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to invoke context " + vaProxy.Context, e);
                vaProxy.WriteToLog("Failed to invoke context " + vaProxy.Context, "red");
            }
        }

        private static MainWindow configWindow = null;
        private static void InvokeConfiguration(ref dynamic vaProxy)
        {
            string config = (string)vaProxy.Context;

            if (configWindow == null && config != "configuration")
            {
                vaProxy.WriteToLog("The EDDI configuration window is not open.", "orange");
                return;
            }

            switch (config)
            {
                case "configuration":
                    // Ensure there's only one instance of the configuration UI
                    if (configWindow == null)
                    {
                        Thread configThread = new Thread(() =>
                        {
                            try
                            {
                                configWindow = new MainWindow(true);
                                configWindow.Closing += new CancelEventHandler(eddiClosing);
                                configWindow.ShowDialog();

                                // Bind Cargo monitor inventory, Material Monitor inventory, & Ship monitor shipyard collections to the EDDI config Window
                                ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor")).EnableConfigBinding(configWindow);
                                ((MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor")).EnableConfigBinding(configWindow);
                                ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).EnableConfigBinding(configWindow);
                                ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).EnableConfigBinding(configWindow);

                                configWindow = null;
                            }
                            catch (ThreadAbortException)
                            {
                                Logging.Debug("Thread aborted");
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("Show configuration window failed", ex);
                            }
                        });
                        configThread.SetApartmentState(ApartmentState.STA);
                        configThread.Start();
                    }
                    else
                    {
                        // Tell the configuration UI to restore its window if minimized
                        setWindowState(ref vaProxy, WindowState.Minimized, true, false);
                        vaProxy.WriteToLog("The EDDI configuration window is already open.", "orange");
                    }
                    break;
                case "configurationminimize":
                    setWindowState(ref vaProxy, WindowState.Minimized);
                    break;
                case "configurationmaximize":
                    setWindowState(ref vaProxy, WindowState.Maximized);
                    break;
                case "configurationrestore":
                    setWindowState(ref vaProxy, WindowState.Normal);
                    break;
                case "configurationclose":
                    // Unbind the Cargo Monitor inventory, Material Monitor inventory, & Ship Monitor shipyard collections from the EDDI config window
                    ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor")).DisableConfigBinding(configWindow);
                    ((MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor")).DisableConfigBinding(configWindow);
                    ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).DisableConfigBinding(configWindow);
                    ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).DisableConfigBinding(configWindow);

                    configWindow.Dispatcher.Invoke(configWindow.Close);

                    if (eddiCloseCancelled)
                    {
                        vaProxy.WriteToLog("The EDDI window cannot be closed at this time.", "orange");
                    }
                    else
                    {
                        configWindow = null;
                    }
                    break;
                default:
                    vaProxy.WriteToLog("Plugin context \"" + (string)vaProxy.Context + "\" not recognized.", "orange");
                    break;
            }
        }

        // Set main window minimize, maximize and normal states. Ignore and warn
        // if the main window is blocked waiting for a modal dialog to close.
        private static void setWindowState(ref dynamic vaProxy, WindowState newState, bool minimizeCheck = false, bool warn = true)
        {
            if (EDDI.Instance.SpeechResponderModalWait && warn)
            {
                System.Media.SystemSounds.Beep.Play();
                vaProxy.WriteToLog("The EDDI window state cannot be changed at this time.", "orange");
            }
            else
            {
                configWindow.Dispatcher.Invoke(configWindow.VaWindowStateChange, new object[] { newState, minimizeCheck });
            }
        }

        // Hook the closing event to see if the main window is blocked waiting
        // for a modal dialog to close, and if it is, warn and cancel the close.
        private static bool eddiCloseCancelled = false;
        private static void eddiClosing(Object sender, CancelEventArgs e)
        {
            if (EDDI.Instance.SpeechResponderModalWait)
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }

            eddiCloseCancelled = e.Cancel;
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile(ref dynamic vaProxy)
        {
            EDDI.Instance.refreshProfile(true);
            setStandardValues(ref vaProxy);
        }

        private static void OpenOrStoreURI(ref dynamic vaProxy, string systemUri)
        {
            if (vaProxy.GetBoolean("EDDI open uri in browser") != false)
            {
                Logging.Debug("Starting process with uri " + systemUri);
                HandleUri(ref vaProxy, systemUri);
            }
            Logging.Debug("Writing URI to `{TXT:EDDI uri}`: " + systemUri);
            vaProxy.SetText("EDDI uri", systemUri);
        }

        public static void InvokeEDDBSystem(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (EDDI.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current system");
                    return;
                }
                string systemUri = "https://eddb.io/system/" + EDDI.Instance.CurrentStarSystem.EDDBID;
                OpenOrStoreURI(ref vaProxy, systemUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send system data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeEDDBStation(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (EDDI.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current station");
                    return;
                }
                Station thisStation = EDDI.Instance.CurrentStarSystem.stations.SingleOrDefault(s => s.name == (EDDI.Instance.CurrentStation?.name));
                if (thisStation == null)
                {
                    // Missing current star system information
                    Logging.Debug("No information on current station");
                    return;
                }
                string stationUri = "https://eddb.io/station/" + thisStation.EDDBID;
                OpenOrStoreURI(ref vaProxy, stationUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send station data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeCoriolis(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip() == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip().CoriolisUri();
                OpenOrStoreURI(ref vaProxy, shipUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send ship data to coriolis", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeEDShipyard(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip() == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip().EDShipyardUri();
                OpenOrStoreURI(ref vaProxy, shipUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send ship data to coriolis", e);
            }
            Logging.Debug("Leaving");
        }

        /// <summary>
        /// Handle a URI, either sending it to the default web browser or putting it on the clipboard
        /// </summary>
        private static void HandleUri(ref dynamic vaProxy, string uri)
        {
            bool? useClipboard = vaProxy.GetBoolean("EDDI use clipboard");
            if (useClipboard != null && useClipboard == true)
            {
                Thread thread = new Thread(() => System.Windows.Clipboard.SetText(uri));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            else
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), "\"" + uri + "\"")
                {
                    UseShellExecute = false
                };
                Process.Start(proc);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSay(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority");
                if (priority == null)
                {
                    priority = 3;
                }

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), speech, true, (int)priority, voice);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run EDDI's internal speech system (say)", e);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary> 
        public static void InvokeTransmit(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority");
                if (priority == null)
                {
                    priority = 3;
                }

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), speech, true, (int)priority, voice, true);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run EDDI's internal speech system (transmit)", e);
            }
        }

        /// <summary>
        /// Stop talking
        /// </summary>
        public static void InvokeShutUp(ref dynamic vaProxy)
        {
            try
            {
                SpeechService.Instance.ShutUp();
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to shut up", e);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSpeech(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority");

                SpeechResponder speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                if (speechResponder == null)
                {
                    Logging.Warn("Unable to find speech responder");
                }

                string voice = vaProxy.GetText("Voice");

                speechResponder.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), script, null, priority, voice);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run internal speech system", e);
            }
        }

        public static void InvokeDisableSpeechResponder(ref dynamic vaProxy)
        {
            try
            {
                EDDI.Instance.State["speechresponder_quiet"] = true;
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to disable speech responder", e);
            }
        }

        public static void InvokeEnableSpeechResponder(ref dynamic vaProxy)
        {
            try
            {
                EDDI.Instance.State["speechresponder_quiet"] = false;
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to enable speech responder", e);
            }
        }

        public static void InvokeSetSpeechResponderPersonality(ref dynamic vaProxy)
        {
            string personality = vaProxy.GetText("Personality");
            try
            {
                SpeechResponder speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                if (speechResponder != null)
                {
                    speechResponder.SetPersonality(personality);
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set speech responder personality", e);
            }
        }

        public static void InvokeSetState(ref dynamic vaProxy)
        {
            try
            {
                string name = vaProxy.GetText("State variable");
                if (string.IsNullOrEmpty(name))
                {
                    Logging.Info("No value in the VoiceAttack text variable 'State variable'; nothing to set");
                    return;
                }

                // State variable names are lower-case
                string stateVariableName = name.ToLowerInvariant().Replace(" ", "_");

                string strValue = vaProxy.GetText(name);
                if (strValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = strValue;
                    return;
                }

                int? intValue = vaProxy.GetInt(name);
                if (intValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = intValue;
                    return;
                }

                bool? boolValue = vaProxy.GetBoolean(name);
                if (boolValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = boolValue;
                    return;
                }

                decimal? decValue = vaProxy.GetDecimal(name);
                if (decValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = decValue;
                    return;
                }

                // Nothing above, so remove the item
                EDDI.Instance.State.Remove(stateVariableName);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set state", e);
            }
        }

        public static string SpeechFromScript(string script)
        {
            if (script == null) { return null; }

            // Variable replacement
            Ship ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
            if (ship != null)
            {
                if (ship != null && ship.phoneticname != null)
                {
                    script = script.Replace("$=", ship.phoneticname);
                }
                else if (ship != null && ship.name != null)
                {
                    script = script.Replace("$=", ship.name);
                }
                else
                {
                    script = script.Replace("$=", "your ship");
                }
            }

            string cmdrScript;
            if (EDDI.Instance.Cmdr == null || EDDI.Instance.Cmdr.name == null || EDDI.Instance.Cmdr.name.Trim().Length == 0)
            {
                cmdrScript = "EDDI.Instance.Cmdr";
            }
            else if (EDDI.Instance.Cmdr.phoneticname == null || EDDI.Instance.Cmdr.phoneticname.Trim().Length == 0)
            {
                cmdrScript = "EDDI.Instance.Cmdr " + EDDI.Instance.Cmdr.name;
            }
            else
            {
                cmdrScript = "EDDI.Instance.Cmdr <phoneme alphabet=\"ipa\" ph=\"" + EDDI.Instance.Cmdr.phoneticname + "\">" + EDDI.Instance.Cmdr.name + "</phoneme>";
            }
            script = script.Replace("$-", cmdrScript);

            // Multiple choice selection
            StringBuilder sb = new StringBuilder();

            // Step 1 - resolve any options in square brackets
            Match matchResult = Regex.Match(script, @"\[[^\]]*\]|[^\[\]]+");
            while (matchResult.Success)
            {
                if (matchResult.Value.StartsWith("["))
                {
                    // Remove the brackets and pick one of the options
                    string result = matchResult.Value.Substring(1, matchResult.Value.Length - 2);
                    string[] options = result.Split(';');
                    sb.Append(options[random.Next(0, options.Length)]);
                }
                else
                {
                    // Pass it right along
                    sb.Append(matchResult.Groups[0].Value);
                }
                matchResult = matchResult.NextMatch();
            }
            string res = sb.ToString();

            // Step 2 - resolve phrases separated by semicolons
            if (res.Contains(";"))
            {
                // Pick one of the options
                string[] options = res.Split(';');
                res = options[random.Next(0, options.Length)];
            }
            return res;
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        public static void InvokeStarMapSystemComment(ref dynamic vaProxy)
        {
            try
            {
                string comment = vaProxy.GetText("EDDI system comment");
                if (comment == null)
                {
                    return;
                }

                if (EDDI.Instance.Cmdr != null && EDDI.Instance.CurrentStarSystem != null && EDDI.Instance.starMapService != null)
                {
                    // Store locally
                    StarSystem here = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(EDDI.Instance.CurrentStarSystem.name);
                    here.comment = comment == "" ? null : comment;
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(here);

                    if (EDDI.Instance.starMapService != null)
                    {
                        // Store in EDSM
                        EDDI.Instance.starMapService.sendStarMapComment(EDDI.Instance.CurrentStarSystem.name, comment);
                    }
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to store system comment", e);
            }
        }

        public static void InvokeMissionsRoute(ref dynamic vaProxy)
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                string system = vaProxy.GetText("System variable");
                switch (type)
                {
                    case "expiring":
                        {
                            ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetExpiringRoute();
                        }
                        break;
                    case "farthest":
                        {
                            ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetFarthestRoute();
                        }
                        break;
                    case "most":
                        {
                            ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetMostRoute();
                        }
                        break;
                    case "nearest":
                        {
                            ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetNearestRoute();
                        }
                        break;
                    case "route":
                        {
                            if (system == null || system == string.Empty)
                            {
                                ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetMissionsRoute();
                            }
                            else
                            {
                                ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).GetMissionsRoute(system);
                            }
                        }
                        break;
                    case "source":
                        {
                            if (system == null || system == string.Empty)
                            {
                                ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor")).GetSourceRoute();
                            }
                            else
                            {
                                ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor")).GetSourceRoute(system);
                            }
                        }
                        break;
                    case "update":
                        {
                            if (system == null || system == string.Empty)
                            {
                                ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).UpdateMissionsRoute();
                            }
                            else
                            {
                                ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor")).UpdateMissionsRoute(system);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to get missions route", e);
            }
        }
    }
}
