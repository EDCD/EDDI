using Cottle.Functions;
using Cottle.Values;
using EddiCargoMonitor;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CargoDetails : ICustomFunction
    {
        public string name => "CargoDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CargoDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            CargoMonitor cargoMonitor = (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
            Cottle.Value value = values[0];
            Cargo result = null;

            if (value.Type == Cottle.ValueContent.String)
            {
                var edname = CommodityDefinition.FromNameOrEDName(value.AsString)?.edname;
                result = cargoMonitor?.GetCargoWithEDName(edname);
            }
            else if (value.Type == Cottle.ValueContent.Number)
            {
                result = cargoMonitor?.GetCargoWithMissionId((long)value.AsNumber);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
