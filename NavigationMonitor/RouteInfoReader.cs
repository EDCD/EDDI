using EddiDataDefinitions;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiNavigationMonitor
{
    public class RouteInfoReader
    {
        public List<RouteInfo> Route { get; set; }

        public RouteInfoReader()
        {
            Route = new List<RouteInfo>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public static RouteInfoReader FromFile(string filename = null)
        {
            RouteInfoReader info = new RouteInfoReader();

            string data = Files.FromSavedGames("Route.json");
            if (data != null)
            {
                info = JsonConvert.DeserializeObject<RouteInfoReader>(data);
            }
            return info;
        }
    }
}