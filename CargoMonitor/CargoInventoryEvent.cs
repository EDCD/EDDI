using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiCargoMonitor
{
    public class CargoInventoryEvent : Event
    {
        public const string NAME = "Cargo inventory";
        public const string DESCRIPTION = "Triggered when you obtain an inventory of your cargo";
        public const string SAMPLE = "{ \"timestamp\":\"2018-04-10T23:33:17Z\", \"event\":\"Cargo\", \"Inventory\":[ { \"Name\":\"hydrogenfuel\", \"Name_Localised\":\"Hydrogen Fuel\", \"Count\":2, \"Stolen\":0 }, { \"Name\":\"survivalequipment\", \"Name_Localised\":\"Survival Equipment\", \"Count\":2, \"Stolen\":0 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoInventoryEvent()
        {
            VARIABLES.Add("inventory", "The cargo in your inventory");
            VARIABLES.Add("cargocarried", "The total amount of cargo in your inventory");
        }

        public List<Cargo> inventory { get; private set; }
        public int cargocarried { get; private set; }

        public CargoInventoryEvent(DateTime timestamp, List<Cargo> inventory, int cargocarried) : base(timestamp, NAME)
        {
            this.inventory = inventory;
            this.cargocarried = cargocarried;
        }
    }
}
