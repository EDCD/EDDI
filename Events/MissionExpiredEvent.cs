using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionExpiredEvent ( DateTime timestamp, ulong missionId, string name ) : Event( timestamp, NAME )
    {
        public const string NAME = "Mission expired";
        public const string DESCRIPTION = "Triggered when a mission has expired";
        public const string SAMPLE = null;

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionId;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;
    }
}
