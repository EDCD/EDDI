using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Spacialise : ICustomFunction
    {
        public string name => "Spacialise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function will allow letters and numbers in a string to be pronounced individually. If SSML is enabled, this function will render the text using SSML. If not, it will add spaces between letters in a string & convert to uppercase to assist the voice with achieving the proper pronunciation. 

Spacialise() takes one argument: the string of characters to Spacialise.

Common usage of this is to provide a more human-sounding reading of a string of letters that are not a part of known word:

    Star luminosity class: {Spacialise(event.luminosityclass)}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values[0].AsString == null) { return ""; }

            bool useSSML = !SpeechServiceConfiguration.FromFile().DisableSsml;
            if (useSSML)
            {
                return Translations.sayAsLettersOrNumbers(values[0].AsString);
            }
            else
            {
                string Entree = values[0].AsString;
                if (Entree == "")
                { return ""; }
                string Sortie = "";
                foreach (char c in Entree)
                {
                    Sortie = Sortie + c + " ";
                }
                var UpperSortie = Sortie.ToUpper();
                return UpperSortie.Trim();
            }
        }, 1);
    }
}
