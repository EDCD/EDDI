using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [ UsedImplicitly ]
        public static bool TryFromFile ( DateTime journalTimeStamp, string expectedStarSystem, string expectedStation,
            long expectedMarketID, [ CanBeNull ] out OutfittingInfo info, [ CanBeNull ] out string rawOutfitting,
            string filename = "Outfitting.json" )
        {
            var attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            
            ( info, rawOutfitting ) = Task.Run( async () =>
            {
                do
                {
                    OutfittingInfo outfittingInfo = null;
                    var raw = Files.FromSavedGames( filename );
                    if ( !string.IsNullOrEmpty( raw ) )
                    {
                        outfittingInfo = JsonConvert.DeserializeObject<OutfittingInfo>( raw );
                    }

                    if ( outfittingInfo?.Items != null &&
                         outfittingInfo.StarSystem == expectedStarSystem &&
                         outfittingInfo.StationName == expectedStation &&
                         outfittingInfo.MarketID == expectedMarketID )
                    {
                        timeDiff = outfittingInfo.timestamp - journalTimeStamp;
                        return ( outfittingInfo, raw );
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

