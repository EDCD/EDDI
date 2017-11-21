using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiEddpMonitor
{
    class SystemStateChangedEvent : Event
    {
        public const string NAME = "System state changed";
        public const string DESCRIPTION = "Triggered when there is a change in the state of a watched system";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        public static SystemStateChangedEvent SAMPLE = new SystemStateChangedEvent(DateTime.Now, "home", "Shinrarta Dezhra", State.CivilUnrest.name, State.CivilWar.name);

        static SystemStateChangedEvent()
        {
            VARIABLES.Add("match", "The name of the pattern that this event matched");
            VARIABLES.Add("system", "The name of the system");
            VARIABLES.Add("oldstate", "The old state of the system");
            VARIABLES.Add("newstate", "The new state of the system");
        }

        public string match { get; private set; }

        public string system { get; private set; }

        public string oldstate { get; private set; }

        public string newstate { get; private set; }

        public SystemStateChangedEvent(DateTime timestamp, string match, string system, string oldstate, string newstate) : base(timestamp, NAME)
        {
            this.match = match;
            this.system = system;
            this.oldstate = oldstate;
            this.newstate = newstate;
        }
    }
}
