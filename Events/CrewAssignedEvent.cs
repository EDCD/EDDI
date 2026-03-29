using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewAssignedEvent ( DateTime timestamp, string name, long crewid, string role )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew assigned";
        public const string DESCRIPTION = "Triggered when you assign crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewAssign\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Role\":\"Active\"}";

        [PublicAPI("The name of the crewmember being assigned")]
        public string name { get; private set; } = name;

        [PublicAPI("The ID of the crewmember being assigned")]
        public long crewid { get; private set; } = crewid;

        [PublicAPI("The role to which the crewmember is being assigned")]
        public string role { get; private set; } = role;
    }
}
