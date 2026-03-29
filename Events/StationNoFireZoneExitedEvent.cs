using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StationNoFireZoneExitedEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Station no fire zone exited";
        public const string DESCRIPTION = "Triggered when your ship exits a station's no fire zone";
        public static readonly StationNoFireZoneExitedEvent SAMPLE = new(DateTime.UtcNow);
    }
}
