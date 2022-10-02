using Cottle.Functions;
using Cottle.Values;
using EddiConfigService;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Linq;

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
            var cargo = ConfigService.Instance.cargoMonitorConfiguration?.cargo;
            Cottle.Value value = values[0];
            Cargo result = null;

            if (value.Type == Cottle.ValueContent.String)
            {
                var edname = CommodityDefinition.FromNameOrEDName(value.AsString)?.edname;
                result = cargo?.FirstOrDefault(c=> c.edname == edname);
            }
            else if (value.Type == Cottle.ValueContent.Number)
            {
                result = cargo?.FirstOrDefault(c => c.haulageData.FirstOrDefault(h => h.missionid == (long)value.AsNumber) != null);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
