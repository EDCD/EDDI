using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MarketInformationUpdatedEvent : Event
    {
        public const string NAME = "Market information updated";
        public const string DESCRIPTION = "Triggered when market information for the currently docked station has been updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MarketInformationUpdatedEvent()
        {
        }

        // Update type - `market`, `outfitting`, `profile`, or `shipyard`
        public string update { get; private set; }

        public MarketInformationUpdatedEvent(DateTime timestamp, string update) : base(timestamp, NAME)
        {
            this.update = update;
        }
    }
}
