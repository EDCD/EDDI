using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommodityCollectedEvent : Event
    {
        public const string NAME = "Commodity collected";
        public const string DESCRIPTION = "Triggered when you pick up a commodity in your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CollectCargo\",\"Type\":\"agriculturalmedicines\",\"Stolen\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityCollectedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity collected");
            VARIABLES.Add("stolen", "If the cargo is stolen");
        }

        [JsonProperty("commodity")]
        public string commodity { get; private set; }

        [JsonProperty("stolen")]
        public bool stolen { get; private set; }

        public CommodityCollectedEvent(DateTime timestamp, Commodity commodity, bool stolen) : base(timestamp, NAME)
        {
            this.commodity = (commodity == null ? "unknown commodity" : commodity.name);
            this.stolen = stolen;
        }
    }
}
