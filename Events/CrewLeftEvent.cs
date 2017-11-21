using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewLeftEvent : Event
    {
        public const string NAME = "Crew left";
        public const string DESCRIPTION = "Triggered when you leave a crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"QuitACrew\",\"Captain\":\"$cmdr_decorate:#name=Jameson;\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewLeftEvent()
        {
            VARIABLES.Add("captain", "The name of the captain of the crew you have left");
        }

        [JsonProperty("captain")]
        public string captain { get; private set; }

        public CrewLeftEvent(DateTime timestamp, string captain) : base(timestamp, NAME)
        {
            this.captain = captain;
        }
    }
}
