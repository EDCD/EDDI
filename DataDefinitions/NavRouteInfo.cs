using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Utilities;

namespace EddiDataDefinitions
{
    public class NavRouteInfo
    {
        [JsonProperty]
        private string timestamp { get; }

        [JsonProperty]
        public List<NavRouteInfoItem> Route { get; }

        public NavRouteInfo(string timestamp, List<NavRouteInfoItem> route)
        {
            this.timestamp = timestamp;
            Route = route ?? new List<NavRouteInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile(DateTime timestamp, bool isRouteExpected, [CanBeNull] out NavRouteInfo info, [CanBeNull] out string rawRoute, string filename = "NavRoute.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawRoute = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawRoute))
                {
                    info = JsonConvert.DeserializeObject<NavRouteInfo>(rawRoute);
                }
                if (info?.Route != null && 
                    ((isRouteExpected && info.Route.Any()) || (!isRouteExpected && !info.Route.Any())))
                {
                    var navRouteDateTime = Dates.FromString(info.timestamp);
                    timeDiff = navRouteDateTime - timestamp;
                }
                attemptsRemaining--;
            } while ((timeDiff == null || timeDiff.Value.Duration().TotalSeconds >= 5) && attemptsRemaining > 0);

            return timeDiff != null && timeDiff.Value.Duration().TotalSeconds < 5;
        }
    }
}
