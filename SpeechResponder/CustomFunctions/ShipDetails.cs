using Cottle.Functions;
using Cottle.Values;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class ShipDetails : ICustomFunction
    {
        public string name => "ShipDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a ship given its name.

ShipDetails() takes a single argument of the model of the ship for which you want more information.

Common usage of this is to provide further information about a ship, for example:

    The Vulture is made by {ShipDetails(""Vulture"").manufacturer}.";
        public NativeFunction function => new NativeFunction((values) =>
        {
            Ship result = ShipDefinitions.FromModel(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
