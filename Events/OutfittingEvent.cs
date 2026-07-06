using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class OutfittingEvent (
        DateTime timestamp,
        long marketId,
        string station,
        string system,
        OutfittingInfo info )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Outfitting";
        public const string DESCRIPTION = "Triggered when the Outfitting.json file has been updated";
        public const string SAMPLE = @"{ ""timestamp"":""2017-10-05T10:11:38Z"", ""event"":""Outfitting"", ""MarketID"":128678535, ""StationName"":""Black Hide"", ""StarSystem"":""Wyrd"" }";

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public OutfittingInfo info { get; private set; } = info;

        public static bool Handle ( DateTime timestamp, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketId = JsonParsing.getLong(data, "MarketID");
            var station = JsonParsing.getString(data, "StationName");
            var system = JsonParsing.getString(data, "StarSystem");
            if ( OutfittingInfo.TryFromFile( timestamp, system, station, marketId, out var info, out var raw ) )
            {
                events.Add( new OutfittingEvent( timestamp, marketId, station, system, info ) { raw = raw, fromLoad = fromLogLoad } );
            }
            return true;
        }
    }
}