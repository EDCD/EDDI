using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MaterialDiscoveredEvent: Event
    {
        public const string NAME = "Material discovered";
        public const string DESCRIPTION = "Triggered when you discover a material";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T14:07:19Z\", \"event\":\"MaterialDiscovered\", \"Category\":\"Raw\", \"Name\":\"iron\", \"DiscoveryNumber\":3 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialDiscoveredEvent()
        {
            VARIABLES.Add("name", "The name of the discovered material");
            VARIABLES.Add("LocalName", "The translated name of the discovered material into the chosen language");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("LocalName")]
        public string LocalName
        {
            get
            {
                if (name != null && name != "")
                {
                    return Material.FromName(name).LocalName;
                }
                else return null;
            }
        }

        public MaterialDiscoveredEvent(DateTime timestamp, Material material) : base(timestamp, NAME)
        {
            this.name = material?.name;
        }
    }
}
