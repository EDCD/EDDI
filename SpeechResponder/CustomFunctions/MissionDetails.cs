using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiMissionMonitor;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class MissionDetails : ICustomFunction
    {
        public string name => "MissionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MissionDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            var missions = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.missions.ToList();

            Mission result = missions?.FirstOrDefault(v => v.missionid == values[0].AsNumber);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
