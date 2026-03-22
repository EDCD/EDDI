using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LowFuelEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Low fuel";
        public const string DESCRIPTION = "Triggered when your fuel level falls below 25% and in 5% increments thereafter";
        public const string SAMPLE = null;
    }
}
