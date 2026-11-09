using System;

namespace EddiDataDefinitions
{
    // Matching bounties are observations, not Frontier's credited mission-kill counter.
    internal sealed record MissionCombatEvidence
    {
        public DateTime acceptedAt { get; init; }
        public long acceptanceOrder { get; init; }
        public int required { get; init; }
        public string targetSystem { get; init; }
        public bool partialHistory { get; init; }
        public long matchingBounties { get; init; }
        public DateTime? observedAt { get; init; }
        public long? bountyOrdinal { get; init; }
    }
}
