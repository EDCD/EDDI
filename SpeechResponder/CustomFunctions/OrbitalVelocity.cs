using Cottle.Functions;
using EddiSpeechResponder.Service;
using System.Linq;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiStatusMonitor;
using JetBrains.Annotations;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class OrbitalVelocity : ICustomFunction
    {
        public string name => "OrbitalVelocity";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.OrbitalVelocity;
        public NativeFunction function => new NativeFunction((values) =>
        {
            Body body;
            decimal? altitudeMeters;
            if (values.Count == 0)
            {
                altitudeMeters = (EDDI.Instance.ObtainMonitor("Status monitor") as StatusMonitor)?.currentStatus?.altitude;
                body = EDDI.Instance.CurrentStellarBody;
            }
            else if (values.Count == 1 && values[0].AsNumber >= 0)
            {
                altitudeMeters = values[0].AsNumber;
                body = EDDI.Instance.CurrentStellarBody;
            }
            else if (values.Count == 2 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString))
            {
                altitudeMeters = values[0].AsNumber;
                body = EDDI.Instance.CurrentStarSystem?.bodies?
                    .FirstOrDefault(b => b.bodyname == values[1].AsString);
            }
            else if (values.Count == 3 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString) && !string.IsNullOrEmpty(values[2].AsString))
            {
                altitudeMeters = values[0].AsNumber;
                body = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString)?.bodies?
                    .FirstOrDefault(b => b.bodyname == values[1].AsString);
            }
            else
            {
                return "The OrbitalVelocity function is used improperly. Please review the documentation for correct usage.";
            }
            if (altitudeMeters is null)
            {
                return "Altitude not found.";
            }
            if (body is null)
            {
                return "Body not found.";
            }
            return body?.GetOrbitalVelocityMetersPerSecond(altitudeMeters) ?? 0;
        }, 0, 3);
    }
}
