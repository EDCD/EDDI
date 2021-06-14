using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MicroResourcesPurchasedEvent : Event
    {
        public const string NAME = "Micro resources purchased";
        public const string DESCRIPTION = "Triggered when you buy micro resources";
        public const string SAMPLE = "{ \"timestamp\":\"2021-04-30T21:41:34Z\", \"event\":\"BuyMicroResources\", \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"Category\":\"Consumable\", \"Count\":2, \"Price\":2000, \"MarketID\":3221524992 }";

        [PublicAPI("The name of the purchased micro resource")]
        public string microresource => microResource?.localizedName;

        [PublicAPI("The category of the purchased micro resource")]
        public string category => microResource?.Category?.localizedName;

        [PublicAPI("The amount of the purchased micro resource")]
        public int amount { get; }

        [PublicAPI("The price paid per unit of the purchased micro resource")]
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
