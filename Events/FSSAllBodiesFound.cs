using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSSAllBodiesFound : Event
    {
        public const string NAME = "System scan complete";
        public const string DESCRIPTION = "Triggered after having identified all bodies in the system";
        public const string SAMPLE = @"{""timestamp"":""2019-03-10T16:09:36Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Dumbae DN-I d10-6057"", ""SystemAddress"":208127228285531, ""Count"":19 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSSAllBodiesFound()
        {
            VARIABLES.Add("system", "The name of the scanned system");
            VARIABLES.Add("systemaddress", "The address of the scanned system");
            VARIABLES.Add("bodies", "The count of bodies from the scanned system");
        }

        [JsonProperty("system")]
        public string System { get; private set; }

        [JsonProperty("systemaddress")]
        public long SystemAddress { get; private set; }

        [JsonProperty("bodies")]
        public int Bodies { get; private set; }
               
        public FSSAllBodiesFound(DateTime timestamp, string system, long systemAddress, int bodies) : base(timestamp, NAME)
        {
            this.System = system;
            this.SystemAddress = systemAddress;
            this.Bodies = bodies;
        }
    }
}
