using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipLowFuelEvent : Event
    {
        public const string NAME = "Low fuel";
        public const string DESCRIPTION = "Triggered when your fuel level falls below 25%";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLowFuelEvent()
        {
        }

        public ShipLowFuelEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
