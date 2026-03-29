using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberRemovedEvent ( DateTime timestamp, string crew, bool? telepresence )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew member removed";
        public const string DESCRIPTION = "Triggered when you remove a commander from your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"KickCrewMember\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the crew member who was removed")]
        public string crew { get; private set; } = crew;

        [PublicAPI("True if the crew member joined via telepresence")]
        public bool? telepresence { get; private set; } = telepresence;
    }
}
