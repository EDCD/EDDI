using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class SecurityLevelDetails : ICustomFunction
    {
        public string name => "SecurityLevelDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a security level given its name.

SecurityLevelDetails() takes a single argument of the security level for which you want more information.

At current this does not have a lot of use as the security level object only contains its name, but expect it to be expanded in future.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            SecurityLevel result = SecurityLevel.FromName(values[0].AsString);
            if (result == null)
            {
                result = SecurityLevel.FromName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
