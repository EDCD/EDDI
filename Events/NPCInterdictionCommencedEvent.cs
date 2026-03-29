using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NPCInterdictionCommencedEvent ( DateTime timestamp, string from, MessageSource source )
        : Event( timestamp, NAME )
    {
        public const string NAME = "NPC interdiction commenced";
        public const string DESCRIPTION = "Triggered when an interdiction attempt on your ship by an NPC is detected";
        public static readonly NPCInterdictionCommencedEvent SAMPLE = new(DateTime.UtcNow, "Internal Security Service", MessageSource.Police);

        [PublicAPI( "The name of the source attempting the interdiction" )]
        public string from { get; private set; } = from;

        [PublicAPI("The localized source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by => Source.localizedName;

        [PublicAPI("The invariant source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing
        public MessageSource Source { get; } = source;
    }
}
