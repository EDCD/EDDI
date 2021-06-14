using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewPromotionEvent : Event
    {
        public const string NAME = "Crew promotion";
        public const string DESCRIPTION = "Triggered when crewmember combat rank increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"NpcCrewRank\",\"NpcCrewName\":\"Margaret Parrish\",\"NpcCrewId\":236064708,\"RankCombat\":3}";

        [PublicAPI("The name of the crewmember promoted")]
        public string name { get; private set; }

        [PublicAPI("The ID of the crewmember promoted")]
        public long crewid { get; private set; }

        [PublicAPI("The combat rating of the crewmember promoted")]
        public string combatrating { get; private set; }

        public CrewPromotionEvent(DateTime timestamp, string name, long crewid, CombatRating combatrating) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.combatrating = combatrating.localizedName;
        }
    }
}
