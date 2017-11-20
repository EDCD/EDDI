using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderProgressEvent : Event
    {
        public const string NAME = "Commander progress";
        public const string DESCRIPTION = "Triggered when your progress is reported";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Progress\",\"Combat\":77,\"Trade\":9,\"Explore\":93,\"Empire\":0,\"Federation\":0,\"CQC\":0}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderProgressEvent()
        {
            VARIABLES.Add("combat", "The percentage progress of the commander's combat rating");
            VARIABLES.Add("trade", "The percentage progress of the commander's trade rating");
            VARIABLES.Add("exploration", "The percentage progress of the commander's exploration rating");
            VARIABLES.Add("cqc", "The percentage progress of the commander's CQC rating");
            VARIABLES.Add("empire", "The percentage progress of the commander's empire rating");
            VARIABLES.Add("federation", "The percentage progress of the commander's federation rating");
        }

        [JsonProperty("combat")]
        public decimal combat { get; private set; }

        [JsonProperty("trade")]
        public decimal trade { get; private set; }

        [JsonProperty("exploration")]
        public decimal exploration { get; private set; }

        [JsonProperty("cqc")]
        public decimal cqc { get; private set; }

        [JsonProperty("empire")]
        public decimal empire { get; private set; }

        [JsonProperty("federation")]
        public decimal federation { get; private set; }

        public CommanderProgressEvent(DateTime timestamp, decimal combat, decimal trade, decimal exploration, decimal cqc, decimal empire, decimal federation) : base(timestamp, NAME)
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
