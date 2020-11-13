using Cottle.Functions;
using EddiSpeechResponder.Service;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    public class Log : ICustomFunction
    {
        public string name => "Log";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => @"
This function will write the supplied text to EDDI's log.

Log() takes a single argument of the string to log.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Logging.Info(values[0].AsString);
            return "";
        }, 1);
    }
}
