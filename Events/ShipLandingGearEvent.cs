using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipLandingGearEvent : Event
    {
        public const string NAME = "Landing gear";
        public const string DESCRIPTION = "Triggered when you deploy or retract your landing gear";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if your landing gear is deployed.")]
        public bool deployed { get; private set; }

        public ShipLandingGearEvent(DateTime timestamp, bool deployed) : base(timestamp, NAME)
        {
            this.deployed = deployed;
        }
    }
}
