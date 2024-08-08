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
    public class MissionDetails : ICustomFunction
    {
        public string name => "MissionDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.MissionDetails;
        public Type ReturnType => typeof( Mission );
        public IFunction function => Function.CreateNative1( ( runtime, missionID, writer ) =>
        {
            var missions = ConfigService.Instance.missionMonitorConfiguration?.missions.ToList();
            var result = missions?.FirstOrDefault(v => v.missionid == Convert.ToInt64( missionID.AsNumber ) );
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        });
    }
}
