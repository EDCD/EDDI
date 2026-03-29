using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberJoinedEvent ( DateTime timestamp, string crew, bool? telepresence )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew member joined";
        public const string DESCRIPTION = "Triggered when a commander joins your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewMemberJoins\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the crew member who joined")]
        public string crew { get; private set; } = crew;

        [PublicAPI("True if the crew member joined via telepresence")]
        public bool? telepresence { get; private set; } = telepresence;
    }
}
