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
        public string description => @"
This function will attempt to provide phonetic pronunciation for the supplied text. This function uses SSML tags.

P() takes a single argument of the string for which to alter the pronunciation.

Common usage of this is to wrap the names of planets, powers, ships etc., for example:

    You are in the {P(system.name)} system.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            string val = values[0].AsString;
            bool useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            return Translations.GetTranslation(val, useICAO);
        }, 1);
    }
}
