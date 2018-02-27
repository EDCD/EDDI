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
        }

        public SRVUnderShipEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
