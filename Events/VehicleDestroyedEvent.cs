using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class VehicleDestroyedEvent : Event
    {
        public const string NAME = "Vehicle destroyed";
        public const string DESCRIPTION = "Triggered when your vehicle (fighter or SRV) is destroyed";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterDestroyed\", \"ID\":13}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static VehicleDestroyedEvent()
        {
            VARIABLES.Add("vehicle", "The vehicle that was destroyed (e.g. fighter or srv)");
            VARIABLES.Add("id", "The vehicle's id");
        }

        [JsonProperty("vehicle")]
        public string vehicle;

        [JsonProperty("id")]
        public int id;

        public VehicleDestroyedEvent(DateTime timestamp, string vehicle, int id) : base(timestamp, NAME)
        {
            this.vehicle = vehicle;
            this.id = id;
        }
    }
}
