using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EngineerProgressedEvent : Event
    {
        public const string NAME = "Engineer progressed";
        public const string DESCRIPTION = "Triggered at startup and when you reach a new rank with an engineer";
        public const string SAMPLE = @"{ ""timestamp"":""2018-01-16T09:34:36Z"", ""event"":""EngineerProgress"", ""Engineer"":""Zacariah Nemo"", ""EngineerID"":300050, ""Rank"":4, ""RankProgress"":0 }";

        [PublicAPI("The name of the engineer with whom you have progressed (not written at startup)")]
        public string engineer => Engineer?.name;

        [PublicAPI("The rank of your relationship with the engineer (not written at startup)")]
        public int? rank => Engineer?.rank;

        [PublicAPI("The current stage of your relations with the engineer (Invited/Known/Unlocked/Barred) (not written at startup)")]
        public string stage => Engineer?.stage;

        [PublicAPI("The type of progress that is applicable (Rank/Stage) (not written at startup)")]
        public string progresstype { get; private set; }

        // Not intended to be user facing
        
        public Engineer Engineer { get; private set; }

        // Omitted, only written at startup and for an array of engineers rather than for a single engineer.
        public int? rankprogress => Engineer?.rankprogress;

        public EngineerProgressedEvent(DateTime timestamp, Engineer Engineer, string type) : base(timestamp, NAME)
        {
            this.Engineer = Engineer;
            this.progresstype = type;
        }
    }
}
