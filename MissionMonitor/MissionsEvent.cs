using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiMissionMonitor
{
    public class MissionsEvent : Event
    {
        public const string NAME = "Missions";
        public const string DESCRIPTION = "Triggered at startup, with basic information of the Mission Log";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionsEvent()
        {
        }

        // Not intended to be user facing

        public List<Mission> missions { get; private set; }

        public MissionsEvent(DateTime timestamp, List<Mission> missions) : base(timestamp, NAME)
        {
            this.missions = missions;
        }
    }
}
