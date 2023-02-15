using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LowFuelEvent : Event
    {
        public const string NAME = "Low fuel";
        public const string DESCRIPTION = "Triggered when your fuel level falls below 25% and in 5% increments thereafter";
        public const string SAMPLE = null;

        public LowFuelEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
