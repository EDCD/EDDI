using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class BountyAwardedEvent : Event
    {
        public const string NAME = "Bounty awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a bounty";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Bounty\",\"Faction\":\"$faction_Federation;\",\"Target\":\"Skimmer\",\"Reward\":1000,\"VictimFaction\":\"MMU\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyAwardedEvent()
        {
            VARIABLES.Add("faction", "The name of the faction awarding the bounty");
            VARIABLES.Add("target", "The name of the pilot you destroyed");
            VARIABLES.Add("victimfaction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The number of credits received");
        }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("target")]
        public string target { get; private set; }

        [JsonProperty("victimfaction")]
        public string victimfaction { get; private set; }

        [JsonProperty("reward")]
        public long reward { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string faction, string target, string victimfaction, long reward) : base(timestamp, NAME)
        {
            this.faction = faction;
            this.target = target;
            this.victimfaction = victimfaction;
            this.reward = reward;
        }
    }
}
