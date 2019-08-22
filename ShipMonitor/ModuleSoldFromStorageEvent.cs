using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ModuleSoldFromStorageEvent : Event
    {
        public const string NAME = "Module sold from storage";
        public const string DESCRIPTION = "Triggered when selling a module in storage";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T02:28:37Z\", \"event\":\"ModuleSellRemote\", \"StorageSlot\":10, \"SellItem\":\"$int_fueltank_size4_class3_name;\", \"SellItem_Localised\":\"Fuel Tank\", \"ServerId\":128064349, \"SellPrice\":24116, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleSoldFromStorageEvent()
        {
            VARIABLES.Add("ship", "The ship from which the module was sold");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was sold");
            // VARIABLES.Add("storageslot", "The storage slot"); // This probably isn't useful to the end user
            // VARIABLES.Add("serverid", "The frontier ID of the item being sold"); // This probably isn't useful to the end user
            VARIABLES.Add("module", "The module (object) being sold");
            VARIABLES.Add("price", "The price of the module being sold");
        }

        public string ship { get; private set; }
        public int? shipid { get; private set; }
        public int storageslot { get; private set; }
        public long serverid { get; private set; }
        public Module module { get; private set; }
        public long price { get; private set; }

        public ModuleSoldFromStorageEvent(DateTime timestamp, string ship, int? shipid, int storageslot, long serverid, Module module, long price) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipid;
            this.storageslot = storageslot;
            this.serverid = serverid;
            this.module = module;
            this.price = price;
        }
    }
}
