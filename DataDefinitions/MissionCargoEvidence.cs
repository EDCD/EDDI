using System;

namespace EddiDataDefinitions
{
    // Persist observations, not derived lifecycle outcomes. Replace the record atomically so UI readers
    // use the same required quantity and cargo quantity throughout a calculation.
    internal sealed record MissionCargoEvidence
    {
        public int? required { get; init; }
        public DateTime? requiredAt { get; init; }
        public int? collected { get; init; }
        public DateTime? collectedAt { get; init; }
        public int? delivered { get; init; }
        public DateTime? deliveredAt { get; init; }
        public int? onboard { get; init; }
        public int? onboardRequired { get; init; }
        public DateTime? onboardAt { get; init; }
    }
}
