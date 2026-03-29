using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NPCCargoScanCommencedEvent ( DateTime timestamp, string from, MessageSource source )
        : Event( timestamp, NAME )
    {
        public const string NAME = "NPC cargo scan commenced";
        public const string DESCRIPTION = "Triggered when a cargo scan on your ship by an NPC is detected";
        public static readonly NPCCargoScanCommencedEvent SAMPLE = new(DateTime.UtcNow, "Nobatu", MessageSource.Pirate);

        [PublicAPI( "The name of the source attempting the cargo scan" )]
        public string from { get; private set; } = from;

        [PublicAPI("The localized source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by => Source.localizedName;

        [PublicAPI("The invariant source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing

        public MessageSource Source { get; } = source;
    }
}
