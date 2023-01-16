using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipyardInfo
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
        public bool AllowCobraMkIV { get; }

        [JsonProperty]
        public List<ShipyardInfoItem> PriceList { get; }

        public ShipyardInfo(DateTime timestamp, long marketID, string stationName, string starSystem, bool horizons, bool allowCobraMkIV, List<ShipyardInfoItem> priceList)
        {
            this.timestamp = timestamp;
            MarketID = marketID;
            StationName = stationName;
            StarSystem = starSystem;
            Horizons = horizons;
            AllowCobraMkIV = allowCobraMkIV;
            PriceList = priceList ?? new List<ShipyardInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile(DateTime journalTimeStamp, string expectedStarSystem, string expectedStation, long expectedMarketID, [CanBeNull] out ShipyardInfo info, [CanBeNull] out string rawShipyard, string filename = "Shipyard.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawShipyard = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawShipyard))
                {
                    info = JsonConvert.DeserializeObject<ShipyardInfo>(rawShipyard);
                }
                if (info?.PriceList != null &&
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

