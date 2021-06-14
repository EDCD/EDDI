using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SuitPurchasedEvent : Event
    {
        public const string NAME = "Suit purchased";
        public const string DESCRIPTION = "Triggered when you buy a space suit";
        public const string SAMPLE = "{ \"timestamp\":\"2021-04-30T21:37:58Z\", \"event\":\"BuySuit\", \"Name\":\"UtilitySuit_Class1\", \"Name_Localised\":\"Maverick Suit\", \"Price\":150000, \"SuitID\":1698502991022131 }";

        [PublicAPI("The name of the space suit")]
        public string suit => Suit?.localizedName;

        [PublicAPI("The invariant name of the space suit")]
        public string suit_invariant => Suit?.invariantName;

        [PublicAPI("The price paid for the space suit")]
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
