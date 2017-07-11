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
    public class ShipRepairedEvent : Event
    {
        public const string NAME = "Ship repaired";
        public const string DESCRIPTION = "Triggered when you repair your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:31:38Z\", \"event\":\"Repair\", \"Item\":\"Wear\", \"Cost\":2824 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        static ShipRepairedEvent()
        {
            VARIABLES.Add("item", "The item repaired, if repairing a specific item");
            VARIABLES.Add("price", "The price of the repair");
        }

        public string item { get; private set; }

        public long price { get; private set; }

        public ShipRepairedEvent(DateTime timestamp, string item, long price) : base(timestamp, NAME)
        {
            this.item = item;
            this.price = price;
        }
    }
}
