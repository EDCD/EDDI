using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockingCancelledEvent : Event
    {
        public const string NAME = "Docking cancelled";
        public const string DESCRIPTION = "Triggered when your ship cancels a docking request at a station or outpost";
        public static DockingCancelledEvent SAMPLE = new DockingCancelledEvent(DateTime.Now, "Jameson Memorial");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingCancelledEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingCancelled\",\"StationName\":\"Jameson Memorial\"}";

            VARIABLES.Add("station", "The station at which the commander has cancelled docking");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        public DockingCancelledEvent(DateTime timestamp, string station) : base(timestamp, NAME)
        {
            this.station = station;
        }
    }
}
