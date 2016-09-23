using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class SRVLaunchedEvent : Event
    {
        public const string NAME = "SRV launched";
        public const string DESCRIPTION = "Triggered when you launch an SRV from your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LaunchSRV\",\"Loadout\":\"starter\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVLaunchedEvent()
        {
            VARIABLES.Add("loadout", "The SRV's loadout");
        }

        [JsonProperty("loadout")]
        public string loadout { get; private set; }

        public SRVLaunchedEvent(DateTime timestamp, string loadout) : base(timestamp, NAME)
        {
            this.loadout = loadout;
        }
    }
}
