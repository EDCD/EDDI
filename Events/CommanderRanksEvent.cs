using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderRatingsEvent : Event
    {
        public const string NAME = "Commander ratings";
        public const string DESCRIPTION = "Triggered when your ratings are reported";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Ranks\",\"Combat\":2,\"Trade\":2,\"Explore\":5,\"Empire\":1,\"Federation\":3,\"CQC\":0}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderRatingsEvent()
        {
            VARIABLES.Add("combat", "The commander's combat rating");
            VARIABLES.Add("trading", "The commander's trading rating");
            VARIABLES.Add("exploration", "The commander's exploration rating");
            VARIABLES.Add("cqc", "The commander's CQC rating");
            VARIABLES.Add("empire", "The commander's empire rating");
            VARIABLES.Add("federation", "The commander's federation rating");
        }

        [JsonProperty("combat")]
        public CombatRating combat { get; private set; }

        [JsonProperty("trade")]
        public TradeRating trade { get; private set; }

        [JsonProperty("exploration")]
        public ExplorationRating exploration { get; private set; }

        [JsonProperty("cqc")]
        public CQCRating cqc { get; private set; }

        [JsonProperty("empire")]
        public EmpireRating empire { get; private set; }

        [JsonProperty("federation")]
        public FederationRating federation { get; private set; }

        public CommanderRatingsEvent(DateTime timestamp, CombatRating combat, TradeRating trade, ExplorationRating exploration, CQCRating cqc, EmpireRating empire, FederationRating federation) : base(timestamp, NAME)
        {
            this.combat = combat;
            this.trade = trade;
            this.exploration = exploration;
            this.cqc = cqc;
            this.empire = empire;
            this.federation = federation;
        }
    }
}
