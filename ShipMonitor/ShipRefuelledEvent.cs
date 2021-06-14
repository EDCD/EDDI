using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipRefuelledEvent : Event
    {
        public const string NAME = "Ship refuelled";
        public const string DESCRIPTION = "Triggered when you refuel your ship";
        //public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T08:00:04Z\", \"event\":\"RefuelAll\", \"Cost\":493, \"Amount\":9.832553 }";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T08:00:04Z\", \"event\":\"FuelScoop\", \"Scooped\":0.4987000, \"Total\":16.0000 }";

        [PublicAPI("The source of the fuel (Market or Scoop)")]
        public string source { get; private set; }

        [PublicAPI("The price of refuelling (only available if the source is Market)")]
        public long? price { get; private set; }

        [PublicAPI("The amount of fuel obtained")]
        public decimal amount { get; private set; }

        [PublicAPI("The new fuel level (only available if the source is Scoop)")]
        public decimal? total { get; private set; }

        [PublicAPI("Whether this is a full refuel")]
        public bool full { get; private set; }

        public ShipRefuelledEvent(DateTime timestamp, string source, long? price, decimal amount, decimal? total, bool full = false) : base(timestamp, NAME)
        {
            this.source = source;
            this.price = price;
            this.amount = amount;
            this.total = total;
            this.full = full;
        }
    }
}
