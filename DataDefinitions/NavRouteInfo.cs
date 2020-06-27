using Newtonsoft.Json;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class NavRouteInfo
    {
        [JsonProperty]
        public string starSystem { get; set; }

        [JsonProperty]
        public long? systemAddress { get; set; }

        [JsonProperty]
        private List<decimal> starPos { get; set; }

        [JsonProperty]
        public string starClass { get; set; }

        public decimal? x => starPos?[0];
        public decimal? y => starPos?[1];
        public decimal? z => starPos?[2];

        // Default Constructor
        public NavRouteInfo()
        { }

        // Copy Constructor
        public NavRouteInfo(NavRouteInfo NavRouteInfo)
        {
            this.starSystem = NavRouteInfo.starSystem;
            this.systemAddress = NavRouteInfo.systemAddress;
            this.starPos = NavRouteInfo.starPos;
            this.starClass = NavRouteInfo.starClass;
        }

        // Main Constructor
        public NavRouteInfo(string StarSystem, long? SystemAddress, List<decimal> StarPos, string StarClass)
        {
            this.starSystem = StarSystem;
            this.systemAddress = SystemAddress;
            this.starPos = StarPos;
            this.starClass = StarClass;
        }
    }
}