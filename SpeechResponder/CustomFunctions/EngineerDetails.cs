using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class EngineerDetails : ICustomFunction
    {
        public string name => "EngineerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for an Engineer given its name (including current progress information if you are in game).

EngineerDetails() takes a single argument of the engineer for which you want more information and returns an Engineer object.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Engineer result = Engineer.FromName(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
