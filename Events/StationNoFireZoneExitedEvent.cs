using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StationNoFireZoneExitedEvent : Event
    {
        public const string NAME = "Station no fire zone exited";
        public const string DESCRIPTION = "Triggered when your ship exits a station's no fire zone";
        public static readonly StationNoFireZoneExitedEvent SAMPLE = new StationNoFireZoneExitedEvent(DateTime.UtcNow);

        public StationNoFireZoneExitedEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
