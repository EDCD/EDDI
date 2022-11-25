using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberRoleChangedEvent : Event
    {
        public const string NAME = "Crew member role changed";
        public const string DESCRIPTION = "Triggered when a crew member changes their role";
        public const string SAMPLE = @"{ ""timestamp"":""2017-03-09T12:28:11Z"", ""event"":""CrewMemberRoleChange"", ""Crew"":""M. Volgrand"", ""Role"":""FighterCon"" }";

        [PublicAPI("The name of the crew member who changed their role")]
        public string crew { get; private set; }

        [PublicAPI("The new role of the crew member")]
        public string role { get; private set; }

        [PublicAPI("True if the crew member joined via telepresence")]
        public bool? telepresence { get; private set; }

        public CrewMemberRoleChangedEvent(DateTime timestamp, string crew, string role, bool? telepresence) : base(timestamp, NAME)
        {
            this.crew = crew;
            this.role = role;
            this.telepresence = telepresence;
        }
    }
}
