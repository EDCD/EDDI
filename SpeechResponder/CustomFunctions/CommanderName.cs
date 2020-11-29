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
        public string description => @"
This function will provide the name of your commander.

If you have set up a phonetic name for your commander it will return that, otherwise if your commander name has been set it will return that. The phonetic name uses SSML tags.";
        public NativeFunction function => new NativeFunction((values) => EDDI.Instance.Cmdr.SpokenName(), 0, 0);
    }
}
