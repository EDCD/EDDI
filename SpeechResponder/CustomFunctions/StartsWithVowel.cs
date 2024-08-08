using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StartsWithVowel : ICustomFunction
    {
        public string name => "StartsWithVowel";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.StartsWithVowel;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, input) =>
        {
            if (string.IsNullOrEmpty( input.AsString )) { return ""; }

            char[] vowels = { 'a', 'à', 'â', 'ä', 'e', 'ê', 'é', 'è', 'ë', 'i', 'î', 'ï', 'o', 'ô', 'ö', 'u', 'ù', 'û', 'ü', 'œ' };
            var firstCharacter = input.AsString.ToLower().ToCharArray().ElementAt(0);
            return vowels.Contains( firstCharacter );
        });
    }
}
