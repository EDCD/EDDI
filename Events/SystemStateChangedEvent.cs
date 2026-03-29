using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SystemStateChangedEvent (
        DateTime timestamp,
        string match,
        string system,
        FactionState oldSystemState,
        FactionState newSystemState )
        : Event( timestamp, NAME )
    {
        public const string NAME = "System state changed";
        public const string DESCRIPTION = "Triggered when there is a change in the state of a watched system";
        public static readonly SystemStateChangedEvent SAMPLE = new(DateTime.UtcNow, "home", "Shinrarta Dezhra", FactionState.CivilUnrest, FactionState.CivilWar);

        [PublicAPI("The name of the pattern that this event matched")]
        public string match { get; private set; } = match;

        [PublicAPI("The name of the system")]
        public string system { get; private set; } = system;

        [PublicAPI("The old state of the system")]
        public string oldstate => oldSystemState.localizedName;

        [PublicAPI("The new state of the system")]
        public string newstate => newSystemState.localizedName;

        // Not intended to be user facing

        public FactionState oldSystemState { get; private set; } = oldSystemState;

        public FactionState newSystemState { get; private set; } = newSystemState;
    }
}
