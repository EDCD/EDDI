using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionWarningEvent ( DateTime timestamp, ulong missionId, string name, int remaining )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Mission warning";
        public const string DESCRIPTION = "Triggered when a mission is about to expire, based on a set threshold";
        public const string SAMPLE = null;

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; private set; } = missionId;

        [PublicAPI("The name of the mission")]
        public string name { get; private set; } = name;

        [PublicAPI("The time remaining (in minutes) to complete the mission")]
        public int remaining { get; private set; } = remaining;
    }
}
