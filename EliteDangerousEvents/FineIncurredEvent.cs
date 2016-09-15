using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class FineIncurredEvent : Event
    {
        public const string NAME = "Fine incurred";
        public static FineIncurredEvent SAMPLE = new FineIncurredEvent(DateTime.Now, "friendly fire", "The Pilot's Federation", "Braben", 1000000M);

        static FineIncurredEvent()
        {
            VARIABLES.Add("crimetype", "The type of crime committed");
            VARIABLES.Add("victim", "The name of the victim of the crime");
            VARIABLES.Add("faction", "The name of the faction issuing the fine");
            VARIABLES.Add("bounty", "The number of credits issued as the fine");
        }

        [JsonProperty("crimetype")]
        public string crimetype { get; private set; }

        [JsonProperty("victim")]
        public string victim { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("fine")]
        public decimal fine { get; private set; }

        public FineIncurredEvent(DateTime timestamp, string crimetype, string faction, string victim, decimal fine) : base(timestamp, NAME)
        {
            this.crimetype = crimetype;
            this.faction = faction;
            this.victim = victim;
            this.fine = fine;
        }
    }
}
