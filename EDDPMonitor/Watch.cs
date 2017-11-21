using Newtonsoft.Json;

namespace EddiEddpMonitor
{
    /// <summary>
    /// The parameters to match EDDP messages
    /// </summary>
    public class Watch
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("system")]
        public string System { get; set; }
        [JsonProperty("station")]
        public string Station { get; set; }
        [JsonProperty("faction")]
        public string Faction { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("maxdistancefromship")]
        public long? MaxDistanceFromShip { get; set; }
        [JsonProperty("maxdistancefromhome")]
        public long? MaxDistanceFromHome { get; set; }

        public Watch() { }
    }
}
