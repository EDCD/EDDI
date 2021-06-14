using EddiEvents;
using System;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionWarningEvent : Event
    {
        public const string NAME = "Mission warning";
        public const string DESCRIPTION = "Triggered when a mission is about to expire, based on a set threshold";
        public const string SAMPLE = null;

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The name of the mission")]
        public string name { get; private set; }

        [PublicAPI("The time remaining (in minutes) to complete the mission")]
        public int remaining { get; private set; }

        public MissionWarningEvent(DateTime timestamp, long? MissionId, string Name, int Remaining) : base(timestamp, NAME)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.remaining = Remaining;
        }
    }
}
