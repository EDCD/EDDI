using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class NPCInterdictionCommencedEvent : Event
    {
        public const string NAME = "NPC interdiction commenced";
        public const string DESCRIPTION = "Triggered when an interdiction attempt on your ship by an NPC is detected";
        public static readonly NPCInterdictionCommencedEvent SAMPLE = new NPCInterdictionCommencedEvent(DateTime.UtcNow, MessageSource.Police);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NPCInterdictionCommencedEvent()
        {
            VARIABLES.Add("by", "The localized source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
            VARIABLES.Add("by_invariant", "The invariant source of the interdiction (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
        }

        public string by => Source.localizedName;
        public string by_invariant => Source.invariantName;

        // Not intended to be user facing
        public MessageSource Source { get; }

        public NPCInterdictionCommencedEvent(DateTime timestamp, MessageSource source) : base(timestamp, NAME)
        {
            Source = source;
        }
    }
}
