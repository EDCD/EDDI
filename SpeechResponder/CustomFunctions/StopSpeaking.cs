using Cottle;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StopSpeaking : ICustomFunction
    {
        public string name => "StopSpeaking";
        public FunctionCategory Category => FunctionCategory.Voice;
        public string description => Properties.CustomFunctions_Untranslated.StopSpeaking;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative0( ( _, _ ) =>
        {
            SpeechService.Instance.ShutUp();
            return "";
        });
    }
}
