using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockingRequestedEvent : Event
    {
        public const string NAME = "Docking requested";
        public const string DESCRIPTION = "Triggered when your ship requests docking at a station or outpost";
        public static DockingRequestedEvent SAMPLE = new DockingRequestedEvent(DateTime.Now, "Jameson Memorial");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingRequestedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingRequested\",\"StationName\":\"Jameson Memorial\"}";

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
