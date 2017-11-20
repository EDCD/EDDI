using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipRefuelledEvent : Event
    {
        public const string NAME = "Ship refuelled";
        public const string DESCRIPTION = "Triggered when you refuel your ship";
        //public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T08:00:04Z\", \"event\":\"RefuelAll\", \"Cost\":493, \"Amount\":9.832553 }";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T08:00:04Z\", \"event\":\"FuelScoop\", \"Scooped\":0.4987000, \"Total\":16.0000 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRefuelledEvent()
        {
            VARIABLES.Add("source", "The source of the fuel (Market or Scoop)");
            VARIABLES.Add("price", "The price of refuelling (only available if the source is Market)");
            VARIABLES.Add("amount", "The amount of fuel obtained");
            VARIABLES.Add("total", "The new fuel level (only available if the source is Scoop)");
        }

        public string source { get; private set; }

        public long? price { get; private set; }

        public decimal amount{ get; private set; }

        public decimal? total { get; private set; }

        public ShipRefuelledEvent(DateTime timestamp, string source, long? price, decimal amount, decimal? total) : base(timestamp, NAME)
        {
            this.source = source;
            this.price = price;
            this.amount = amount;
            this.total = total;
        }
    }
}
