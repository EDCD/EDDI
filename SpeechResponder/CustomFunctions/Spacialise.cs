using Cottle;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Spacialise : ICustomFunction
    {
        public string name => "Spacialise";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Spacialise;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            if ( string.IsNullOrEmpty( input.AsString ) ) { return ""; }
            var useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            return Translations.sayAsLettersOrNumbers( input.AsString, false, useICAO );
        });
    }
}
