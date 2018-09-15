using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public long? missionid { get; private set; }

        public string name { get; private set; }

        public MissionExpiredEvent(DateTime timestamp, long? MissionId, string Name) : base(timestamp, NAME)
        {
            this.missionid = MissionId;
            this.name = Name;
        }
    }
}
