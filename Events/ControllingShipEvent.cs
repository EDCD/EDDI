using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ControllingShipEvent : Event
    {
        public const string NAME = "Controlling ship";
        public const string DESCRIPTION = "Triggered when you switch control from your fighter to your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\",\"To\":\"Mothership\"}";

        public ControllingShipEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
