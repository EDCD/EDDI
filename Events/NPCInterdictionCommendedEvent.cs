using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class NPCInterdictionCommencedEvent : Event
    {
        public const string NAME = "NPC interdiction commenced";
        public const string DESCRIPTION = "Triggered when an interdiction attempt on your ship by an NPC is detected";
        public static readonly NPCInterdictionCommencedEvent SAMPLE = new NPCInterdictionCommencedEvent(DateTime.Now, "Police");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NPCInterdictionCommencedEvent()
        {
            VARIABLES.Add("by", "Who the interdiction is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
        }

        public string by { get; private set; }

        public NPCInterdictionCommencedEvent(DateTime timestamp, string by) : base(timestamp, NAME)
        {
            this.by = by;
        }
    }
}
