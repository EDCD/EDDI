using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ExplorationDataPurchasedEvent : Event
    {
        public const string NAME = "Exploration data purchased";
        public const string DESCRIPTION = "Triggered when you purchase exploration data";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T23:20:21Z\", \"event\":\"BuyExplorationData\", \"System\":\"Yen Ti\", \"Cost\":1567 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ExplorationDataPurchasedEvent()
        {
            VARIABLES.Add("system", "The system for which the exploration data was purchased");
            VARIABLES.Add("price", "The price of the purchase");
        }

        public string system { get; private set; }

        public long price { get; private set; }

        public ExplorationDataPurchasedEvent(DateTime timestamp, string system, long price) : base(timestamp, NAME)
        {
            this.system = system;
            this.price = price;
        }
    }
}
