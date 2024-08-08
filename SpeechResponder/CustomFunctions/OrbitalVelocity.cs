using Cottle;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class OrbitalVelocity : ICustomFunction
    {
        public string name => "OrbitalVelocity";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.OrbitalVelocity;
        public Type ReturnType => typeof( decimal? );
        public static decimal? currentAltitudeMeters = null;
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Body body;
            if (values.Count == 0)
            {
                body = EDDI.Instance.CurrentStellarBody;
            }
            else if (values.Count == 1 && values[0].AsNumber >= 0)
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = EDDI.Instance.CurrentStellarBody;
            }
            else if (values.Count == 2 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString))
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = EDDI.Instance.CurrentStarSystem?.bodies?
                    .FirstOrDefault(b => b.bodyname == values[1].AsString);
            }
            else if (values.Count == 3 && values[0].AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString) && !string.IsNullOrEmpty(values[2].AsString))
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(values[2].AsString)?.bodies?
                    .FirstOrDefault(b => b.bodyname == values[1].AsString);
            }
            else
            {
                return "The OrbitalVelocity function is used improperly. Please review the documentation for correct usage.";
            }
            if ( currentAltitudeMeters is null)
            {
                return "Altitude not found.";
            }
            if (body is null)
            {
                return "Body not found.";
            }
            return body.GetOrbitalVelocityMetersPerSecond(currentAltitudeMeters) ?? 0;
        }, 0, 3);
    }
}
