using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderLoadingEvent : Event
    {
        public const string NAME = "Commander loading";
        public const string DESCRIPTION = "Triggered at the very beginning of loading a game";
        public const string SAMPLE = "{ \"timestamp\":\"2019-08-01T05:13:56Z\", \"event\":\"Commander\", \"FID\":\"F0000000\", \"Name\":\"HRC1\" }";

        [PublicAPI("The name of the player account being loaded")]
        public string name { get; private set; }

        // Not intended to be user facing

        public string frontierID { get; private set; }

        public CommanderLoadingEvent(DateTime timestamp, string name, string frontierID) : base(timestamp, NAME)
        {
            this.name = name;
            this.frontierID = frontierID;
        }
    }
}
