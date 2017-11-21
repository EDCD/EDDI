using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ControllingShipEvent : Event
    {
        public const string NAME = "Controlling ship";
        public const string DESCRIPTION = "Triggered when you switch control from your fighter to your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\",\"To\":\"Mothership\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ControllingShipEvent()
        {
        }

        public ControllingShipEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
