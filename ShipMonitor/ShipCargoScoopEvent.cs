using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipCargoScoopEvent : Event
    {
        public const string NAME = "Cargo scoop";
        public const string DESCRIPTION = "Triggered when you deploy or retract your cargo scoop";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipCargoScoopEvent()
        {
            VARIABLES.Add("deployed", "A boolean value. True if your cargo scoop is deployed.");
        }

        [JsonProperty("deployed")]
        public bool deployed { get; private set; }

        public ShipCargoScoopEvent(DateTime timestamp, bool deployed) : base(timestamp, NAME)
        {
            this.deployed = deployed;
        }
    }
}
