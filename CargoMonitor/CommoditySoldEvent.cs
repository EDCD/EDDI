using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCargoMonitor
{
    public class CommoditySoldEvent : Event
    {
        public const string NAME = "Commodity sold";
        public const string DESCRIPTION = "Triggered when you sell a commodity to the markets";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-07T16:29:44Z\", \"event\":\"MarketSell\", \"MarketID\":3224801280, \"Type\":\"coffee\", \"Count\":1, \"SellPrice\":1138, \"TotalSale\":1138, \"AvgPricePaid\":1198 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommoditySoldEvent()
        {
            VARIABLES.Add("marketid", "The market ID of the commodity sold");
            VARIABLES.Add("commodity", "The name of the commodity sold");
            VARIABLES.Add("amount", "The amount of the commodity sold");
            VARIABLES.Add("price", "The price obtained per unit of the commodity sold");
            VARIABLES.Add("profit", "The number of credits profit per unit of the commodity sold");
            VARIABLES.Add("illegal", "True if the commodity is illegal at the place of sale");
            VARIABLES.Add("stolen", "True if the commodity was stolen");
            VARIABLES.Add("blackmarket", "True if the commodity was sold to a black market");
        }

        public long marketid { get; }
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";
        public int amount { get; }
        public long price { get; }
        public long profit { get; }
        public bool illegal { get; }
        public bool stolen { get; }
        public bool blackmarket { get; }
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
