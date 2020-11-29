using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class EconomyDetails : ICustomFunction
    {
        public string name => "EconomyDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for an economy given its name.

EconomyDetails() takes a single argument of the economy for which you want more information.

At current this does not have a lot of use as the economy object only contains its name, but expect it to be expanded in future.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Economy result = Economy.FromName(values[0].AsString);
            if (result == null)
            {
                result = Economy.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
