using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionAbandonedEvent ( DateTime timestamp, ulong missionid, string name, long fine )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission abandoned";
        public const string DESCRIPTION = "Triggered when you abandon a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionAbandoned\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517, \"Fine\":20000 }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionid;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The fine levied")]
        public long fine { get; private set; } = fine;
    }
}
