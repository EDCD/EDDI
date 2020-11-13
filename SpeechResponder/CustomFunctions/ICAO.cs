using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    public class ICAO : ICustomFunction
    {
        public string name => "ICAO";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => @"
This function will turn its argument into an ICAO spoken value, for example ""NCC"" becomes ""November Charlie Charlie"".

ICAO() takes one argument: the value to turn in to ICAO.

Common usage of this is to provide clear callsigns and idents for ships, for example:

    Ship ident is {ICAO(ship.ident)}.";
        public NativeFunction function => new NativeFunction((values) =>
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
    }
}
