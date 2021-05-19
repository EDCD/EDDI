using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MicroResourcesPurchasedEvent : Event
    {
        public const string NAME = "Micro resources purchased";
        public const string DESCRIPTION = "Triggered when you buy micro resources";
        public const string SAMPLE = "{ \"timestamp\":\"2021-04-30T21:41:34Z\", \"event\":\"BuyMicroResources\", \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"Category\":\"Consumable\", \"Count\":2, \"Price\":2000, \"MarketID\":3221524992 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MicroResourcesPurchasedEvent()
        {
            VARIABLES.Add("microresource", "The name of the purchased micro resource");
            VARIABLES.Add("category", "The category of the purchased micro resource");
            VARIABLES.Add("amount", "The amount of the purchased micro resource");
            VARIABLES.Add("price", "The price paid per unit of the purchased micro resource");
        }

        [JsonProperty("microresource")]
        public string microresource => microResource?.localizedName;

        [JsonProperty("category")]
        public string category => microResource?.Category?.localizedName;

        [JsonProperty("amount")]
        public int amount { get; }

        [JsonProperty("price")]
        public int price { get; }

        // Not intended to be user facing
        public MicroResource microResource { get; }
        public long? marketid { get; }

        public MicroResourcesPurchasedEvent(DateTime timestamp, MicroResource microResource, int amount, int price, long? marketid) : base(timestamp, NAME)
        {
            this.microResource = microResource;
            this.amount = amount;
            this.price = price;
            this.marketid = marketid;
        }
    }
}
