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
        public string description => @"
This function will provide the orbital velocity in meters per second which is required to maintain orbit at the current altitude.

OrbitalVelocity() takes up to three arguments. If no arguments are provided, it'll try to return the velocity needed to maintain orbit around the current body at the current altitude. 
- The first optional argument is the altitude in meters to use for the calculation. If no other arguments are provided, the function will provide the orbital velocity relative to the current body.
- The second optional argument is the name of the body. If no third argument is provided, the function will provide the orbital velocity relative the named body in the current star system.
- The third optional argument is the name of the star system to search for the named body provided as the second argument.

Common usage of this is to provide the velocity in meters per second required to orbit a body, for example:

    {set velocity to OrbitalVelocity(status.altitude, ""Tethys"", ""Sol"")}
    Orbital velocity to orbit Tethys at the current altitude is {velocity} meters per second.";
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
