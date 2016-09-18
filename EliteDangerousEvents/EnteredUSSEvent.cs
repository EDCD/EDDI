using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class EnteredUSSEvent : Event
    {
        public const string NAME = "Entered USS";
        public const string DESCRIPTION = "Triggered when your ship enters a signal source";
        public static EnteredUSSEvent SAMPLE = new EnteredUSSEvent(DateTime.Now, "Disrupted wake echoes", 0);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredUSSEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"USSDrop\",\"USSType\":\"Disrupted wake echoes\",\"USSThreat\":0}";

            VARIABLES.Add("source", "The type of the signal source");
            VARIABLES.Add("threat", "The threat level of the signal source (0-4)");
        }

        [JsonProperty("source")]
        public string source { get; private set; }

        [JsonProperty("threat")]
        public int threat{ get; private set; }

        public EnteredUSSEvent(DateTime timestamp, string source, int threat) : base(timestamp, NAME)
        {
            this.source = source;
            this.threat = threat;
        }
    }
}
