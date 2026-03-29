using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionFailedEvent ( DateTime timestamp, ulong missionid, string name, long fine )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission failed";
        public const string DESCRIPTION = "Triggered when you fail a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionFailed\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517 }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionid;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The fine levied")]
        public long fine { get; private set; } = fine;
    }
}
