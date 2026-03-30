using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class NavRouteInfoItem
    {
        [PublicAPI("The name of the star system"), JsonProperty("StarSystem")]
        public string systemname { get; set; }

        [JsonIgnore, Obsolete("Please use systemname instead.")]
        public string name => systemname;

        [PublicAPI("The numeric system address of the star system"), JsonProperty( "SystemAddress")]
        public ulong systemAddress { get; set; }

        [JsonProperty( "StarPos")]
        private List<decimal> starPos { get; set; }

        [PublicAPI ("The stellar class of the primary star"), JsonProperty( "StarClass")]
        public string stellarclass { get; set; }

        [PublicAPI("The X coordinate of the star system")]
        public decimal x => starPos[0];

        [PublicAPI( "The Y coordinate of the star system" )]
        public decimal y => starPos[1];

        [PublicAPI( "The Z coordinate of the star system" )]
        public decimal z => starPos[2];

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
        public NavRouteInfoItem(string systemname, ulong SystemAddress, List<decimal> StarPos, string stellarclass)
        {
            this.systemname = systemname;
            this.systemAddress = SystemAddress;
            this.starPos = StarPos;
            this.stellarclass = stellarclass;
        }
    }
}