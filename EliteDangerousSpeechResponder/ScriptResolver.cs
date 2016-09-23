using Cottle.Builtins;
using Cottle.Documents;
using Cottle.Functions;
using Cottle.Settings;
using Cottle.Stores;
using Cottle.Values;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;
using EliteDangerousSpeechService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    public class ScriptResolver
    {
        private Dictionary<string, Script> scripts = new Dictionary<string, Script>();
        private Random random;
        private CustomSetting setting;

        public ScriptResolver(Dictionary<string, Script> scripts)
        {
            random = new Random();
            if (scripts != null) { this.scripts = scripts; }
            setting = new CustomSetting();
            setting.Trimmer = BuiltinTrimmers.CollapseBlankCharacters;
        }

        public string resolve(string name, Dictionary<string, Cottle.Value> vars)
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

            return resolveScript(script.Value, vars);
        }

        public string resolveScript(string script, Dictionary<string, Cottle.Value> vars)
        {
            try
            {
                var document = new SimpleDocument(script, setting);
                var result = document.Render(buildStore(vars));
                Logging.Debug("Turned script " + script + " in to speech " + result);
                return result;
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to resolve script: " + e.Message);
                return "There is a problem with the script: " + e.Message;
            }
        }

        private BuiltinStore buildStore(Dictionary<string, Cottle.Value> vars)
        {
            BuiltinStore store = new BuiltinStore();

            // Function to call another script
            store["F"] = new NativeFunction((values) =>
            {
                return new ScriptResolver(scripts).resolve(values[0].AsString, vars);
            }, 1);

            // Translation functions
            store["P"] = new NativeFunction((values) =>
            {
                string val = values[0].AsString;
                string translation = val;
                if (translation == val)
                {
                    translation = Translations.ShipModel(val);
                }
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
                return new ScriptResolver(scripts).resolveScript(values[random.Next(values.Count)].AsString, vars);
            });

            // Helper functions
            store["Occasionally"] = new NativeFunction((values) =>
            {
                if (random.Next((int)values[0].AsNumber) == 0)
                {
                    return new ScriptResolver(scripts).resolveScript(values[1].AsString, vars);
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
                return new ReflectionValue(result);
            }, 1);

            store["CombatRatingDetails"] = new NativeFunction((values) =>
            {
                CombatRating result = CombatRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = CombatRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["TradeRatingDetails"] = new NativeFunction((values) =>
            {
                TradeRating result = TradeRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = TradeRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["ExplorationRatingDetails"] = new NativeFunction((values) =>
            {
                ExplorationRating result = ExplorationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = ExplorationRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["EmpireRatingDetails"] = new NativeFunction((values) =>
            {
                EmpireRating result = EmpireRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = EmpireRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["FederationRatingDetails"] = new NativeFunction((values) =>
            {
                FederationRating result = FederationRating.FromName(values[0].AsString);
                if (result == null)
                {
                    result = FederationRating.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["SystemDetails"] = new NativeFunction((values) =>
            {
                StarSystem result = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(values[0].AsString, true);
                return new ReflectionValue(result);
            }, 1);
        
            store["SuperpowerDetails"] = new NativeFunction((values) =>
            {
                Superpower result = Superpower.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Superpower.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["StateDetails"] = new NativeFunction((values) =>
            {
                State result = State.FromName(values[0].AsString);
                if (result == null)
                {
                    result = State.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["EconomyDetails"] = new NativeFunction((values) =>
            {
                Economy result = Economy.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Economy.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["GovernmentDetails"] = new NativeFunction((values) =>
            {
                Government result = Government.FromName(values[0].AsString);
                if (result == null)
                {
                    result = Government.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
            }, 1);

            store["SecurityLevelDetails"] = new NativeFunction((values) =>
            {
                SecurityLevel result = SecurityLevel.FromName(values[0].AsString);
                if (result == null)
                {
                    result = SecurityLevel.FromEDName(values[0].AsString);
                }
                return new ReflectionValue(result);
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
