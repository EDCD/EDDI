using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewAssignedEvent : Event
    {
        public const string NAME = "Crew assigned";
        public const string DESCRIPTION = "Triggered when you assign crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewAssign\",\"Name\":\"Margaret Parrish\",\"Role\":\"Active\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewAssignedEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember being assigned");
            VARIABLES.Add("role", "The role to which the crewmember is being assigned");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("role")]
        public string role { get; private set; }

        public CrewAssignedEvent(DateTime timestamp, string name, string role) : base(timestamp, NAME)
        {
            this.name = name;
            this.role = role;
        }
    }
}
