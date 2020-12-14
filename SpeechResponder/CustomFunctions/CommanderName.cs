using Cottle.Functions;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CommanderName : ICustomFunction
    {
        public string name => "CommanderName";
        public FunctionCategory Category => FunctionCategory.Phonetic;
        public string description => Properties.CustomFunctions_Untranslated.CommanderName;
        public NativeFunction function => new NativeFunction((values) => EDDI.Instance.Cmdr.SpokenName(), 0, 0);
    }
}
