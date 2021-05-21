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
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T02:17:37Z\", \"event\":\"Rank\", \"Combat\":7, \"Trade\":8, \"Explore\":8, \"Soldier\":0, \"Exobiologist\":0, \"Empire\":14, \"Federation\":14, \"CQC\":2 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderRatingsEvent()
        {
            VARIABLES.Add("combat", "The commander's combat rating");
            VARIABLES.Add("trading", "The commander's trading rating");
            VARIABLES.Add("exploration", "The commander's exploration rating");
            VARIABLES.Add("cqc", "The commander's CQC rating");
            VARIABLES.Add("empire", "The commander's empire rating");
            VARIABLES.Add("federation", "The commander's federation rating");
            VARIABLES.Add("mercenary", "The commander's mercenary rating");
            VARIABLES.Add("exobiologist", "The commander's exobiologist rating");
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

        [JsonProperty("mercenary")]
        public MercenaryRating mercenary { get; private set; }

        [JsonProperty("exobiologist")]
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
