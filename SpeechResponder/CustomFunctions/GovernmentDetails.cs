using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class GovernmentDetails : ICustomFunction
    {
        public string name => "GovernmentDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a government given its name.

GovernmentDetails() takes a single argument of the government for which you want more information.

At current this does not have a lot of use as the government object only contains its name, but expect it to be expanded in future.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Government result = Government.FromName(values[0].AsString);
            if (result == null)
            {
                result = Government.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
