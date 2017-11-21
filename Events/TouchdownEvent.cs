using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
        }

        [JsonProperty("longitude")]
        public decimal? longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal? latitude { get; private set; }

        [JsonProperty("playercontrolled")]
        public bool playercontrolled { get; private set; }

        public TouchdownEvent(DateTime timestamp, decimal? longitude, decimal? latitude, bool playercontrolled) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.playercontrolled = playercontrolled;
        }
    }
}
