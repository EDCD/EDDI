using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class MaterialCollectedEvent : Event
    {
        public const string NAME = "Material collected";
        public const string DESCRIPTION = "Triggered when you collect a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T11:32:57Z\", \"event\":\"MaterialCollected\", \"Category\":\"Encoded\", \"Name\":\"shieldpatternanalysis\", \"Count\":3 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialCollectedEvent()
        {
            VARIABLES.Add("name", "The name of the collected material");
            VARIABLES.Add("amount", "The amount of the collected material");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("amount")]
        public int amount { get; private set; }

        // Admin
        [JsonProperty("edname")]
        public string edname { get; private set; }

        public MaterialCollectedEvent(DateTime timestamp, Material material, int amount) : base(timestamp, NAME)
        {
            this.name = material?.name;
            this.amount = amount;
            this.edname = material?.EDName;
        }
    }
}
