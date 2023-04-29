using Cottle.Functions;
using Cottle.Values;
using EddiConfigService;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CargoDetails : ICustomFunction
    {
        public string name => "CargoDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CargoDetails;
        public Type ReturnType => typeof( Cargo );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var cargoInventory = ConfigService.Instance.cargoMonitorConfiguration?.cargo;
            Cottle.Value value = values[0];
            Cargo result = null;

            if (value.Type == Cottle.ValueContent.String)
            {
                var edname = CommodityDefinition.FromNameOrEDName(value.AsString)?.edname;
                result = cargoInventory?.FirstOrDefault(c=> c.edname == edname) ?? new Cargo(edname);
            }
            else if (value.Type == Cottle.ValueContent.Number)
            {
                result = cargoInventory?.FirstOrDefault(c => c.haulageData.FirstOrDefault(h => h.missionid == (long)value.AsNumber) != null);
            }
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
