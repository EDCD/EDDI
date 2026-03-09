using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class NavRouteInfo
    {
        [JsonProperty]
        private DateTime timestamp { get; }

        [JsonProperty]
        public List<NavRouteInfoItem> Route { get; }

        public NavRouteInfo(DateTime timestamp, List<NavRouteInfoItem> route)
        {
            this.timestamp = timestamp;
            Route = route ?? new List<NavRouteInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp, bool isRouteExpected, 
            [CanBeNull] out NavRouteInfo info, [CanBeNull] out string rawRoute, 
            string filename = "NavRoute.json" )
        {
            info = null;
            rawRoute = null;

            var (raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<NavRouteInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( parsed?.Route != null &&
                 ( ( isRouteExpected && parsed.Route.Count > 0 ) || ( !isRouteExpected && parsed.Route.Count == 0 ) ) )
            {
                info = parsed;
                rawRoute = raw;
                return true;
            }

            return false;
        }
    }
}
