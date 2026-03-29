using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NPCAttackCommencedEvent ( DateTime timestamp, string from, MessageSource source )
        : Event( timestamp, NAME )
    {
        public const string NAME = "NPC attack commenced";
        public const string DESCRIPTION = "Triggered when an attack on your ship by an NPC is detected";
        public static readonly NPCAttackCommencedEvent SAMPLE = new(DateTime.UtcNow, "Herne", MessageSource.BountyHunter);

        [PublicAPI( "The name of the source attacking you" )]
        public string from { get; private set; } = from;

        [PublicAPI("The localized source of the attack (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by => Source.localizedName;

        [PublicAPI("The invariant source of the attack (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing

        public MessageSource Source { get; } = source;
    }
}
