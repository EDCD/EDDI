using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class BountyIncurredEvent : Event
    {
        public const string NAME = "Bounty incurred";
        public const string DESCRIPTION = "Triggered when you incur a bounty";
        public static BountyIncurredEvent SAMPLE = new BountyIncurredEvent(DateTime.Now, "Assault", "The Pilot's Federation", "Potapinski", 210M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyIncurredEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CommitCrime\",\"CrimeType\":\"assault\",\"Faction\":\"The Pilots Federation\",\"Victim\":\"Potapinski\",\"Bounty\":210}";

            VARIABLES.Add("crimetype", "The type of crime committed");
            VARIABLES.Add("victim", "The name of the victim of the crime");
            VARIABLES.Add("faction", "The name of the faction issuing the bounty");
            VARIABLES.Add("bounty", "The number of credits issued as the bounty");
        }

        [JsonProperty("crimetype")]
        public string crimetype { get; private set; }

        [JsonProperty("victim")]
        public string victim { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("bounty")]
        public decimal bounty { get; private set; }

        public BountyIncurredEvent(DateTime timestamp, string crimetype, string faction, string victim, decimal bounty) : base(timestamp, NAME)
        {
            this.crimetype = crimetype;
            this.faction = faction;
            this.victim = victim;
            this.bounty = bounty;
        }
    }
}
