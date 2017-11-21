using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommodityEjectedEvent : Event
    {
        public const string NAME = "Commodity ejected";
        public const string DESCRIPTION = "Triggered when you eject a commodity from your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"EjectCargo\",\"Type\":\"agriculturalmedicines\",\"Count\":2,\"Abandoned\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityEjectedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity ejected");
            VARIABLES.Add("amount", "The amount of cargo ejected");
            VARIABLES.Add("abandoned", "If the cargo has been abandoned");
        }

        [JsonProperty("commodity")]
        public string commodity { get; private set; }

        [JsonProperty("amount")]
        public int amount { get; private set; }

        [JsonProperty("abandoned")]
        public bool abandoned { get; private set; }

        public CommodityEjectedEvent(DateTime timestamp, Commodity commodity , int amount, bool abandoned) : base(timestamp, NAME)
        {
            this.commodity = (commodity == null ? "unknown commodity" : commodity.name);
            this.amount = amount;
            this.abandoned = abandoned;
        }
    }
}
