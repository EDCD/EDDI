using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockingTimedOutEvent : Event
    {
        public const string NAME = "Docking timed out";
        public const string DESCRIPTION = "Triggered when your docking request times out";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingTimedOutEvent()
        {
            VARIABLES.Add("station", "The station at which the docking request has timed out");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        public DockingTimedOutEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
