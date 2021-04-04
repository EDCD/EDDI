using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiMissionMonitor
{
    public class MissionAbandonedEvent : Event
    {
        public const string NAME = "Mission abandoned";
        public const string DESCRIPTION = "Triggered when you abandon a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionAbandoned\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517, \"Fine\":20000 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionAbandonedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("fine", "The fine levied");
        }

        [PublicAPI]
        public long? missionid { get; private set; }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public long fine { get; private set; }

        public MissionAbandonedEvent(DateTime timestamp, long? missionid, string name, long fine) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.fine = fine;
        }
    }
}
