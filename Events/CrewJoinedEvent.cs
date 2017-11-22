using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewJoinedEvent : Event
    {
        public const string NAME = "Crew joined";
        public const string DESCRIPTION = "Triggered when you join a crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"JoinACrew\",\"Captain\":\"$cmdr_decorate:#name=Jameson;\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewJoinedEvent()
        {
            VARIABLES.Add("captain", "The name of the captain of the crew you have joined");
        }

        [JsonProperty("captain")]
        public string captain { get; private set; }

        public CrewJoinedEvent(DateTime timestamp, string captain) : base(timestamp, NAME)
        {
            this.captain = captain;
        }
    }
}
