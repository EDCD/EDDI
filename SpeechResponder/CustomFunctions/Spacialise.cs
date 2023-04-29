using Cottle.Functions;
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
        public NativeFunction function => new NativeFunction((values) =>
        {
            if (values[0].AsString == null) { return ""; }
            bool useICAO = SpeechServiceConfiguration.FromFile().EnableIcao;
            return Translations.sayAsLettersOrNumbers(values[0].AsString, false, useICAO);
        }, 1);
    }
}
