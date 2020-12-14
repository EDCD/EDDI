using Cottle.Functions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class Log : ICustomFunction
    {
        public string name => "Log";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.Log;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Logging.Info(values[0].AsString);
            return "";
        }, 1);
    }
}
