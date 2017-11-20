using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockingRequestedEvent : Event
    {
        public const string NAME = "Docking requested";
        public const string DESCRIPTION = "Triggered when your ship requests docking at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingRequested\",\"StationName\":\"Jameson Memorial\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingRequestedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has requested docking");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        public DockingRequestedEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
