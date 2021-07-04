using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SafeEvent : Event
    {
        public const string NAME = "Safe";
        public const string DESCRIPTION = "Triggered when you are no longer in danger";
        public const string SAMPLE = null;

        public SafeEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
