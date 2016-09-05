using Cottle.Documents;
using Cottle.Functions;
using Cottle.Stores;
using EliteDangerousSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EDDI
{
    public class ScriptResolver
    {
        private Random random;
        public ScriptResolver()
        {
            random = new Random();
        }

        public string resolve(string script, Dictionary<string, Cottle.Value> vars)
        {
            var document = new SimpleDocument(script);
            var result = document.Render(buildStore(vars));
            Logging.Debug("Turned script " + script + " in to speech " + result);
            return result;
        }

        private BuiltinStore buildStore(Dictionary<string, Cottle.Value> vars)
        {
            BuiltinStore store = new BuiltinStore();
            // Translation functions
            store["P"] = new NativeFunction((values) =>
            {
                string val = values[0].AsString;
                string translation = val;
                if (translation == val)
                {
                    translation = Translations.StarSystem(val);
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
                    translation = Translations.ShipModel(val);
                }
                return translation;
            }, 1);
            store["Faction"] = new NativeFunction((values) =>
            {
                return Translations.Faction(values[0].AsString);
            }, 1);
            store["System"] = new NativeFunction((values) =>
            {
                return Translations.StarSystem(values[0].AsString);
            }, 1);
            store["Power"] = new NativeFunction((values) =>
            {
                return Translations.Power(values[0].AsString);
            }, 1);
            store["Callsign"] = new NativeFunction((values) =>
            {
                return Translations.CallSign(values[0].AsString);
            }, 1);
            store["ShipModel"] = new NativeFunction((values) =>
            {
                return Translations.ShipModel(values[0].AsString);
            }, 1);

            // Helper functions
            store["OneOf"] = new NativeFunction((values) =>
            {
                return values[random.Next(values.Count)];
            });

            store["Humanise"] = new NativeFunction((values) =>
            {
                return Translations.humanize(values[0].AsNumber);
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
