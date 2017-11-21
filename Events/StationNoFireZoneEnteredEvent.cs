using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StationNoFireZoneEnteredEvent : Event
    {
        public const string NAME = "Station no fire zone entered";
        public const string DESCRIPTION = "Triggered when your ship enters a station's no fire zone";
        public static readonly StationNoFireZoneEnteredEvent SAMPLE = new StationNoFireZoneEnteredEvent(DateTime.Now, true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StationNoFireZoneEnteredEvent()
        {
            VARIABLES.Add("weaponsdeployed", "True if the ship's weapons are deployed when entering the zone");
        }

        public bool weaponsdeployed { get; private set; }

        public StationNoFireZoneEnteredEvent(DateTime timestamp, bool weaponsdeployed) : base(timestamp, NAME)
        {
            this.weaponsdeployed = weaponsdeployed;
        }
    }
}
