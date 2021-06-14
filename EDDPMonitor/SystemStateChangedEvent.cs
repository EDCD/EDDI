using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiEddpMonitor
{
    [PublicAPI]
    class SystemStateChangedEvent : Event
    {
        public const string NAME = "System state changed";
        public const string DESCRIPTION = "Triggered when there is a change in the state of a watched system";
        public static SystemStateChangedEvent SAMPLE = new SystemStateChangedEvent(DateTime.UtcNow, "home", "Shinrarta Dezhra", FactionState.FromEDName("CivilUnrest"), FactionState.FromEDName("CivilWar"));

        [PublicAPI("The name of the pattern that this event matched")]
        public string match { get; private set; }

        [PublicAPI("The name of the system")]
        public string system { get; private set; }

        [PublicAPI("The old state of the system")]
        public string oldstate => oldSystemState.localizedName;

        [PublicAPI("The new state of the system")]
        public string newstate => newSystemState.localizedName;

        // Not intended to be user facing

        public FactionState oldSystemState { get; private set; }

        public FactionState newSystemState { get; private set; }

        public SystemStateChangedEvent(DateTime timestamp, string match, string system, FactionState oldSystemState, FactionState newSystemState) : base(timestamp, NAME)
        {
            this.match = match;
            this.system = system;
            this.oldSystemState = oldSystemState;
            this.newSystemState = newSystemState;
        }
    }
}
