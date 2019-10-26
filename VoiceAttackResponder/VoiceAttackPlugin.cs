using Eddi;
using EddiCargoMonitor;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiMaterialMonitor;
using EddiNavigationService;
using EddiShipMonitor;
using EddiSpeechResponder;
using EddiSpeechService;
using EddiStarMapService;
using EddiStatusMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        public static ConcurrentQueue<Event> eventQueue = new ConcurrentQueue<Event>();
        public static Thread updaterThread = null;

        private static readonly object vaProxyLock = new object();

        public static void VA_Init1(dynamic vaProxy)
        {
            // Initialize and launch an EDDI instance without opening the main window
            // VoiceAttack commands will be used to manipulate the window state.
            App.vaProxy = vaProxy;
            if (App.AlreadyRunning()) { return; }

            Thread appThread = new Thread(App.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();

            try
            {
                int timeout = 0;
                while (Application.Current == null)
                {
                    if (timeout < 200)
                    {
                        Thread.Sleep(50);
                        timeout++;
                    }
                    else
                    {
                        throw new TimeoutException("EDDI VoiceAttack plugin initialisation has timed out");
                    }
                }

                Logging.Info("Initialising EDDI VoiceAttack plugin");

                // Set up our event responders
                VoiceAttackResponder.RaiseEvent += (s, theEvent) =>
                {
                    try
                    {
                        eventQueue.Enqueue(theEvent);
                        Thread eventHandler = new Thread(() => dequeueEvent(ref vaProxy))
                        {
                            Name = "VoiceAttackEventHandler",
                            IsBackground = true
                        };
                        eventHandler.Start();
                        eventHandler.Join();
                    }
                    catch (ThreadAbortException tax)
                    {
                        Thread.ResetAbort();
                        Logging.Debug("Thread aborted", tax);
                    }
                    catch (Exception ex)
                    {
                        Dictionary<string, object> data = new Dictionary<string, object>
                        {
                            { "event", JsonConvert.SerializeObject(theEvent) },
                            { "exception", ex.Message },
                            { "stacktrace", ex.StackTrace }
                        };
                        Logging.Error("VoiceAttack failed to handle event.", data);
                    }
                };

                // Add notifiers for changes in variables we want to react to 
                // (we can only use event handlers with classes which are always constructed - nullable objects will be updated via responder events)
                EDDI.Instance.State.CollectionChanged += (s, e) => setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);
                SpeechService.Instance.PropertyChanged += (s, e) => setSpeaking(SpeechService.Instance.eddiSpeaking, ref vaProxy);

                CargoMonitor cargoMonitor = (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
                cargoMonitor.InventoryUpdatedEvent += (s, e) =>
                {
                    lock (vaProxyLock)
                    {
                        setCargo(cargoMonitor, ref vaProxy);
                    }
                };

                ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
                if (shipMonitor != null)
                {
                    shipMonitor.ShipyardUpdatedEvent += (s, e) =>
                    {
                        lock (vaProxyLock)
                        {
                            setShipValues(shipMonitor.GetCurrentShip(), "Ship", ref vaProxy);
                            Task.Run(() => setShipyardValues(shipMonitor.shipyard?.ToList(), ref vaProxy));
                        }
                    };
                }

                StatusMonitor statusMonitor = (StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor");
                if (statusMonitor != null)
                {
                    statusMonitor.StatusUpdatedEvent += (s, e) =>
                    {
                        lock (vaProxyLock)
                        {
                            setStatusValues(statusMonitor.currentStatus, "Status", ref vaProxy);
                        }
                    };
                }

                // Display instance information if available
                if (EDDI.Instance.UpgradeRequired)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "red");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, 0);
                }
                else if (EDDI.Instance.UpgradeAvailable)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, 0);
                }

                if (EDDI.Instance.Motd != null)
                {
                    vaProxy.WriteToLog("Message from EDDI: " + EDDI.Instance.Motd, "black");
                    string msg = String.Format(Eddi.Properties.EddiResources.msg_from_eddi, EDDI.Instance.Motd);
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, 0);
                }

                // Set the initial values from the main EDDI objects
                setStandardValues(ref vaProxy);

                vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                setStatus(ref vaProxy, "Operational");

                // Fire an event once the VA plugin is initialized
                Event @event = new VAInitializedEvent(DateTime.UtcNow);

                if (initEventEnabled(@event.type))
                {
                    EDDI.Instance.enqueueEvent(@event);
                }

                // Set a variable indicating the version of VoiceAttack in use
                System.Version v = vaProxy.VAVersion;
                EDDI.Instance.vaVersion = v.ToString();

                // Set a variable indicating whether EDDI is speaking
                try
                {
                    setSpeaking(SpeechService.Instance.eddiSpeaking, ref vaProxy);
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

        private static void dequeueEvent(ref dynamic vaProxy)
        {
            if (eventQueue.TryDequeue(out Event @event))
            {
                try
                {
                    if (@event?.type != null)
                    {
                        updateValuesOnEvent(@event, ref vaProxy);
                        triggerVACommands(@event, ref vaProxy);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to handle event in VoiceAttack", ex);
                }
            }
        }

        public static void updateValuesOnEvent(Event @event, ref dynamic vaProxy)
        {
            try
            {
                lock (vaProxyLock)
                {
                    vaProxy.SetText("EDDI event", @event.type);

                    // Event-specific values  
                    List<string> setKeys = new List<string>();
                    // We start off setting the keys which are official and known  
                    setEventValues(vaProxy, @event, setKeys);
                    // Now we carry out a generic walk through the event object to create whatever we find  
                    setEventExtendedValues(vaProxy, "EDDI " + @event.type.ToLowerInvariant(), JsonConvert.DeserializeObject(JsonConvert.SerializeObject(@event)), setKeys);

                    // Update all standard values  
                    setStandardValues(ref vaProxy);
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set variables in VoiceAttack", ex);
            }
        }

        private static void triggerVACommands(Event @event, ref dynamic vaProxy)
        {
            string commandName = "((EDDI " + @event.type.ToLowerInvariant() + "))";
            try
            {
                lock (vaProxyLock)
                {
                    // Fire local command if present  
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
                Logging.Error("Failed to trigger local VoiceAttack command " + commandName, ex);
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

            return script?.Enabled ?? false;
        }

        public static void VA_Exit1(dynamic vaProxy)
        {
            Logging.Info("EDDI VoiceAttack plugin exiting");

            if (Application.Current?.Dispatcher != null)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() => Application.Current.MainWindow?.Close());
                }
                catch (Exception ex)
                {
                    Logging.Debug("EDDI configuration UI close from VA failed." + ex + ".");
                }
            }

            updaterThread?.Abort();
            Application.Current?.Dispatcher?.Invoke(() => Application.Current.Shutdown());
            App.eddiMutex.ReleaseMutex();
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
                        if (App.FromVA && Application.Current != null)
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
                        if (App.FromVA && Application.Current != null)
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
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to invoke context " + vaProxy.Context, e);
                vaProxy.WriteToLog("Failed to invoke context " + vaProxy.Context, "red");
            }
        }

        private static void InvokeInaraProfileDetails(ref dynamic vaProxy)
        {
            string commanderName = vaProxy.GetText("Name");
            if (commanderName == null)
            {
                return;
            }
            try
            {
                var profile = EddiInaraService.InaraService.Instance.GetCommanderProfile(commanderName);
                if (profile != null)
                {
                    OpenOrStoreURI(ref vaProxy, profile.url);
                }
                else
                {
                    Logging.Debug("No information on commander " + commanderName);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to obtain Inara details on commander " + commanderName, ex);
            }
        }

        private static void InvokeConfiguration(ref dynamic vaProxy)
        {
            string config = (string)vaProxy.Context;

            if (Application.Current?.Dispatcher != null 
                && (bool)Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow == null) 
                && config != "configuration")
            {
                vaProxy.WriteToLog("The EDDI configuration window is not open.", "orange");
                return;
            }

            switch (config)
            {
                case "configuration":
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (Application.Current?.MainWindow?.Visibility == Visibility.Collapsed
                                    || Application.Current?.MainWindow?.Visibility == Visibility.Hidden)
                                {
                                    Application.Current.MainWindow?.Show();
                                }
                                else
                                {
                                        // Tell the configuration UI to restore its window if minimized
                                        setWindowState(ref App.vaProxy, WindowState.Minimized, true, false);
                                    App.vaProxy.WriteToLog("The EDDI configuration window is already open.", "orange");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("Show configuration window failed", ex);
                            }
                        });
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
                    Application.Current?.Dispatcher?.Invoke(() => Application.Current?.MainWindow?.Hide());
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
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MainWindow mainwindow = (MainWindow)Application.Current?.MainWindow;
                    mainwindow?.Dispatcher?.Invoke(mainwindow.VaWindowStateChange, newState, minimizeCheck);
                });
            }
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
                Thread thread = new Thread(() => Clipboard.SetText(uri));
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

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), speech, (int)priority, voice, false, null, true);
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

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), speech, (int)priority, voice, true, null, true);
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

                string voice = vaProxy.GetText("Voice");

                SpeechResponder speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                if (speechResponder == null)
                {
                    Logging.Warn("Unable to find speech responder");
                }

                // sayOutLoud must be true to match the behavior described by the wiki for the `disablespeechresponder` command
                // i.e. "not talk unless specifically asked for information"
                speechResponder?.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), script, null, priority, voice, true, true);
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
                SpeechService.Instance.speechQueue.DequeueAllSpeech();
                SpeechService.Instance.StopCurrentSpeech();
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
                speechResponder?.SetPersonality(personality);
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
                if (ship.phoneticname != null)
                {
                    script = script.Replace("$=", ship.phoneticname);
                }
                else if (ship.name != null)
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

                if (EDDI.Instance.CurrentStarSystem != null)
                {
                    // Store locally
                    StarSystem here = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(EDDI.Instance.CurrentStarSystem.systemname);
                    here.comment = comment == "" ? null : comment;
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(here);

                    // Store in EDSM
                    StarMapService.Instance?.sendStarMapComment(EDDI.Instance.CurrentStarSystem.systemname, comment);
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to store system comment", e);
            }
        }

        public static void InvokeJumpDetails(ref dynamic vaProxy)
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                if (!string.IsNullOrEmpty(type))
                {
                    ShipMonitor.JumpDetail detail = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).JumpDetails(type);
                    vaProxy.SetDecimal("Ship jump detail distance", detail?.distance);
                    vaProxy.SetInt("Ship jump detail jumps", detail?.jumps);
                    vaProxy.SetText("Type variable", null);
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to get jump details", e);
            }
        }

        public static void InvokeRouteDetails(ref dynamic vaProxy)
        {
            try
            {
                CrimeMonitor crimeMonitor = (CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor");
                MaterialMonitor materialMonitor = (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor");
                int materialDistance = materialMonitor.maxStationDistanceFromStarLs ?? 10000;
                string type = vaProxy.GetText("Type variable");
                string system = vaProxy.GetText("System variable");
                string station = vaProxy.GetText("Station variable");

                switch (type)
                {
                    case "cancel":
                        {
                            NavigationService.Instance.CancelDestination();
                        }
                        break;
                    case "encoded":
                        {
                            NavigationService.Instance.GetServiceRoute("encoded", materialDistance);
                        }
                        break;
                    case "expiring":
                        {
                            NavigationService.Instance.GetExpiringRoute();
                        }
                        break;
                    case "facilitator":
                        {
                            int distance = crimeMonitor.maxStationDistanceFromStarLs ?? 10000;
                            bool isChecked = crimeMonitor.prioritizeOrbitalStations;
                            NavigationService.Instance.GetServiceRoute("facilitator", distance, isChecked);
                        }
                        break;
                    case "farthest":
                        {
                            NavigationService.Instance.GetFarthestRoute();
                        }
                        break;
                    case "guardian":
                        {
                            NavigationService.Instance.GetServiceRoute("guardian", materialDistance);
                        }
                        break;
                    case "human":
                        {
                            NavigationService.Instance.GetServiceRoute("human", materialDistance);
                        }
                        break;
                    case "manufactured":
                        {
                            NavigationService.Instance.GetServiceRoute("manufactured", materialDistance);
                        }
                        break;
                    case "most":
                        {
                            if (string.IsNullOrEmpty(system))
                            {
                                NavigationService.Instance.GetMostRoute();
                            }
                            else
                            {
                                NavigationService.Instance.GetMostRoute(system);
                            }
                        }
                        break;
                    case "nearest":
                        {
                            NavigationService.Instance.GetNearestRoute();
                        }
                        break;
                    case "next":
                        {
                            NavigationService.Instance.GetNextInRoute();
                        }
                        break;
                    case "raw":
                        {
                            NavigationService.Instance.GetServiceRoute("raw", materialDistance);
                        }
                        break;
                    case "route":
                        {
                            if (string.IsNullOrEmpty(system))
                            {
                                NavigationService.Instance.GetMissionsRoute();
                            }
                            else
                            {
                                NavigationService.Instance.GetMissionsRoute(system);
                            }
                        }
                        break;
                    case "scoop":
                        {
                            ShipMonitor.JumpDetail detail = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).JumpDetails("total");
                            NavigationService.Instance.GetScoopRoute(detail.distance);
                        }
                        break;
                    case "set":
                        {
                            if (string.IsNullOrEmpty(system))
                            {
                                NavigationService.Instance.SetDestination();
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(station))
                                {
                                    NavigationService.Instance.SetDestination(system);
                                }
                                else
                                {
                                    NavigationService.Instance.SetDestination(system, station);
                                }
                            }
                        }
                        break;
                    case "source":
                        {
                            if (string.IsNullOrEmpty(system))
                            {
                                NavigationService.Instance.GetSourceRoute();
                            }
                            else
                            {
                                NavigationService.Instance.GetSourceRoute(system);
                            }
                        }
                        break;
                    case "update":
                        {
                            if (string.IsNullOrEmpty(system))
                            {
                                NavigationService.Instance.UpdateRoute();
                            }
                            else
                            {
                                NavigationService.Instance.UpdateRoute(system);
                            }
                        }
                        break;
                }
                vaProxy.SetText("Type variable", null);
                vaProxy.SetText("System variable", null);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to get missions route", e);
            }
        }
    }
}
