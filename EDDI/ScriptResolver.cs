using Cottle.Documents;
using Cottle.Functions;
using Cottle.Stores;
using EliteDangerousSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //if (result.Contains("<phoneme"))
            //{
            //    // TODO obtain language of voice
            //    return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"" + synth.Voice.Culture.Name + "\"><s>" + result + "</s></speak>";
            //}
            //else
            //{
            //    return result;
            //}
            return result;
        }

        private BuiltinStore buildStore(Dictionary<string, Cottle.Value> vars)
        {
            BuiltinStore store = new BuiltinStore();
            // Translation functions
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
            store["Ship"] = new NativeFunction((values) =>
            {
                return Translations.ShipModel(values[0].AsString);
            }, 1);

            // Helper functions
            store["OneOf"] = new NativeFunction((values) =>
            {
                return values[random.Next(values.Count)];
            });

            // Variables
            foreach (KeyValuePair<string, Cottle.Value> entry in vars)
            {
                store[entry.Key] = entry.Value;
            }
            return store;
        }
    }
}
