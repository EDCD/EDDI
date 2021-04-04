using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiMissionMonitor
{
    public class MissionExpiredEvent : Event
    {
        public const string NAME = "Mission expired";
        public const string DESCRIPTION = "Triggered when a mission has expired";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionExpiredEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
        }

        [PublicAPI]
        public long? missionid { get; private set; }

        [PublicAPI]
        public string name { get; private set; }

        public MissionExpiredEvent(DateTime timestamp, long? MissionId, string Name) : base(timestamp, NAME)
        {
            this.missionid = MissionId;
            this.name = Name;
        }
    }
}
