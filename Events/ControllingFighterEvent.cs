using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ControllingFighterEvent : Event
    {
        public const string NAME = "Controlling fighter";
        public const string DESCRIPTION = "Triggered when you switch control from your ship to your fighter";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"VehicleSwitch\",\"To\":\"Fighter\"}";

        public ControllingFighterEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
