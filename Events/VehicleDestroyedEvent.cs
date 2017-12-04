using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class VehicleDestroyedEvent : Event
    {
        public const string NAME = "Vehicle destroyed";
        public const string DESCRIPTION = "Triggered when your vehicle (fighter or SRV) is destroyed";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static VehicleDestroyedEvent()
        {
        }

        public VehicleDestroyedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
