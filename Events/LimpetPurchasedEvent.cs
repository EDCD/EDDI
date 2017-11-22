using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class LimpetPurchasedEvent : Event
    {
        public const string NAME = "Limpet purchased";
        public const string DESCRIPTION = "Triggered when you buy limpets from a station";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-21T06:53:53Z\", \"event\":\"BuyDrones\", \"Type\":\"Drones\", \"Count\":19, \"BuyPrice\":101, \"TotalCost\":1919 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LimpetPurchasedEvent()
        {
            VARIABLES.Add("amount", "The amount of limpets purchased");
            VARIABLES.Add("price", "The price paid per limpet");
        }

        public int amount { get; private set; }
        public long price { get; private set; }

        public LimpetPurchasedEvent(DateTime timestamp, int amount, long price) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.price = price;
        }
    }
}
