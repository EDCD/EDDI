using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class UndockedEvent ( DateTime timestamp, string station, long? marketId ) : Event( timestamp, NAME )
    {
        public const string NAME = "Undocked";
        public const string DESCRIPTION = "Triggered when your ship undocks from a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"Undocked\",\"StationName\":\"Long Sight Base\", \"MarketID\": 128678023}";

        [PublicAPI("The station from which the commander has undocked")]
        public string station { get; private set; } = station;

        [PublicAPI("Market ID of the station from which the commander has undocked")]
        public long? marketId { get; private set; } = marketId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var stationName = JsonParsing.getString(data, "StationName");
            long? marketId = JsonParsing.getLong(data, "MarketID");
            events.Add( new UndockedEvent( timestamp, stationName, marketId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}