using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewHiredEvent : Event
    {
        public const string NAME = "Crew hired";
        public const string DESCRIPTION = "Triggered when you hire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewHire\",\"Name\":\"Margaret Parrish\",\"Faction\":\"The Dark Wheel\",\"Cost\":15000,\"CombatRank\":1}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewHiredEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember being hired");
            VARIABLES.Add("faction", "The faction of the crewmember being hired");
            VARIABLES.Add("price", "The price of the crewmember being hired");
            VARIABLES.Add("combatrating", "The combat rating of the crewmember being hired");
        }

        [JsonProperty("name")]
        public string name { get; private set; }
        [JsonProperty("faction")]
        public string faction { get; private set; }
        [JsonProperty("price")]
        public long price { get; private set; }
        [JsonProperty("combatrating")]
        public string combatrating { get; private set; }

        public CrewHiredEvent(DateTime timestamp, string name, string faction, long price, CombatRating combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.faction = faction;
            this.price = price;
            this.combatrating = combatrating.name;
        }
    }
}
