using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FinePaidEvent : Event
    {
        public const string NAME = "Fine paid";
        public const string DESCRIPTION = "Triggered when you pay a fine";
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-06T09:30:36Z\", \"event\":\"PayLegacyFines\", \"Amount\":255 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FinePaidEvent()
        {
            VARIABLES.Add("amount", "The amount of the fine paid");
            VARIABLES.Add("brokerpercentage", "Broker precentage fee (if paid via a Broker)");
            VARIABLES.Add("legacy", "True if the payment is for a legacy fine");
        }

        [JsonProperty("amount")]
        public long amount { get; private set; }

        [JsonProperty("brokerpercentage")]
        public decimal? brokerpercentage { get; private set; }

        [JsonProperty("legacy")]
        public bool legacy { get; private set; }

        public FinePaidEvent(DateTime timestamp, long amount, decimal? brokerpercentage, bool legacy) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.legacy = legacy;
        }
    }
}
