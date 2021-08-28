using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiNavigationMonitor
{
    public class NavRouteInfoReader
    {
        public List<NavWaypoint> Route { get; set; }

        public NavRouteInfoReader()
        {
            Route = new List<NavWaypoint>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static NavRouteInfoReader FromFile(string filename = null)
        {
            NavRouteInfoReader info = new NavRouteInfoReader();

            string data = Files.FromSavedGames("NavRoute.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<NavRouteInfoReader>(data);
            }
            return info;
        }
    }
}
