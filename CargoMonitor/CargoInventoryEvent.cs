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
        public const string SAMPLE = "{ \"timestamp\":\"2017-02-10T14:25:51Z\", \"event\":\"Cargo\", \"Inventory\":[ { \"Name\":\"syntheticmeat\", \"Count\":2, \"Stolen\": 0 }, { \"Name\":\"evacuationshelter\", \"Count\":1, \"Stolen\": 0 }, { \"Name\":\"progenitorcells\", \"Count\":3, \"Stolen\": 3 }, { \"Name\":\"bioreducinglichen\", \"Count\":1, \"Stolen\": 0 }, { \"Name\":\"neofabricinsulation\", \"Count\":2, \"Stolen\": 0 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoInventoryEvent()
        {
            VARIABLES.Add("inventory", "The cargo in your inventory");
        }

        public List<Cargo> inventory { get; private set; }

        public CargoInventoryEvent(DateTime timestamp, List<Cargo> inventory) : base(timestamp, NAME)
        {
            this.inventory = inventory;
        }
    }
}
