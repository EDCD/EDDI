using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SRVUnderShipEvent : Event
    {
        public const string NAME = "SRV under ship";
        public const string DESCRIPTION = "Triggered when your SRV enters or leaves the proximity zone around your ship";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVUnderShipEvent()
        {
            VARIABLES.Add("entering", "A boolean value. True if you are entering the space under your ship");
        }

        bool entering { get; set; }

        public SRVUnderShipEvent(DateTime timestamp, bool entering) : base(timestamp, NAME)
        {
            this.entering = entering;
        }
    }
}
