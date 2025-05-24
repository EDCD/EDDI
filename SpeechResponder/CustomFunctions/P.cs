using Cottle;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using EddiSpeechService.SpeechConversions;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class P : ICustomFunction
    {
        public string name => "P";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.P;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer) =>
        {
            var val = values[0].AsString;
            var type = values.Count > 1 ? values[1].AsString : null;
            var useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            return SpeechConversions.GetTranslation(val, useICAO, type);
        }, 1, 2);
    }
}
