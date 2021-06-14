using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipLowFuelEvent : Event
    {
        public const string NAME = "Low fuel";
        public const string DESCRIPTION = "Triggered when your fuel level falls below 25%";
        public const string SAMPLE = null;

        public ShipLowFuelEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
