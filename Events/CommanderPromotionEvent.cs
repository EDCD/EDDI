using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderPromotionEvent : Event
    {
        public const string NAME = "Commander promotion";
        public const string DESCRIPTION = "Triggered when one of your commander ranks increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Promotion\",\"Combat\":5}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderPromotionEvent()
        {
            VARIABLES.Add("rank_type", "The type of rank being promoted");
            VARIABLES.Add("rank", "The commander's new rank");
            VARIABLES.Add("rank_invariant", "The commander's new invariant rank");
        }

        [JsonProperty("rank_type")]
        public string rank_type { get; private set; }

        [JsonProperty("rank")]
        public string rank { get; private set; }

        [JsonProperty("rank_invariant")]
        public string rank_invariant { get; private set; }

        // Not intended to be user facing
        public object ratingObject { get; }

        public CommanderPromotionEvent(DateTime timestamp, object ratingObject, string gender = null) : base(timestamp, NAME)
        {
            string getRankType (object ratingObj)
            {
                var temp = ratingObj.GetType();
                return ratingObj
                    .GetType()
                    .Name
                    .Replace("Rating", "")
                    .Replace("Ranking", "")
                    .Replace("Rank", "");
            }

            if (ratingObject is CombatRating combatRating)
            {
                this.rank_type = getRankType(combatRating);
                this.rank = combatRating.localizedName;
                this.rank_invariant = combatRating.invariantName;
            }
            else if (ratingObject is CQCRating cqcRating)
            {
                this.rank_type = getRankType(cqcRating);
                this.rank = cqcRating.localizedName;
                this.rank_invariant = cqcRating.invariantName;
            }
            else if (ratingObject is EmpireRating empireRating)
            {
                this.rank_type = getRankType(empireRating);
                if (string.Equals(gender, "Female", StringComparison.InvariantCultureIgnoreCase))
                {
                    this.rank = empireRating.femaleRank.localizedName;
                    this.rank_invariant = empireRating.femaleRank.invariantName;
                }
                else
                {
                    this.rank = empireRating.maleRank.localizedName;
                    this.rank_invariant = empireRating.maleRank.invariantName;
                }
            }
            else if (ratingObject is ExplorationRating explorationRating)
            {
                this.rank_type = getRankType(explorationRating);
                this.rank = explorationRating.localizedName;
                this.rank_invariant = explorationRating.invariantName;
            }
            else if (ratingObject is ExobiologistRating exobiologistRating)
            {
                this.rank_type = getRankType(exobiologistRating);
                this.rank = exobiologistRating.localizedName;
                this.rank_invariant = exobiologistRating.invariantName;
            }
            else if (ratingObject is FederationRating federationRating)
            {
                this.rank_type = getRankType(federationRating);
                this.rank = federationRating.localizedName;
                this.rank_invariant = federationRating.invariantName;
            }
            else if (ratingObject is MercenaryRating mercenaryRating)
            {
                this.rank_type = getRankType(mercenaryRating);
                this.rank = mercenaryRating.localizedName;
                this.rank_invariant = mercenaryRating.invariantName;
            }
            else if (ratingObject is TradeRating tradeRating)
            {
                this.rank_type = getRankType(tradeRating);
                this.rank = tradeRating.localizedName;
                this.rank_invariant = tradeRating.invariantName;
            }
            this.ratingObject = ratingObject;
        }
    }
}
