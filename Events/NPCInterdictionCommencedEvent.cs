using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NPCInterdictionCommencedEvent : Event
    {
        public const string NAME = "NPC interdiction commenced";
        public const string DESCRIPTION = "Triggered when an interdiction attempt on your ship by an NPC is detected";
        public static readonly NPCInterdictionCommencedEvent SAMPLE = new NPCInterdictionCommencedEvent(DateTime.UtcNow, "Internal Security Service", MessageSource.Police);

        [PublicAPI( "The name of the source attempting the interdiction" )]
        public string from { get; private set; }

        [PublicAPI("The localized source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by => Source.localizedName;

        [PublicAPI("The invariant source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)")]
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing
        public MessageSource Source { get; }

        public NPCInterdictionCommencedEvent(DateTime timestamp, string from, MessageSource source) : base(timestamp, NAME)
        {
            this.from = from;
            Source = source;
        }
    }
}
