using EddiEvents;
using System;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionExpiredEvent : Event
    {
        public const string NAME = "Mission expired";
        public const string DESCRIPTION = "Triggered when a mission has expired";
        public const string SAMPLE = null;

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The name of the mission")]
        public string name { get; private set; }

        public MissionExpiredEvent(DateTime timestamp, long? MissionId, string Name) : base(timestamp, NAME)
        {
            this.missionid = MissionId;
            this.name = Name;
        }
    }
}
