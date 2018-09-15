using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiMissionMonitor
{
    public class MissionWarningEvent : Event
    {
        public const string NAME = "Mission warning";
        public const string DESCRIPTION = "Triggered when a mission is about to expire, based on a set threshold";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionWarningEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("remaining", "The time remaining (in minutes) to complete the mission");
        }

        public long? missionid { get; private set; }

        public string name { get; private set; }

        public int remaining { get; private set; }

        public MissionWarningEvent(DateTime timestamp, long? MissionId, string Name, int Remaining) : base(timestamp, NAME)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.remaining = Remaining;
        }
    }
}
