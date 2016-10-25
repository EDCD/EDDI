using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class MissionCompletedEvent: Event
    {
        public const string NAME = "Mission completed";
        public const string DESCRIPTION = "Triggered when you complete a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T11:54:32Z\", \"event\":\"MissionCompleted\", \"Faction\":\"Partnership of GD 215\", \"Name\":\"Mission_PassengerVIP_CEO_BUST_name\", \"MissionID\":26493515, \"Commodity\":\"$FruitAndVegetables_Name;\", \"Commodity_Localised\":\"Fruit and Vegetables\", \"Count\":2, \"DestinationSystem\":\"GD 215\", \"DestinationStation\":\"Kingsbury Base\", \"Reward\":0 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionCompletedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("system", "The system in which the mission was obtained");
            VARIABLES.Add("communal", "True if the mission is a community goal");
            VARIABLES.Add("reward", "The monetary reward for completing the mission");
            VARIABLES.Add("donation", "The monetary donatin when completing the mission");
        }

        [JsonProperty("missionid")]
        public long? missionid { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("communal")]
        public bool communal { get; private set; }

        [JsonProperty("reward")]
        public long reward { get; private set; }

        [JsonProperty("donation")]
        public long donation { get; private set; }

        public MissionCompletedEvent(DateTime timestamp, long? missionid, string name, string system, bool communal, long reward, long donation) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.system = system;
            this.communal = communal;
            this.reward = reward;
        }
    }
}
