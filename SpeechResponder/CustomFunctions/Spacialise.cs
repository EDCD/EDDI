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
        public string description => Properties.CustomFunctions_Untranslated.Spacialise;
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
