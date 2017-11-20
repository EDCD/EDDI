using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CargoInventoryEvent : Event
    {
        public const string NAME = "Cargo inventory";
        public const string DESCRIPTION = "Triggered when you obtain an inventory of your cargo";
        public const string SAMPLE = @"{ ""timestamp"":""2017-04-03T16:15:32Z"", ""event"":""Cargo"", ""Inventory"":[ { ""Name"":""fish"", ""Count"":10 }, { ""Name"":""beer"", ""Count"":12 }, { ""Name"":""tobacco"", ""Count"":8 }, { ""Name"":""drones"", ""Count"":20 } ] }";
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
