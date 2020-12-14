using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using Utilities;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class SystemDetails : ICustomFunction
    {
        public string name => "SystemDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => Properties.CustomFunctions_Untranslated.SystemDetails;
        public NativeFunction function => new NativeFunction((values) =>
        {
            StarSystem result;
            if (values.Count == 0)
            {
                result = EDDI.Instance.CurrentStarSystem;
            }
            else if (values.Count > 0 && values[0].AsString?.ToLowerInvariant() == EDDI.Instance.CurrentStarSystem?.systemname?.ToLowerInvariant())
            {
                result = EDDI.Instance.CurrentStarSystem;
            }
            else
            {
                result = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[0].AsString, true);
            }

            var distanceFromHome = result?.DistanceFromStarSystem(EDDI.Instance.HomeStarSystem);
            if (distanceFromHome != null)
            {
                Logging.Debug("Distance from home is " + distanceFromHome);
                result.distancefromhome = distanceFromHome;
            }

            return new ReflectionValue(result ?? new object());
        }, 1);
    }
}
