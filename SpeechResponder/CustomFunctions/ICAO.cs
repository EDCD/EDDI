using Cottle;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ICAO : ICustomFunction
    {
        public string name => "ICAO";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.ICAO;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            // Turn a string in to an ICAO definition
            var value = input.AsString;
            if (string.IsNullOrEmpty(value)) { return ""; }

            // Translate to ICAO, removing anything that isn't alphanumeric
            return Translations.ICAO( value.ToUpperInvariant().Replace( "[^A-Z0-9]", "" ) );
        });
    }
}
