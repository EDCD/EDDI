using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CrewHiredEvent : Event
    {
        public const string NAME = "Crew hired";
        public const string DESCRIPTION = "Triggered when you hire crew";
        public static CrewHiredEvent SAMPLE = new CrewHiredEvent(DateTime.Now, "Margaret Parrish", "The Dark Wheel", 15000, 1);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewHiredEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewHire\",\"Name\":\"Margaret Parrish\",\"Faction\":\"The Dark Wheel\",\"Cost\":15000,\"CombatRank\":1}";

            VARIABLES.Add("name", "The name of the crewmember being hired");
            VARIABLES.Add("faction", "The faction of the crewmember being hired");
            VARIABLES.Add("cost", "The cost of the crewmember being hired");
            VARIABLES.Add("combatrank", "The combat rank of the crewmember being hired");
            VARIABLES.Add("combatrating", "The combat rating of the crewmember being hired");
        }

        [JsonProperty("name")]
        public string name { get; private set; }
        [JsonProperty("faction")]
        public string faction { get; private set; }
        [JsonProperty("cost")]
        public decimal cost { get; private set; }
        [JsonProperty("combatrank")]
        public string combatrank { get; private set; }
        [JsonProperty("combatrating")]
        public decimal combatrating { get; private set; }

        public CrewHiredEvent(DateTime timestamp, string name, string factinon, decimal cost, int combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.faction = faction;
            this.cost = cost;
            this.combatrating = combatrating;
            this.combatrank = Commander.combatRanks[combatrating];
        }
    }
}
