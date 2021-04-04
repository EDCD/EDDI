using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class LimpetSoldEvent : Event
    {
        public const string NAME = "Limpet sold";
        public const string DESCRIPTION = "Triggered when you sell limpets to a station";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-24T00:03:25Z\", \"event\":\"SellDrones\", \"Type\":\"Drones\", \"Count\":8, \"SellPrice\":101, \"TotalSale\":808 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LimpetSoldEvent()
        {
            VARIABLES.Add("amount", "The amount of limpets sold");
            VARIABLES.Add("price", "The price obtained per limpet");
        }

        [PublicAPI]
        public int amount { get; }

        [PublicAPI]
        public long price { get; }

        public LimpetSoldEvent(DateTime timestamp, int amount, long price) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.price = price;
        }
    }
}
