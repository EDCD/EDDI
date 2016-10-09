using Cottle.Builtins;
using Cottle.Documents;
using Cottle.Functions;
using Cottle.Settings;
using Cottle.Stores;
using Cottle.Values;
using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechResponder
{
    public class ScriptResolver
    {
        private Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        private Random random;
        private CustomSetting setting;

        public static object Instance { get; set; }

        public ScriptResolver(Dictionary<string, Script> scripts)
        {
            random = new Random();
            if (scripts != null) { this.scripts = scripts; }
            setting = new CustomSetting();
            setting.Trimmer = BuiltinTrimmers.CollapseBlankCharacters;
        }

        public string resolve(string name, Dictionary<string, Cottle.Value> vars)
        {
            return resolve(name, buildStore(vars));
        }

        public int priority(string name)
        {
            Script script;
            scripts.TryGetValue(name, out script);
            return (script == null ? 5 : script.Priority);
        }

        public string resolve(string name, BuiltinStore store)
        {
            Logging.Debug("Resolving script " + name);
            Script script;
            scripts.TryGetValue(name, out script);
            if (script == null || script.Value == null)
            {
                Logging.Debug("No script");
                return null;
            }
            Logging.Debug("Found script");

            return resolveScript(script.Value, store);
        }

        /// <summary>
        /// Resolve a script with an existing store
        /// </summary>
        public string resolveScript(string script, BuiltinStore store)
        {
            try
            {
                var document = new SimpleDocument(script, setting);
                var result = document.Render(store);
                Logging.Debug("Turned script " + script + " in to speech " + result);
                return result.Trim() == "" ? null : result.Trim();
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to resolve script: " + e.ToString());
                return "There is a problem with the script: " + e.Message.Replace("'", "");
            }
        }

        /// <summary>
        /// Build a store from a list of variables
        /// </summary>
        private BuiltinStore buildStore(Dictionary<string, Cottle.Value> vars)
        {
            BuiltinStore store = new BuiltinStore();

            // Function to call another script
            store["F"] = new NativeFunction((values) =>
            {
                return new ScriptResolver(scripts).resolve(values[0].AsString, store);
            }, 1);

            // Translation functions
            store["P"] = new NativeFunction((values) =>
            {
                string val = values[0].AsString;
                string translation = val;
                if (translation == val)
                {
                    translation = Translations.Faction(val);
                }
                if (translation == val)
                {
                    translation = Translations.Power(val);
                }
                if (translation == val)
                {
                    translation = Translations.StarSystem(val);
                }
                return translation;
            }, 1);

            // Helper functions
            store["OneOf"] = new NativeFunction((values) =>
            {
                return new ScriptResolver(scripts).resolveScript(values[random.Next(values.Count)].AsString, store);
            });

            store["Occasionally"] = new NativeFunction((values) =>
            {
                if (random.Next((int)values[0].AsNumber) == 0)
                {
                    return new ScriptResolver(scripts).resolveScript(values[1].AsString, store);
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

            store["Pause"] = new NativeFunction((values) =>
            {
                return @"<break time =""" + values[0].AsNumber + @"ms"" />";
            }, 1);

            //
            // Commander-specific functions
            //
            store["ShipName"] = new NativeFunction((values) =>
            {
                Ship ship = null;
                if (values.Count == 0)
                {
                    ship = EDDI.Instance.Ship;
                }
                else if (values.Count == 1)
                {
                    int shipId = (int)values[0].AsNumber;
                    ship = EDDI.Instance.Shipyard.FirstOrDefault(v => v.LocalId == shipId);
                }
                string result = (ship == null ? "your ship" : ship.SpokenName());
                return result;
            }, 0, 1);

            store["ShipCallsign"] = new NativeFunction((values) =>
            {
                Ship ship = null;
                if (values.Count == 0)
                {
                    ship = EDDI.Instance.Ship;
                }
                else if (values.Count == 1)
                {
                    int shipId = (int)values[0].AsNumber;
                    ship = EDDI.Instance.Shipyard.FirstOrDefault(v => v.LocalId == shipId);
                }

                string result;
                if (ship != null)
                {
                    if (EDDI.Instance.Cmdr != null)
                    {
                        // Obtain the first three characters
                        string chars = new Regex("[^a-zA-Z0-9]").Replace(EDDI.Instance.Cmdr.name, "").ToUpperInvariant().Substring(0, 3);
                        result = Translations.Manufacturer(ship.manufacturer) + " " + Translations.CallSign(chars);
                    }
                    else
                    {
                        result = "unidentified " + Translations.Manufacturer(ship.manufacturer) + " ship";
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

            store["ShipDetails"] = new NativeFunction((values) =>
            {
                Ship result = ShipDefinitions.FromModel(values[0].AsString);
                if (result == null)
                {
                    result = ShipDefinitions.FromEDModel(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["CombatRatingDetails"] = new NativeFunction((values) =>
            {
                CombatRating result = CombatRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = CombatRating.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["TradeRatingDetails"] = new NativeFunction((values) =>
            {
                TradeRating result = TradeRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = TradeRating.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["ExplorationRatingDetails"] = new NativeFunction((values) =>
            {
                ExplorationRating result = ExplorationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = ExplorationRating.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["EmpireRatingDetails"] = new NativeFunction((values) =>
            {
                EmpireRating result = EmpireRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = EmpireRating.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["FederationRatingDetails"] = new NativeFunction((values) =>
            {
                FederationRating result = FederationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FederationRating.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["SystemDetails"] = new NativeFunction((values) =>
            {
                StarSystem result = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(values[0].AsString, true);
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["StationDetails"] = new NativeFunction((values) =>
            {
                if (values.Count == 0)
                {
                    return null;
                }
                StarSystem system;
                if (values.Count == 1)
                {
                    // Current system
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    system = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(values[1].AsString, true);
                }
                Station result = system.stations.FirstOrDefault(v => v.name == values[0].AsString);
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1, 2);

            store["SuperpowerDetails"] = new NativeFunction((values) =>
            {
                Superpower result = Superpower.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Superpower.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["StateDetails"] = new NativeFunction((values) =>
            {
                State result = State.FromName(values[0].AsString);
                if (result == null)
                {
                    result = State.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["EconomyDetails"] = new NativeFunction((values) =>
            {
                Economy result = Economy.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Economy.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["GovernmentDetails"] = new NativeFunction((values) =>
            {
                Government result = Government.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Government.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["SecurityLevelDetails"] = new NativeFunction((values) =>
            {
                SecurityLevel result = SecurityLevel.FromName(values[0].AsString);
                if (result == null)
                {
                    result = SecurityLevel.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            store["MaterialDetails"] = new NativeFunction((values) =>
            {
                Material result = Material.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Material.FromEDName(values[0].AsString);
                }
                return (result == null ? new ReflectionValue(new object()) : new ReflectionValue(result));
            }, 1);

            // Variables
            foreach (KeyValuePair<string, Cottle.Value> entry in vars)
            {
                store[entry.Key] = entry.Value;
            }

            return store;
        }
    }
}
