using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Pause : ICustomFunction
    {
        public string name => "Pause";
        public FunctionCategory Category => FunctionCategory.Tempo;
        public string description => Properties.CustomFunctions_Untranslated.Pause;
        public Type ReturnType => typeof( string );
        public NativeFunction function => new NativeFunction((values) =>
        {
            return @"<break time=""" + values[0].AsNumber + @"ms"" />";
        }, 1);
    }
}
