using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class CargoInventoryEvent : Event
    {
        public const string NAME = "Cargo inventory";
        public const string DESCRIPTION = "Triggered when you obtain an inventory of your cargo";
        public const string SAMPLE = @"{ ""timestamp"":""2017-03-31T10:38:23Z"", ""event"":""Cargo"", ""Inventory"":[ { ""Name"":""fish"", ""Count"":64 } ] }";
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
