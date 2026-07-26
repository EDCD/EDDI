using Cottle;
using EddiConfigService;
using EddiSpeechService.SpeechConversions;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
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
            var useICAO = ConfigService.Instance.speechServiceConfiguration.EnableIcao;
            return SpeechConversions.GetTranslation(val, useICAO, type);
        }, 1, 2);
    }
}
