using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MaterialDonatedEvent : Event
    {
        public const string NAME = "Material donated";
        public const string DESCRIPTION = "Triggered when you donate a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T11:32:57Z\", \"event\":\"ScientificResearch\", \"Name\":\"nickel\", \"Category\":\"Raw\", \"Count\":5 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialDonatedEvent()
        {
            VARIABLES.Add("name", "The name of the donated material");
            VARIABLES.Add("amount", "The amount of the donated material");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("amount")]
        public int amount { get; private set; }

        // Admin
        [JsonProperty("edname")]
        public string edname { get; private set; }

        public MaterialDonatedEvent(DateTime timestamp, Material material, int amount) : base(timestamp, NAME)
        {
            this.name = material?.name;
            this.amount = amount;
            this.edname = material?.EDName;
        }
    }
}
