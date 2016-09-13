using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class BondAwardedEvent : Event
    {
        public const string NAME = "Bond awarded";
        public static BondAwardedEvent SAMPLE = new BondAwardedEvent(DateTime.Now, "The Pilot's Federation", "The Dark Wheel", 1250M);

        [JsonProperty("awardingfaction")]
        public string awardingfaction { get; private set; }

        [JsonProperty("victimfaction")]
        public string victimfaction { get; private set; }

        [JsonProperty("reward")]
        public decimal reward { get; private set; }

        public BondAwardedEvent(DateTime timestamp, string awardingfaction, string victimfaction, decimal reward) : base(timestamp, NAME)
        {
            this.awardingfaction = awardingfaction;
            this.victimfaction = victimfaction;
            this.reward = reward;
        }
    }
}
