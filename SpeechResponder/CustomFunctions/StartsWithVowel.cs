using System.Linq;
using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StartsWithVowel : ICustomFunction
    {
        public string name => "StartsWithVowel";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.StartsWithVowel;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string Entree = values[0].AsString;
            if (Entree == "")
            { return ""; }

            char[] vowels = { 'a', 'à', 'â', 'ä', 'e', 'ê', 'é', 'è', 'ë', 'i', 'î', 'ï', 'o', 'ô', 'ö', 'u', 'ù', 'û', 'ü', 'œ' };
            char firstCharacter = Entree.ToLower().ToCharArray().ElementAt(0);
            bool result = vowels.Contains(firstCharacter);

            return result;

        }, 1);
    }
}
