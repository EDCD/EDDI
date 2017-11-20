using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class JumpingEvent : Event
    {
        public const string NAME = "Jumping";
        public const string DESCRIPTION = "NO LONGER IN USE";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        public JumpingEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
