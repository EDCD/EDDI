using EddiEvents;
using System;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionFailedEvent : Event
    {
        public const string NAME = "Mission failed";
        public const string DESCRIPTION = "Triggered when you fail a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionFailed\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517 }";

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The name of the mission")]
        public string name { get; private set; }

        [PublicAPI("The fine levied")]
        public long fine { get; private set; }

        public MissionFailedEvent(DateTime timestamp, long? missionid, string name, long fine) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.fine = fine;
        }
    }
}
