using Cottle;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Log : ICustomFunction
    {
        public string name => "Log";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Log;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreatePure1( ( runtime, input ) =>
        {
            Logging.Info( input.AsString );
            return "";
        });
    }
}
