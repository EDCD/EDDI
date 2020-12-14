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
        public string description => Properties.CustomFunctions_Untranslated.ShipDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Ship result = ShipDefinitions.FromModel(values[0].AsString);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
