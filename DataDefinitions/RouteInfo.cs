using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class RouteInfo
    {
        [JsonProperty]
        public string starSystem { get; set; }

        [JsonProperty]
        public long? systemAddress { get; set; }

        [JsonProperty]
        public starPos starPos { get; set; }

        public string starClass { get; set; }

        // Default Constructor
        public RouteInfo()
        { }

        // Copy Constructor
        public RouteInfo(RouteInfo RouteInfo)
        {
            this.starSystem = RouteInfo.starSystem;
            this.systemAddress = RouteInfo.systemAddress;
            this.starPos = RouteInfo.starPos;
            this.starClass = RouteInfo.starClass;
        }

        // Main Constructor
        public RouteInfo(string StarSystem, long? SystemAddress, starPos StarPos, string StarClass)
        {
            this.starSystem = StarSystem;
            this.systemAddress = SystemAddress;
            this.starPos = StarPos;
            this.starClass = StarClass;
        }
    }

    public class starPos
    {
        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        public starPos(decimal? x, decimal? y, decimal? z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}