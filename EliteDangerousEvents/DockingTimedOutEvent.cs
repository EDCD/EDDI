using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockingTimedOutEvent : Event
    {
        public const string NAME = "Docking timed out";
        public const string DESCRIPTION = "Triggered when your docking request times out";
        public static DockingTimedOutEvent SAMPLE = new DockingTimedOutEvent(DateTime.Now, "Jameson Memorial");
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
