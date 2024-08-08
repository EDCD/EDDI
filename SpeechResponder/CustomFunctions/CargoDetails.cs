using Cottle;
using EddiConfigService;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class CargoDetails : ICustomFunction
    {
        public string name => "CargoDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.CargoDetails;
        public Type ReturnType => typeof( Cargo );
        public IFunction function => Function.CreateNative1( ( runtime, input, writer ) =>
        {
            var cargoInventory = ConfigService.Instance.cargoMonitorConfiguration?.cargo;
            Cargo result = null;

            if (input.Type == ValueContent.String)
            {
                var edname = CommodityDefinition.FromNameOrEDName(input.AsString)?.edname;
                result = cargoInventory?.FirstOrDefault(c=> c.edname == edname) ?? new Cargo(edname);
            }
            else if (input.Type == ValueContent.Number)
            {
                result = cargoInventory?.FirstOrDefault(c => c.missionCargo.Any(h => h.Key == Convert.ToInt64(input.AsNumber)));
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
