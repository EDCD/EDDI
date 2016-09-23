using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class SoldToMarketEvent : Event
    {
        public const string NAME = "Sold to market";
        public const string DESCRIPTION = "Triggered when you sell something to the markets";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MarketSell\",\"Type\":\"agriculturalmedicines\",\"Count\":10,\"SellPrice\":42,\"TotalSale\":420,\"AvgPricePaid\":39}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SoldToMarketEvent()
        {
            VARIABLES.Add("cargo", "The name of the cargo sold");
            VARIABLES.Add("amount", "The amount of the cargo sold");
            VARIABLES.Add("price", "The price obtained per unit of the cargo sold");
            VARIABLES.Add("profit", "The number of credits profit per unit of the cargo sold");
        }

        [JsonProperty("cargo")]
        public string cargo { get; private set; }
        [JsonProperty("amount")]
        public int amount { get; private set; }
        [JsonProperty("price")]
        public decimal price { get; private set; }
        [JsonProperty("profit")]
        public decimal profit { get; private set; }

        public SoldToMarketEvent(DateTime timestamp, string cargo, int amount, decimal price, decimal profit) : base(timestamp, NAME)
        {
            this.cargo = cargo;
            this.amount = amount;
            this.price = price;
            this.profit = profit;
        }
    }
}
