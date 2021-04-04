using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class ShipLandingGearEvent : Event
    {
        public const string NAME = "Landing gear";
        public const string DESCRIPTION = "Triggered when you deploy or retract your landing gear";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLandingGearEvent()
        {
            VARIABLES.Add("deployed", "A boolean value. True if your landing gear is deployed.");
        }

        [PublicAPI]
        public bool deployed { get; private set; }

        public ShipLandingGearEvent(DateTime timestamp, bool deployed) : base(timestamp, NAME)
        {
            this.deployed = deployed;
        }
    }
}
