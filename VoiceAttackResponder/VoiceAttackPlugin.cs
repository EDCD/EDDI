using EddiDataProviderService;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using EddiSpeechService;
using Utilities;
using Eddi;
using EddiEvents;
using System.Text;
using System.Text.RegularExpressions;
using EddiSpeechResponder;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                EDDI.Instance.Start();

                // Add a notifier for state changes
                EDDI.Instance.State.CollectionChanged += (s, e) => setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);

                // Display instance information if available
                if (EDDI.Instance.UpgradeRequired)
                {
                    string msg = "EDDI too old to work; please shut down VoiceAttack and run EDDI standalone to upgrade";
                    vaProxy.WriteToLog(msg, "red");
                    SpeechService.Instance.Say(EDDI.Instance.Ship, msg, false);
                }
                else if (EDDI.Instance.UpgradeAvailable)
                {
                    string msg = "EDDI version " + EDDI.Instance.UpgradeVersion + " is now available; please shut down VoiceAttack and run EDDI standalone to upgrade";
                    vaProxy.WriteToLog(msg, "orange");
                    SpeechService.Instance.Say(EDDI.Instance.Ship, msg, false);
                }
                if (EDDI.Instance.Motd != null)
                {
                    string msg = "Message from EDDI: " + EDDI.Instance.Motd;
                    vaProxy.WriteToLog(msg, "black");
                    SpeechService.Instance.Say(EDDI.Instance.Ship, msg, false);
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
                            Logging.Info("Searching for command " + commandName);
                            if (vaProxy.CommandExists(commandName))
                            {
                                Logging.Info("Found command " + commandName);
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
                });
                updaterThread.IsBackground = true;
                updaterThread.Start();

                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialise VoiceAttack plugin", e);
                vaProxy.WriteToLog("Failed to initialise EDDI.  Some functions might not work", "red");
            }
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
                        Logging.Warn("Handling element " + i);
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
                    Logging.Warn("Found object");
                    setJsonValues(ref vaProxy, prefix + " " + child.Name, child.Value, new List<string>());
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
            updaterThread.Abort();
            SpeechService.Instance.ShutUp();
            EDDI.Instance.Stop();
        }

        public static void VA_Invoke1(dynamic vaProxy)
        {
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
                    case "configuration":
                        InvokeConfiguration(ref vaProxy);
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
                Logging.Error("Failed to invoke action " + vaProxy.Context, e);
                vaProxy.WriteToLog("Failed to invoke action " + vaProxy.Context);
            }
        }

        public static void VA_StopCommand()
        {
        }

        private static void InvokeConfiguration(ref dynamic vaProxy)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    MainWindow window = new MainWindow(true);
                    window.ShowDialog();
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
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile(ref dynamic vaProxy)
        {
            EDDI.Instance.refreshProfile();
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
                Station thisStation = EDDI.Instance.CurrentStarSystem.stations.SingleOrDefault(s => s.name == (EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.name));
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
                if (EDDI.Instance.Ship == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = Coriolis.ShipUri(EDDI.Instance.Ship);

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
                Thread thread = new Thread(() => Clipboard.SetText(uri));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            else
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), "\"" + uri + "\"");
                proc.UseShellExecute = false;
                Process.Start(proc);
            }
        }

        /// <summary>Find a module in outfitting that matches our existing module and provide its price</summary>
        private static void setShipModuleValues(Module module, string name, ref dynamic vaProxy)
        {
            vaProxy.SetText(name, module == null ? null : module.name);
            vaProxy.SetInt(name + " class", module == null ? (int?)null : module.@class);
            vaProxy.SetText(name + " grade", module == null ? null : module.grade);
            vaProxy.SetDecimal(name + " health", module == null ? (decimal?)null : module.health);
            vaProxy.SetDecimal(name + " cost", module == null ? (decimal?)null : (decimal)module.price);
            vaProxy.SetDecimal(name + " value", module == null ? (decimal?)null : (decimal)module.value);
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

                SpeechService.Instance.Say(EDDI.Instance.Ship, speech, true, (int)priority, voice);
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

                speechResponder.Say(script, null, priority, voice);
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
                EDDI.Instance.DisableResponder("Speech responder");
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
                EDDI.Instance.EnableResponder("Speech responder");
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
                if (name == null)
                {
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
            if (EDDI.Instance.Ship != null)
            {
                if (EDDI.Instance.Ship != null && EDDI.Instance.Ship.phoneticname != null)
                {
                    script = script.Replace("$=", EDDI.Instance.Ship.phoneticname);
                }
                else if (EDDI.Instance.Ship != null && EDDI.Instance.Ship.name != null)
                {
                    script = script.Replace("$=", EDDI.Instance.Ship.name);
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
            setCommanderValues(EDDI.Instance.Cmdr, ref vaProxy);
            setShipValues(EDDI.Instance.Ship, "Ship", ref vaProxy);
            int currentStoredShip = 1;
            if (EDDI.Instance.Shipyard != null)
            {
                foreach (Ship StoredShip in EDDI.Instance.Shipyard)
                {
                    setShipValues(StoredShip, "Stored ship " + currentStoredShip, ref vaProxy);
                    currentStoredShip++;
                }
                vaProxy.SetInt("Stored ship entries", EDDI.Instance.Shipyard.Count);
            }
            setStarSystemValues(EDDI.Instance.CurrentStarSystem, "System", ref vaProxy);
            setStarSystemValues(EDDI.Instance.LastStarSystem, "Last system", ref vaProxy);
            setStarSystemValues(EDDI.Instance.HomeStarSystem, "Home system", ref vaProxy);

            setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);

            // Backwards-compatibility with 1.x
            if (EDDI.Instance.HomeStarSystem != null)
            {
                vaProxy.SetText("Home system", EDDI.Instance.HomeStarSystem.name);
                vaProxy.SetText("Home system (spoken)", Translations.StarSystem(EDDI.Instance.HomeStarSystem.name));
            }
            if (EDDI.Instance.HomeStation != null)
            {
                vaProxy.SetText("Home station", EDDI.Instance.HomeStation.name);
            }

            setStationValues(EDDI.Instance.CurrentStation, "Last station", ref vaProxy);

            vaProxy.SetText("Environment", EDDI.Instance.Environment);

            vaProxy.SetText("Vehicle", EDDI.Instance.Vehicle);

            vaProxy.SetText("EDDI version", Constants.EDDI_VERSION);

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
                    vaProxy.SetInt(varname, (int?)value);
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
                vaProxy.SetText("Name", cmdr == null ? null : cmdr.name);
                vaProxy.SetInt("Combat rating", cmdr == null || cmdr.combatrating == null ? (int?)null : cmdr.combatrating.rank);
                vaProxy.SetText("Combat rank", cmdr == null || cmdr.combatrating == null ? null : cmdr.combatrating.name);
                vaProxy.SetInt("Trade rating", cmdr == null || cmdr.traderating == null ? (int?)null : cmdr.traderating.rank);
                vaProxy.SetText("Trade rank", cmdr == null || cmdr.traderating == null ? null : cmdr.traderating.name);
                vaProxy.SetInt("Explore rating", cmdr == null || cmdr.explorationrating == null ? (int?)null : cmdr.explorationrating.rank);
                vaProxy.SetText("Explore rank", cmdr == null || cmdr.explorationrating == null ? null : cmdr.explorationrating.name);
                vaProxy.SetInt("Empire rating", cmdr == null || cmdr.empirerating == null ? (int?)null : cmdr.empirerating.rank);
                vaProxy.SetText("Empire rank", cmdr == null || cmdr.empirerating == null ? null : cmdr.empirerating.name);
                vaProxy.SetInt("Federation rating", cmdr == null || cmdr.federationrating == null ? (int?)null : cmdr.federationrating.rank);
                vaProxy.SetText("Federation rank", cmdr == null || cmdr.federationrating == null ? null : cmdr.federationrating.name);
                vaProxy.SetDecimal("Credits", cmdr == null ? (decimal?)null : cmdr.credits);
                vaProxy.SetText("Credits (spoken)", cmdr == null ? null : Translations.Humanize(cmdr.credits));
                vaProxy.SetDecimal("Debt", cmdr == null ? (decimal?)null : cmdr.debt);
                vaProxy.SetText("Debt (spoken)", cmdr == null ? null : Translations.Humanize(cmdr.debt));

                vaProxy.SetText("Title", cmdr == null ? null : cmdr.title);

                vaProxy.SetDecimal("Insurance", cmdr == null ? null : cmdr.insurance);

                // Backwards-compatibility with 1.x
                vaProxy.SetText("System rank", cmdr == null ? null : cmdr.title);

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
            Logging.Debug("Setting ship information (" + prefix + ")");
            try
            {
                vaProxy.SetText(prefix + " manufacturer", ship == null ? null : ship.manufacturer);
                vaProxy.SetText(prefix + " model", ship == null ? null : ship.model);
                vaProxy.SetText(prefix + " model (spoken)", ship == null ? null : ship.SpokenModel());

                if (EDDI.Instance.Ship != null && EDDI.Instance.Cmdr != null && EDDI.Instance.Cmdr.name != null)
                {
                    vaProxy.SetText(prefix + " callsign", ship == null ? null : ship.manufacturer + " " + EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant());
                    vaProxy.SetText(prefix + " callsign (spoken)", ship == null ? null : ship.SpokenManufacturer() + " " + Translations.CallSign(EDDI.Instance.Cmdr.name.Substring(0, 3).ToUpperInvariant()));
                }

                vaProxy.SetText(prefix + " name", ship == null ? null : ship.name);
                vaProxy.SetText(prefix + " role", ship == null || ship.role == null ? null : ship.role.ToString());
                vaProxy.SetText(prefix + " size", ship == null || ship.size == null ? null : ship.size.ToString());
                vaProxy.SetDecimal(prefix + " value", ship == null ? (decimal?)null : ship.value);
                vaProxy.SetText(prefix + " value (spoken)", ship == null ? null : Translations.Humanize(ship.value));
                vaProxy.SetDecimal(prefix + " health", ship == null ? (decimal?)null : ship.health);
                vaProxy.SetInt(prefix + " cargo capacity", ship == null ? (int?)null : ship.cargocapacity);
                vaProxy.SetInt(prefix + " cargo carried", ship == null ? (int?)null : ship.cargocarried);
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

                setShipModuleValues(ship == null ? null : ship.bulkheads, prefix + " bulkheads", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.bulkheads, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " bulkheads", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.powerplant, prefix + " power plant", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.powerplant, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " power plant", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.thrusters, prefix + " thrusters", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.thrusters, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " thrusters", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.frameshiftdrive, prefix + " frame shift drive", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.frameshiftdrive, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " frame shift drive", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.lifesupport, prefix + " life support", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.lifesupport, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " life support", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.powerdistributor, prefix + " power distributor", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.powerdistributor, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " power distributor", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.sensors, prefix + " sensors", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.sensors, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " sensors", ref vaProxy);
                setShipModuleValues(ship == null ? null : ship.fueltank, prefix + " fuel tank", ref vaProxy);
                setShipModuleOutfittingValues(ship == null ? null : ship.fueltank, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, prefix + " fuel tank", ref vaProxy);

                // Special for fuel tank - capacity and total capacity
                vaProxy.SetDecimal(prefix + " fuel tank capacity", ship == null ? (decimal?)null : ship.fueltankcapacity);
                vaProxy.SetDecimal(prefix + " total fuel tank capacity", ship == null ? (decimal?)null : ship.fueltanktotalcapacity);

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
                        setShipModuleOutfittingValues(ship == null ? null : Hardpoint.module, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, baseHardpointName + " module", ref vaProxy);
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
                        setShipModuleOutfittingValues(ship == null ? null : Compartment.module, EDDI.Instance.CurrentStation == null ? null : EDDI.Instance.CurrentStation.outfitting, baseCompartmentName + " module", ref vaProxy);
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
            Logging.Debug("Setting system information (" + prefix + ")");
            try
            {
                vaProxy.SetText(prefix + " name", system == null ? null : system.name);
                vaProxy.SetText(prefix + " name (spoken)", system == null ? null : Translations.StarSystem(system.name));
                vaProxy.SetDecimal(prefix + " population", system == null ? null : (decimal?)system.population);
                vaProxy.SetText(prefix + " population (spoken)", system == null ? null : Translations.Humanize(system.population));
                vaProxy.SetText(prefix + " allegiance", system == null ? null : system.allegiance);
                vaProxy.SetText(prefix + " government", system == null ? null : system.government);
                vaProxy.SetText(prefix + " faction", system == null ? null : system.faction);
                vaProxy.SetText(prefix + " primary economy", system == null ? null : system.primaryeconomy);
                vaProxy.SetText(prefix + " state", system == null ? null : system.state);
                vaProxy.SetText(prefix + " security", system == null ? null : system.security);
                vaProxy.SetText(prefix + " power", system == null ? null : system.power);
                vaProxy.SetText(prefix + " power (spoken)", EDDI.Instance.CurrentStarSystem == null ? null : Translations.Power(EDDI.Instance.CurrentStarSystem.power));
                vaProxy.SetText(prefix + " power state", system == null ? null : system.powerstate);
                vaProxy.SetDecimal(prefix + " X", system == null ? null : system.x);
                vaProxy.SetDecimal(prefix + " Y", system == null ? null : system.y);
                vaProxy.SetDecimal(prefix + " Z", system == null ? null : system.z);
                vaProxy.SetInt(prefix + " visits", system == null ? (int?)null : system.visits);
                vaProxy.SetText(prefix + " comment", system == null ? null : system.comment);
                vaProxy.SetDecimal(prefix + " distance from home", system == null ? null : system.distancefromhome);

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
            vaProxy.SetText(prefix + " stellar class", body == null ? null : body.stellarclass);
            vaProxy.SetInt(prefix + " age", body == null ? null : (int?)body.age);
            Logging.Debug("Set body information (" + prefix + ")");
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
                vaProxy.SetText("EDDI exception", exception == null ? null : exception.ToString());
            }
        }
    }
}
