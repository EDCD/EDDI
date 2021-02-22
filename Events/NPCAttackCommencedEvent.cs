using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class NPCAttackCommencedEvent : Event
    {
        public const string NAME = "NPC attack commenced";
        public const string DESCRIPTION = "Triggered when an attack on your ship by an NPC is detected";
        public static readonly NPCAttackCommencedEvent SAMPLE = new NPCAttackCommencedEvent(DateTime.UtcNow, MessageSource.BountyHunter);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NPCAttackCommencedEvent()
        {
            VARIABLES.Add("by", "Who the attack is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
        }

        public string by => Source.localizedName;

        // Not intended to be user facing
        public MessageSource Source { get; }

        public NPCAttackCommencedEvent(DateTime timestamp, MessageSource source) : base(timestamp, NAME)
        {
            Source = source;
        }
    }
}
