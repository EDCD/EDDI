using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    class FlightAssistEvent : Event
    {
        public const string NAME = "Flight assist";
        public const string DESCRIPTION = "Triggered when flight assist is toggled";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FlightAssistEvent()
        {
            VARIABLES.Add("off", "A boolean value. True if flight assist is off.");
        }

        [JsonProperty("off")]
        public bool off { get; private set; }

        public FlightAssistEvent(DateTime timestamp, bool flight_assist_off) : base(timestamp, NAME)
        {
            this.off = flight_assist_off;
        }
    }
}
