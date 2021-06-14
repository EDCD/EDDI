using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SquadronStatusEvent : Event
    {
        public const string NAME = "Squadron status";
        public const string DESCRIPTION = "Triggered when your status with a squadron has changed";
        public const string SAMPLE = null;

        [PublicAPI("The squadron name")]
        public string name { get; private set; }

        [PublicAPI("The squadron status (`applied`, `created`, `disbanded`, `invited`, `joined`, `kicked`, `left`)")]
        public string status { get; private set; }

        public SquadronStatusEvent(DateTime timestamp, string name, string status) : base(timestamp, NAME)
        {
            this.name = name;
            this.status = status;
        }
    }
}
