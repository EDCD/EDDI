using Cottle.Functions;
using Cottle.Values;
using EddiCargoMonitor;
using EddiCore;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class HaulageDetails : ICustomFunction
    {
        public string name => "HaulageDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide 'haulage' information for a mission-related cargo. See the 'haulage' object for variable details.

HaulageDetails() takes one mandatory argument, a mission ID associated with the haulage. If the mission ID is not associated with haulage, a 'null' is returned.

Common usage of this is to provide further information about a particular mission haulage, for example:

    {set haulage to HaulageDetails(event.missionid)}
    {if haulage && haulage.deleivered > 0:
        {set total to haulage.amount + haulage.deleivered}
	    {haulage.type} mission to the cargo depot is {round(haulage.delivered / total * 100, 0)} percent complete.
    }";
        public NativeFunction function => new NativeFunction((values) =>
        {
            var result = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor"))?.GetHaulageWithMissionId((long)values[0].AsNumber);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
