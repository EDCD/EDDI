﻿using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewMemberLeftEvent : Event
    {
        public const string NAME = "Crew member left";
        public const string DESCRIPTION = "Triggered when a commander leaves your crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewMemberQuits\",\"Crew\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the crew member who left")]
        public string crew { get; private set; }

        [PublicAPI("True if the crew member joined via telepresence")]
        public bool? telepresence { get; private set; }

        public CrewMemberLeftEvent(DateTime timestamp, string crew, bool? telepresence) : base(timestamp, NAME)
        {
            this.crew = crew;
            this.telepresence = telepresence;
        }
    }
}
