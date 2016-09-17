using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class DockingGrantedEvent : Event
    {
        public const string NAME = "Docking granted";
        public const string DESCRIPTION = "Triggered when your ship is granted docking permission at a station or outpost";
        public static DockingGrantedEvent SAMPLE = new DockingGrantedEvent(DateTime.Now, "Jameson Memorial", 2);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingGrantedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingDenied\",\"StationName\":\"Jameson Memorial\",\"LandingPad\":2}";

            VARIABLES.Add("station", "The station at which the commander has been granted docking");
            VARIABLES.Add("landingpad", "The landing apd at which the commander has been granted docking");
        }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("landingpad")]
        public int landingpad { get; private set; }

        public DockingGrantedEvent(DateTime timestamp, string station, int landingpad) : base(timestamp, NAME)
        {
            this.station = station;
            this.landingpad = landingpad;
        }
    }
}
