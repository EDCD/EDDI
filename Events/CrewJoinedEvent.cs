using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewJoinedEvent : Event
    {
        public const string NAME = "Crew joined";
        public const string DESCRIPTION = "Triggered when you join a crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"JoinACrew\",\"Captain\":\"$cmdr_decorate:#name=Jameson;\"}";

        [PublicAPI("The name of the captain of the crew you have joined")]
        public string captain { get; private set; }

        public CrewJoinedEvent(DateTime timestamp, string captain) : base(timestamp, NAME)
        {
            this.captain = captain;
        }
    }
}
