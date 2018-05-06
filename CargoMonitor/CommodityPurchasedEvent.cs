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

        public long marketid { get; private set; }
        public string commodity { get; private set; }
        public int amount { get; private set; }
        public int price { get; private set; }
        public CommodityDefinition commodityDefinition { get; private set; }

        public CommodityPurchasedEvent(DateTime timestamp, long marketid, CommodityDefinition commodity, int amount, int price) : base(timestamp, NAME)
        {
            this.marketid = marketid;
            this.commodity = commodity?.localizedName ?? "unknown commodity";
            this.amount = amount;
            this.price = price;
            this.commodityDefinition = commodity;
        }
    }
}
