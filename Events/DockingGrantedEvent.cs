using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockingGrantedEvent (
        DateTime timestamp,
        string station,
        StationModel stationType,
        long marketId,
        int landingpad )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Docking granted";
        public const string DESCRIPTION = "Triggered when your ship is granted docking permission at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingGranted\",\"MarketID\": 128666762,\"StationName\":\"Jameson Memorial\",\"StationType\":\"Orbis\",\"LandingPad\":2}";

        [PublicAPI("The station at which the commander has been granted docking")]
        public string station { get; private set; } = station;

        [PublicAPI("The localized model / type of the station at which the commander has been granted docking")]
        public string stationtype => stationDefinition?.localizedName;

        [PublicAPI("The landing pad at which the commander has been granted docking")]
        public int landingpad { get; private set; } = landingpad;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public StationModel stationDefinition { get; private set; } = stationType;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketId = JsonParsing.getLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
            var landingPad = JsonParsing.getInt(data, "LandingPad");
            events.Add( new DockingGrantedEvent( timestamp, stationLocalizedName ?? stationName, stationType, marketId, landingPad ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
