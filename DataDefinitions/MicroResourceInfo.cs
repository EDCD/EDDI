using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class MicroResourceInfo
    {
        [JsonProperty]
        public DateTime timestamp { get; private set; }

        [JsonProperty]
        public List<MicroResourceAmount> Items { get; private set; }

        [JsonProperty]
        public List<MicroResourceAmount> Components { get; private set; }

        [JsonProperty]
        public List<MicroResourceAmount> Consumables { get; private set; }

        [JsonProperty]
        public List<MicroResourceAmount> Data { get; private set; }

        public MicroResourceInfo(DateTime timestamp, List<MicroResourceAmount> items, List<MicroResourceAmount> components, List<MicroResourceAmount> consumables, List<MicroResourceAmount> data)
        {
            this.timestamp = timestamp;
            Items = items ?? new List<MicroResourceAmount>();
            Components = components ?? new List<MicroResourceAmount>();
            Consumables = consumables ?? new List<MicroResourceAmount>();
            Data = data ?? new List<MicroResourceAmount>();

            Items.ForEach(i =>
                i.microResource.Category = i.microResource.Category ?? MicroResourceCategory.Items);
            Components.ForEach(i =>
                i.microResource.Category = i.microResource.Category ?? MicroResourceCategory.Components);
            Consumables.ForEach(i =>
                i.microResource.Category = i.microResource.Category ?? MicroResourceCategory.Consumables);
            Data.ForEach(i =>
                i.microResource.Category = i.microResource.Category ?? MicroResourceCategory.Data);
        }

        [UsedImplicitly]
        public static bool TryFromFile(DateTime journalTimeStamp, [CanBeNull] out MicroResourceInfo info, [CanBeNull] out string rawMicroResources, string filename)
        {
            info = null;
            int attemptsRemaining = 10;
            TimeSpan? timeDiff = null;
            do
            {
                if (attemptsRemaining < 10) { Thread.Sleep(200); }
                rawMicroResources = Files.FromSavedGames(filename);
                if (!string.IsNullOrEmpty(rawMicroResources))
                {
                    info = JsonConvert.DeserializeObject<MicroResourceInfo>(rawMicroResources);
                }
                if (info?.Items != null &&
                    info.Components != null &&
                    info.Consumables != null &&
                    info.Data != null)
                {
                    timeDiff = info.timestamp - journalTimeStamp;
                }
                attemptsRemaining--;
            } while ((timeDiff == null || timeDiff.Value.Duration().TotalSeconds >= 5) && attemptsRemaining > 0);

            return timeDiff != null && timeDiff.Value.Duration().TotalSeconds < 5;
        }

        [UsedImplicitly]
        public static List<MicroResourceAmount> ReadMicroResources(string key, IDictionary<string, object> data)
        {
            var result = new List<MicroResourceAmount>();
            if (data.TryGetValue(key, out object val))
            {
                if (val is List<object> listVal)
                {
                    foreach (IDictionary<string, object> microResourceVal in listVal)
                    {
                        var edname = JsonParsing.getString(microResourceVal, "Name");
                        var fallbackName = JsonParsing.getString(microResourceVal, "Name_Localised");
                        var categoryEdName = JsonParsing.getString(microResourceVal, "Type") ?? MicroResourceCategory.Unknown.edname;
                        var ownerId = JsonParsing.getOptionalLong(microResourceVal, "OwnerID");
                        var missionId = JsonParsing.getOptionalLong(microResourceVal, "MissionID");
                        var amount = JsonParsing.getInt(microResourceVal, "Count");
                        result.Add(new MicroResourceAmount(edname, ownerId, amount, categoryEdName, fallbackName, missionId));
                    }
                }
            }
            return result;
        }
    }
}
