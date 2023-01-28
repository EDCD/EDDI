using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class OutfittingInfo
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
        public bool Horizons { get; }
        
        [JsonProperty]
        public List<OutfittingInfoItem> Items { get; }

        public OutfittingInfo (DateTime timestamp, long marketID, string stationName, string starSystem, List<OutfittingInfoItem> items)
        {
            this.timestamp = timestamp;
            MarketID = marketID;
            StationName = stationName;
            StarSystem = starSystem;
            Items = items ?? new List<OutfittingInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile(DateTime journalTimeStamp, string expectedStarSystem, string expectedStation, long expectedMarketID, [CanBeNull] out OutfittingInfo info, [CanBeNull] out string rawOutfitting, string filename = "Outfitting.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawOutfitting = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawOutfitting))
                {
                    info = JsonConvert.DeserializeObject<OutfittingInfo>(rawOutfitting);
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

