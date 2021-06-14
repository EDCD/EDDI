using System;

namespace EddiEvents
{
    public class JumpingEvent : Event
    {
        public const string NAME = "Jumping";
        public const string DESCRIPTION = "NO LONGER IN USE";

        public JumpingEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
