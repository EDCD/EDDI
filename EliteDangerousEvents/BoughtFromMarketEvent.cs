using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class BoughtFromMarketEvent : Event
    {
        public const string NAME = "Bought from market";
        public const string DESCRIPTION = "Triggered when you buy something from the markets";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"MarketBuy\",\"Type\":\"agriculturalmedicines\",\"Count\":10,\"BuyPrice\":39,\"TotalCost\":390}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BoughtFromMarketEvent()
        {
            VARIABLES.Add("cargo", "The name of the cargo bought");
            VARIABLES.Add("amount", "The amount of the cargo bought");
            VARIABLES.Add("price", "The price paid per unit of the cargo bought");
        }

        [JsonProperty("cargo")]
        public string cargo { get; private set; }
        [JsonProperty("amount")]
        public int amount { get; private set; }
        [JsonProperty("price")]
        public decimal price { get; private set; }

        public BoughtFromMarketEvent(DateTime timestamp, string cargo, int amount, decimal price) : base(timestamp, NAME)
        {
            this.cargo = cargo;
            this.amount = amount;
            this.price = price;
        }
    }
}
