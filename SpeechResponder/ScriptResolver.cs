﻿using Cottle;
using Eddi;
using EddiBgsService;
using EddiCargoMonitor;
using EddiCompanionAppService;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiMaterialMonitor;
using EddiMissionMonitor;
using EddiNavigationService;
using EddiShipMonitor;
using EddiSpeechService;
using EddiStatusMonitor;
using GalnetMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using EddiCore;
using Utilities;

namespace EddiSpeechResponder
{
    public class ScriptResolver
    {
        private readonly Dictionary<string, Script> scripts;
        private readonly Random random;
        private readonly DocumentConfiguration setting;
        private readonly DataProviderService dataProviderService;
        private readonly BgsService bgsService;
        private readonly BindingFlags bindingFlags;
        public static EventHandler ScriptErrorEventHandler;

        public static object Instance { get; set; }

        public ScriptResolver(Dictionary<string, Script> scripts)
        {

            dataProviderService = new DataProviderService();
            bgsService = new BgsService();
            random = new Random();
            this.scripts = scripts ?? new Dictionary<string, Script>();
            setting = new DocumentConfiguration
            {
                Trimmer = DocumentConfiguration.TrimRepeatedWhitespaces
            };
            bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        }

        public int priority(string name)
        {
            scripts.TryGetValue(name, out Script script);
            return script?.Priority ?? 5;
        }

        /// <summary> From a custom dictionary of variable values in the default store </summary>
        public string resolveFromName(string name, Dictionary<Value, Value> vars, bool master = true)
        {
            return resolveFromName(name, buildStore(vars), master);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromName(string name, IContext store, bool master = true)
        {
            Logging.Debug("Resolving script " + name);
            scripts.TryGetValue(name, out Script script);
            if (script == null || script.Value == null)
            {
                Logging.Debug("No script");
                return null;
            }
            Logging.Debug("Found script");
            if (script.Enabled == false)
            {
                Logging.Debug("Script disabled");
                return null;
            }

            return resolveFromValue(script.Value, store, master, script);
        }

        /// <summary> From the default dictionary of variable values in the default store </summary>
        public string resolveFromValue(string scriptValue, bool master = true)
        {
            return resolveFromValue(scriptValue, createVariables(), master);
        }

        /// <summary> From a custom dictionary of variable values in the default store </summary>
        public string resolveFromValue(string scriptValue, Dictionary<Value, Value> vars, bool master = true)
        {
            return resolveFromValue(scriptValue, buildStore(vars), master);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromValue(string script, IContext store, bool master = true, Script scriptObject = null)
        {
            try
            {
                // Before we start, we remove the context for master scripts.
                // This means that scripts without context still work as expected
                if (master)
                {
                    EDDI.Instance.State["eddi_context_last_subject"] = null;
                    EDDI.Instance.State["eddi_context_last_action"] = null;
                }

                var document = Document.CreateDefault(script, setting).DocumentOrThrow;
                var result = document.Render(store);
                // Tidy up the output script
                result = Regex.Replace(result, " +", " ").Replace(" ,", ",").Replace(" .", ".").Trim();
                Logging.Debug("Turned script " + script + " in to speech " + result);
                result = result.Trim() == "" ? null : result.Trim();

                if (master && result != null)
                {
                    string stored = result;
                    // Remove any leading pause
                    if (stored.StartsWith("<break"))
                    {
                        string pattern = "^<break[^>]*>";
                        string replacement = "";
                        Regex rgx = new Regex(pattern);
                        stored = rgx.Replace(stored, replacement);
                    }

                    EDDI.Instance.State["eddi_context_last_speech"] = stored;
                    if (EDDI.Instance.State.TryGetValue("eddi_context_last_subject", out object lastSubject))
                    {
                        if (lastSubject != null)
                        {
                            string csLastSubject = ((string)lastSubject).ToLowerInvariant().Replace(" ", "_");
                            EDDI.Instance.State["eddi_context_last_speech_" + csLastSubject] = stored;
                        }
                    }
                }

                return result;
            }
            catch (Cottle.Exceptions.ParseException e)
            {
                // Report the failing the script name, if it is available
                string scriptName;
                ScriptError scriptError = new ScriptError(e.Message, script, e.LocationStart, e.LocationLength);
                if (scriptObject != null)
                {
                    scriptName = "the script \"" + scriptObject.Name + "\"";
                }
                else
                {
                    scriptName = "this script";
                }
                Logging.Warn($"Failed to resolve {scriptName} at line {scriptError.line}. {e}");
                ScriptErrorEventHandler?.Invoke(scriptError, new EventArgs());
                return $"There is a problem with {scriptName} at line {scriptError.line}. {errorTranslation(e.Message)}";
            }
        }

        private string errorTranslation(string msg)
        {
            // Give human readable descriptions for select cottle errors
            return msg
                    .Replace("'", "")
                    .Replace("<EOF>", "opening curly bracket")
                    .Replace("<eof>", "incomplete expression")
                    .Replace("{", "opening curly bracket")
                    .Replace("}", "closing curly bracket");
        }

        // Create Cottle variables from the EDDI information
        protected internal Dictionary<Value, Value> createVariables(Event theEvent = null)
        {
            Dictionary<Value, Value> dict = new Dictionary<Value, Value>
            {
                ["capi_active"] = CompanionAppService.Instance?.active ?? false,
                ["destinationdistance"] = EDDI.Instance.DestinationDistanceLy,
                ["searchdistance"] = NavigationService.Instance.SearchDistanceLy,
                ["environment"] = EDDI.Instance.Environment,
                ["horizons"] = EDDI.Instance.inHorizons,
                ["va_active"] = App.FromVA,
                ["vehicle"] = EDDI.Instance.Vehicle,
                ["icao_active"] = SpeechService.Instance.Configuration.EnableIcao,
                ["ssml_active"] = !SpeechService.Instance.Configuration.DisableSsml,
            };

            if (EDDI.Instance.Cmdr != null)
            {
                dict["cmdr"] = Value.FromReflection(EDDI.Instance.Cmdr, bindingFlags);
            }

            if (EDDI.Instance.HomeStarSystem != null)
            {
                dict["homesystem"] = Value.FromReflection(EDDI.Instance.HomeStarSystem, bindingFlags);
            }

            if (EDDI.Instance.HomeStation != null)
            {
                dict["homestation"] = Value.FromReflection(EDDI.Instance.HomeStation, bindingFlags);
            }

            if (EDDI.Instance.SquadronStarSystem != null)
            {
                dict["squadronsystem"] = Value.FromReflection(EDDI.Instance.SquadronStarSystem, bindingFlags);
            }

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                dict["system"] = Value.FromReflection(EDDI.Instance.CurrentStarSystem, bindingFlags);
            }

            if (EDDI.Instance.LastStarSystem != null)
            {
                dict["lastsystem"] = Value.FromReflection(EDDI.Instance.LastStarSystem, bindingFlags);
            }

            if (EDDI.Instance.NextStarSystem != null)
            {
                dict["nextsystem"] = Value.FromReflection(EDDI.Instance.NextStarSystem, bindingFlags);
            }

            if (EDDI.Instance.DestinationStarSystem != null)
            {
                dict["destinationsystem"] = Value.FromReflection(EDDI.Instance.DestinationStarSystem, bindingFlags);
            }

            if (NavigationService.Instance.SearchStarSystem != null)
            {
                dict["searchsystem"] = new ReflectionValue(NavigationService.Instance.SearchStarSystem);
            }
            
            if (NavigationService.Instance.SearchStation != null)
            {
                dict["searchstation"] = new ReflectionValue(NavigationService.Instance.SearchStation);
            }

            if (EDDI.Instance.CurrentStation != null)
            {
                dict["station"] = Value.FromReflection(EDDI.Instance.CurrentStation, bindingFlags);
            }

            if (EDDI.Instance.CurrentStellarBody != null)
            {
                dict["body"] = Value.FromReflection(EDDI.Instance.CurrentStellarBody, bindingFlags);
            }

            if (((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus != null)
            {
                dict["status"] = Value.FromReflection(((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus, bindingFlags);
            }

            if (theEvent != null)
            {
                dict["event"] = Value.FromReflection(theEvent, bindingFlags);
            }

            if (EDDI.Instance.State != null)
            {
                dict["state"] = ScriptResolver.buildState();
                Logging.Debug("State is " + JsonConvert.SerializeObject(EDDI.Instance.State));
            }

            // Obtain additional variables from each monitor
            foreach (EDDIMonitor monitor in EDDI.Instance.monitors)
            {
                IDictionary<string, object> monitorVariables = monitor.GetVariables();
                if (monitorVariables != null)
                {
                    foreach (string key in monitorVariables.Keys)
                    {
                        if (monitorVariables[key] == null)
                        {
                            dict.Remove(key);
                        }
                        else
                        {
                            dict[key] = Value.FromReflection(monitorVariables[key], bindingFlags);
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Build a store from a list of variables
        /// </summary>
        private IContext buildStore(Dictionary<Value, Value> vars)
        {
            IContext context = Context.CreateBuiltin(vars);

            // TODO fetch this from configuration
            bool useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            bool useSSML = !SpeechServiceConfiguration.FromFile().DisableSsml;

            // Function to call another script
            vars["F"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                return new ScriptResolver(scripts).resolveFromName(values[0].AsString, vars, false);
            }, 1));

            // Translation functions
            vars["P"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string val = values[0].AsString;
                return Translations.GetTranslation(val, useICAO);
            }, 1));

            // Boolean constants
            vars["true"] = true;
            vars["false"] = false;

            // Helper functions
            vars["OneOf"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                return new ScriptResolver(scripts).resolveFromValue(values[random.Next(values.Count)].AsString, vars, false);
            }));

            vars["Occasionally"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (random.Next((int)values[0].AsNumber) == 0)
                {
                    return new ScriptResolver(scripts).resolveFromValue(values[1].AsString, vars, false);
                }
                else
                {
                    return "";
                }
            }, 2));

            vars["Humanise"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                return Translations.Humanize((decimal)values[0].AsNumber);
            }, 1));

            vars["List"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string result = String.Empty;
                string localisedAnd = Properties.SpeechResponder.localizedAnd;
                if (values.Count == 1)
                {
                    foreach (KeyValuePair<Cottle.Value, Cottle.Value> value in values[0].Fields)
                    {
                        string valueString = value.Value.AsString;
                        if (value.Key == 0)
                        {
                            result = valueString;
                        }
                        else if (value.Key < (values[0].Fields.Count - 1))
                        {
                            result = $"{result}, {valueString}";
                        }
                        else
                        {
                            result = $"{result}{(values[0].Fields.Count() > 2 ? "," : "")} {localisedAnd} {valueString}";
                        }
                    }
                }
                return result;
            }, 1));

            vars["Pause"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                return @"<break time=""" + values[0].AsNumber + @"ms"" />";
            }, 1));

            vars["Play"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                return @"<audio src=""" + values[0].AsString + @""" />";
            }, 1));

            vars["Spacialise"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (values[0].AsString == null) { return ""; }

                if (useSSML)
                {
                    return Translations.sayAsLettersOrNumbers(values[0].AsString);
                }
                else
                {
                    string Entree = values[0].AsString;
                    if (Entree == "")
                    { return ""; }
                    string Sortie = "";
                    foreach (char c in Entree)
                    {
                        Sortie = Sortie + c + " ";
                    }
                    var UpperSortie = Sortie.ToUpper();
                    return UpperSortie.Trim();
                }
            }, 1));

            vars["Emphasize"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (values.Count == 1)
                {
                    return @"<emphasis level =""strong"">" + values[0].AsString + @"</emphasis>";
                }
                if (values.Count == 2)
                {
                    return @"<emphasis level =""" + values[1].AsString + @""">" + values[0].AsString + @"</emphasis>";
                }
                return "The Emphasize function is used improperly. Please review the documentation for correct usage.";
            }, 1, 2));

            vars["SpeechPitch"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string text = values[0].AsString;
                if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
                {
                    return text;
                }
                if (values.Count == 2)
                {
                    string pitch = values[1].AsString ?? "default";
                    return @"<prosody pitch=""" + pitch + @""">" + text + "</prosody>";
                }
                return "The SpeechPitch function is used improperly. Please review the documentation for correct usage.";
            }, 1, 2));

            vars["SpeechRate"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string text = values[0].AsString;
                if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
                {
                    return text;
                }
                if (values.Count == 2)
                {
                    string rate = values[1].AsString ?? "default";
                    return @"<prosody rate=""" + rate + @""">" + text + "</prosody>";
                }
                return "The SpeechRate function is used improperly. Please review the documentation for correct usage.";
            }, 1, 2));

            vars["SpeechVolume"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string text = values[0].AsString;
                if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString))
                {
                    return text;
                }
                if (values.Count == 2)
                {
                    string volume = values[1].AsString ?? "default";
                    return @"<prosody volume=""" + volume + @""">" + text + "</prosody>";
                }
                return "The SpeechVolume function is used improperly. Please review the documentation for correct usage.";
            }, 1, 2));

            vars["Transmit"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (values.Count == 1)
                {
                    return new ScriptResolver(scripts).resolveFromValue(@"<transmit>" + values[0].AsString + "</transmit>", vars, false);
                } 
                return "The Transmit function is used improperly. Please review the documentation for correct usage.";
            }, 1));

            vars["StartsWithVowel"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string Entree = values[0].AsString;
                if (Entree == "")
                { return ""; }

                char[] vowels = { 'a', 'à', 'â', 'ä', 'e', 'ê', 'é', 'è', 'ë', 'i', 'î', 'ï', 'o', 'ô', 'ö', 'u', 'ù', 'û', 'ü', 'œ' };
                char firstCharacter = Entree.ToLower().ToCharArray().ElementAt(0);
                Boolean result = vowels.Contains(firstCharacter);

                return result;

            }, 1));

            vars["Voice"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string text = values[0].AsString ?? string.Empty;
                string voice = values[1].AsString ?? string.Empty;

                if (SpeechService.Instance?.synth != null)
                {
                    foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                    {
                        if (vc.VoiceInfo.Name.ToLowerInvariant().Contains(voice?.ToLowerInvariant())
                            && !vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                        {
                            voice = vc.VoiceInfo.Name;
                            break;
                        }
                    }
                }

                if (values.Count == 2)
                {
                    return @"<voice name=""" + voice + @""">" + text + "</voice>";
                }
                return "The Voice function is used improperly. Please review the documentation for correct usage.";
            }, 1, 2));

            vars["VoiceDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (values.Count == 0)
                {
                    List<VoiceDetail> voices = new List<VoiceDetail>();
                    if (SpeechService.Instance?.synth != null)
                    {
                        foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                        {
                            if (!vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                            {
                                voices.Add(new VoiceDetail(
                                    vc.VoiceInfo.Name,
                                    vc.VoiceInfo.Culture.Parent.EnglishName,
                                    vc.VoiceInfo.Culture.Parent.NativeName,
                                    vc.VoiceInfo.Culture.Name,
                                    vc.VoiceInfo.Gender.ToString(),
                                    vc.Enabled
                                    ));
                            }
                        }
                    }
                    return Value.FromReflection(voices, bindingFlags);
                }
                if (values.Count == 1)
                {
                    VoiceDetail result = null;
                    if (SpeechService.Instance?.synth != null && !string.IsNullOrEmpty(values[0].AsString))
                    {
                        foreach (System.Speech.Synthesis.InstalledVoice vc in SpeechService.Instance.synth.GetInstalledVoices())
                        {
                            if (vc.VoiceInfo.Name.ToLowerInvariant().Contains(values[0].AsString.ToLowerInvariant())
                            && !vc.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                            {
                                result = new VoiceDetail(
                                    vc.VoiceInfo.Name,
                                    vc.VoiceInfo.Culture.Parent.EnglishName,
                                    vc.VoiceInfo.Culture.Parent.NativeName,
                                    vc.VoiceInfo.Culture.Name,
                                    vc.VoiceInfo.Gender.ToString(),
                                    vc.Enabled
                                    );
                                break;
                            }
                        }
                    }
                    return Value.FromReflection(result ?? new object(), bindingFlags);
                }
                return "The VoiceDetails function is used improperly. Please review the documentation for correct usage.";
            }, 0, 1));

            //
            // Commander-specific functions
            //
            vars["CommanderName"] = Value.FromFunction(Function.Create((state, values, output) => EDDI.Instance.Cmdr.SpokenName(), 0, 0));

            vars["ShipName"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
                string model = (values.Count == 2 ? values[1].AsString : null);
                Ship ship = findShip(localId, model);
                return ship.SpokenName();
            }, 0, 2));

            vars["ShipCallsign"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
                Ship ship = findShip(localId, null);

                string result;
                if (ship != null)
                {
                    if (EDDI.Instance.Cmdr != null && EDDI.Instance.Cmdr.name != null)
                    {
                        // Obtain the first three characters
                        string chars = new Regex("[^a-zA-Z0-9]").Replace(EDDI.Instance.Cmdr.name, "").ToUpperInvariant().Substring(0, 3);
                        result = ship.phoneticmanufacturer + " " + Translations.ICAO(chars);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(ship.phoneticmanufacturer))
                        {
                            result = "unidentified ship";
                        }
                        else
                        {
                            result = "unidentified " + ship.phoneticmanufacturer + " " + ship.phoneticmodel;
                        }
                    }
                }
                else
                {
                    result = "unidentified ship";
                }
                return result;
            }, 0, 1));

            //
            // Obtain definition objects for various items
            //

            vars["SecondsSince"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                var date = values[0].AsNumber;
                var now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);
                return now - date;
            }, 1));

            vars["ICAO"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                // Turn a string in to an ICAO definition
                string value = values[0].AsString;
                if (string.IsNullOrEmpty(value))
                {
                    return "";
                }

                // Remove anything that isn't alphanumeric
                Logging.Warn("value is " + value);
                value = value.ToUpperInvariant().Replace("[^A-Z0-9]", "");
                Logging.Warn("value is " + value);

                // Translate to ICAO
                return Translations.ICAO(value);
            }, 1));

            vars["ShipDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Ship result = ShipDefinitions.FromModel(values[0].AsString);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["JumpDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string value = values[0].AsString;
                if (string.IsNullOrEmpty(value)) { return ""; }
                var result = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.JumpDetails(value);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["CombatRatingDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                CombatRating result = CombatRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = CombatRating.FromEDName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["TradeRatingDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                TradeRating result = TradeRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = TradeRating.FromEDName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["ExplorationRatingDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                ExplorationRating result = ExplorationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = ExplorationRating.FromEDName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["EmpireRatingDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                EmpireRating result = EmpireRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = EmpireRating.FromEDName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["FederationRatingDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                FederationRating result = FederationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FederationRating.FromEDName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["SystemDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                StarSystem result;
                if (values.Count == 0)
                {
                    result = EDDI.Instance.CurrentStarSystem;
                }
                else if (values[0].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
                {
                    result = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    result = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                }
                setSystemDistanceFromHome(result);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["BodyDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                StarSystem system;
                if (values.Count == 0)
                {
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString) || values[1].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
                {
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    // Named system
                    system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                }
                Body result = system?.bodies?.Find(v => v.bodyname?.ToLowerInvariant() == values[0].AsString?.ToLowerInvariant());
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 2));

            vars["MissionDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                var missions = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.missions.ToList();

                Mission result = missions?.FirstOrDefault(v => v.missionid == values[0].AsNumber);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["RouteDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string result = null;
                string value = values[0].AsString;
                if (!string.IsNullOrEmpty(value))
                {
                    switch (value)
                    {
                        case "cancel":
                            {
                                NavigationService.Instance.CancelRoute();
                            }
                            break;
                        case "encoded":
                            {
                                result = NavigationService.Instance.GetServiceRoute("encoded");
                            }
                            break;
                        case "expiring":
                            {
                                result = NavigationService.Instance.GetExpiringRoute();
                            }
                            break;
                        case "facilitator":
                            {
                                result = NavigationService.Instance.GetServiceRoute("facilitator");
                            }
                            break;
                        case "farthest":
                            {
                                result = NavigationService.Instance.GetFarthestRoute();
                            }
                            break;
                        case "guardian":
                            {
                                result = NavigationService.Instance.GetServiceRoute("guardian");
                            }
                            break;
                        case "human":
                            {
                                result = NavigationService.Instance.GetServiceRoute("human");
                            }
                            break;
                        case "manufactured":
                            {
                                result = NavigationService.Instance.GetServiceRoute("manufactured");
                            }
                            break;
                        case "most":
                            {
                                if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.GetMostRoute(values[1].AsString);
                                }
                                else
                                {
                                    result = NavigationService.Instance.GetMostRoute();
                                }
                            }
                            break;
                        case "nearest":
                            {
                                result = NavigationService.Instance.GetNearestRoute();
                            }
                            break;
                        case "next":
                            {
                                result = NavigationService.Instance.GetNextInRoute();
                            }
                            break;
                        case "raw":
                            {
                                result = NavigationService.Instance.GetServiceRoute("raw");
                            }
                            break;
                        case "route":
                            {
                                if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.GetMissionsRoute(values[1].AsString);
                                }
                                else
                                {
                                    result = NavigationService.Instance.GetMissionsRoute();
                                }
                            }
                            break;
                        case "scoop":
                            {
                                if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.GetScoopRoute((decimal)values[1].AsNumber);
                                }
                                else
                                {
                                    ShipMonitor.JumpDetail detail = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).JumpDetails("total");
                                    result = NavigationService.Instance.GetScoopRoute(detail.distance);
                                }
                            }
                            break;
                        case "set":
                            {
                                if (values.Count == 3)
                                {
                                    result = NavigationService.Instance.SetRoute(values[1].AsString, values[2].AsString);
                                }
                                else if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.SetRoute(values[1].AsString);
                                }
                            }
                            break;
                        case "source":
                            {
                                if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.GetSourceRoute(values[1].AsString);
                                }
                                else
                                {
                                    result = NavigationService.Instance.GetSourceRoute();
                                }
                            }
                            break;
                        case "update":
                            {
                                if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.UpdateRoute(values[1].AsString);
                                }
                                else
                                {
                                    result = NavigationService.Instance.UpdateRoute();
                                }
                            }
                            break;
                    }
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 3));

            vars["StationDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Station result;
                if (values.Count == 0 || values[0].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStation?.name?.ToLowerInvariant())
                {
                    result = EDDI.Instance.CurrentStation;
                }
                else
                {
                    StarSystem system;
                    if (values.Count == 1 || values[1].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
                    {
                        // Current system
                        system = EDDI.Instance.CurrentStarSystem;
                    }
                    else
                    {
                        // Named system
                        system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                    }
                    result = system != null && system.stations != null ? system.stations.FirstOrDefault(v => v.name.ToLowerInvariant() == values[0].AsString.ToLowerInvariant()) : null;
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 2));

            vars["FactionDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Faction result;
                if (values.Count == 0)
                {
                    result = EDDI.Instance.CurrentStarSystem.Faction;
                }
                else if (values.Count == 1)
                {
                    result = bgsService.GetFactionByName(values[0].AsString);
                }
                else
                {
                    result = bgsService.GetFactionByName(values[0].AsString, values[1].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 2));

            vars["SuperpowerDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Superpower result = Superpower.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Superpower.FromNameOrEdName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["StateDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                FactionState result = FactionState.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FactionState.FromName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["EconomyDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Economy result = Economy.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Economy.FromName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["EngineerDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Engineer result = Engineer.FromName(values[0].AsString);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["GovernmentDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Government result = Government.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Government.FromName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["SecurityLevelDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                SecurityLevel result = SecurityLevel.FromName(values[0].AsString);
                if (result == null)
                {
                    result = SecurityLevel.FromName(values[0].AsString);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["MaterialDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Material result = Material.FromName(values[0].AsString);
                if (result?.edname != null && values.Count == 2)
                {
                    StarSystem starSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                    if (starSystem != null)
                    {
                        Body body = Material.highestPercentBody(result.edname, starSystem.bodies);
                        result.bodyname = body.bodyname;
                        result.bodyshortname = body.shortname;
                    }
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 2));

            vars["CommodityMarketDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                CommodityMarketQuote result = null;
                CommodityMarketQuote CommodityDetails(string commodityLocalizedName, Station station)
                {
                    return station?.commodities?.FirstOrDefault(c => c.localizedName == commodityLocalizedName);
                }

                if (values.Count == 1)
                {
                    // Named commodity, current station
                    Station station = EDDI.Instance.CurrentStation;
                    result = CommodityDetails(values[0].AsString, station);
                }
                else if (values.Count == 2)
                {
                    // Named commodity, named station, current system 
                    StarSystem system = EDDI.Instance.CurrentStarSystem;
                    string stationName = values[1].AsString;
                    Station station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                    result = CommodityDetails(values[0].AsString, station);
                }
                else if (values.Count == 3)
                {
                    // Named commodity, named station, named system 
                    StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString);
                    string stationName = values[1].AsString;
                    Station station = system?.stations?.FirstOrDefault(v => v.name == stationName);
                    result = CommodityDetails(values[0].AsString, station);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 0, 3));

            vars["CargoDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                CargoMonitor cargoMonitor = (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
                Cottle.Value value = values[0];
                Cargo result = null;

                if (value.Type == Cottle.ValueContent.String)
                {
                    var edname = CommodityDefinition.FromNameOrEDName(value.AsString)?.edname;
                    result = cargoMonitor?.GetCargoWithEDName(edname);
                }
                else if (value.Type == Cottle.ValueContent.Number)
                {
                    result = cargoMonitor?.GetCargoWithMissionId((long)value.AsNumber);
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["HaulageDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                var result = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor"))?.GetHaulageWithMissionId((long)values[0].AsNumber);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["BlueprintDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string blueprintName = values[0].AsString;
                int blueprintGrade = Convert.ToInt32(values[1].AsNumber);
                Blueprint result = Blueprint.FromNameAndGrade(blueprintName, blueprintGrade);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 2));

            vars["TrafficDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Traffic result = null;
                string systemName = values[0].AsString;
                if (!string.IsNullOrEmpty(systemName))
                {
                    if (values.Count == 2)
                    {
                        if (values[1].AsString == "traffic")
                        {
                            result = dataProviderService.GetSystemTraffic(systemName);
                        }
                        if (values[1].AsString == "deaths")
                        {
                            result = dataProviderService.GetSystemDeaths(systemName);
                        }
                        else if (values[1].AsString == "hostility")
                        {
                            result = dataProviderService.GetSystemHostility(systemName);
                        }
                    }
                    if (result == null)
                    {
                        result = dataProviderService.GetSystemTraffic(systemName);
                    }
                }
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1, 2));

            vars["GalnetNewsArticle"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                return Value.FromReflection(result ?? new object(), bindingFlags);
            }, 1));

            vars["GalnetNewsArticles"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                List<News> results = null;
                if (values.Count == 0)
                {
                    // Obtain all unread articles
                    results = GalnetSqLiteRepository.Instance.GetArticles();
                }
                else if (values.Count == 1)
                {
                    // Obtain all unread news of a given category
                    results = GalnetSqLiteRepository.Instance.GetArticles(values[0].AsString);
                }
                else if (values.Count == 2)
                {
                    // Obtain all news of a given category
                    results = GalnetSqLiteRepository.Instance.GetArticles(values[0].AsString, values[1].AsBoolean);
                }
                return Value.FromReflection(results ?? new List<News>(), bindingFlags);
            }, 0, 2));

            vars["GalnetNewsMarkRead"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.MarkRead(result);
                }
                return "";
            }, 1));

            vars["GalnetNewsMarkUnread"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.MarkUnread(result);
                }
                return "";
            }, 1));

            vars["GalnetNewsDelete"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.DeleteNews(result);
                }
                return "";
            }, 1));

            vars["Distance"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                double square(double x) => x * x;
                decimal result = 0;
                bool numVal = values[0].Type == Cottle.ValueContent.Number;
                bool stringVal = values[0].Type == Cottle.ValueContent.String;

                StarSystem curr = new StarSystem();
                StarSystem dest = new StarSystem();
                if (values.Count == 1 && stringVal)
                {
                    curr = EDDI.Instance?.CurrentStarSystem;
                    dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                }
                else if (values.Count == 2 && stringVal)
                {
                    curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                    dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                }
                else if (values.Count == 6 && numVal)
                {
                    curr.x = (decimal)values[0].AsNumber;
                    curr.y = (decimal)values[1].AsNumber;
                    curr.z = (decimal)values[2].AsNumber;
                    dest.x = (decimal)values[3].AsNumber;
                    dest.y = (decimal)values[4].AsNumber;
                    dest.z = (decimal)values[5].AsNumber;
                }

                if (curr?.x != null && dest?.x != null)
                {
                    result = (decimal)Math.Round(Math.Sqrt(square((double)(curr.x - dest.x))
                                + square((double)(curr.y ?? 0 - dest.y ?? 0))
                                + square((double)(curr.z ?? 0 - dest.z ?? 0))), 2);
                }

                return Value.FromReflection(result, bindingFlags);
            }, 1, 6));

            vars["Log"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                Logging.Info(values[0].AsString);
                return "";
            }, 1));

            vars["SetState"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                string name = values[0].AsString.ToLowerInvariant().Replace(" ", "_");
                Cottle.Value value = values[1];
                if (value.Type == Cottle.ValueContent.Boolean)
                {
                    EDDI.Instance.State[name] = value.AsBoolean;
                    vars["state"] = buildState();
                }
                else if (value.Type == Cottle.ValueContent.Number)
                {
                    EDDI.Instance.State[name] = value.AsNumber;
                    vars["state"] = buildState();
                }
                else if (value.Type == Cottle.ValueContent.String)
                {
                    EDDI.Instance.State[name] = value.AsString;
                    vars["state"] = buildState();
                }
                // Ignore other possibilities
                return "";
            }, 2));

            vars["RefreshProfile"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                bool stationRefresh = (values.Count == 0 ? false : values[0].AsBoolean);
                EDDI.Instance.refreshProfile(stationRefresh);
                return "";
            }, 0, 1));

            vars["InaraDetails"] = Value.FromFunction(Function.Create((state, values, output) =>
            {
                if (values[0].AsString is string commanderName)
                {
                    if (!string.IsNullOrWhiteSpace(commanderName))
                    {
                        EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                        var result = inaraService.GetCommanderProfile(commanderName);
                        return Value.FromReflection(result ?? new object(), bindingFlags);
                    }
                }
                return "";
            }, 1));

            return context;
        }

        public static Dictionary<Value, Value> buildState()
        {
            if (EDDI.Instance.State == null)
            {
                return null;
            }

            Dictionary<Value, Value> state = new Dictionary<Value, Value>();
            foreach (string key in EDDI.Instance.State.Keys)
            {
                object value = EDDI.Instance.State[key];
                if (value == null)
                {
                    continue;
                }
                Type valueType = value.GetType();
                if (valueType == typeof(string))
                {
                    state[key] = (string)value;
                }
                else if (valueType == typeof(int))
                {
                    state[key] = (int)value;
                }
                else if (valueType == typeof(bool))
                {
                    state[key] = (bool)value;
                }
                else if (valueType == typeof(decimal))
                {
                    state[key] = (decimal)value;
                }
            }
            return state;
        }

        private static Ship findShip(int? localId, string model)
        {
            ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
            Ship ship = null;
            if (localId == null)
            {
                // No local ID so take the current ship
                ship = shipMonitor?.GetCurrentShip();
            }
            else
            {
                // Find the ship with the given local ID
                ship = shipMonitor?.GetShip(localId);
            }

            if (ship == null)
            {
                // Provide a basic ship based on the model template
                ship = ShipDefinitions.FromModel(model);
                if (ship == null)
                {
                    ship = ShipDefinitions.FromEDModel(model);
                }
            }
            return ship;
        }

        private void setSystemDistanceFromHome(StarSystem system)
        {
            if (system == null) { return; }
            if (EDDI.Instance.HomeStarSystem != null && EDDI.Instance.HomeStarSystem.x != null && system.x != null)
            {
                system.distancefromhome = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(system.x ?? 0 - EDDI.Instance.HomeStarSystem.x ?? 0), 2)
                                                                      + Math.Pow((double)(system.y ?? 0 - EDDI.Instance.HomeStarSystem.y ?? 0), 2)
                                                                      + Math.Pow((double)(system.z ?? 0 - EDDI.Instance.HomeStarSystem.z ?? 0), 2)), 2);
                Logging.Debug("Distance from home is " + system.distancefromhome);
            }
        }
    }

    class VoiceDetail
    {
        private string name { get; set; }
        private string cultureinvariantname { get; set; }
        private string culturename { get; set; }
        private string culturecode { get; set; }
        private string gender { get; set; }
        public bool enabled { get; set; }

        public VoiceDetail(string name, string cultureinvariantname, string culturename, string culturecode, string gender, bool enabled)
        {
            this.name = name;
            this.cultureinvariantname = cultureinvariantname;
            this.culturename = culturename;
            this.culturecode = culturecode;
            this.gender = gender;
            this.enabled = enabled;
        }
    }
}
