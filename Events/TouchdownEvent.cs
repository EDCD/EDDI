using EddiDataDefinitions;
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
            VARIABLES.Add("systemname", "The name of the star system where the ship has touched down");
            VARIABLES.Add("bodyname", "The name of the body where the ship has touched down");
            //VARIABLES.Add("onstation", "True if the ship has touched down at a settlement or station"); // Not sure how to interpret this yet.
            //VARIABLES.Add("onplanet", "True if the ship has touched down in an unsettled region"); // Not sure how to interpret this yet.
            VARIABLES.Add("longitude", "The longitude from where the ship has touched down");
            VARIABLES.Add("latitude", "The latitude from where the ship has touched down");
            VARIABLES.Add("playercontrolled", "True if the ship is controlled by the player");
            VARIABLES.Add("nearestdestination", "The nearest location from where the ship has touched down");
        }

        [JsonProperty("systemname")]
        public string systemname { get; private set; }

        [JsonProperty("bodyname")]
        public string bodyname { get; private set; }

        [JsonProperty("longitude")]
        public decimal? longitude { get; private set; }

        [JsonProperty("latitude")]
        public decimal? latitude { get; private set; }

        [JsonProperty("onstation")]
        public bool? onstation { get; private set; }

        [JsonProperty("onplanet")]
        public bool? onplanet { get; private set; }

        [JsonProperty("playercontrolled")]
        public bool playercontrolled { get; private set; }

        [JsonProperty("nearestdestination")]
        public string nearestdestination => nearestDestination.localizedName;

        // Not intended to be user facing

        public SignalSource nearestDestination { get; private set; }

        public long systemAddress { get; private set; }

        public long? bodyId { get; private set; }

        public TouchdownEvent(DateTime timestamp, decimal? longitude, decimal? latitude, string system, long systemAddress, string body, long? bodyId, bool? onStation, bool? onPlanet, bool playercontrolled, SignalSource nearestDestination) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.systemname = system;
            this.systemAddress = systemAddress;
            this.bodyname = body;
            this.bodyId = bodyId;
            this.onstation = onStation;
            this.onplanet = onPlanet;
            this.playercontrolled = playercontrolled;
            this.nearestDestination = nearestDestination;
        }
    }
}
