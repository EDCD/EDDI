using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredSignalSourceEvent : Event
    {
        public const string NAME = "Entered signal source";
        public const string DESCRIPTION = "Triggered when your ship enters a signal source";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"USSDrop\",\"USSType\":\"Disrupted wake echoes\",\"USSThreat\":0}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredSignalSourceEvent()
        {
            VARIABLES.Add("source", "The type of the signal source");
            VARIABLES.Add("threat", "The threat level of the signal source (0-4)");
        }

        [JsonProperty("source")]
        public string source { get; private set; }

        [JsonProperty("threat")]
        public int threat{ get; private set; }

        public EnteredSignalSourceEvent(DateTime timestamp, string source, int threat) : base(timestamp, NAME)
        {
            this.source = source;
            this.threat = threat;
        }
    }
}
