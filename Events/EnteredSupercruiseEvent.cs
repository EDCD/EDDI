using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredSupercruiseEvent : Event
    {
        public const string NAME = "Entered supercruise";
        public const string DESCRIPTION = "Triggered when your ship enters supercruise";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseEntry\",\"StarSystem\":\"Yuetu\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredSupercruiseEvent()
        {
            VARIABLES.Add("system", "The system at which the commander has entered supercruise");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        public EnteredSupercruiseEvent(DateTime timestamp, string system) : base(timestamp, NAME)
        {
            this.system = system;
        }
    }
}
