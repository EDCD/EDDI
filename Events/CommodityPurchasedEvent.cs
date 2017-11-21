using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommodityPurchasedEvent : Event
    {
        public const string NAME = "Commodity purchased";
        public const string DESCRIPTION = "Triggered when you buy a commodity from the markets";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MarketBuy\",\"Type\":\"agriculturalmedicines\",\"Count\":10,\"BuyPrice\":39,\"TotalCost\":390}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityPurchasedEvent()
        {
            VARIABLES.Add("commodity", "The name of the purchased commodity");
            VARIABLES.Add("amount", "The amount of the purchased commodity");
            VARIABLES.Add("price", "The price paid per unit of the purchased commodity");
        }

        public string commodity { get; private set; }
        public int amount { get; private set; }
        public long price { get; private set; }

        public CommodityPurchasedEvent(DateTime timestamp, Commodity commodity, int amount, long price) : base(timestamp, NAME)
        {
            this.commodity = (commodity == null ? "unknown commodity" : commodity.name);
            this.amount = amount;
            this.price = price;
        }
    }
}
