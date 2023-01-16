using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class ModuleInfo
    {
        [JsonProperty]
        public DateTime timestamp { get; }

        [JsonProperty]
        public List<ModuleInfoItem> Modules { get; } 

        public ModuleInfo(DateTime timestamp, List<ModuleInfoItem> modules)
        {
            this.timestamp = timestamp;
            Modules = modules ?? new List<ModuleInfoItem>();
        }

        [UsedImplicitly]
        public static bool TryFromFile(DateTime journalTimeStamp, [CanBeNull] out ModuleInfo info, [CanBeNull] out string rawModules, string filename = "ModulesInfo.json")
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawModules = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawModules))
                {
                    info = JsonConvert.DeserializeObject<ModuleInfo>(rawModules);
                }
                if (info?.Modules != null)
                {
                    timeDiff = info.timestamp - journalTimeStamp;
                }
                attemptsRemaining--;
            } while ((timeDiff == null || timeDiff.Value.Duration().TotalSeconds >= 5) && attemptsRemaining > 0);

            return timeDiff != null && timeDiff.Value.Duration().TotalSeconds < 5;
        }
    }
}
