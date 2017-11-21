using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewMemberLeftEvent : Event
    {
        public const string NAME = "Crew member left";
        public const string DESCRIPTION = "Triggered when a commander leaves your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewMemberQuits\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewMemberLeftEvent()
        {
            VARIABLES.Add("crew", "The name of the crew member who left");
        }

        [JsonProperty("crew")]
        public string crew { get; private set; }

        public CrewMemberLeftEvent(DateTime timestamp, string crew) : base(timestamp, NAME)
        {
            this.crew = crew;
        }
    }
}
