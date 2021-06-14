using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipHardpointsEvent : Event
    {
        public const string NAME = "Hardpoints";
        public const string DESCRIPTION = "Triggered when you deploy or retract your hardpoints";
        public const string SAMPLE = "null";

        [PublicAPI("A boolean value. True if you hardpoints are deployed.")]
        public bool deployed { get; private set; }

        public ShipHardpointsEvent(DateTime timestamp, bool deployed) : base(timestamp, NAME)
        {
            this.deployed = deployed;
        }
    }
}