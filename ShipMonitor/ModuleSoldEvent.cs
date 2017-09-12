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
    public class ModuleSoldEvent : Event
    {
        public const string NAME = "Module sold";
        public const string DESCRIPTION = "Triggered when you sell a module to outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSell\", \"Slot\":\"Slot06_Size2\", \"SellItem\":\"int_cargorack_size1_class1\", \"SellPrice\":877, \"Ship\":\"asp\", \"ShipID\":1 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleSoldEvent()
        {
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("item", "The item being sold");
            VARIABLES.Add("price", "The price of the item being sold");
            VARIABLES.Add("ship", "The ship from which the item was sold");
            VARIABLES.Add("shipid", "The ID of the ship from which the item was sold");

        }
        [JsonProperty("slot")]
        public string slot { get; private set; }

        [JsonProperty("item")]
        public string item { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        public ModuleSoldEvent(DateTime timestamp, string slot, string item, long price, string ship, int? shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.item = item;
            this.price = price;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}