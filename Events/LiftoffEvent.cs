using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class LiftoffEvent : Event
    {
        public const string NAME = "Liftoff";
        public const string DESCRIPTION = "Triggered when your ship lifts off from a planet's surface";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-01T21:45:52Z\", \"event\":\"Liftoff\", \"PlayerControlled\":true, \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Nervi\", \"SystemAddress\":2518721481067, \"Body\":\"Nervi 2 a\", \"BodyID\":17, \"OnStation\":false, \"OnPlanet\":true, \"Latitude\":40.741573, \"Longitude\":65.081490 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LiftoffEvent()
        {
            VARIABLES.Add("starsystem", "The name of the star system from where the ship has lifted off");
            VARIABLES.Add("bodyname", "The name of the body from where the ship has lifted off");
            //VARIABLES.Add("onstation", "True if the ship has lifted off from a settlement or station"); // Not sure how to interpret this yet.
            //VARIABLES.Add("onplanet", "True if the ship has lifted off from an unsettled region"); // Not sure how to interpret this yet.
            VARIABLES.Add("longitude", "The longitude from where the ship has lifted off");
            VARIABLES.Add("latitude", "The latitude from where the ship has lifted off");
            VARIABLES.Add("playercontrolled", "True if the ship is controlled by the player");
            VARIABLES.Add("taxi", "True if the ship is an Apex taxi");
            VARIABLES.Add("multicrew", "True if the ship is belongs to another player");
            VARIABLES.Add("nearestdestination", "The nearest location from where the ship has lifted off");
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

        [JsonProperty("taxi")]
        public bool? taxi { get; private set; }

        [JsonProperty("multicrew")]
        public bool? multicrew { get; private set; }

        [JsonProperty("playercontrolled")]
        public bool playercontrolled { get; private set; }

        [JsonProperty("nearestdestination")]
        public string nearestdestination => nearestDestination.localizedName;

        // Not intended to be user facing

        public SignalSource nearestDestination { get; private set; }

        public long? systemAddress { get; private set; }

        public long? bodyId { get; private set; }

        public LiftoffEvent(DateTime timestamp, decimal? longitude, decimal? latitude, string system, long? systemAddress, string body, long? bodyId, bool? onStation, bool? onPlanet, bool? taxi, bool? multicrew, bool playercontrolled, SignalSource nearestDestination) : base(timestamp, NAME)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.systemname = system;
            this.systemAddress = systemAddress;
            this.bodyname = body;
            this.bodyId = bodyId;
            this.onstation = onStation;
            this.onplanet = onPlanet;
            this.taxi = taxi;
            this.multicrew = multicrew;
            this.playercontrolled = playercontrolled;
            this.nearestDestination = nearestDestination;
            Logging.Info($"Liftoff event: onStation = {onStation}; onPlanet = {onPlanet}");
        }
    }
}
