using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CommodityPurchasedEvent : Event
    {
        public const string NAME = "Commodity purchased";
        public const string DESCRIPTION = "Triggered when you buy a commodity from the markets";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T16:29:39Z\", \"event\":\"MarketBuy\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"BuyPrice\":1198, \"TotalCost\":1198 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityPurchasedEvent()
        {
            VARIABLES.Add("marketid", "The market ID of the purchased commodity");
            VARIABLES.Add("commodity", "The name of the purchased commodity");
            VARIABLES.Add("amount", "The amount of the purchased commodity");
            VARIABLES.Add("price", "The price paid per unit of the purchased commodity");
        }

        [PublicAPI]
        public long marketid { get; }

        [PublicAPI]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI]
        public int amount { get; }

        [PublicAPI]
        public int price { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public CommodityPurchasedEvent(DateTime timestamp, long marketid, CommodityDefinition commodity, int amount, int price) : base(timestamp, NAME)
        {
            this.marketid = marketid;
            this.amount = amount;
            this.price = price;
            this.commodityDefinition = commodity;
        }
    }
}
