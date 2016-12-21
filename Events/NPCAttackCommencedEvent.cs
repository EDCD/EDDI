using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class NPCAttackCommencedEvent : Event
    {
        public const string NAME = "NPC attack commenced";
        public const string DESCRIPTION = "Triggered when an attach on your ship by an NPC is detected";
        public static readonly NPCAttackCommencedEvent SAMPLE = new NPCAttackCommencedEvent(DateTime.Now, "Bounty hunter");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NPCAttackCommencedEvent()
        {
            VARIABLES.Add("by", "Who the attack is by (Pirate, Military, Bounty hunter, Cargo hunter, etc)");
        }

        public string by { get; private set; }

        public NPCAttackCommencedEvent(DateTime timestamp, string by) : base(timestamp, NAME)
        {
            this.by = by;
        }
    }
}
