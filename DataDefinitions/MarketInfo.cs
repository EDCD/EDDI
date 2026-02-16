using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class MarketInfo
    {
        [JsonProperty]
        public DateTime timestamp { get; }
        
        [JsonProperty]
        public long MarketID { get; }
        
        [JsonProperty]
        public string StationName { get; }
        
        [JsonProperty]
        public string StarSystem { get; }

        [JsonProperty]
        public List<MarketInfoItem> Items { get; }

        public MarketInfo(DateTime timestamp, long marketID, string stationName, string starSystem, List<MarketInfoItem> items)
        {
            this.timestamp = timestamp;
            MarketID = marketID;
            StationName = stationName;
            StarSystem = starSystem;
            Items = items ?? new List<MarketInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp,
            string expectedStarSystem, string expectedStation, long expectedMarketID,
            [CanBeNull] out MarketInfo info, [CanBeNull] out string rawMarket,
            string filename = "Market.json" )
        {
            info = null;
            rawMarket = null;

            var (raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<MarketInfo>( json );
                    return ( o?.timestamp, o );
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( parsed?.Items == null ||
                 parsed.StarSystem != expectedStarSystem ||
                 parsed.StationName != expectedStation ||
                 parsed.MarketID != expectedMarketID )
            {
                return false;
            }

            info = parsed;
            rawMarket = raw;
            return true;
        }
    }
}
