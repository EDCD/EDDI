using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class BountyAwardedEvent : Event
    {
        public const string NAME = "Bounty awarded";
        public static BountyAwardedEvent SAMPLE = new BountyAwardedEvent(DateTime.Now, "The Pilot's Federation", "cobramkiii", "The Dark Wheel", 1250M);

        static BountyAwardedEvent()
        {
            VARIABLES.Add("awardingfaction", "The name of the faction awarding the bounty");
            VARIABLES.Add("target", "The name of the pilot you destroyed");
            VARIABLES.Add("victimfaction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The number of credits received");
        }

        [JsonProperty("awardingfaction")]
        public string awardingfaction { get; private set; }

        [JsonProperty("target")]
        public string target { get; private set; }

        [JsonProperty("victimfaction")]
        public string victimfaction { get; private set; }

        [JsonProperty("reward")]
        public decimal reward { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string awardingfaction, string target, string victimfaction, decimal reward) : base(timestamp, NAME)
        {
            this.awardingfaction = awardingfaction;
            this.target = target;
            this.victimfaction = victimfaction;
            this.reward = reward;
        }
    }
}
