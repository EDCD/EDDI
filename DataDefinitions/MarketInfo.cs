using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class MarketInfo (
        DateTime timestamp,
        long marketId,
        string stationName,
        string starSystem,
        List<MarketInfoItem> items )
    {
        [JsonProperty]
        public DateTime timestamp { get; } = timestamp;

        [JsonProperty]
        public long MarketID { get; } = marketId;

        [JsonProperty]
        public string StationName { get; } = stationName;

        [JsonProperty]
        public string StarSystem { get; } = starSystem;

        [JsonProperty]
        public List<MarketInfoItem> Items { get; } = items ?? [ ];

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp,
            string expectedStarSystem, string expectedStation, long expectedMarketID,
            [CanBeNull] out MarketInfo info, [CanBeNull] out string rawMarket,
            string filename = "Market.json" )
        {
            info = null;
            rawMarket = null;

            var (raw, parsed, isRecent) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<MarketInfo>( json );
                    return ( o?.timestamp, o );
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( isRecent && parsed?.Items != null &&
                 parsed.StarSystem == expectedStarSystem &&
                 parsed.StationName == expectedStation &&
                 parsed.MarketID == expectedMarketID )
            {
                info = parsed;
                rawMarket = raw;
                return true;
            }

            return false;
        }
    }
}
