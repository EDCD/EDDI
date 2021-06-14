using EddiEvents;
using System;
using Utilities;

namespace EddiEddpMonitor
{
    [PublicAPI]
    class SystemFactionChangedEvent : Event
    {
        public const string NAME = "System faction changed";
        public const string DESCRIPTION = "Triggered when there is a change in the controlling faction of a watched system";
        public static SystemFactionChangedEvent SAMPLE = new SystemFactionChangedEvent(DateTime.UtcNow, "home", "Shinrarta Dezhra", "The Pilots Federation", "The Dark Wheel");

        [PublicAPI("The name of the pattern that this event matched")]
        public string match { get; private set; }

        [PublicAPI("The name of the system")]
        public string system { get; private set; }

        [PublicAPI("The name of the old controlling faction of the system")]
        public string oldfaction { get; private set; }

        [PublicAPI("The name of the new controlling faction of the system")]
        public string newfaction { get; private set; }

        public SystemFactionChangedEvent(DateTime timestamp, string match, string system, string oldfaction, string newfaction) : base(timestamp, NAME)
        {
            this.match = match;
            this.system = system;
            this.oldfaction = oldfaction;
            this.newfaction = newfaction;
        }
    }
}
