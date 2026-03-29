using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommodityPurchasedEvent (
        DateTime timestamp,
        long marketid,
        CommodityDefinition commodity,
        int amount,
        int price )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commodity purchased";
        public const string DESCRIPTION = "Triggered when you buy a commodity from the markets";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T16:29:39Z\", \"event\":\"MarketBuy\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"BuyPrice\":1198, \"TotalCost\":1198 }";

        [PublicAPI("The market ID of the purchased commodity")]
        public long marketid { get; } = marketid;

        [PublicAPI("The name of the purchased commodity")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI("The amount of the purchased commodity")]
        public int amount { get; } = amount;

        [PublicAPI("The price paid per unit of the purchased commodity")]
        public int price { get; } = price;

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; } = commodity;
    }
}
