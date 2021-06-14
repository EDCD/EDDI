using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewAssignedEvent : Event
    {
        public const string NAME = "Crew assigned";
        public const string DESCRIPTION = "Triggered when you assign crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewAssign\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Role\":\"Active\"}";

        [PublicAPI("The name of the crewmember being assigned")]
        public string name { get; private set; }

        [PublicAPI("The ID of the crewmember being assigned")]
        public long crewid { get; private set; }

        [PublicAPI("The role to which the crewmember is being assigned")]
        public string role { get; private set; }

        public CrewAssignedEvent(DateTime timestamp, string name, long crewid, string role) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
            this.role = role;
        }
    }
}
