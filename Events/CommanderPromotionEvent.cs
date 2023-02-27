﻿using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderPromotionEvent : Event
    {
        public const string NAME = "Commander promotion";
        public const string DESCRIPTION = "Triggered when one of your commander ranks increases";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Promotion\",\"Combat\":5}";

        [PublicAPI("The type of rank being promoted")]
        public string rank_type { get; private set; }

        [PublicAPI("The commander's new rank")]
        public string rank { get; private set; }

        [PublicAPI("The commander's new invariant rank")]
        public string rank_invariant { get; private set; }

        // Not intended to be user facing
        public object ratingObject { get; }

        public CommanderPromotionEvent(DateTime timestamp, object ratingObject, string gender = null) : base(timestamp, NAME)
        {
            string getRankType (object ratingObj)
            {
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
