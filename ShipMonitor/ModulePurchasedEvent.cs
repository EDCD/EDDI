using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiShipMonitor
{
    public class ModulePurchasedEvent : Event
    {
        public const string NAME = "Module purchased";
        public const string DESCRIPTION = "Triggered when you purchase a module in outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleBuy\", \"Slot\":\"MediumHardpoint2\", \"SellItem\":\"hpt_pulselaser_fixed_medium\", \"SellPrice\":0, \"BuyItem\":\"hpt_multicannon_gimbal_medium\", \"BuyPrice\":50018, \"Ship\":\"cobramkiii\", \"ShipID\":1  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModulePurchasedEvent()
        {
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("item", "The item being purchased");
            VARIABLES.Add("price", "The price of the item being purchased");
            VARIABLES.Add("solditem", "The item being sold");
            VARIABLES.Add("soldprice", "The price of the item being sold");
            VARIABLES.Add("storeditem", "The item being stored");
            VARIABLES.Add("ship", "The ship for which the item was purchased");
            VARIABLES.Add("shipid", "The ID of the ship for which the item was purchased");

        }
        [JsonProperty("slot")]
        public string slot { get; private set; }

        [JsonProperty("item")]
        public string item { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }

        [JsonProperty("solditem")]
        public string solditem { get; private set; }

        [JsonProperty("soldprice")]
        public long? soldprice { get; private set; }

        [JsonProperty("storeditem")]
        public string storeditem { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        public ModulePurchasedEvent(DateTime timestamp, string slot, string item, long price, string solditem, long? soldprice, string storeditem, string ship, int? shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.item = item;
            this.price = price;
            this.solditem = solditem;
            this.soldprice = soldprice;
            this.storeditem = storeditem;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}
