using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class StateDetails : ICustomFunction
    {
        public string name => "StateDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a state given its name.

StateDetails() takes a single argument of the state for which you want more information.

At current this does not have a lot of use as the state object only contains its name, but expect it to be expanded in future.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            FactionState result = FactionState.FromName(values[0].AsString);
            if (result == null)
            {
                result = FactionState.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
