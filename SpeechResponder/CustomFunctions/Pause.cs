using Cottle;
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
        public IFunction function => Function.CreateNative1( ( runtime, milliseconds, writer ) => @"<break time=""" + Convert.ToInt64( milliseconds.AsNumber ) + @"ms"" />" );
    }
}
