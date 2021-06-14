using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class HeatDamageEvent : Event
    {
        public const string NAME = "Heat damage";
        public const string DESCRIPTION = "Triggered when your ship is taking damage from excessive heat";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-25T12:00:23Z\",\"event\":\"HeatDamage\"}";

        public HeatDamageEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
