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
        public static NavRouteInfo FromFile(string filename = null)
        {
            NavRouteInfo info = new NavRouteInfo();

            string data = Files.FromSavedGames("NavRoute.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<NavRouteInfo>(data);
            }
            return info;
        }
    }
}
