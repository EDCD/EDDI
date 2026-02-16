using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            info = null;
            rawOutfitting = null;

            var ( raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<OutfittingInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout ( TimeSpan.FromSeconds( 5 ) );

            if ( parsed?.Items != null &&
                 parsed.StarSystem == expectedStarSystem &&
                 parsed.StationName == expectedStation &&
                 parsed.MarketID == expectedMarketID )
            {
                info = parsed;
                rawOutfitting = raw;
                return true;
            }

            return false;
        }
    }
}

