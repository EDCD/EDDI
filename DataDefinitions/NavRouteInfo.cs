using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class NavRouteInfo
    {
        public string timestamp { get; set; }
        public List<NavRouteInfoItem> Route { get; set; }

        public NavRouteInfo()
        {
            Route = new List<NavRouteInfoItem>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static void FromFile(DateTime timestamp, out NavRouteInfo info, out string rawRoute, string filename = null)
        {
            info = new NavRouteInfo();

            rawRoute = Files.FromSavedGames("NavRoute.json");
            if (rawRoute != null)
            {
                info = JsonConvert.DeserializeObject<NavRouteInfo>(rawRoute);
            }
        }

        public static bool TryFromFile(DateTime timestamp, bool isRouteExpected, out NavRouteInfo info, out string rawRoute, string filename = null)
        {
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                FromFile(timestamp, out info, out rawRoute);
                if ((isRouteExpected && info.Route.Any()) || (!isRouteExpected && !info.Route.Any()))
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
