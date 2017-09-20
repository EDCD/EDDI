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
        public const string DESCRIPTION = "Triggered when selling a module to outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSell\", \"Slot\":\"Slot06_Size2\", \"SellItem\":\"int_cargorack_size1_class1\", \"SellPrice\":877, \"Ship\":\"asp\", \"ShipID\":1 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleSoldEvent()
        {
            VARIABLES.Add("ship", "The ship from which the module was sold");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was sold");
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("module", "The module (object) being sold");
            VARIABLES.Add("price", "The price of the module being sold");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("slot")]
        public string slot { get; private set; }

        [JsonProperty("module")]
        public Module module { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }

        public ModuleSoldEvent(DateTime timestamp, string ship, int? shipid, string slot, Module module, long price) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipid;
            this.slot = slot;
            this.module = module;
            this.price = price;
        }
    }
}