using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Details about a commander
    /// </summary>
    public class Commander
    {
        /// <summary>The commander's name</summary>
        public string name { get; set;  }
        /// <summary>The commander's name as spoken</summary>
        public string phoneticname { get; set; }

        /// <summary> The commander's title.  This is dependent on the current system</summary>
        public string title { get; set; }

        /// <summary>The commander's combat rating</summary>
        public CombatRating combatrating { get; set; }

        /// <summary>The commander's trade rating</summary>
        public TradeRating traderating { get; set; }
        
        /// <summary>The commander's exploration rating</summary>
        public ExplorationRating explorationrating { get; set; }

        /// <summary>The commander's CQC rating</summary>
        public CQCRating cqcrating { get; set; }

        /// <summary>The commander's empire rating</summary>
        public EmpireRating empirerating { get; set; }

        /// <summary>The commander's federation rating</summary>
        public FederationRating federationrating { get; set; }

        /// <summary>The number of credits the commander holds</summary>
        public long credits { get; set; }
        /// <summary>The amount of debt the commander owes</summary>
        public long debt { get; set; }

        /// <summary>The insurance excess percentage the commander has to pay</summary>
        public decimal? insurance { get; set;  }
    }
}
