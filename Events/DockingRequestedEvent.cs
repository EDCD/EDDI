using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockingRequestedEvent (
        DateTime timestamp,
        string station,
        StationModel stationType,
        long marketId,
        StationLandingPads landingPads )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Docking requested";
        public const string DESCRIPTION = "Triggered when your ship requests docking at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingRequested\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"MarketID\": 128666762, \"LandingPads\": { \"Large\": 9, \"Medium\": 18, \"Small\": 7 } }";

        [PublicAPI("The station at which the commander has requested docking")]
        public string station { get; private set; } = station;

        [PublicAPI("The localized model / type of the station at which the commander has requested docking")]
        public string stationtype => stationDefinition?.localizedName;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public StationModel stationDefinition { get; private set; } = stationType;

        public StationLandingPads landingPads { get; private set; } = landingPads;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketId = JsonParsing.getLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );

            // Get station landing pads data
            var landingPads = EventParsing.LandingPadsCollection(data);

            events.Add( new DockingRequestedEvent( timestamp, stationLocalizedName ?? stationName, stationType, marketId, landingPads ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
