using Cottle;
using EddiConfigService;
using EddiSpeechService.SpeechConversions;
using JetBrains.Annotations;
using System;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class SpellOut : ICustomFunction
    {
        public string name => "SpellOut";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.SpellOut;
        public Type ReturnType => typeof( string );
        public IFunction function => CreateFunction();

        internal static IFunction CreateFunction() => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            if ( string.IsNullOrEmpty( input.AsString ) ) { return ""; }
            var useICAO = ConfigService.Instance.speechServiceConfiguration.EnableIcao;
            return SpeechConversions.sayAsLettersOrNumbers( input.AsString, false, useICAO );
        } );
    }
}
