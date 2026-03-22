using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FlightAssistEvent ( DateTime timestamp, bool flightAssistOff ) : Event( timestamp, NAME )
    {
        public const string NAME = "Flight assist";
        public const string DESCRIPTION = "Triggered when flight assist is toggled";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if flight assist is off.")]
        public bool off { get; private set; } = flightAssistOff;
    }
}
