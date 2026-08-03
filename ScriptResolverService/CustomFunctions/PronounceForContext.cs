using Cottle;
using EddiConfigService;
using EddiSpeechService.SpeechConversions;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class PronounceForContext : ICustomFunction
    {
        public string name => "PronounceForContext";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.PronounceForContext;
        public Type ReturnType => typeof( string );
        public IFunction function => CreateFunction();

        internal static IFunction CreateFunction() => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            var val = values[ 0 ].AsString;
            var type = values.Count > 1 ? values[ 1 ].AsString : null;
            var useICAO = ConfigService.Instance.speechServiceConfiguration.EnableIcao;
            return SpeechConversions.GetTranslation( val, useICAO, type );
        }, 1, 2 );
    }
}
