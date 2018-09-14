using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiMissionMonitor
{
    public class PassengersEvent : Event
    {
        public const string NAME = "Passengers";
        public const string DESCRIPTION = "Triggered at startup, with basic information of the Passenger Manifest";
        public const string SAMPLE = "{ \"timestamp\":\"2018-06-04T17:07:02Z\", \"event\":\"Passengers\", \"Manifest\":[ { \"MissionID\":387643501, \"Type\":\"Criminal\", \"VIP\":true, \"Wanted\":true, \"Count\":5 }, { \"MissionID\":387642036, \"Type\":\"Criminal\", \"VIP\":true, \"Wanted\":true, \"Count\":5 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PassengersEvent()
        {
            VARIABLES.Add("passengers", "The manifest of passengers on your ship");
        }

        [JsonProperty("passengers")]
        public List<Passenger> passengers { get; private set; }

        public PassengersEvent(DateTime timestamp, List<Passenger> passengers) : base(timestamp, NAME)
        {
            this.passengers = passengers;
        }
    }
}
