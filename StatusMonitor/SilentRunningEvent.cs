using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SilentRunningEvent : Event
    {
        public const string NAME = "Silent running";
        public const string DESCRIPTION = "Triggered when you activate or deactivate silent running";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if silent running is active.")]
        public bool silentrunning { get; private set; }

        public SilentRunningEvent(DateTime timestamp, bool silentRunning) : base(timestamp, NAME)
        {
            this.silentrunning = silentRunning;
        }
    }
}
