using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SystemFactionChangedEvent (
        DateTime timestamp,
        string match,
        string system,
        string oldfaction,
        string newfaction )
        : Event( timestamp, NAME )
    {
        public const string NAME = "System faction changed";
        public const string DESCRIPTION = "Triggered when there is a change in the controlling faction of a watched system";
        public static readonly SystemFactionChangedEvent SAMPLE = new(DateTime.UtcNow, "home", "Shinrarta Dezhra", "The Pilots Federation", "The Dark Wheel");

        [PublicAPI("The name of the pattern that this event matched")]
        public string match { get; private set; } = match;

        [PublicAPI("The name of the system")]
        public string system { get; private set; } = system;

        [PublicAPI("The name of the old controlling faction of the system")]
        public string oldfaction { get; private set; } = oldfaction;

        [PublicAPI("The name of the new controlling faction of the system")]
        public string newfaction { get; private set; } = newfaction;
    }
}
