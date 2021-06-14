using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipLightsEvent : Event
    {
        public const string NAME = "Lights";
        public const string DESCRIPTION = "Triggered when you activate or deactivate your lights";
        public const string SAMPLE = null;

        [PublicAPI("A boolean value. True if your lights are on.")]
        public bool lightson { get; private set; }

        public ShipLightsEvent(DateTime timestamp, bool lightson) : base(timestamp, NAME)
        {
            this.lightson = lightson;
        }
    }
}
