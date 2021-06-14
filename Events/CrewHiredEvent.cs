using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewHiredEvent : Event
    {
        public const string NAME = "Crew hired";
        public const string DESCRIPTION = "Triggered when you hire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewHire\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Faction\":\"The Dark Wheel\",\"Cost\":15000,\"CombatRank\":1}";

        [PublicAPI("The name of the crewmember being hired")]
        public string name { get; private set; }

        [PublicAPI("The ID of the crewmember being hired")]
        public long crewid { get; private set; }

        [PublicAPI("The faction of the crewmember being hired")]
        public string faction { get; private set; }

        [PublicAPI("The price of the crewmember being hired")]
        public long price { get; private set; }

        [PublicAPI("The combat rating of the crewmember being hired")]
        public string combatrating { get; private set; }

        public CrewHiredEvent(DateTime timestamp, string name, long crewid, string faction, long price, CombatRating combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.faction = faction;
            this.price = price;
            this.combatrating = combatrating.localizedName;
        }
    }
}
