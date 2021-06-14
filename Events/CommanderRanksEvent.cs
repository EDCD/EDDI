using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderRatingsEvent : Event
    {
        public const string NAME = "Commander ratings";
        public const string DESCRIPTION = "Triggered when your ratings are reported";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T02:17:37Z\", \"event\":\"Rank\", \"Combat\":7, \"Trade\":8, \"Explore\":8, \"Soldier\":0, \"Exobiologist\":0, \"Empire\":14, \"Federation\":14, \"CQC\":2 }";

        [PublicAPI("The commander's combat rating (this is an object)")]
        public CombatRating combat { get; private set; }

        [PublicAPI("The commander's trading rating (this is an object)")]
        public TradeRating trade { get; private set; }

        [PublicAPI("The commander's exploration rating (this is an object)")]
        public ExplorationRating exploration { get; private set; }

        [PublicAPI("The commander's CQC rating (this is an object)")]
        public CQCRating cqc { get; private set; }

        [PublicAPI("The commander's empire rating (this is an object)")]
        public EmpireRating empire { get; private set; }

        [PublicAPI("The commander's federation rating (this is an object)")]
        public FederationRating federation { get; private set; }

        [PublicAPI("The commander's mercenary rating  (this is an object)")]
        public MercenaryRating mercenary { get; private set; }

        [PublicAPI("The commander's exobiologist rating  (this is an object)")]
        public ExobiologistRating exobiologist { get; private set; }

        public CommanderRatingsEvent(DateTime timestamp, CombatRating combat, TradeRating trade, ExplorationRating exploration, CQCRating cqc, EmpireRating empire, FederationRating federation, MercenaryRating mercenary, ExobiologistRating exobiologist) : base(timestamp, NAME)
        {
            this.combat = combat;
            this.trade = trade;
            this.exploration = exploration;
            this.cqc = cqc;
            this.empire = empire;
            this.federation = federation;
            this.mercenary = mercenary;
            this.exobiologist = exobiologist;
        }
    }
}
