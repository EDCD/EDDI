using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommoditySoldEvent : Event
    {
        public const string NAME = "Commodity sold";
        public const string DESCRIPTION = "Triggered when you sell a commodity to the markets";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MarketSell\",\"Type\":\"agriculturalmedicines\",\"Count\":10,\"SellPrice\":42,\"TotalSale\":420,\"AvgPricePaid\":39}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommoditySoldEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity sold");
            VARIABLES.Add("amount", "The amount of the commodity sold");
            VARIABLES.Add("price", "The price obtained per unit of the commodity sold");
            VARIABLES.Add("profit", "The number of credits profit per unit of the commodity sold");
            VARIABLES.Add("illegal", "True if the commodity is illegal at the place of sale");
            VARIABLES.Add("stolen", "True if the commodity was stolen");
            VARIABLES.Add("blackmarket", "True if the commodity was sold to a black market");
        }

        public string commodity { get; private set; }
        public int amount { get; private set; }
        public long price { get; private set; }
        public long profit { get; private set; }
        public bool illegal { get; private set; }
        public bool stolen { get; private set; }
        public bool blackmarket { get; private set; }

        public CommoditySoldEvent(DateTime timestamp, Commodity commodity, int amount, long price, long profit, bool illegal, bool stolen, bool blackmarket) : base(timestamp, NAME)
        {
            this.commodity = (commodity == null ? "unknown commodity" : commodity.name);
            this.amount = amount;
            this.price = price;
            this.profit = profit;
            this.illegal = illegal;
            this.stolen = stolen;
            this.blackmarket = blackmarket;
        }
    }
}
