using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewRoleChangedEvent : Event
    {
        public const string NAME = "Crew role changed";
        public const string DESCRIPTION = "Triggered when your role in the crew changes";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"ChangeCrewRole\",\"Role\":\"FireCon\"}";

        [PublicAPI("The crew role you have been assigned (gunner, fighter, idle)")]
        public string role { get; private set; }

        public CrewRoleChangedEvent(DateTime timestamp, string role) : base(timestamp, NAME)
        {
            this.role = role;
        }
    }
}
