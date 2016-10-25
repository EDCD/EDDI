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
        public const string SAMPLE = "{ \"timestamp\":\"2016-10-05T11:28:53Z\", \"event\":\"Bounty\", \"Rewards\":[ { \"Faction\":\"The Dark Wheel\", \"Reward\":9840 }, { \"Faction\":\"The Pilots Federation\", \"Reward\":21255 } ], \"TotalReward\":31095, \"VictimFaction\":\"Future of Arro Naga\" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyAwardedEvent()
        {
            VARIABLES.Add("target", "The name of the pilot you destroyed");
            VARIABLES.Add("faction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The total number of credits obtained for destroying the ship");
            VARIABLES.Add("rewards", "The rewards obtained for destroying the ship");
        }

        [JsonProperty("target")]
        public string target { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("reward")]
        public long reward { get; private set; }

        [JsonProperty("rewards")]
        public List<Reward> rewards { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string target, string faction, long reward, List<Reward> rewards) : base(timestamp, NAME)
        {
            this.target = target;
            this.faction = faction;
            this.reward = reward;
            this.rewards = rewards;
        }
    }

    public class Reward
    {
        public string faction { get; private set; }
        public decimal amount { get; private set; }

        public Reward(string faction, decimal amount)
        {
            this.faction = faction;
            this.amount = amount;
        }
    }
}
