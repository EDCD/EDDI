using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DatalinkMessageEvent : Event
    {
        public const string NAME = "Datalink message";
        public const string DESCRIPTION = "Triggered upon completion of Datalink scan";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-30T04:58:49Z\", \"event\":\"DatalinkScan\", \"Message\":\"$DATAPOINT_GAMEPLAY_complete;\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DatalinkMessageEvent()
        {
            VARIABLES.Add("message", "Datalink message");
        }

        [JsonProperty("message")]
        public string message { get; private set; }

        public DatalinkMessageEvent(DateTime timestamp, string message) : base(timestamp, NAME)
        {
            this.message = message;
        }
    }
}
