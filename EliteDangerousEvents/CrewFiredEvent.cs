using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CrewFiredEvent : Event
    {
        public const string NAME = "Crew fired";
        public const string DESCRIPTION = "Triggered when you fire crew";
        public static CrewFiredEvent SAMPLE = new CrewFiredEvent(DateTime.Now, "Margaret Parrish");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewFiredEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewFire\",\"Name\":\"Margaret Parrish\"}";

            VARIABLES.Add("name", "The name of the crewmember being fired");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        public CrewFiredEvent(DateTime timestamp, string name) : base(timestamp, NAME)
        {
            this.name = name;
        }
    }
}
