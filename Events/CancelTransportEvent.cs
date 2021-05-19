using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CancelTransportEvent : Event
    {
        public const string NAME = "Cancel transport";
        public const string DESCRIPTION = "Triggered when canceling a taxi or deployment for on foot combat";
        public const string SAMPLE = "{ \"timestamp\":\"2021-03-30T22:30:18Z\", \"event\":\"CancelTaxi\", \"Refund\":100 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CancelTransportEvent()
        {
            VARIABLES.Add("transporttype", @"The type of transport being cancelled (e.g. ""Taxi"", ""Dropship"")");
            VARIABLES.Add("refund", "The credits that you were refunded for the cancelled transport)");
        }

        [JsonProperty("transporttype")]
        public string transporttype { get; }

        [JsonProperty("refund")]
        public int? refund { get; }

        public CancelTransportEvent(DateTime timestamp, string transporttype, int? refund) : base(timestamp, NAME)
        {
            this.transporttype = transporttype;
            this.refund = refund;
        }
    }
}