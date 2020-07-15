using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class TouchdownEvent : Event
    {
        public const string NAME = "Touchdown";
        public const string DESCRIPTION = "Triggered when your ship touches down on a planet's surface";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:38:46Z\",\"event\":\"Touchdown\",\"Latitude\":63.468872,\"Longitude\":157.599380}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static TouchdownEvent()
        {
            VARIABLES.Add("longitude", "The longitude from where the ship has touched down");
            VARIABLES.Add("latitude", "The latitude from where the ship has touched down");
            VARIABLES.Add("playercontrolled", "True if the ship is controlled by the player");
            VARIABLES.Add("nearestdestination", "The nearest location from where the ship has touched down");
        }

        public decimal? longitude { get; private set; }

        public decimal? latitude { get; private set; }

        public bool playercontrolled { get; private set; }

        public string nearestdestination => nearestDestination.localizedName;

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public SignalSource nearestDestination { get; private set; }

        public TouchdownEvent(DateTime timestamp, decimal? longitude, decimal? latitude, bool playercontrolled, SignalSource nearestDestination) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.playercontrolled = playercontrolled;
            this.nearestDestination = nearestDestination;
        }
    }
}
