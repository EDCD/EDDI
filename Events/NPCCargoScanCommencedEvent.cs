using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NPCCargoScanCommencedEvent : Event
    {
        public const string NAME = "NPC cargo scan commenced";
        public const string DESCRIPTION = "Triggered when a cargo scan on your ship by an NPC is detected";
        public static readonly NPCCargoScanCommencedEvent SAMPLE = new NPCCargoScanCommencedEvent(DateTime.UtcNow, "Nobatu", MessageSource.Pirate);

        [PublicAPI( "The name of the source attempting the cargo scan" )]
        public string from { get; private set; }

        [PublicAPI("The localized source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by => Source.localizedName;

        [PublicAPI("The invariant source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing

        public MessageSource Source { get; }

        public NPCCargoScanCommencedEvent(DateTime timestamp, string from, MessageSource source) : base(timestamp, NAME)
        {
            this.from = from;
            Source = source;
        }
    }
}
