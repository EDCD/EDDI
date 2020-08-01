using Cottle;
using Cottle.Builtins;
using Cottle.Documents;
using Cottle.Functions;
using Cottle.Settings;
using Cottle.Stores;
using Cottle.Values;
using Eddi;
using EddiBgsService;
using EddiCargoMonitor;
using EddiCompanionAppService;
using EddiCore;
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
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechResponder
{
    public class ScriptResolver
    {
        private readonly Dictionary<string, Script> scripts;
        private readonly Random random;
        private readonly CustomSetting setting;
        private readonly DataProviderService dataProviderService;
        private readonly BgsService bgsService;

        public static object Instance { get; set; }

        public ScriptResolver(Dictionary<string, Script> scripts)
        {

            dataProviderService = new DataProviderService();
            bgsService = new BgsService();
            random = new Random();
            this.scripts = scripts ?? new Dictionary<string, Script>();
            setting = new CustomSetting
            {
                Trimmer = BuiltinTrimmers.CollapseBlankCharacters
            };
        }

        public int priority(string name)
        {
            scripts.TryGetValue(name, out Script script);
            return script?.Priority ?? 5;
        }

        /// <summary> From a custom dictionary of variable values in the default store </summary>
        public string resolveFromName(string name, Dictionary<string, Cottle.Value> vars, bool isTopLevelScript)
        {
            BuiltinStore store = buildStore(vars);
            return resolveFromName(name, store, isTopLevelScript);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromName(string name, BuiltinStore store, bool isTopLevelScript)
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

            return resolveFromValue(script.Value, store, isTopLevelScript, script);
        }

        /// <summary> From the default dictionary of variable values in the default store </summary>
        public string resolveFromValue(string scriptValue, bool isTopLevelScript)
        {
            Dictionary<string, Value> vars = createVariables();
            BuiltinStore store = buildStore(vars);
            return resolveFromValue(scriptValue, store, isTopLevelScript);
        }

        /// <summary> From a custom store </summary>
        public string resolveFromValue(string script, BuiltinStore store, bool isTopLevelScript, Script scriptObject = null)
        {
            try
            {
                var document = new SimpleDocument(script, setting);
                var result = document.Render(store);
                // Tidy up the output script
                result = Regex.Replace(result, " +", " ").Replace(" ,", ",").Replace(" .", ".").Trim();
                Logging.Debug("Turned script " + script + " in to speech " + result);
                result = result.Trim() == "" ? null : result.Trim();

                if (isTopLevelScript && result != null)
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
                }

                return result;
            }
            catch (Cottle.Exceptions.ParseException e)
            {
                // Report the failing the script name, if it is available
                string scriptName;
                if (scriptObject != null)
                {
                    scriptName = "the script \"" + scriptObject.Name + "\"";
                }
                else
                {
                    scriptName = "this script";
                }

                Logging.Warn($"Failed to resolve {scriptName} at line {e.Line}. {e}");
                return $"There is a problem with {scriptName} at line {e.Line}. {errorTranslation(e.Message)}";
            }
            catch (Exception e)
            {
                Logging.Warn(e.Message, e);
                return $"Error with {scriptObject?.Name ?? "this"} script: {e.Message}";
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
        protected internal Dictionary<string, Cottle.Value> createVariables(Event theEvent = null)
        {
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>
            {
                ["capi_active"] = CompanionAppService.Instance?.active ?? false,
                ["destinationdistance"] = EDDI.Instance.DestinationDistanceLy,
                ["environment"] = EDDI.Instance.Environment,
                ["horizons"] = EDDI.Instance.inHorizons,
                ["va_active"] = App.FromVA,
                ["vehicle"] = EDDI.Instance.Vehicle,
                ["icao_active"] = SpeechService.Instance.Configuration.EnableIcao,
                ["ssml_active"] = !SpeechService.Instance.Configuration.DisableSsml,
            };

            if (EDDI.Instance.Cmdr != null)
            {
                dict["cmdr"] = new ReflectionValue(EDDI.Instance.Cmdr);
            }

            if (EDDI.Instance.HomeStarSystem != null)
            {
                dict["homesystem"] = new ReflectionValue(EDDI.Instance.HomeStarSystem);
            }

            if (EDDI.Instance.HomeStation != null)
            {
                dict["homestation"] = new ReflectionValue(EDDI.Instance.HomeStation);
            }

            if (EDDI.Instance.SquadronStarSystem != null)
            {
                dict["squadronsystem"] = new ReflectionValue(EDDI.Instance.SquadronStarSystem);
            }

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                dict["system"] = new ReflectionValue(EDDI.Instance.CurrentStarSystem);
            }

            if (EDDI.Instance.LastStarSystem != null)
            {
                dict["lastsystem"] = new ReflectionValue(EDDI.Instance.LastStarSystem);
            }

            if (EDDI.Instance.NextStarSystem != null)
            {
                dict["nextsystem"] = new ReflectionValue(EDDI.Instance.NextStarSystem);
            }

            if (EDDI.Instance.DestinationStarSystem != null)
            {
                dict["destinationsystem"] = new ReflectionValue(EDDI.Instance.DestinationStarSystem);
            }

            if (EDDI.Instance.DestinationStation != null)
            {
                dict["destinationstation"] = new ReflectionValue(EDDI.Instance.DestinationStation);
            }

            if (EDDI.Instance.CurrentStation != null)
            {
                dict["station"] = new ReflectionValue(EDDI.Instance.CurrentStation);
            }

            if (EDDI.Instance.CurrentStellarBody != null)
            {
                dict["body"] = new ReflectionValue(EDDI.Instance.CurrentStellarBody);
            }

            if (((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus != null)
            {
                dict["status"] = new ReflectionValue(((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor"))?.currentStatus);
            }

            if (theEvent != null)
            {
                dict["event"] = new ReflectionValue(theEvent);
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
                            dict[key] = new ReflectionValue(monitorVariables[key]);
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Build a store from a list of variables
        /// </summary>
        private BuiltinStore buildStore(Dictionary<string, Cottle.Value> vars)
        {
            BuiltinStore store = new BuiltinStore();

            // TODO fetch this from configuration
            bool useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            bool useSSML = !SpeechServiceConfiguration.FromFile().DisableSsml;

            // Function to call another script
            store["F"] = new NativeFunction((values) =>
            {
                return new ScriptResolver(scripts).resolveFromName(values[0].AsString, store, false);
            }, 1);

            // Translation functions
            store["P"] = new NativeFunction((values) =>
            {
                string val = values[0].AsString;
                return Translations.GetTranslation(val, useICAO);
            }, 1);

            // Boolean constants
            store["true"] = true;
            store["false"] = false;

            // Helper functions
            store["OneOf"] = new NativeFunction((values) =>
            {
                return new ScriptResolver(scripts).resolveFromValue(values[random.Next(values.Count)].AsString, store, false);
            });

            store["Occasionally"] = new NativeFunction((values) =>
            {
                if (random.Next((int)values[0].AsNumber) == 0)
                {
                    return new ScriptResolver(scripts).resolveFromValue(values[1].AsString, store, false);
                }
                else
                {
                    return "";
                }
            }, 2);

            store["Humanise"] = new NativeFunction((values) =>
            {
                return Translations.Humanize(values[0].AsNumber);
            }, 1);

            store["List"] = new NativeFunction((values) =>
            {
                string output = String.Empty;
                string localisedAnd = Properties.SpeechResponder.localizedAnd;
                if (values.Count == 1)
                {
                    foreach (KeyValuePair<Cottle.Value, Cottle.Value> value in values[0].Fields)
                    {
                        string valueString = value.Value.AsString;
                        if (value.Key == 0)
                        {
                            output = valueString;
                        }
                        else if (value.Key < (values[0].Fields.Count - 1))
                        {
                            output = $"{output}, {valueString}";
                        }
                        else
                        {
                            output = $"{output}{(values[0].Fields.Count() > 2 ? "," : "")} {localisedAnd} {valueString}";
                        }
                    }
                }
                return output;
            }, 1);

            store["Pause"] = new NativeFunction((values) =>
            {
                return @"<break time=""" + values[0].AsNumber + @"ms"" />";
            }, 1);

            store["Play"] = new NativeFunction((values) =>
            {
                return @"<audio src=""" + values[0].AsString + @""" />";
            }, 1);

            store["Spacialise"] = new NativeFunction((values) =>
            {
                if (values[0].AsString == null) { return null; }

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
            }, 1);

            store["Emphasize"] = new NativeFunction((values) =>
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
            }, 1, 2);

            store["SpeechPitch"] = new NativeFunction((values) =>
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
            }, 1, 2);

            store["SpeechRate"] = new NativeFunction((values) =>
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
            }, 1, 2);

            store["SpeechVolume"] = new NativeFunction((values) =>
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
            }, 1, 2);

            store["Transmit"] = new NativeFunction((values) =>
            {
                if (values.Count == 1)
                {
                    return new ScriptResolver(scripts).resolveFromValue(@"<transmit>" + values[0].AsString + "</transmit>", store, false);
                }
                return "The Transmit function is used improperly. Please review the documentation for correct usage.";
            }, 1);

            store["StartsWithVowel"] = new NativeFunction((values) =>
            {
                string Entree = values[0].AsString;
                if (Entree == "")
                { return ""; }

                char[] vowels = { 'a', 'à', 'â', 'ä', 'e', 'ê', 'é', 'è', 'ë', 'i', 'î', 'ï', 'o', 'ô', 'ö', 'u', 'ù', 'û', 'ü', 'œ' };
                char firstCharacter = Entree.ToLower().ToCharArray().ElementAt(0);
                Boolean result = vowels.Contains(firstCharacter);

                return result;

            }, 1);

            store["Voice"] = new NativeFunction((values) =>
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
            }, 1, 2);

            store["VoiceDetails"] = new NativeFunction((values) =>
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
                    return new ReflectionValue(voices);
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
                    return new ReflectionValue(result ?? new object());
                }
                return "The VoiceDetails function is used improperly. Please review the documentation for correct usage.";
            }, 0, 1);

            //
            // Commander-specific functions
            //
            store["CommanderName"] = new NativeFunction((values) => EDDI.Instance.Cmdr.SpokenName(), 0, 0);

            store["ShipName"] = new NativeFunction((values) =>
            {
                int? localId = (values.Count == 0 ? (int?)null : (int)values[0].AsNumber);
                string model = (values.Count == 2 ? values[1].AsString : null);
                Ship ship = findShip(localId, model);
                return ship.SpokenName();
            }, 0, 2);

            store["ShipCallsign"] = new NativeFunction((values) =>
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
            }, 0, 1);

            //
            // Obtain definition objects for various items
            //

            store["SecondsSince"] = new NativeFunction((values) =>
            {
                long? date = (long?)values[0].AsNumber;
                long? now = Dates.fromDateTimeToSeconds(DateTime.UtcNow);

                return now - date;
            }, 1);

            store["ICAO"] = new NativeFunction((values) =>
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
            }, 1);

            store["ShipDetails"] = new NativeFunction((values) =>
            {
                Ship result = ShipDefinitions.FromModel(values[0].AsString);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["JumpDetails"] = new NativeFunction((values) =>
            {
                string value = values[0].AsString;
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                var result = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.JumpDetails(value);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["CombatRatingDetails"] = new NativeFunction((values) =>
            {
                CombatRating result = CombatRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = CombatRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["TradeRatingDetails"] = new NativeFunction((values) =>
            {
                TradeRating result = TradeRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = TradeRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["ExplorationRatingDetails"] = new NativeFunction((values) =>
            {
                ExplorationRating result = ExplorationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = ExplorationRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["EmpireRatingDetails"] = new NativeFunction((values) =>
            {
                EmpireRating result = EmpireRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = EmpireRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["FederationRatingDetails"] = new NativeFunction((values) =>
            {
                FederationRating result = FederationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FederationRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["SystemDetails"] = new NativeFunction((values) =>
            {
                StarSystem result;
                if (values.Count == 0)
                {
                    result = EDDI.Instance.CurrentStarSystem;
                }
                else if (values[0]?.AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
                {
                    result = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    result = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
                }

                var distanceFromHome = result?.DistanceFromStarSystem(EDDI.Instance.HomeStarSystem);
                if (distanceFromHome != null)
                {
                    Logging.Debug("Distance from home is " + distanceFromHome);
                    result.distancefromhome = distanceFromHome;
                }

                return new ReflectionValue(result ?? new object());
            }, 1);

            store["BodyDetails"] = new NativeFunction((values) =>
            {
                StarSystem system;
                if (values.Count == 0)
                {
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else if (values.Count == 1 || string.IsNullOrEmpty(values[1].AsString) || values[1]?.AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
                {
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    // Named system
                    system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                }
                Body result = system?.bodies?.Find(v => v.bodyname?.ToLowerInvariant() == values[0].AsString?.ToLowerInvariant());
                return new ReflectionValue(result ?? new object());
            }, 1, 2);

            store["MissionDetails"] = new NativeFunction((values) =>
            {
                var missions = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.missions.ToList();

                Mission result = missions?.FirstOrDefault(v => v.missionid == values[0].AsNumber);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["RouteDetails"] = new NativeFunction((values) =>
            {
                CrimeMonitor crimeMonitor = (CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor");
                MaterialMonitor materialMonitor = (MaterialMonitor)EDDI.Instance.ObtainMonitor("Material monitor");
                int materialMonitorDistance = materialMonitor.maxStationDistanceFromStarLs ?? Constants.maxStationDistanceDefault;
                string result = null;
                string value = values[0].AsString;
                if (!string.IsNullOrEmpty(value))
                {
                    switch (value)
                    {
                        case "cancel":
                            {
                                NavigationService.Instance.CancelDestination();
                            }
                            break;
                        case "encoded":
                            {
                                result = NavigationService.Instance.GetServiceRoute("encoded", materialMonitorDistance);
                            }
                            break;
                        case "expiring":
                            {
                                result = NavigationService.Instance.GetExpiringRoute();
                            }
                            break;
                        case "facilitator":
                            {
                                int distance = crimeMonitor.maxStationDistanceFromStarLs ?? 10000;
                                bool isChecked = crimeMonitor.prioritizeOrbitalStations;
                                result = NavigationService.Instance.GetServiceRoute("facilitator", distance, isChecked);
                            }
                            break;
                        case "farthest":
                            {
                                result = NavigationService.Instance.GetFarthestRoute();
                            }
                            break;
                        case "guardian":
                            {
                                result = NavigationService.Instance.GetServiceRoute("guardian", materialMonitorDistance);
                            }
                            break;
                        case "human":
                            {
                                result = NavigationService.Instance.GetServiceRoute("human", materialMonitorDistance);
                            }
                            break;
                        case "manufactured":
                            {
                                result = NavigationService.Instance.GetServiceRoute("manufactured", materialMonitorDistance);
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
                                result = NavigationService.Instance.GetServiceRoute("raw", materialMonitorDistance);
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
                                    result = NavigationService.Instance.GetScoopRoute(values[1].AsNumber);
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
                                    result = NavigationService.Instance.SetDestination(values[1].AsString, values[2].AsString);
                                }
                                else if (values.Count == 2)
                                {
                                    result = NavigationService.Instance.SetDestination(values[1].AsString);
                                }
                                else
                                {
                                    result = NavigationService.Instance.SetDestination();
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
                return new ReflectionValue(result ?? new object());
            }, 1, 3);

            store["StationDetails"] = new NativeFunction((values) =>
            {
                Station result;
                if (values.Count == 0 || values[0]?.AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStation?.name?.ToLowerInvariant())
                {
                    result = EDDI.Instance.CurrentStation;
                }
                else
                {
                    StarSystem system;
                    if (values.Count == 1 || values[1]?.AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
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
                return new ReflectionValue(result ?? new object());
            }, 1, 2);

            store["FactionDetails"] = new NativeFunction((values) =>
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
                return new ReflectionValue(result ?? new object());
            }, 1, 2);

            store["SuperpowerDetails"] = new NativeFunction((values) =>
            {
                Superpower result = Superpower.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Superpower.FromNameOrEdName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["StateDetails"] = new NativeFunction((values) =>
            {
                FactionState result = FactionState.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FactionState.FromName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["EconomyDetails"] = new NativeFunction((values) =>
            {
                Economy result = Economy.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Economy.FromName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["EngineerDetails"] = new NativeFunction((values) =>
            {
                Engineer result = Engineer.FromName(values[0].AsString);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["GovernmentDetails"] = new NativeFunction((values) =>
            {
                Government result = Government.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Government.FromName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["SecurityLevelDetails"] = new NativeFunction((values) =>
            {
                SecurityLevel result = SecurityLevel.FromName(values[0].AsString);
                if (result == null)
                {
                    result = SecurityLevel.FromName(values[0].AsString);
                }
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["MaterialDetails"] = new NativeFunction((values) =>
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
                return new ReflectionValue(result ?? new object());
            }, 1, 2);

            store["CommodityMarketDetails"] = new NativeFunction((values) =>
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
                return new ReflectionValue(result ?? new object());
            }, 0, 3);

            store["CargoDetails"] = new NativeFunction((values) =>
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
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["HaulageDetails"] = new NativeFunction((values) =>
            {
                var result = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor"))?.GetHaulageWithMissionId((long)values[0].AsNumber);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["BlueprintDetails"] = new NativeFunction((values) =>
            {
                string blueprintName = values[0].AsString;
                int blueprintGrade = Convert.ToInt32(values[1].AsNumber);
                Blueprint result = Blueprint.FromNameAndGrade(blueprintName, blueprintGrade);
                return new ReflectionValue(result ?? new object());
            }, 2);

            store["TrafficDetails"] = new NativeFunction((values) =>
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
                return new ReflectionValue(result ?? new object());
            }, 1, 2);

            store["GalnetNewsArticle"] = new NativeFunction((values) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                return new ReflectionValue(result ?? new object());
            }, 1);

            store["GalnetNewsArticles"] = new NativeFunction((values) =>
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
                return new ReflectionValue(results ?? new List<News>());
            }, 0, 2);

            store["GalnetNewsMarkRead"] = new NativeFunction((values) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.MarkRead(result);
                }
                return "";
            }, 1);

            store["GalnetNewsMarkUnread"] = new NativeFunction((values) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.MarkUnread(result);
                }
                return "";
            }, 1);

            store["GalnetNewsDelete"] = new NativeFunction((values) =>
            {
                News result = GalnetSqLiteRepository.Instance.GetArticle(values[0].AsString);
                if (result != null)
                {
                    GalnetSqLiteRepository.Instance.DeleteNews(result);
                }
                return "";
            }, 1);

            store["Distance"] = new NativeFunction((values) =>
            {
                bool numVal = values[0].Type == Cottle.ValueContent.Number;
                bool stringVal = values[0].Type == Cottle.ValueContent.String;

                StarSystem curr = null;
                StarSystem dest = null;
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
                if (curr != null && dest != null)
                {
                    var result = curr.DistanceFromStarSystem(dest);
                    if (result is null)
                    {
                        return $"Unable to calculate distance between {curr.systemname} and {dest.systemname}. Could not obtain system coordinates.";
                    }
                    return new ReflectionValue(result);
                }
                else if (values.Count == 6 && numVal)
                {
                    var x1 = values[0].AsNumber;
                    var y1 = values[1].AsNumber;
                    var z1 = values[2].AsNumber;
                    var x2 = values[3].AsNumber;
                    var y2 = values[4].AsNumber;
                    var z2 = values[5].AsNumber;
                    var result = Functions.DistanceFromCoordinates(x1, y1, z1, x2, y2, z2);
                    return new ReflectionValue(result);
                }
                else
                {
                    return "The Distance function is used improperly. Please review the documentation for correct usage.";
                }
            }, 1, 6);

            store["Log"] = new NativeFunction((values) =>
            {
                Logging.Info(values[0].AsString);
                return "";
            }, 1);

            store["SetState"] = new NativeFunction((values) =>
            {
                string name = values[0].AsString.ToLowerInvariant().Replace(" ", "_");
                Cottle.Value value = values[1];
                if (value.Type == Cottle.ValueContent.Boolean)
                {
                    EDDI.Instance.State[name] = value.AsBoolean;
                    store["state"] = buildState();
                }
                else if (value.Type == Cottle.ValueContent.Number)
                {
                    EDDI.Instance.State[name] = value.AsNumber;
                    store["state"] = buildState();
                }
                else if (value.Type == Cottle.ValueContent.String)
                {
                    EDDI.Instance.State[name] = value.AsString;
                    store["state"] = buildState();
                }
                // Ignore other possibilities
                return "";
            }, 2);

            store["RefreshProfile"] = new NativeFunction((values) =>
            {
                bool stationRefresh = (values.Count != 0 && values[0].AsBoolean);
                EDDI.Instance.refreshProfile(stationRefresh);
                return "";
            }, 0, 1);

            store["InaraDetails"] = new NativeFunction((values) =>
            {
                if (values[0].AsString is string commanderName)
                {
                    if (!string.IsNullOrWhiteSpace(commanderName))
                    {
                        EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                        var result = inaraService.GetCommanderProfile(commanderName);
                        return new ReflectionValue(result ?? new object());
                    }
                }
                return "";
            }, 1);

            store["OrbitalVelocity"] = new NativeFunction((values) =>
            {
                Body body;
                decimal? altitudeMeters;
                if (values.Count == 0)
                {
                    altitudeMeters = (EDDI.Instance.ObtainMonitor("Status monitor") as StatusMonitor)?.currentStatus?.altitude;
                    body = EDDI.Instance.CurrentStellarBody;
                }
                else if (values.Count == 1 && values[0].AsNumber >= 0)
                {
                    altitudeMeters = values[0].AsNumber;
                    body = EDDI.Instance.CurrentStellarBody;
                }
                else if (values.Count == 2 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString))
                {
                    altitudeMeters = values[0].AsNumber;
                    body = EDDI.Instance.CurrentStarSystem?.bodies?
                        .FirstOrDefault(b => b.bodyname == values[1].AsString);
                }
                else if (values.Count == 3 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString) && !string.IsNullOrEmpty(values[2].AsString))
                {
                    altitudeMeters = values[0].AsNumber;
                    body = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString)?.bodies?
                        .FirstOrDefault(b => b.bodyname == values[1].AsString);
                }
                else
                {
                    return "The OrbitalVelocity function is used improperly. Please review the documentation for correct usage.";
                }
                if (altitudeMeters is null)
                {
                    return "Altitude not found.";
                }
                if (body is null)
                {
                    return "Body not found.";
                }
                return body?.GetOrbitalVelocityMetersPerSecond(altitudeMeters) ?? 0;
            }, 0, 3);

            // Variables
            foreach (KeyValuePair<string, Cottle.Value> entry in vars)
            {
                store[entry.Key] = entry.Value;
            }

            return store;
        }

        public static Dictionary<Cottle.Value, Cottle.Value> buildState()
        {
            if (EDDI.Instance.State == null)
            {
                return null;
            }

            Dictionary<Cottle.Value, Cottle.Value> state = new Dictionary<Cottle.Value, Cottle.Value>();
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
            Ship ship;
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
