using Cottle;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Humanise : ICustomFunction
    {
        public string name => "Humanise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Humanise;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) => Translations.Humanize( (decimal?)Convert.ToDecimal( input.AsNumber ) ) );
    }
}
