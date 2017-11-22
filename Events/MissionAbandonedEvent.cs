using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MissionAbandonedEvent: Event
    {
        public const string NAME = "Mission abandoned";
        public const string DESCRIPTION = "Triggered when you abandon a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"MissionAbandoned\", \"Name\":\"Mission_PassengerVIP_name\", \"MissionID\":26493517 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionAbandonedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
        }

        [JsonProperty("missionid")]
        public long missionid { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        public MissionAbandonedEvent(DateTime timestamp, long missionid, string name) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
        }
    }
}
