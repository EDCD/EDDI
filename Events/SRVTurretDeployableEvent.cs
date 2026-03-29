using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVTurretDeployableEvent ( DateTime timestamp, bool deployable ) : Event( timestamp, NAME )
    {
        public const string NAME = "SRV turret deployable";
        public const string DESCRIPTION = "Triggered when your SRV enters or leaves the restriction zone around a ship.";
        public static readonly SRVTurretDeployableEvent SAMPLE = new(DateTime.UtcNow, true);

        [PublicAPI("A boolean value. True if you are leaving the restriction zone around a ship.")]
        public bool deployable { get; } = deployable;
    }
}
