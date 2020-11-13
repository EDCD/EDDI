using System.Linq;
using Cottle.Functions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class StartsWithVowel : ICustomFunction
    {
        public string name => "StartsWithVowel";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function returns true or false depending on whether the first letter in a string is a vowel.

StartsWithVowel() takes one argument: the string that may or may not start with a vowel.

Common usage of this is to select the word that should proceed the string (e.g. **a** Adaptive Encryptors Capture vs **an** Adaptive Encryptors Capture).
               
    {if StartsWithVowel(event.name): an |else: a } {event.name}";
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
