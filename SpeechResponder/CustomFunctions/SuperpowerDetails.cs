using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SuperpowerDetails : ICustomFunction
    {
        public string name => "SuperpowerDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a superpower given its name.

SuperpowerDetails() takes a single argument of the superpower for which you want more information.

At current this does not have a lot of use as the superpower object only contains its name, but expect it to be expanded in future.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Superpower result = Superpower.FromName(values[0].AsString);
            if (result == null)
            {
                result = Superpower.FromNameOrEdName(values[0].AsString);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
