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
    public class MissionDetails : ICustomFunction
    {
        public string name => "MissionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MissionDetails;
        public Type ReturnType => typeof( Mission );
        public NativeFunction function => new NativeFunction((values) =>
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration?.missions.ToList();
            var result = missions?.FirstOrDefault(v => v.missionid == values[0].AsNumber);
            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
