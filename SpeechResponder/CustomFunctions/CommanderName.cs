using Cottle;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CommanderName : ICustomFunction
    {
        public string name => "CommanderName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.CommanderName;
        public Type ReturnType => typeof( string );
        public IFunction function => Function.CreateNative0( ( runtime, writer ) => EDDI.Instance.Cmdr?.SpokenName() );
    }
}
