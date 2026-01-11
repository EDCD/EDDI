using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public static bool TryFromFile ( DateTime journalTimeStamp, string expectedStarSystem, string expectedStation,
            long expectedMarketID, [ CanBeNull ] out ShipyardInfo info, [ CanBeNull ] out string rawShipyard,
            string filename = "Shipyard.json" )
        {
            var attemptsRemaining = 10;
            TimeSpan? timeDiff = null;

            ( info, rawShipyard ) = Task.Run( async () =>
            {
                do
                {
                    ShipyardInfo shipyardInfo = null;
                    var raw = Files.FromSavedGames( filename );
                    if ( !string.IsNullOrEmpty( raw ) )
                    {
                        shipyardInfo = JsonConvert.DeserializeObject<ShipyardInfo>( raw );
                    }

                    if ( shipyardInfo?.PriceList != null &&
                         shipyardInfo.StarSystem == expectedStarSystem &&
                         shipyardInfo.StationName == expectedStation &&
                         shipyardInfo.MarketID == expectedMarketID )
                    {
                        timeDiff = shipyardInfo.timestamp - journalTimeStamp;
                        return ( shipyardInfo, raw );
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

