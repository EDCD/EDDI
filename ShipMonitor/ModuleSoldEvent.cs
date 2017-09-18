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
            VARIABLES.Add("module", "The module being sold");
            VARIABLES.Add("price", "The price of the module being sold");
            VARIABLES.Add("ship", "The ship from which the module was sold");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was sold");
        }

        public string slot { get; private set; }
        public Module module { get; private set; }
        public long price { get; private set; }
        public string ship { get; private set; }
        public int shipid { get; private set; }

        public ModuleSoldEvent(DateTime timestamp, string slot, Module module, long price, string ship, int shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.module = module;
            this.price = price;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}