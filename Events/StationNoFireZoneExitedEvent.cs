using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StationNoFireZoneExitedEvent : Event
    {
        public const string NAME = "Station no fire zone exited";
        public const string DESCRIPTION = "Triggered when your ship exits a station's no fire zone";
        public static readonly StationNoFireZoneExitedEvent SAMPLE = new StationNoFireZoneExitedEvent(DateTime.Now);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        public StationNoFireZoneExitedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
