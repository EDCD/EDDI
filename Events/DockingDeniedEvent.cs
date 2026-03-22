using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockingDeniedEvent (
        DateTime timestamp,
        string station,
        StationModel stationType,
        long marketId,
        string reason )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Docking denied";
        public const string DESCRIPTION = "Triggered when your ship is denied docking at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingDenied\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"MarketID\": 128666762,\"Reason\":\"Distance\"}";

        [PublicAPI("The station at which the commander has been denied docking")]
        public string station { get; private set; } = station;

        [PublicAPI("The localized model / type of the station at which the commander has been denied docking")]
        public string stationtype => stationDefinition?.localizedName;

        [PublicAPI("The reason why commander has been denied docking (too far, fighter deployed etc)")]
        public string reason { get; private set; } = reason;

        // These properties are not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public StationModel stationDefinition { get; private set; } = stationType;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketId = JsonParsing.getLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
            var reason = JsonParsing.getString(data, "Reason");
            events.Add( new DockingDeniedEvent( timestamp, stationLocalizedName ?? stationName, stationType, marketId, reason ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
