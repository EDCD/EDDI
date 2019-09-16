using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderReputationEvent : Event
    {
        public const string NAME = "Commander reputation";
        public const string DESCRIPTION = "Triggered when your reputation is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2019-08-07T06:38:38Z\", \"event\":\"Reputation\", \"Empire\":75.000000, \"Federation\":96.557602, \"Independent\":3.346750, \"Alliance\":75.000000 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderReputationEvent()
        {
            VARIABLES.Add("empire", "The percentage progress of the commander's empire superpower reputation");
            VARIABLES.Add("federation", "The percentage progress of the commander's federation superpower reputation");
            VARIABLES.Add("independent", "The percentage progress of the commander's independent faction reputation");
            VARIABLES.Add("alliance", "The percentage progress of the commander's alliance superpower reputation");
        }

        [JsonProperty("empire")]
        public decimal empire { get; private set; }

        [JsonProperty("federation")]
        public decimal federation { get; private set; }

        [JsonProperty("independent")]
        public decimal independent { get; private set; }

        [JsonProperty("alliance")]
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
