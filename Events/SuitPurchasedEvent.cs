using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SuitPurchasedEvent : Event
    {
        public const string NAME = "Suit purchased";
        public const string DESCRIPTION = "Triggered when you buy a space suit";
        public const string SAMPLE = "{ \"timestamp\":\"2021-04-30T21:37:58Z\", \"event\":\"BuySuit\", \"Name\":\"UtilitySuit_Class1\", \"Name_Localised\":\"Maverick Suit\", \"Price\":150000, \"SuitID\":1698502991022131 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SuitPurchasedEvent()
        {
            VARIABLES.Add("suit", "The name of the space suit");
            VARIABLES.Add("suit_invariant", "The invariant name of the space suit");
            VARIABLES.Add("price", "The price paid for the space suit");
        }

        [JsonProperty("suit")]
        public string suit => Suit?.localizedName;

        [JsonProperty("suit_invariant")]
        public string suit_invariant => Suit?.invariantName;

        [JsonProperty("price")]
        public int? price { get; }

        // Not intended to be user facing
        public Suit Suit { get; }

        public SuitPurchasedEvent(DateTime timestamp, Suit suit, int? price) : base(timestamp, NAME)
        {
            this.Suit = suit;
            this.price = price;
        }
    }
}
