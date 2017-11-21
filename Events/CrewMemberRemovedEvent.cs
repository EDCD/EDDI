using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewMemberRemovedEvent : Event
    {
        public const string NAME = "Crew member removed";
        public const string DESCRIPTION = "Triggered when you remove a commander from your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"KickCrewMember\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewMemberRemovedEvent()
        {
            VARIABLES.Add("crew", "The name of the crew member who was removed");
        }

        [JsonProperty("crew")]
        public string crew { get; private set; }

        public CrewMemberRemovedEvent(DateTime timestamp, string crew) : base(timestamp, NAME)
        {
            this.crew = crew;
        }
    }
}
