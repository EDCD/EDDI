using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ICAO : ICustomFunction
    {
        public string name => "ICAO";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ICAO;
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
