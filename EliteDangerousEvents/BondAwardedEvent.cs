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
        public const string DESCRIPTION = "Triggered when you are awarded a combat bond";
        public static BondAwardedEvent SAMPLE = new BondAwardedEvent(DateTime.Now, "The Pilot's Federation", "The Dark Wheel", 1250M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BondAwardedEvent()
        {
            VARIABLES.Add("awardingfaction", "The name of the faction awarding the bond");
            VARIABLES.Add("victimfaction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The number of credits received");
        }

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
