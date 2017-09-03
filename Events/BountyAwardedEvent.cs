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
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:10:11Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""FrogCorp"", ""Reward"":400 }, { ""Faction"":""Federation"", ""Reward"":123187 } ], ""Target"":""federation_dropship_mkii"", ""TotalReward"":123587, ""VictimFaction"":""TZ Arietis Purple Council"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyAwardedEvent()
        {
            VARIABLES.Add("target", "The name of the pilot or ship you destroyed (if applicable)");
            VARIABLES.Add("faction", "The name of the faction whose ship you destroyed");
            VARIABLES.Add("reward", "The total number of credits obtained for destroying the ship");
            VARIABLES.Add("rewards", "The rewards obtained for destroying the ship");
            VARIABLES.Add("shared", "True if the rewards have been shared with wing-mates");
        }

        [JsonProperty("target")]
        public string target { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("reward")]
        public long reward { get; private set; }

        [JsonProperty("rewards")]
        public List<Reward> rewards { get; private set; }

        [JsonProperty("shared")]
        public bool shared { get; private set; }

        public BountyAwardedEvent(DateTime timestamp, string target, string faction, long reward, List<Reward> rewards, bool shared) : base(timestamp, NAME)
        {
            this.target = target;
            this.faction = faction;
            this.reward = reward;
            this.rewards = rewards;
            this.shared = shared;
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
