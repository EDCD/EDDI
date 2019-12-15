using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EngineerProgressedEvent : Event
    {
        public const string NAME = "Engineer progressed";
        public const string DESCRIPTION = "Triggered at startup and when you reach a new rank with an engineer";
        public const string SAMPLE = @"{ ""timestamp"":""2018-01-16T09:34:36Z"", ""event"":""EngineerProgress"", ""Engineer"":""Zacariah Nemo"", ""EngineerID"":300050, ""Rank"":4, ""RankProgress"":0 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EngineerProgressedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer with whom you have progressed (not written at startup)");
            VARIABLES.Add("rank", "The rank of your relationship with the engineer (not written at startup)");
            VARIABLES.Add("stage", "The current stage of your relations with the engineer (Invited/Known/Unlocked/Barred) (not written at startup)");
            // VARIABLES.Add("rankprogress", "The percentage towards your next rank with the engineer"); // Omitted, only written at startup and for an array of engineers rather than for a single engineer.
            VARIABLES.Add("progresstype", "The type of progress that is applicable (Rank/Stage) (not written at startup)");
        }

        public string engineer => Engineer?.name;

        public int? rank => Engineer?.rank;

        public int? rankprogress => Engineer?.rankprogress;

        public string stage => Engineer?.stage;

        public string progresstype { get; private set; }

        // Not intended to be user facing
        public Engineer Engineer { get; private set; }

        public EngineerProgressedEvent(DateTime timestamp, Engineer Engineer, string type) : base(timestamp, NAME)
        {
            this.Engineer = Engineer;
            this.progresstype = type;
        }
    }
}
