using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FighterDockedEvent : Event
    {
        public const string NAME = "Fighter docked";
        public const string DESCRIPTION = "Triggered when you dock a fighter with your ship";
        public const string SAMPLE = "{\"timestamp\":\"2019-04-29T00:09:17Z\", \"event\":\"DockFighter\", \"ID\":13 }";

        [PublicAPI("The fighter's id")]
        public int id { get; private set; }

        public FighterDockedEvent(DateTime timestamp, int id) : base(timestamp, NAME)
        {
            this.id = id;
        }
    }
}
