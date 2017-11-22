using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiEddpMonitor
{
    class SystemFactionChangedEvent : Event
    {
        public const string NAME = "System faction changed";
        public const string DESCRIPTION = "Triggered when there is a change in the controlling faction of a watched system";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        public static SystemFactionChangedEvent SAMPLE = new SystemFactionChangedEvent(DateTime.Now, "home", "Shinrarta Dezhra", "The Pilots Federation", "The Dark Wheel");

        static SystemFactionChangedEvent()
        {
            VARIABLES.Add("match", "The name of the pattern that this event matched");
            VARIABLES.Add("system", "The name of the system");
            VARIABLES.Add("oldfaction", "The name of the old controlling faction of the system");
            VARIABLES.Add("newfaction", "The name of the new controlling faction of the system");
        }

        public string match { get; private set; }

        public string system { get; private set; }

        public string oldfaction { get; private set; }

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
