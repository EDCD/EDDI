using Eddi;
using EddiEvents;
using EddiDataProviderService;
using EddiDataDefinitions;
using EddiShipMonitor;
using EddiSpeechResponder;
using EddiSpeechService;
using EddiStatusMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class VoiceAttackPlugin
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
        public static Thread updaterThread = null;

        public static void VA_Init1(dynamic vaProxy)
        {
            Logging.Info("Initialising EDDI VoiceAttack plugin");

            try
            {
                GetEddiInstance(ref vaProxy);
                EDDI.Instance.Start();

                // Add notifiers for state and property changes 
                EDDI.Instance.State.CollectionChanged += (s, e) => setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);

                SpeechService.Instance.PropertyChanged += (s, e) => setSpeaking(SpeechService.eddiSpeaking, ref vaProxy);

                // Display instance information if available
                if (EDDI.Instance.UpgradeRequired)
                {
                    string msg = "Please shut down VoiceAttack and run Eddi standalone to upgrade";
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "red");
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }
                else if (EDDI.Instance.UpgradeAvailable)
                {
                    string msg = "Please shut down VoiceAttack and run Eddi standalone to upgrade";
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange");
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }

                if (EDDI.Instance.Motd != null)
                {
                    string msg = "Message from Eddi: " + EDDI.Instance.Motd;
                    vaProxy.WriteToLog("Message from EDDI: " + EDDI.Instance.Motd, "black");
                    SpeechService.Instance.Say(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), msg, false);
                }

                // Set the initial values from the main EDDI objects
                setValues(ref vaProxy);

                // Spin out a worker thread to keep the VoiceAttack events up-to-date and run event-specific commands
                updaterThread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Event theEvent = EventQueue.Take();
                            vaProxy.SetText("EDDI event", theEvent.type);

                            // Update all standard values
                            setValues(ref vaProxy);

                            // Event-specific values
                            List<string> setKeys = new List<string>();
                            // We start off setting the keys which are official and known
                            foreach (string key in Events.VARIABLES[theEvent.type].Keys)
                            {
                                // Obtain the value by name.  Actually looking for a method get_<name>
                                System.Reflection.MethodInfo method = theEvent.GetType().GetMethod("get_" + key);
                                if (method != null)
                                {
                                    Type returnType = method.ReturnType;
                                    if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                    {
                                        returnType = Nullable.GetUnderlyingType(returnType);
                                    }

                                    string varname = "EDDI " + theEvent.type.ToLowerInvariant() + " " + key;

                                    if (returnType == typeof(string))
                                    {
                                        Logging.Debug("Setting string value " + varname + " to " + (string)method.Invoke(theEvent, null));
                                        vaProxy.SetText(varname, (string)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                    else if (returnType == typeof(int))
                                    {
                                        Logging.Debug("Setting int value " + varname + " to " + (int?)method.Invoke(theEvent, null));
                                        vaProxy.SetInt(varname, (int?)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                    else if (returnType == typeof(bool))
                                    {
                                        Logging.Debug("Setting boolean value " + varname + " to " + (bool?)method.Invoke(theEvent, null));
                                        vaProxy.SetBoolean(varname, (bool?)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                    else if (returnType == typeof(decimal))
                                    {
                                        Logging.Debug("Setting decimal value " + varname + " to " + (decimal?)method.Invoke(theEvent, null));
                                        vaProxy.SetDecimal(varname, (decimal?)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                    else if (returnType == typeof(double))
                                    {
                                        // Doubles are stored as decimals
                                        Logging.Debug("Setting decimal value " + varname + " to " + (decimal?)(double?)method.Invoke(theEvent, null));
                                        vaProxy.SetDecimal(varname, (decimal?)(double?)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                    else if (returnType == typeof(long))
                                    {
                                        Logging.Debug("Setting long value " + varname + " to " + (long?)method.Invoke(theEvent, null));
                                        vaProxy.SetDecimal(varname, (decimal?)(long?)method.Invoke(theEvent, null));
                                        setKeys.Add(key);
                                    }
                                }
                            }
                            // Now we carry out a generic walk through the event object to create whatever we find
                            setJsonValues(ref vaProxy, "EDDI " + theEvent.type.ToLowerInvariant(), JsonConvert.DeserializeObject(JsonConvert.SerializeObject(theEvent)), setKeys);

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
                        catch (ThreadAbortException)
                        {
                            Logging.Debug("Thread aborted");
                        }
                        catch (Exception ex)
                        {
                            Logging.Warn("Failed to handle event", ex);
                        }
                    }
                })
                {
                    IsBackground = true
                };
                updaterThread.Start();

                vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                setStatus(ref vaProxy, "Operational");

                // Fire an event once the VA plugin is initialized
                Event @event = new VAInitializedEvent(DateTime.UtcNow);

                if (initEventEnabled(@event.type))
                {
                    EDDI.Instance.eventHandler(@event);
                }

                Logging.Info("EDDI VoiceAttack plugin initialization complete");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize VoiceAttack plugin", e);
                vaProxy.WriteToLog("Unable to fully initialize EDDI. Some functions may not work.", "red");
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
                        throw new Exception("EDDI initialization canceled by user.");
                    }
                }
            }

            eddiInstance = true;
        }

        /// <summary>
        /// Walk a JSON object and write out all of the possible fields
        /// </summary>
        private static void setJsonValues(ref dynamic vaProxy, string prefix, dynamic json, List<string> setKeys)
        {
            foreach (JProperty child in json)
            {
                // We only take fully lower-cased entities, and ignore the raw key (as it's the raw journal event)
                if ((!new Regex("^[a-z]+$").IsMatch(child.Name)) || child.Name == "raw")
                {
                    Logging.Debug("Ignoring key " + child.Name);
                    continue;
                }
                // We also ignore any keys that we have already set elsewhere
                if (setKeys.Contains(child.Name))
                {
                    Logging.Debug("Skipping already-set key " + child.Name);
                    continue;
                }

                string name = prefix + " " + child.Name;

                if (child.Value == null)
                {
                    // No idea what it might have been so reset everything
                    Logging.Debug(prefix + " " + child.Name + " is null; need to reset all values");
                    vaProxy.SetText(name, null);
                    vaProxy.SetInt(name, null);
                    vaProxy.SetDecimal(name, null);
                    vaProxy.SetBoolean(name, null);
                    vaProxy.SetDate(name, null);
                    continue;
                }
                if (child.Value.Type == JTokenType.Boolean)
                {
                    Logging.Debug("Setting boolean value " + name + " to " + (bool?)child.Value);
                    vaProxy.SetBoolean(name, (bool?)child.Value);
                }
                else if (child.Value.Type == JTokenType.String)
                {
                    Logging.Debug("Setting string value " + name + " to " + (string)child.Value);
                    vaProxy.SetText(name, (string)child.Value);
                }
                else if (child.Value.Type == JTokenType.Float)
                {
                    Logging.Debug("Setting decimal value " + name + " to " + (decimal?)(double?)child.Value);
                    vaProxy.SetDecimal(name, (decimal?)(double?)child.Value);
                }
                else if (child.Value.Type == JTokenType.Integer)
                {
                    // We set integers as decimals
                    Logging.Debug("Setting decimal value " + name + " to " + (decimal?)(long?)child.Value);
                    vaProxy.SetDecimal(name, (decimal?)(long?)child.Value);
                }
                else if (child.Value.Type == JTokenType.Date)
                {
                    Logging.Debug("Setting date value " + name + " to " + (DateTime?)child.Value);
                    vaProxy.SetDate(name, (DateTime?)child.Value);
                }
                else if (child.Value.Type == JTokenType.Array)
                {
                    int i = 0;
                    foreach (JToken arrayChild in child.Value.Children())
                    {
                        Logging.Debug("Handling element " + i);
                        string childName = name + " " + i;
                        if (arrayChild.Type == JTokenType.Boolean)
                        {
                            Logging.Debug("Setting boolean value " + childName + " to " + arrayChild.Value<bool?>());
                            vaProxy.SetBoolean(childName, arrayChild.Value<bool?>());
                        }
                        else if (arrayChild.Type == JTokenType.String)
                        {
                            Logging.Debug("Setting string value " + childName + " to " + arrayChild.Value<string>());
                            vaProxy.SetText(childName, arrayChild.Value<string>());
                        }
                        else if (arrayChild.Type == JTokenType.Float)
                        {
                            Logging.Debug("Setting decimal value " + childName + " to " + arrayChild.Value<decimal?>());
                            vaProxy.SetDecimal(childName, arrayChild.Value<decimal?>());
                        }
                        else if (arrayChild.Type == JTokenType.Integer)
                        {
                            Logging.Debug("Setting decimal value " + childName + " to " + arrayChild.Value<decimal?>());
                            vaProxy.SetDecimal(childName, arrayChild.Value<decimal?>());
                        }
                        else if (arrayChild.Type == JTokenType.Date)
                        {
                            Logging.Debug("Setting date value " + childName + " to " + arrayChild.Value<DateTime?>());
                            vaProxy.SetDate(childName, arrayChild.Value<DateTime?>());
                        }
                        else if (arrayChild.Type == JTokenType.Null)
                        {
                            Logging.Debug("Setting null value " + childName);
                            vaProxy.SetText(childName, null);
                            vaProxy.SetInt(childName, null);
                            vaProxy.SetDecimal(childName, null);
                            vaProxy.SetBoolean(childName, null);
                            vaProxy.SetDate(childName, null);
                        }
                        else if (arrayChild.Type == JTokenType.Object)
                        {
                            setJsonValues(ref vaProxy, childName, arrayChild, new List<string>());
                        }
                        i++;
                    }
                    vaProxy.SetInt(name + " entries", i);
                }
                else if (child.Value.Type == JTokenType.Object)
                {
                    Logging.Debug("Found object");
                    setJsonValues(ref vaProxy, prefix + " " + child.Name, child.Value, new List<string>());
                }
                else if (child.Value.Type == JTokenType.Null)
                {
                    // Because the type is NULL we don't know which VA item it was; empty all of them
                    vaProxy.SetBoolean(prefix + " " + child.Name, null);
                    vaProxy.SetText(prefix + " " + child.Name, null);
                    vaProxy.SetDecimal(prefix + " " + child.Name, null);
                    vaProxy.SetDate(prefix + " " + child.Name, null);
                }
                else
                {
                    Logging.Warn(child.Value.Type + ": " + child.Name + "=" + child.Value);
                }
            }
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
            setValues(ref vaProxy);
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

                Logging.Debug("Starting process with uri " + systemUri);

                setStatus(ref vaProxy, "Operational");

                HandleUri(ref vaProxy, systemUri);
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

                Logging.Debug("Starting process with uri " + stationUri);

                HandleUri(ref vaProxy, stationUri);

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

                Logging.Debug("Starting process with uri " + shipUri);

                HandleUri(ref vaProxy, shipUri);

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

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name, ref dynamic vaProxy)
        {
            vaProxy.SetText(name, module?.name);
            vaProxy.SetInt(name + " class", module?.@class);
            vaProxy.SetText(name + " grade", module?.grade);
            vaProxy.SetDecimal(name + " health", module?.health);
            vaProxy.SetDecimal(name + " cost", module?.price);
            vaProxy.SetDecimal(name + " value", module?.value);
            if (module != null && module.price < module.value)
            {
                decimal discount = Math.Round((1 - (((decimal)module.price) / ((decimal)module.value))) * 100, 1);
                vaProxy.SetDecimal(name + " discount", discount > 0.01M ? discount : (decimal?)null);
            }
            else
            {
                vaProxy.SetDecimal(name + " discount", null);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleOutfittingValues(Module existing, List<Module> outfittingModules, string name, ref dynamic vaProxy)
        {
            if (existing != null && outfittingModules != null)
            {
                foreach (Module Module in outfittingModules)
                {
                    if (existing.EDDBID == Module.EDDBID)
                    {
                        // Found it
                        vaProxy.SetDecimal(name + " station cost", (decimal?)Module.price);
                        if (Module.price < existing.price)
                        {
                            // And it's cheaper
                            vaProxy.SetDecimal(name + " station discount", existing.price - Module.price);
                            vaProxy.SetText(name + " station discount (spoken)", Translations.Humanize(existing.price - Module.price));
                        }
                        return;
                    }
                }
            }
            // Not found so remove any existing
            vaProxy.SetDecimal("Ship " + name + " station cost", (decimal?)null);
            vaProxy.SetDecimal("Ship " + name + " station discount", (decimal?)null);
            vaProxy.SetText("Ship " + name + " station discount (spoken)", (string)null);
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
                setStatus(ref vaProxy, "Failed to run internal speech system", e);
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

        /// <summary>Set all values</summary>
        private static void setValues(ref dynamic vaProxy)
        {
            Logging.Debug("Setting values");
            try
            {
                setCommanderValues(EDDI.Instance.Cmdr, ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set commander values", ex);
            }

            try
            {
                setShipValues(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), "Ship", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set current ship values", ex);
            }

            try
            {
                List<Ship> shipyard = new List<Ship>(((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.shipyard);
                if (shipyard != null)
                {
                    int currentStoredShip = 1;
                    foreach (Ship StoredShip in shipyard)
                    {
                        setShipValues(StoredShip, "Stored ship " + currentStoredShip, ref vaProxy);
                        currentStoredShip++;
                    }
                    vaProxy.SetInt("Stored ship entries", ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).shipyard.Count);
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set shipyard", ex);
            }

            try
            {
                setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set current system", ex);
            }

            try
            {
                setStatusValues(StatusMonitor.currentStatus, "Status", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set current status", ex);
            }

            try
            {
                setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set last system", ex);
            }

            try
            {
                setStarSystemValues(EDDI.Instance.HomeStarSystem, "Home system", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set home system", ex);
            }

            try
            {
                setSpeechValues(ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set initial speech service variables", ex);
            }

            try
            {
                setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set state", ex);
            }

            // Backwards-compatibility with 1.x
            try
            {
                if (EDDI.Instance.HomeStarSystem != null)
                {
                    vaProxy.SetText("Home system", EDDI.Instance.HomeStarSystem.name);
                    vaProxy.SetText("Home system (spoken)", Translations.StarSystem(EDDI.Instance.HomeStarSystem.name));
                }
                if (EDDI.Instance.HomeStation != null)
                {
                    vaProxy.SetText("Home station", EDDI.Instance.HomeStation.name);
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set 1.x values", ex);
            }

            try
            {
                setStationValues(EDDI.Instance.CurrentStation, "Last station", ref vaProxy);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set last station", ex);
            }

            try
            {
                vaProxy.SetText("Environment", EDDI.Instance.Environment);

                vaProxy.SetText("Vehicle", EDDI.Instance.Vehicle);

                vaProxy.SetText("EDDI version", Constants.EDDI_VERSION);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set misc values", ex);
            }

            Logging.Debug("Set values");
        }

        // Set values from a dictionary
        private static void setDictionaryValues(IDictionary<string, object> dict, string prefix, ref dynamic vaProxy)
        {
            foreach (string key in dict.Keys)
            {
                object value = dict[key];
                if (value == null)
                {
                    continue;
                }
                Type valueType = value.GetType();
                if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    valueType = Nullable.GetUnderlyingType(valueType);
                }

                string varname = "EDDI " + prefix + " " + key;

                if (valueType == typeof(string))
                {
                    vaProxy.SetText(varname, (string)value);
                }
                else if (valueType == typeof(int))
                {
                    vaProxy.SetInt(varname, (int)value);
                }
                else if (valueType == typeof(bool))
                {
                    vaProxy.SetBoolean(varname, (bool?)value);
                }
                else if (valueType == typeof(decimal))
                {
                    vaProxy.SetDecimal(varname, (decimal?)value);
                }
                else if (valueType == typeof(double))
                {
                    vaProxy.SetDecimal(varname, (decimal?)(double?)value);
                }
                else if (valueType == typeof(long))
                {
                    vaProxy.SetDecimal(varname, (decimal?)(long?)value);
                }
                else
                {
                    Logging.Debug("Not handling state value type " + valueType);
                }
            }

        }

        /// <summary>Set values for a station</summary>
        private static void setStationValues(Station station, string prefix, ref dynamic vaProxy)
        {
            Logging.Debug("Setting station information");

            if (station == null)
            {
                // We don't have any data so remove any info that we might have in history
                vaProxy.SetText(prefix + " name", null);
                vaProxy.SetDecimal(prefix + " distance from star", null);
                vaProxy.SetText(prefix + " government", null);
                vaProxy.SetText(prefix + " allegiance", null);
                vaProxy.SetText(prefix + " faction", null);
                vaProxy.SetText(prefix + " state", null);
                vaProxy.SetText(prefix + " primary economy", null);
                vaProxy.SetText(prefix + " secondary economy", null);
                vaProxy.SetText(prefix + " tertiary economy", null);
                vaProxy.SetBoolean(prefix + " has refuel", null);
                vaProxy.SetBoolean(prefix + " has repair", null);
                vaProxy.SetBoolean(prefix + " has rearm", null);
                vaProxy.SetBoolean(prefix + " has market", null);
                vaProxy.SetBoolean(prefix + " has black market", null);
                vaProxy.SetBoolean(prefix + " has outfitting", null);
                vaProxy.SetBoolean(prefix + " has shipyard", null);
            }
            else
            {
                vaProxy.SetText(prefix + " name", station.name);
                vaProxy.SetDecimal(prefix + " distance from star", station.distancefromstar);
                vaProxy.SetText(prefix + " government", station.government);
                vaProxy.SetText(prefix + " allegiance", station.allegiance);
                vaProxy.SetText(prefix + " faction", station.faction);
                vaProxy.SetText(prefix + " state", station.state);
                vaProxy.SetText(prefix + " primary economy", station.primaryeconomy);
                // Services
                vaProxy.SetBoolean(prefix + " has refuel", station.hasrefuel);
                vaProxy.SetBoolean(prefix + " has repair", station.hasrepair);
                vaProxy.SetBoolean(prefix + " has rearm", station.hasrearm);
                vaProxy.SetBoolean(prefix + " has market", station.hasmarket);
                vaProxy.SetBoolean(prefix + " has black market", station.hasblackmarket);
                vaProxy.SetBoolean(prefix + " has outfitting", station.hasoutfitting);
                vaProxy.SetBoolean(prefix + " has shipyard", station.hasshipyard);
            }

            Logging.Debug("Set station information");
        }

        private static void setCommanderValues(Commander cmdr, ref dynamic vaProxy)
        {
            try
            {
                vaProxy.SetText("Name", cmdr?.name);
                vaProxy.SetInt("Combat rating", cmdr?.combatrating?.rank);
                vaProxy.SetText("Combat rank", cmdr?.combatrating?.name);
                vaProxy.SetInt("Trade rating", cmdr?.traderating?.rank);
                vaProxy.SetText("Trade rank", cmdr?.traderating?.name);
                vaProxy.SetInt("Explore rating", cmdr?.explorationrating?.rank);
                vaProxy.SetText("Explore rank", cmdr?.explorationrating?.name);
                vaProxy.SetInt("Empire rating", cmdr?.empirerating?.rank);
                vaProxy.SetText("Empire rank", cmdr?.empirerating?.name);
                vaProxy.SetInt("Federation rating", cmdr?.federationrating?.rank);
                vaProxy.SetText("Federation rank", cmdr?.federationrating?.name);
                vaProxy.SetDecimal("Credits", cmdr?.credits);
                vaProxy.SetText("Credits (spoken)", Translations.Humanize(cmdr?.credits));
                vaProxy.SetDecimal("Debt", cmdr?.debt);
                vaProxy.SetText("Debt (spoken)", Translations.Humanize(cmdr?.debt));
                vaProxy.SetText("Title", cmdr?.title ?? "Commander");
                vaProxy.SetText("Gender", cmdr?.gender ?? "Neither");
                vaProxy.SetDecimal("Insurance", cmdr?.insurance);

                // Backwards-compatibility with 1.x
                vaProxy.SetText("System rank", cmdr?.title);

                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set commander information", e);
            }

            Logging.Debug("Set commander information");
        }

        private static void setShipValues(Ship ship, string prefix, ref dynamic vaProxy)
        {
            if (ship == null)
            {
                return;
            }
            Logging.Debug("Setting ship information (" + prefix + ")");
            try
            {
                vaProxy.SetText(prefix + " manufacturer", ship?.manufacturer);
                vaProxy.SetText(prefix + " model", ship?.model);
                vaProxy.SetText(prefix + " model (spoken)", ship?.SpokenModel());

                if (((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip() != null && EDDI.Instance.Cmdr != null && EDDI.Instance.Cmdr.name != null)
                {
                    vaProxy.SetText(prefix + " callsign", ship == null ? null : ship.manufacturer + " " + EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant());
                    vaProxy.SetText(prefix + " callsign (spoken)", ship == null ? null : ship.SpokenManufacturer() + " " + Translations.ICAO(EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant()));
                }

                vaProxy.SetText(prefix + " name", ship?.name);
                vaProxy.SetText(prefix + " name (spoken)", ship?.phoneticname);
                vaProxy.SetText(prefix + " ident", ship?.ident);
                vaProxy.SetText(prefix + " ident (spoken)", Translations.ICAO(ship?.ident, false));
                vaProxy.SetText(prefix + " role", ship?.role?.ToString());
                vaProxy.SetText(prefix + " size", ship?.size?.ToString());
                vaProxy.SetDecimal(prefix + " value", ship?.value);
                vaProxy.SetText(prefix + " value (spoken)", Translations.Humanize(ship?.value));
                vaProxy.SetDecimal(prefix + " health", ship?.health);
                vaProxy.SetInt(prefix + " cargo capacity", ship?.cargocapacity);
                vaProxy.SetInt(prefix + " cargo carried", ship?.cargocarried);
                // Add number of limpets carried
                if (ship == null || ship.cargo == null)
                {
                    vaProxy.SetInt(prefix + " limpets carried", null);
                }
                else
                {
                    int limpets = 0;
                    foreach (Cargo cargo in ship.cargo)
                    {
                        if (cargo.commodity.name == "Limpet")
                        {
                            limpets += cargo.amount;
                        }
                    }
                    vaProxy.SetInt(prefix + " limpets carried", limpets);
                }

                setShipModuleValues(ship?.bulkheads, prefix + " bulkheads", ref vaProxy);
                setShipModuleOutfittingValues(ship?.bulkheads, EDDI.Instance.CurrentStation?.outfitting, prefix + " bulkheads", ref vaProxy);
                setShipModuleValues(ship?.powerplant, prefix + " power plant", ref vaProxy);
                setShipModuleOutfittingValues(ship?.powerplant, EDDI.Instance.CurrentStation?.outfitting, prefix + " power plant", ref vaProxy);
                setShipModuleValues(ship?.thrusters, prefix + " thrusters", ref vaProxy);
                setShipModuleOutfittingValues(ship?.thrusters, EDDI.Instance.CurrentStation?.outfitting, prefix + " thrusters", ref vaProxy);
                setShipModuleValues(ship?.frameshiftdrive, prefix + " frame shift drive", ref vaProxy);
                setShipModuleOutfittingValues(ship?.frameshiftdrive, EDDI.Instance.CurrentStation?.outfitting, prefix + " frame shift drive", ref vaProxy);
                setShipModuleValues(ship?.lifesupport, prefix + " life support", ref vaProxy);
                setShipModuleOutfittingValues(ship?.lifesupport, EDDI.Instance.CurrentStation?.outfitting, prefix + " life support", ref vaProxy);
                setShipModuleValues(ship?.powerdistributor, prefix + " power distributor", ref vaProxy);
                setShipModuleOutfittingValues(ship?.powerdistributor, EDDI.Instance.CurrentStation?.outfitting, prefix + " power distributor", ref vaProxy);
                setShipModuleValues(ship?.sensors, prefix + " sensors", ref vaProxy);
                setShipModuleOutfittingValues(ship?.sensors, EDDI.Instance.CurrentStation?.outfitting, prefix + " sensors", ref vaProxy);
                setShipModuleValues(ship?.fueltank, prefix + " fuel tank", ref vaProxy);
                setShipModuleOutfittingValues(ship?.fueltank, EDDI.Instance.CurrentStation?.outfitting, prefix + " fuel tank", ref vaProxy);

                // Special for fuel tank - capacity and total capacity
                vaProxy.SetDecimal(prefix + " fuel tank capacity", ship?.fueltankcapacity);
                vaProxy.SetDecimal(prefix + " total fuel tank capacity", ship?.fueltanktotalcapacity);

                // Hardpoints
                if (ship != null)
                {
                    int numTinyHardpoints = 0;
                    int numSmallHardpoints = 0;
                    int numMediumHardpoints = 0;
                    int numLargeHardpoints = 0;
                    int numHugeHardpoints = 0;
                    foreach (Hardpoint Hardpoint in ship.hardpoints)
                    {
                        string baseHardpointName = prefix;
                        switch (Hardpoint.size)
                        {
                            case 0:
                                baseHardpointName = prefix + " tiny hardpoint " + ++numTinyHardpoints;
                                break;
                            case 1:
                                baseHardpointName = prefix + " small hardpoint " + ++numSmallHardpoints;
                                break;
                            case 2:
                                baseHardpointName = prefix + " medium hardpoint " + ++numMediumHardpoints;
                                break;
                            case 3:
                                baseHardpointName = prefix + " large hardpoint " + ++numLargeHardpoints;
                                break;
                            case 4:
                                baseHardpointName = prefix + " huge hardpoint " + ++numHugeHardpoints;
                                break;
                        }

                        vaProxy.SetBoolean(baseHardpointName + " occupied", Hardpoint.module != null);
                        setShipModuleValues(Hardpoint.module, baseHardpointName + " module", ref vaProxy);
                        setShipModuleOutfittingValues(ship == null ? null : Hardpoint.module, EDDI.Instance.CurrentStation?.outfitting, baseHardpointName + " module", ref vaProxy);
                    }

                    vaProxy.SetInt(prefix + " hardpoints", numSmallHardpoints + numMediumHardpoints + numLargeHardpoints + numHugeHardpoints);
                    vaProxy.SetInt(prefix + " utility slots", numTinyHardpoints);
                    // Compartments
                    int curCompartment = 0;
                    foreach (Compartment Compartment in ship.compartments)
                    {
                        string baseCompartmentName = prefix + " compartment " + ++curCompartment;
                        vaProxy.SetInt(baseCompartmentName + " size", Compartment.size);
                        vaProxy.SetBoolean(baseCompartmentName + " occupied", Compartment.module != null);
                        setShipModuleValues(Compartment.module, baseCompartmentName + " module", ref vaProxy);
                        setShipModuleOutfittingValues(ship == null ? null : Compartment.module, EDDI.Instance.CurrentStation?.outfitting, baseCompartmentName + " module", ref vaProxy);
                    }
                    vaProxy.SetInt(prefix + " compartments", curCompartment);
                }

                // Fetch the star system in which the ship is stored
                if (ship != null && ship.starsystem != null)
                {
                    vaProxy.SetText(prefix + " system", ship.starsystem);
                    vaProxy.SetText(prefix + " station", ship.station);
                    StarSystem StoredShipStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(ship.starsystem);
                    // Have to grab a local copy of our star system as CurrentStarSystem might not have been initialised yet
                    StarSystem ThisStarSystem = StarSystemSqLiteRepository.Instance.GetStarSystem(EDDI.Instance.CurrentStarSystem.name);

                    // Work out the distance to the system where the ship is stored if we can
                    if (ThisStarSystem != null && ThisStarSystem.x != null && StoredShipStarSystem.x != null)
                    {
                        decimal distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(ThisStarSystem.x - StoredShipStarSystem.x), 2)
                            + Math.Pow((double)(ThisStarSystem.y - StoredShipStarSystem.y), 2)
                            + Math.Pow((double)(ThisStarSystem.z - StoredShipStarSystem.z), 2)), 2);
                        vaProxy.SetDecimal(prefix + " distance", distance);
                    }
                    else
                    {
                        // We don't know how far away the ship is
                        vaProxy.SetDecimal(prefix + " distance", null);
                    }
                }

                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set ship information", e);
            }

            Logging.Debug("Set ship information");
        }

        private static void setStarSystemValues(StarSystem system, string prefix, ref dynamic vaProxy)
        {
            if (system == null)
            {
                return;
            }
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                vaProxy.SetText(prefix + " name", system?.name);
                vaProxy.SetText(prefix + " name (spoken)", Translations.StarSystem(system?.name));
                vaProxy.SetDecimal(prefix + " population", system?.population);
                vaProxy.SetText(prefix + " population (spoken)", Translations.Humanize(system?.population));
                vaProxy.SetText(prefix + " allegiance", system?.allegiance);
                vaProxy.SetText(prefix + " government", system?.government);
                vaProxy.SetText(prefix + " faction", system?.faction);
                vaProxy.SetText(prefix + " primary economy", system?.primaryeconomy);
                vaProxy.SetText(prefix + " state", system?.state);
                vaProxy.SetText(prefix + " security", system?.security);
                vaProxy.SetText(prefix + " power", system?.power);
                vaProxy.SetText(prefix + " power (spoken)", Translations.Power(EDDI.Instance.CurrentStarSystem?.power));
                vaProxy.SetText(prefix + " power state", system?.powerstate);
                vaProxy.SetDecimal(prefix + " X", system?.x);
                vaProxy.SetDecimal(prefix + " Y", system?.y);
                vaProxy.SetDecimal(prefix + " Z", system?.z);
                vaProxy.SetInt(prefix + " visits", system?.visits);
                vaProxy.SetDate(prefix + " previous visit", system?.visits > 1 ? system.lastvisit : null);
                vaProxy.SetDecimal(prefix + " minutes since previous visit", system?.visits > 1 && system?.lastvisit.HasValue == true ? (decimal)(long)(DateTime.Now - system.lastvisit.Value).TotalMinutes : (decimal?)null);
                vaProxy.SetText(prefix + " comment", system?.comment);
                vaProxy.SetDecimal(prefix + " distance from home", system?.distancefromhome);

                if (system != null)
                {
                    foreach (Station Station in system.stations)
                    {
                        vaProxy.SetText(prefix + " station name", Station.name);
                    }
                    vaProxy.SetInt(prefix + " stations", system.stations.Count);
                    vaProxy.SetInt(prefix + " orbital stations", system.stations.Count(s => !s.IsPlanetary()));
                    vaProxy.SetInt(prefix + " starports", system.stations.Count(s => s.IsStarport()));
                    vaProxy.SetInt(prefix + " outposts", system.stations.Count(s => s.IsOutpost()));
                    vaProxy.SetInt(prefix + " planetary stations", system.stations.Count(s => s.IsPlanetary()));
                    vaProxy.SetInt(prefix + " planetary outposts", system.stations.Count(s => s.IsPlanetaryOutpost()));
                    vaProxy.SetInt(prefix + " planetary ports", system.stations.Count(s => s.IsPlanetaryPort()));

                    Body primaryBody = null;
                    if (system.bodies != null && system.bodies.Count > 0)
                    {
                        primaryBody = (system.bodies[0].distance == 0 ? system.bodies[0] : null);
                    }
                    setBodyValues(primaryBody, prefix + " main star", vaProxy);
                }
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set system information", e);
            }
            Logging.Debug("Set system information (" + prefix + ")");
        }

        private static void setBodyValues(Body body, string prefix, dynamic vaProxy)
        {
            Logging.Debug("Setting body information (" + prefix + ")");
            vaProxy.SetText(prefix + " stellar class", body?.stellarclass);
            if (body?.age == null)
            {
                vaProxy.SetDecimal(prefix + " age", null);
            }
            else
            {
                vaProxy.SetDecimal(prefix + " age", (decimal)(long)body.age);
            }
            Logging.Debug("Set body information (" + prefix + ")");
        }

        private static void setSpeechValues(ref dynamic vaProxy)
        {
            setSpeaking(SpeechService.eddiSpeaking, ref vaProxy);
        }

        private static void setSpeaking(bool eddiSpeaking, ref dynamic vaProxy)
        {
            vaProxy.SetBoolean("EDDI speaking", eddiSpeaking);
        }

        private static void setStatus(ref dynamic vaProxy, string status, Exception exception = null)
        {
            vaProxy.SetText("EDDI status", status);
            if (status == "Operational")
            {
                vaProxy.SetText("EDDI exception", null);
            }
            else
            {
                Logging.Warn("EDDI exception: " + (exception == null ? "<null>" : exception.ToString()));
                vaProxy.SetText("EDDI exception", exception?.ToString());
            }
        }

        private static void setStatusValues(Status status, string prefix, ref dynamic vaProxy)
        {
            if (status == null)
            {
                return;
            }

            try
            {
                // Variables set from status flags
                vaProxy.SetText(prefix + " vehicle", status?.vehicle);
                vaProxy.SetBoolean(prefix + " being interdicted", status?.being_interdicted);
                vaProxy.SetBoolean(prefix + " in danger", status?.in_danger);
                vaProxy.SetBoolean(prefix + " near surface", status?.near_surface);
                vaProxy.SetBoolean(prefix + " overheating", status?.overheating);
                vaProxy.SetBoolean(prefix + " low fuel", status?.low_fuel);
                vaProxy.SetText(prefix + " fsd status", status?.fsd_status);
                vaProxy.SetBoolean(prefix + " srv drive assist", status?.srv_drive_assist);
                vaProxy.SetBoolean(prefix + " srv under ship", status?.srv_under_ship);
                vaProxy.SetBoolean(prefix + " srv turret deployed", status?.srv_turret_deployed);
                vaProxy.SetBoolean(prefix + " srv handbrake activated", status?.srv_handbrake_activated);
                vaProxy.SetBoolean(prefix + " scooping fuel", status?.scooping_fuel);
                vaProxy.SetBoolean(prefix + " silent running", status?.silent_running);
                vaProxy.SetBoolean(prefix + " cargo scoop deployed", status?.cargo_scoop_deployed);
                vaProxy.SetBoolean(prefix + " lights on", status?.lights_on);
                vaProxy.SetBoolean(prefix + " in wing", status?.in_wing);
                vaProxy.SetBoolean(prefix + " hardpoints deployed", status?.hardpoints_deployed);
                vaProxy.SetBoolean(prefix + " flight assist off", status?.flight_assist_off);
                vaProxy.SetBoolean(prefix + " supercruise", status?.supercruise);
                vaProxy.SetBoolean(prefix + " shields up", status?.shields_up);
                vaProxy.SetBoolean(prefix + " landing gear down", status?.landing_gear_down);
                vaProxy.SetBoolean(prefix + " landed", status?.landed);
                vaProxy.SetBoolean(prefix + " docked", status?.docked);

                // Variables set from pips (these are not always present in the event)
                vaProxy.SetDecimal(prefix + " system pips", status?.pips_sys);
                vaProxy.SetDecimal(prefix + " engine pips", status?.pips_eng);
                vaProxy.SetDecimal(prefix + " weapon pips", status?.pips_wea);

                // Variables set directly from the event (these are not always present in the event)
                vaProxy.SetInt(prefix + " firegroup", status?.firegroup);
                vaProxy.SetText(prefix + " gui focus", status?.gui_focus);
                vaProxy.SetDecimal(prefix + " latitude", status?.latitude);
                vaProxy.SetDecimal(prefix + " longitude", status?.longitude);
                vaProxy.SetDecimal(prefix + " altitude", status?.altitude);
                vaProxy.SetDecimal(prefix + " heading", status?.heading);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set real-time status information", e);
            }

            Logging.Debug("Set real-time status information");
        }
    }
}
