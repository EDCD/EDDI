using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CockpitBreachedEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Cockpit breached";
        public const string DESCRIPTION = "Triggered when your ship's cockpit is broken";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CockpitBreached\"}";
    }
}
