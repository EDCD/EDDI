using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipRefuelledEvent : Event
    {
        public const string NAME = "Ship refuelled";
        public const string DESCRIPTION = "Triggered when you refuel your ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T08:00:04Z\", \"event\":\"RefuelAll\", \"Cost\":493, \"Amount\":9.832553 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRefuelledEvent()
        {
            VARIABLES.Add("price", "The price of refuelling");
            VARIABLES.Add("amount", "The amount of fuel supplied");
        }

        public long price { get; private set; }

        public decimal amount{ get; private set; }

        public ShipRefuelledEvent(DateTime timestamp, long price, decimal amount) : base(timestamp, NAME)
        {
            this.price = price;
            this.amount = amount;
        }
    }
}
