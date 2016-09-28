using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class MaterialCollectedEvent : Event
    {
        public const string NAME = "Material collected";
        public const string DESCRIPTION = "Triggered when you collect a material";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MaterialCollected\",\"Category\":\"Encoded\",\"Name\":\"shieldcyclerecordings\"}";
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

        public MaterialCollectedEvent(DateTime timestamp, Material material, int amount) : base(timestamp, NAME)
        {
            this.name = (material == null ? null : material.name);
            this.amount = amount;
        }
    }
}
