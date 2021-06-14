using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderReputationEvent : Event
    {
        public const string NAME = "Commander reputation";
        public const string DESCRIPTION = "Triggered when your reputation is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2019-08-07T06:38:38Z\", \"event\":\"Reputation\", \"Empire\":75.000000, \"Federation\":96.557602, \"Independent\":3.346750, \"Alliance\":75.000000 }";

        [PublicAPI("The percentage progress of the commander's empire superpower reputation")]
        public decimal empire { get; private set; }

        [PublicAPI("The percentage progress of the commander's federation superpower reputation")]
        public decimal federation { get; private set; }

        [PublicAPI("The percentage progress of the commander's independent faction reputation")]
        public decimal independent { get; private set; }

        [PublicAPI("The percentage progress of the commander's alliance superpower reputation")]
        public decimal alliance { get; private set; }

        public CommanderReputationEvent(DateTime timestamp, decimal empire, decimal federation, decimal independent, decimal alliance) : base(timestamp, NAME)
        {
            this.empire = empire;
            this.federation = federation;
            this.independent = independent;
            this.alliance = alliance;
        }
    }
}
