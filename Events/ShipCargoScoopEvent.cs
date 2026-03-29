using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipCargoScoopEvent ( DateTime timestamp, bool deployed ) : Event( timestamp, NAME )
    {
        public const string NAME = "Cargo scoop";
        public const string DESCRIPTION = "Triggered when you deploy or retract your cargo scoop";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if your cargo scoop is deployed.")]
        public bool deployed { get; private set; } = deployed;
    }
}
