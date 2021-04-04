using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class ShipLightsEvent : Event
    {
        public const string NAME = "Lights";
        public const string DESCRIPTION = "Triggered when you activate or deactivate your lights";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLightsEvent()
        {
            VARIABLES.Add("lightson", "A boolean value. True if your lights are on.");
        }

        [PublicAPI]
        public bool lightson { get; private set; }

        public ShipLightsEvent(DateTime timestamp, bool lightson) : base(timestamp, NAME)
        {
            this.lightson = lightson;
        }
    }
}
