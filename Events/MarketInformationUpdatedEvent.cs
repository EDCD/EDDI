using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    /// <summary>The timestamp recorded for this event must be generated from game or server data.
    /// System time (e.g. DateTime.UtcNow) cannot be trusted for reporting to EDDN and may not be used.</summary>
    [PublicAPI]
    public class MarketInformationUpdatedEvent ( DateTime timestamp, long marketID, string stationName, string systemName,
        HashSet<string> updates ) : Event(timestamp, NAME)
    {
        public const string NAME = "Market information updated";
        public const string DESCRIPTION = "Triggered when market information for the currently docked station has been updated";
        public const string SAMPLE = null;

        [PublicAPI( "A list of the updates triggering the event (which may include 'market', 'outfitting', and 'shipyard'" )]
        public HashSet<string> updates { get; private set; } = updates;

        [PublicAPI( "The numeric market ID of the station" )]
        public long marketID { get; private set; } = marketID;

        [PublicAPI( "The name of the station" )]
        public string stationName { get; private set; } = stationName;

        [PublicAPI( "The name of the star system" )]
        public string systemName { get; private set; } = systemName;
    }
}
