using System;

namespace EddiEvents
{
    [Utilities.PublicAPI]
    public class CrewPaidWageEvent ( DateTime timestamp, string name, long crewid, long amount )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew paid wage";
        public const string DESCRIPTION = "Triggered when npc crew receives a profit share";
        public const string SAMPLE = "{\"timestamp\":\"2019-03-09T18:46:52Z\", \"event\":\"NpcCrewPaidWage\", \"NpcCrewName\":\"Xenia Hoover\", \"NpcCrewId\":236064708, \"Amount\":8649}";

        [Utilities.PublicAPI("The name of the crewmember")]
        public string name { get; private set; } = name;

        [Utilities.PublicAPI("The ID of the crewmember")]
        public long crewid { get; private set; } = crewid;

        [Utilities.PublicAPI("The amount paid to the crewmember")]
        public long amount { get; private set; } = amount;
    }
}
