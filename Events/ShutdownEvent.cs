using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShutdownEvent : Event
    {
        public const string NAME = "Shutdown";
        public const string DESCRIPTION = "Triggered on a clean shutdown of the game";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-05T05:41:51Z\", \"event\":\"Shutdown\" }";

        public ShutdownEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
