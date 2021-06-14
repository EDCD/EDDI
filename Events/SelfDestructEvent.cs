using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SelfDestructEvent : Event
    {
        public const string NAME = "Self destruct";
        public const string DESCRIPTION = "Triggered when you start the self destruct sequence";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"SelfDestruct\"}";

        public SelfDestructEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
