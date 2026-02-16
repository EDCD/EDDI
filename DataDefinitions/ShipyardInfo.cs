using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        [ UsedImplicitly ]
        public static bool TryFromFile ( DateTime journalTimeStamp, 
            string expectedStarSystem, string expectedStation, long expectedMarketID, 
            [ CanBeNull ] out ShipyardInfo info, [ CanBeNull ] out string rawShipyard,
            string filename = "Shipyard.json" )
        {
            info = null;
            rawShipyard = null;

            var ( raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<ShipyardInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout ( TimeSpan.FromSeconds( 5 ) );

            if (parsed?.PriceList != null &&
                parsed.StarSystem == expectedStarSystem &&
                parsed.StationName == expectedStation &&
                parsed.MarketID == expectedMarketID )
            {
                return false;
            }

            info = parsed;
            rawShipyard = raw;
            return true;
        }
    }
}

