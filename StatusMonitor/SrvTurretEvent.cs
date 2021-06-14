using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVTurretEvent : Event
    {
        public const string NAME = "SRV turret";
        public const string DESCRIPTION = "Triggered when you deploy or retract your SRV's turret";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if the turret is being deployed and false if it is being retracted")]
        public bool deployed { get; private set; }

        public SRVTurretEvent(DateTime timestamp, bool srvTurretDeployed) : base(timestamp, NAME)
        {
            this.deployed = srvTurretDeployed;
        }
    }
}
