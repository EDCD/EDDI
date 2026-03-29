using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipFuelScoopEvent ( DateTime timestamp, bool active ) : Event( timestamp, NAME )
    {
        public const string NAME = "Fuel scoop";
        public const string DESCRIPTION = "Triggered when you activate or deactivate your fuel scoop";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if your fuel scoop is activated.")]
        public bool active { get; private set; } = active;
    }
}
