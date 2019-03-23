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
            VARIABLES.Add("systemname", "The name of the scanned system");
            VARIABLES.Add("count", "The count of bodies from the scanned system");
        }

        [JsonProperty("systemname")]
        public string systemName { get; private set; }

        [JsonProperty("systemaddress")]
        public long systemAddress { get; private set; }

        [JsonProperty("count")]
        public int count { get; private set; }
               
        public FSSAllBodiesFound(DateTime timestamp, string systemName, int count) : base(timestamp, NAME)
        {
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            this.count = count;

        }
    }
}
