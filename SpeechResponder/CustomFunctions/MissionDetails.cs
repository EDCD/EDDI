using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiMissionMonitor;
using EddiSpeechResponder.Service;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    public class MissionDetails : ICustomFunction
    {
        public string name => "MissionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a mission given its mission ID.

MissionDetails() takes a single argument of the mission ID for which you want more information.

Common usage of this is to provide detailed information about a previously accepted mission, for example:

    {set mission to MissionDetails(event.missionid)}";
        public NativeFunction function => new NativeFunction((values) =>
        {
            var missions = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.missions.ToList();

            Mission result = missions?.FirstOrDefault(v => v.missionid == values[0].AsNumber);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
