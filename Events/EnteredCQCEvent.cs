using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredCQCEvent : Event
    {
        public const string NAME = "Entered CQC";
        public const string DESCRIPTION = "Triggered when you enter CQC";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"Credits\":600120,\"Loan\":0}";

        [PublicAPI("The commander's name")]
        public string commander { get; private set; }

        public EnteredCQCEvent(DateTime timestamp, string commander) : base(timestamp, NAME)
        {
            this.commander = commander;
        }
    }
}
