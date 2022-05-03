using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class NavRouteInfo
    {
        public List<NavRouteInfoItem> Route { get; set; }

        public NavRouteInfo()
        {
            Route = new List<NavRouteInfoItem>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static void FromFile(out NavRouteInfo info, out string rawRoute, string filename = null)
        {
            info = new NavRouteInfo();

            rawRoute = Files.FromSavedGames("NavRoute.json");
            if (rawRoute != null)
            {
                info = JsonConvert.DeserializeObject<NavRouteInfo>(rawRoute);
            }
        }
    }
}
