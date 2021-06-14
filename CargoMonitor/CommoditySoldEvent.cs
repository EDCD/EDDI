using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiCargoMonitor
{
    [PublicAPI]
    public class CommoditySoldEvent : Event
    {
        public const string NAME = "Commodity sold";
        public const string DESCRIPTION = "Triggered when you sell a commodity to the markets";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T16:29:44Z\", \"event\":\"MarketSell\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"SellPrice\":1138, \"TotalSale\":1138, \"AvgPricePaid\":1198 }";

        [PublicAPI("The market ID of the commodity sold")]
        public long marketid { get; }
        
        [PublicAPI ("The name of the commodity sold")]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI("The amount of the commodity sold")]
        public int amount { get; }

        [PublicAPI("The price obtained per unit of the commodity sold")]
        public long price { get; }

        [PublicAPI("The number of credits profit per unit of the commodity sold")]
        public long profit { get; }

        [PublicAPI("True if the commodity is illegal at the place of sale")]
        public bool illegal { get; }

        [PublicAPI("True if the commodity was stolen")]
        public bool stolen { get; }

        [PublicAPI("True if the commodity was sold to a black market")]
        public bool blackmarket { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public CommoditySoldEvent(DateTime timestamp, long marketid, CommodityDefinition commodity, int amount, long price, long profit, bool illegal, bool stolen, bool blackmarket) : base(timestamp, NAME)
        {
            this.marketid = marketid;
            this.amount = amount;
            this.price = price;
            this.profit = profit;
            this.illegal = illegal;
            this.stolen = stolen;
            this.blackmarket = blackmarket;
            this.commodityDefinition = commodity;
        }
    }
}
