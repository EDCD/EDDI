using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
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
        public static bool TryFromFile(DateTime journalTimeStamp, string expectedStarSystem, string expectedStation, long expectedMarketID, [CanBeNull] out MarketInfo info, [CanBeNull] out string rawMarket, string filename = "Market.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawMarket = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawMarket))
                {
                    info = JsonConvert.DeserializeObject<MarketInfo>(rawMarket);
                }
                if (info?.Items != null &&
                    info.StarSystem == expectedStarSystem && 
                    info.StationName == expectedStation && 
                    info.MarketID == expectedMarketID)
                {
                    timeDiff = info.timestamp - journalTimeStamp;
                }
                attemptsRemaining--;
            } while ((timeDiff == null || timeDiff.Value.Duration().TotalSeconds >= 5) && attemptsRemaining > 0);

            return timeDiff != null && timeDiff.Value.Duration().TotalSeconds < 5;
        }
    }
}
