using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class EnteredNormalSpaceEvent : Event
    {
        public const string NAME = "Entered normal space";
        public const string DESCRIPTION = "Triggered when your ship enters normal space";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseExit\",\"StarSystem\":\"Yuetu\",\"Body\":\"Yuetu B\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredNormalSpaceEvent()
        {
            VARIABLES.Add("system", "The system at which the commander has entered normal space");
            VARIABLES.Add("body", "The nearest body to the commander when entering normal space");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("body")]
        public string body{ get; private set; }

        public EnteredNormalSpaceEvent(DateTime timestamp, string system, string body) : base(timestamp, NAME)
        {
            this.system = system;
            this.body = body;
        }
    }
}
