using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class NPCCargoScanCommencedEvent : Event
    {
        public const string NAME = "NPC cargo scan commenced";
        public const string DESCRIPTION = "Triggered when a cargo scan on your ship by an NPC is detected";
        public static readonly NPCCargoScanCommencedEvent SAMPLE = new NPCCargoScanCommencedEvent(DateTime.UtcNow, MessageSource.Pirate);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NPCCargoScanCommencedEvent()
        {
            VARIABLES.Add("by", "The localized source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
            VARIABLES.Add("by_invariant", "The invariant source of the cargo scan (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
        }

        public string by => Source.localizedName;
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing
        public MessageSource Source { get; }

        public NPCCargoScanCommencedEvent(DateTime timestamp, MessageSource source) : base(timestamp, NAME)
        {
            Source = source;
        }
    }
}
