using System;
using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommunityGoal
    {
        [JsonProperty("CGID")]
        public int cgid { get; set; }

        [JsonProperty("Title")]
        public string name { get; set; }

        [JsonProperty("SystemName")]
        public string system { get; set; }

        [JsonProperty("MarketName")]
        public string station { get; set; }

        [JsonProperty("Expiry")]
        public DateTime expiryDateTime { get; set; }

        [JsonIgnore]
        public long expiry => Dates.fromDateTimeToSeconds(expiryDateTime);

        [JsonProperty("IsComplete")]
        public bool iscomplete { get; set; }

        [JsonProperty("CurrentTotal")]
        public long total { get; set; }

        [JsonProperty("PlayerContribution")]
        public long contribution { get; set; }

        [JsonProperty("NumContributors")]
        public int contributors { get; set; }

        /// <summary> An integer percentile like 100, 75, 50, 25, 10, and (for fixed size top ranks only) 0 </summary>
        [JsonProperty("PlayerPercentileBand")]
        public int percentileband { get; set; }

        // Top Tier data (top rank size or may not be fixed - i.e. max reward for top 10 players)

        [JsonProperty("TopTier")]
        public TopTier TopTier { get; set; }

        [JsonIgnore]
        public int toptier => TopTier?.Name == null ? 0 : int.Parse(TopTier.Name.Replace("Tier ", ""));

        [JsonIgnore]
        public string toptierreward => TopTier?.Bonus;

        [JsonProperty("TopRankSize")]
        public int? topranksize { get; set; }

        [JsonProperty("PlayerInTopRank")]
        public bool? toprank { get; set; }

        // Current Tier data (only written once Tier 1 or greater has been reached)

        /// <summary> A string similar to "Tier 5" </summary>
        [JsonProperty("TierReached")]
        public string Tier { get; set; }

        [JsonIgnore]
        public int tier => Tier == null ? 0 : int.Parse(Tier.Replace("Tier ", ""));

        [JsonProperty("Bonus")]
        public long? tierreward { get; set; }
    }

    public class TopTier
    {
        /// <summary> A string similar to "Tier 8" </summary>
        public string Name { get; set; }

        /// <summary> A string similar to "All systems supported" </summary>
        public string Bonus { get; set; }

        public TopTier(string name, string bonus = null)
        {
            this.Name = name;
            this.Bonus = bonus;
        }
    }
}
