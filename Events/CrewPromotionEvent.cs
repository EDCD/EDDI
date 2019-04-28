using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewPromotionEvent : Event
    {
        public const string NAME = "Crew promotion";
        public const string DESCRIPTION = "Triggered when crewmember combat rank increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"NpcCrewRank\",\"NpcCrewName\":\"Margaret Parrish\",\"NpcCrewId\":236064708,\"RankCombat\":3}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewPromotionEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember promoted");
            VARIABLES.Add("crewid", "The ID of the crewmember promoted");
            VARIABLES.Add("combatrating", "The combat rating of the crewmember promoted");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("crewid")]
        public long crewid { get; private set; }

        [JsonProperty("combatrating")]
        public string combatrating { get; private set; }

        public CrewPromotionEvent(DateTime timestamp, string name, long crewid, CombatRating combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.combatrating = combatrating.localizedName;
        }
    }
}
