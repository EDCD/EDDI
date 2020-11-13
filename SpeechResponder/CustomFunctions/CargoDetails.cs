using Cottle.Functions;
using Cottle.Values;
using EddiCargoMonitor;
using EddiCore;
using EddiDataDefinitions;
using EddiSpeechResponder.Service;

namespace EddiSpeechResponder.CustomFunctions
{
    public class CargoDetails : ICustomFunction
    {
        public string name => "CargoDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a cargo, carried in the commander's hold.

CargoDetails() takes one mandatory argument, of two possible forms. 
- The first form, a commodity name of the cargo. If the commodity is not in the hold, a 'null' is returned.
- The second form, a mission ID associated with the cargo, as haulage. If the mission ID is not associated with haulage, a 'null' is returned.

Common usage of this is to provide further information about a particular cargo, for example:

    {set cargo to CargoDetails(""Tea"")}
    {if cargo && cargo.total > 0: You have {cargo.total} tonne{if cargo.total != 1: s} of {cargo.name} in your cargo hold.}

or for a mission-related event,

    {set cargo to CargoDetails(event.missionid)}
    {if cargo: {cargo.total} tonne{if cargo.total != 1: s} of {cargo.name} is in your hold for this mission.}";
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
