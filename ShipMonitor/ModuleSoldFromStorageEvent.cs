using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ModuleSoldFromStorageEvent : Event
    {
        public const string NAME = "Module sold from storage";
        public const string DESCRIPTION = "Triggered when selling a module in storage";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T02:28:37Z\", \"event\":\"ModuleSellRemote\", \"StorageSlot\":10, \"SellItem\":\"$int_fueltank_size4_class3_name;\", \"SellItem_Localised\":\"Fuel Tank\", \"ServerId\":128064349, \"SellPrice\":24116, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";

        [PublicAPI("The ship from which the module was sold")]
        public string ship { get; private set; }

        [PublicAPI("The ID of the ship from which the module was sold")]
        public int? shipid { get; private set; }

        [PublicAPI("The module (object) being sold")]
        public Module module { get; private set; }

        [PublicAPI("The price of the module being sold")]
        public long price { get; private set; }

        // Not intended to be user facing

        public int storageslot { get; private set; }

        public long serverid { get; private set; }

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
