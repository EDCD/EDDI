using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class CargoInfo
    {
        [JsonProperty]
        public DateTime timestamp { get; }

        [JsonProperty]
        public string Vessel { get; }

        [JsonProperty]
        public int Count { get; }

        [JsonProperty]
        public List<CargoInfoItem> Inventory { get; }

        public CargoInfo(DateTime timestamp, string vessel, int count, List<CargoInfoItem> inventory)
        {
            this.timestamp = timestamp;
            Vessel = vessel;
            Count = count;
            Inventory = inventory ?? new List<CargoInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile (
            DateTime journalTimeStamp,
            string expectedVessel, int expectedCount,
            [CanBeNull] out CargoInfo info, [CanBeNull] out string rawCargo,
            string filename = "Cargo.json" )
        {
            info = null;
            rawCargo = null;

            var (raw, parsed) = Files.FromSavedGamesAsync(
                filename,
                extract: json =>
                {
                    var o = JsonConvert.DeserializeObject<CargoInfo>( json );
                    return (o?.timestamp, o);
                },
                compareTo: journalTimeStamp
            ).GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );

            if ( parsed?.Inventory != null &&
                 parsed.Vessel == expectedVessel &&
                 parsed.Count == expectedCount )
            {
                info = parsed;
                rawCargo = raw;
                return true;
            }

            return false;
        }
    }
}
