using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipRestockedEvent : Event
    {
        public const string NAME = "Ship restocked";
        public const string DESCRIPTION = "Triggered when you restock your ship's ammunition";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T11:13:00Z\", \"event\":\"BuyAmmo\", \"Cost\":36001 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRestockedEvent()
        {
            VARIABLES.Add("price", "The price of restocking");
        }

        public long price { get; private set; }

        public ShipRestockedEvent(DateTime timestamp, long price) : base(timestamp, NAME)
        {
            this.price = price;
        }
    }
}
