using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipyardInfo (
        DateTime timestamp,
        long marketId,
        string stationName,
        string starSystem,
        bool horizons,
        bool allowCobraMkIv,
        List<ShipyardInfoItem> priceList )
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
        public bool Horizons { get; } = horizons;

        [JsonProperty]
        public bool AllowCobraMkIV { get; } = allowCobraMkIv;

        [JsonProperty]
        public List<ShipyardInfoItem> PriceList { get; } = priceList ?? [ ];

        [ UsedImplicitly ]
        public static bool TryFromFile ( DateTime journalTimeStamp, 
            string expectedStarSystem, string expectedStation, long expectedMarketID, 
            [ CanBeNull ] out ShipyardInfo info, [ CanBeNull ] out string rawShipyard,
            string filename = "Shipyard.json" )
        {
            info = null;
            rawShipyard = null;

            var ( raw, parsed, isRecent) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<ShipyardInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout ( TimeSpan.FromSeconds( 5 ) );

            if ( isRecent && parsed?.PriceList != null
                          && parsed.StarSystem == expectedStarSystem
                          && parsed.StationName == expectedStation
                          && parsed.MarketID == expectedMarketID )
            {
                info = parsed;
                rawShipyard = raw;
                return true;
            }

            return false;
        }
    }
}

