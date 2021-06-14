using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderProgressEvent : Event
    {
        public const string NAME = "Commander progress";
        public const string DESCRIPTION = "Triggered when your progress is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T02:17:37Z\", \"event\":\"Progress\", \"Combat\":66, \"Trade\":100, \"Explore\":100, \"Soldier\":0, \"Exobiologist\":0, \"Empire\":100, \"Federation\":100, \"CQC\":87 }";

        [PublicAPI("The percentage progress of the commander's combat rating")]
        public decimal combat { get; private set; }

        [PublicAPI("The percentage progress of the commander's trade rating")]
        public decimal trade { get; private set; }

        [PublicAPI("The percentage progress of the commander's exploration rating")]
        public decimal exploration { get; private set; }

        [PublicAPI("The percentage progress of the commander's CQC rating")]
        public decimal cqc { get; private set; }

        [PublicAPI("The percentage progress of the commander's empire rating")]
        public decimal empire { get; private set; }

        [PublicAPI("The percentage progress of the commander's federation rating")]
        public decimal federation { get; private set; }

        [PublicAPI("The percentage progress of the commander's mercenary rating")]
        public decimal mercenary { get; private set; }

        [PublicAPI("The percentage progress of the commander's exobiologist rating")]
        public decimal exobiologist { get; private set; }

        public CommanderProgressEvent(DateTime timestamp, decimal combat, decimal trade, decimal exploration, decimal cqc, decimal empire, decimal federation, decimal mercenary, decimal exobiologist) : base(timestamp, NAME)
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
