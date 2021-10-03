using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class NavRouteInfoItem
    {
        [JsonProperty("StarSystem")]
        public string systemname { get; set; }

        [JsonIgnore, Obsolete("Please use systemname instead.")]
        public string name => systemname;

        [JsonProperty("SystemAddress")]
        public long? systemAddress { get; set; }

        [JsonProperty("StarPos")]
        private List<decimal> starPos { get; set; }

        [JsonProperty("StarClass")]
        public string stellarclass { get; set; }

        public decimal? x => starPos?[0];
        public decimal? y => starPos?[1];
        public decimal? z => starPos?[2];

        // Default Constructor
        public NavRouteInfoItem()
        { }

        // Copy Constructor
        public NavRouteInfoItem(NavRouteInfoItem navWaypoint)
        {
            this.systemname = navWaypoint.systemname;
            this.systemAddress = navWaypoint.systemAddress;
            this.starPos = navWaypoint.starPos;
            this.stellarclass = navWaypoint.stellarclass;
        }

        // Main Constructor
        public NavRouteInfoItem(string systemname, long? SystemAddress, List<decimal> StarPos, string stellarclass)
        {
            this.systemname = systemname;
            this.systemAddress = SystemAddress;
            this.starPos = StarPos;
            this.stellarclass = stellarclass;
        }
    }
}