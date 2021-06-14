using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewPaidWageEvent : Event
    {
        public const string NAME = "Crew paid wage";
        public const string DESCRIPTION = "Triggered when npc crew receives a profit share";
        public const string SAMPLE = "{\"timestamp\":\"2019-03-09T18:46:52Z\", \"event\":\"NpcCrewPaidWage\", \"NpcCrewName\":\"Xenia Hoover\", \"NpcCrewId\":236064708, \"Amount\":0}";

        [PublicAPI("The name of the crewmember")]
        public string name { get; private set; }

        [PublicAPI("The ID of the crewmember")]
        public long crewid { get; private set; }

        [PublicAPI("The amount paid to the crewmember")]
        public long amount { get; private set; }

        public CrewPaidWageEvent(DateTime timestamp, string name, long crewid, long amount) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.amount = amount;
        }
    }
}
