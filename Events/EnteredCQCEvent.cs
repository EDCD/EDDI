using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredCQCEvent : Event
    {
        public const string NAME = "Entered CQC";
        public const string DESCRIPTION = "Triggered when you enter CQC";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"Credits\":600120,\"Loan\":0}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredCQCEvent()
        {
            VARIABLES.Add("commander", "The commander's name");
        }

        [JsonProperty("commander")]
        public string commander { get; private set; }

        public EnteredCQCEvent(DateTime timestamp, string commander) : base(timestamp, NAME)
        {
            this.commander = commander;
        }
    }
}
