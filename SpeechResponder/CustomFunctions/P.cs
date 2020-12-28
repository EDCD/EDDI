using Cottle.Functions;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class P : ICustomFunction
    {
        public string name => "P";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.P;
        public NativeFunction function => new NativeFunction((values) =>
        {
            string val = values[0].AsString;
            string type = values.Count > 1 ? values[1].AsString : null;
            bool useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            return Translations.GetTranslation(val, useICAO, type);
        }, 1, 2);
    }
}
