using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [ UsedImplicitly ]
        public static bool TryFromFile ( DateTime journalTimeStamp, string expectedStarSystem, string expectedStation,
            long expectedMarketID, [ CanBeNull ] out MarketInfo info, [ CanBeNull ] out string rawMarket,
            string filename = "Market.json" )
        {
            var attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            
            ( info, rawMarket ) = Task.Run( async () =>
            {
                do
                {
                    MarketInfo marketInfo = null;
                    var raw = Files.FromSavedGames( filename );
                    if ( !string.IsNullOrEmpty( raw ) )
                    {
                        marketInfo = JsonConvert.DeserializeObject<MarketInfo>( raw );
                    }

                    if ( marketInfo?.Items != null &&
                         marketInfo.StarSystem == expectedStarSystem &&
                         marketInfo.StationName == expectedStation &&
                         marketInfo.MarketID == expectedMarketID )
                    {
                        timeDiff = marketInfo.timestamp - journalTimeStamp;
                        return ( marketInfo, raw );
                    }

                    attemptsRemaining--;
                    await Task.Delay( 200 );
                } while ( !fileIsRecent( timeDiff ) && attemptsRemaining > 0 );

                return ( null, null );
            } ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            return fileIsRecent( timeDiff );

            bool fileIsRecent ( TimeSpan? timeDifference ) =>
                timeDifference == null || timeDifference.Value.Duration().TotalSeconds >= 5;
        }
    }
}
