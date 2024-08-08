using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Reflection;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StationDetails : ICustomFunction
    {
        public string name => "StationDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.StationDetails;
        public Type ReturnType => typeof( Station );
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Station result;
            if (values.Count == 0 || (values.Count > 0 && values[0].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStation?.name?.ToLowerInvariant()))
            {
                result = EDDI.Instance.CurrentStation;
            }
            else
            {
                StarSystem system;
                if (values.Count == 1 || (values.Count > 1 && values[1].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant()))
                {
                    // Current system
                    system = EDDI.Instance.CurrentStarSystem;
                }
                else
                {
                    // Named system
                    system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[1].AsString, true);
                }
                result = system != null && system.stations != null ? system.stations.FirstOrDefault(v => v.name?.ToLowerInvariant() == values[0].AsString.ToLowerInvariant()) : null;
            }
            return result is null ? Value.EmptyMap : Value.FromReflection( result, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        }, 1, 2);
    }
}
