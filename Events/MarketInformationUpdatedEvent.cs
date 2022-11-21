using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MarketInformationUpdatedEvent : Event
    {
        public const string NAME = "Market information updated";
        public const string DESCRIPTION = "Triggered when market information for the currently docked station has been updated";
        public const string SAMPLE = null;

        [PublicAPI("A list of the updates triggering the event (which may include 'market', 'outfitting', and 'shipyard'")]
        public HashSet<string> updates { get; private set; } = new HashSet<string>();

        /// <summary>The timestamp recorded for this event must be generated from game or server data.
        /// System time (e.g. DateTime.UtcNow) cannot be trusted for reporting to EDDN and may not be used.</summary>
        public MarketInformationUpdatedEvent(DateTime timestamp, string starSystem, string stationName, long? marketId, HashSet<string> updates) : base(timestamp, NAME)
        {
            this.updates = updates;
        }
    }
}
