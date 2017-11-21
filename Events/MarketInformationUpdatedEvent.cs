using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MarketInformationUpdatedEvent : Event
    {
        public const string NAME = "Market information updated";
        public const string DESCRIPTION = "Triggered when market information for the currently docked station has been updated";
        public static MarketInformationUpdatedEvent SAMPLE = new MarketInformationUpdatedEvent(DateTime.Now);

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MarketInformationUpdatedEvent()
        {
        }

        public MarketInformationUpdatedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
