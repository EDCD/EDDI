using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EddiDataDefinitions;
using Newtonsoft.Json;

namespace EddiEvents
{
    public class MissionsEvent : Event
    {
        public const string NAME = "Technology broker";
        public const string DESCRIPTION = "Triggered when using the Technology Broker to unlock new purchasable technology";
        public const string SAMPLE = "{ \"timestamp\":\"2017-10-02T10:37:58Z\", \"event\":\"Missions\", \"Active\":[ { \"MissionID\":65380900, \"Name\":\"Mission_Courier_name\", \"PassengerMission\":false, \"Expires\":82751 } ], \"Failed\":[  ], \"Complete\":[  ]}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionsEvent()
        {
            VARIABLES.Add("missions", "missions in the mission log (this is a list of Mission objects)");
        }

        [JsonProperty("missions")]
        public List<Mission> missions { get; private set; }

       public MissionsEvent(DateTime timestamp, List<Mission> missions) : base(timestamp, NAME)
        {
            this.missions = missions;
        }
    }
}
