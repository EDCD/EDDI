using Cottle.Functions;
using Cottle.Values;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiSpeechResponder.Service;
using JetBrains.Annotations;
using System.Linq;

namespace EddiSpeechResponder.CustomFunctions
{
    [UsedImplicitly]
    public class StationDetails : ICustomFunction
    {
        public string name => "StationDetails";
        public FunctionCategory Category => FunctionCategory.Details;
        public string description => @"
This function will provide full information for a station given its name and optional system.

StationDetails() takes a single mandatory argument of the name of the station for which you want more information.  If the station is not in the current system then it can be provided with a second parameter of the name of the system.

Common usage of this is to provide further information about a station, for example:

    {set station to StationDetails(""Jameson Memorial"", ""Shinrarta Dezhra"")}
    Jameson Memorial is {station.distancefromstar} light years from the system's main star.";
        public NativeFunction function => new NativeFunction((values) =>
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
                result = system != null && system.stations != null ? system.stations.FirstOrDefault(v => v.name.ToLowerInvariant() == values[0].AsString.ToLowerInvariant()) : null;
            }
            return new ReflectionValue(result ?? new object());
        }, 1, 2);
    }
}
