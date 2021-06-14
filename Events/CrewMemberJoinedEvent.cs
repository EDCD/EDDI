using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberJoinedEvent : Event
    {
        public const string NAME = "Crew member joined";
        public const string DESCRIPTION = "Triggered when a commander joins your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewMemberJoins\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the crew member who joined")]
        public string crew { get; private set; }

        public CrewMemberJoinedEvent(DateTime timestamp, string crew) : base(timestamp, NAME)
        {
            this.crew = crew;
        }
    }
}
