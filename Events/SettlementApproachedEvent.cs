using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SettlementApproachedEvent : Event
    {
        public const string NAME = "Settlement approached";
        public const string DESCRIPTION = "Triggered when you approach a settlement";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-22T15:24:14Z"", ""event"":""ApproachSettlement"", ""Name"":""Freud's Inheritance"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SettlementApproachedEvent()
        {
            VARIABLES.Add("name", "The name of the settlement");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        public SettlementApproachedEvent(DateTime timestamp, string name) : base(timestamp, NAME)
        {
            this.name = name;
        }
    }
}
