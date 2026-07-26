using Cottle;
using EddiCore;
using EddiDataDefinitions;
using JetBrains.Annotations;
using System;
using System.Linq;

namespace EddiScriptResolverService.CustomFunctions
{
    [UsedImplicitly]
    public class OrbitalVelocity : ICustomFunction
    {
        public string name => "OrbitalVelocity";
        public FunctionCategory Category => FunctionCategory.Utility;
        public string description => Properties.CustomFunctions_Untranslated.OrbitalVelocity;
        public Type ReturnType => typeof( decimal? );

        public static decimal? currentAltitudeMeters { get; set; } = null;
        public IFunction function => Function.CreateNativeMinMax( ( runtime, values, writer ) =>
        {
            Body body;
            if (values.Count == 0)
            {
                body = EDDI.Instance.GameState.CurrentStellarBody;
            }
            else if (values is [ var value ] && value.AsNumber >= 0)
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = EDDI.Instance.GameState.CurrentStellarBody;
            }
            else if (values is [ var value1, _] && value1.AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString))
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = EDDI.Instance.GameState.CurrentStarSystem?.bodies?
                    .FirstOrDefault(b => b.bodyname == values[1].AsString);
            }
            else if (values is [ var b1, _, _] && b1.AsNumber >= 0 && !string.IsNullOrEmpty(values[1].AsString) && !string.IsNullOrEmpty(values[2].AsString))
            {
                currentAltitudeMeters = Convert.ToDecimal(values[0].AsNumber);
                body = EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync(values[2].AsString, true, false).GetAwaiter().GetResult()?.bodies?
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
