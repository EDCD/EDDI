using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CockpitBreachedEvent : Event
    {
        public const string NAME = "Cockpit breached";
        public const string DESCRIPTION = "Triggered when your ship's cockpit is broken";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CockpitBreached\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CockpitBreachedEvent()
        {
        }

        public CockpitBreachedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
