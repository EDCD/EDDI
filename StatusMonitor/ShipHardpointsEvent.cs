using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipHardpointsEvent : Event
    {
        public const string NAME = "Hardpoints";
        public const string DESCRIPTION = "Triggered when you deploy or retract your hardpoints";
        public const string SAMPLE = "null";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipHardpointsEvent()
        {
            VARIABLES.Add("deployed", "A boolean value. True if you hardpoints are deployed.");
        }

        [JsonProperty("deployed")]
        public bool deployed { get; private set; }

        public ShipHardpointsEvent(DateTime timestamp, bool deployed) : base(timestamp, NAME)
        {
            this.deployed = deployed;
        }
    }
}