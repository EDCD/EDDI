using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewRoleChangedEvent : Event
    {
        public const string NAME = "Crew role changed";
        public const string DESCRIPTION = "Triggered when your role in the crew changes";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"ChangeCrewRole\",\"Role\":\"FireCon\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewRoleChangedEvent()
        {
            VARIABLES.Add("role", "The crew role you have been assigned (gunner, fighter, idle)");
        }

        [JsonProperty("role")]
        public string role { get; private set; }

        public CrewRoleChangedEvent(DateTime timestamp, string role) : base(timestamp, NAME)
        {
            this.role = role;
        }
    }
}
