using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewMemberRoleChangedEvent : Event
    {
        public const string NAME = "Crew member role changed";
        public const string DESCRIPTION = "Triggered when a crew member changes their role";
        public const string SAMPLE = @"{ ""timestamp"":""2017-03-09T12:28:11Z"", ""event"":""CrewMemberRoleChange"", ""Crew"":""M. Volgrand"", ""Role"":""FighterCon"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewMemberRoleChangedEvent()
        {
            VARIABLES.Add("crew", "The name of the crew member who changed their role");
            VARIABLES.Add("role", "The new role of the crew member");
        }

        public string crew { get; private set; }

        public string role { get; private set; }

        public CrewMemberRoleChangedEvent(DateTime timestamp, string crew, string role) : base(timestamp, NAME)
        {
            this.crew = crew;
            this.role = role;
        }
    }
}
