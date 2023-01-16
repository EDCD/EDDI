using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
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
        public static bool TryFromFile(DateTime journalTimeStamp, string expectedVessel, int expectedCount, [CanBeNull] out CargoInfo info, [CanBeNull] out string rawCargo, string filename = "Cargo.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawCargo = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawCargo))
                {
                    info = JsonConvert.DeserializeObject<CargoInfo>(rawCargo);
                }
                if (info?.Inventory != null &&
                    info.Vessel == expectedVessel &&
                    info.Count == expectedCount)
                {
                    timeDiff = info.timestamp - journalTimeStamp;
                }
                attemptsRemaining--;
            } while ((timeDiff == null || timeDiff.Value.Duration().TotalSeconds >= 5) && attemptsRemaining > 0);

            return timeDiff != null && timeDiff.Value.Duration().TotalSeconds < 5;
        }
    }
}
