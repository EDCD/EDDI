using System;

namespace EddiDataDefinitions
{
    // Arrays are privately owned and replaced, never edited after publication.
    internal sealed record MissionWaypointEvidence
    {
        public string[] systems { get; init; } = [ ];
        public string[] scannedSystems { get; init; } = [ ];
        public DateTime? observedAt { get; init; }
    }
}
