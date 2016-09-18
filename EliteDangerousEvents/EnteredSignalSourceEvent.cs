using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class EnteredSignalSourceEvent : Event
    {
        public const string NAME = "Entered signal source";
        public const string DESCRIPTION = "Triggered when your ship enters a signal source";
        public static EnteredSignalSourceEvent SAMPLE = new EnteredSignalSourceEvent(DateTime.Now, SignalSource.None, 0);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredSignalSourceEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"USSDrop\",\"USSType\":\"Disrupted wake echoes\",\"USSThreat\":0}";

            VARIABLES.Add("source", "The type of the signal source");
            VARIABLES.Add("threat", "The threat level of the signal source (0-4)");
        }

        [JsonProperty("source")]
        public SignalSource source { get; private set; }

        [JsonProperty("threat")]
        public int threat{ get; private set; }

        public EnteredSignalSourceEvent(DateTime timestamp, SignalSource source, int threat) : base(timestamp, NAME)
        {
            this.source = source;
            this.threat = threat;
        }
    }
}
