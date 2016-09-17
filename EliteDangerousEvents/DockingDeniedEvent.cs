using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockingDeniedEvent : Event
    {
        public const string NAME = "Docking denied";
        public const string DESCRIPTION = "Triggered when your ship is denied docking at a station or outpost";
        public static DockingDeniedEvent SAMPLE = new DockingDeniedEvent(DateTime.Now, "Jameson Memorial", "Distance");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingDeniedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has been denied docking");
            VARIABLES.Add("reason", "The station at which the commander has been denied docking");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("reason")]
        public string reason { get; private set; }

        public DockingDeniedEvent(DateTime timestamp, string station, string reason) : base(timestamp, NAME)
        {
            this.station = station;
            this.reason = reason;
        }
    }
}
