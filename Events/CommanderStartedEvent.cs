using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderStartedEvent : Event
    {
        public const string NAME = "Commander started";
        public const string DESCRIPTION = "Triggered when you start a new game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"NewCommander\",\"FID\":\"F0000000\",\"Name\":\"HRC1\"}";

        [PublicAPI("The name of the new commander")]
        public string name { get; private set; }

        [PublicAPI("The starting package of the new commander")]
        public string package { get; private set; }

        // Not intended to be user facing

        public string frontierID { get; private set; }

        public CommanderStartedEvent(DateTime timestamp, string name, string frontierID, string package) : base(timestamp, NAME)
        {
            this.name = name;
            this.frontierID = frontierID;
            this.package = package;
        }
    }
}
