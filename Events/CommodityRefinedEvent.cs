using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommodityRefinedEvent : Event
    {
        public const string NAME = "Commodity refined";
        public const string DESCRIPTION = "Triggered when you refine a commodity from the refinery";
        public static string SAMPLE = "{ \"timestamp\":\"2016-09-30T18:00:22Z\", \"event\":\"MiningRefined\", \"Type\":\"$hydrogenperoxide_name;\", \"Type_Localised\":\"Hydrogen Peroxide\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityRefinedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity refined");
        }

        [JsonProperty("commodity")]
        public string commodity { get; private set; }

        public CommodityRefinedEvent(DateTime timestamp, Commodity commodity) : base(timestamp, NAME)
        {
            this.commodity = (commodity == null ? "unknown commodity" : commodity.name);
        }
    }
}
