using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class MicroResourceInfo
    {
        public DateTime timestamp { get; private set; }
        public List<MicroResourceAmount> Items { get; private set; }
        public List<MicroResourceAmount> Components { get; private set; }
        public List<MicroResourceAmount> Consumables { get; private set; }
        public List<MicroResourceAmount> Data { get; private set; }

        public MicroResourceInfo()
        {
            Items = new List<MicroResourceAmount>();
            Components = new List<MicroResourceAmount>();
            Consumables = new List<MicroResourceAmount>();
            Data = new List<MicroResourceAmount>();
        }

        public MicroResourceInfo FromFile(string filename = null)
        {
            var inventory = new MicroResourceInfo();
            string json = Files.FromSavedGames(filename);
            if (!string.IsNullOrEmpty(json))
            {
                var data = Deserializtion.DeserializeData(json);
                inventory = FromData(data);
            }
            return inventory;
        }

        public MicroResourceInfo FromData(IDictionary<string, object> data)
        {
            timestamp = JsonParsing.getDateTime("timestamp", data);
            Components = ReadMicroResources("Components", data, "Component");
            Consumables = ReadMicroResources("Consumables", data, "Consumable");
            Data = ReadMicroResources("Data", data, "Data");
            Items = ReadMicroResources("Items", data, "Item");
            return this;
        }

        public static List<MicroResourceAmount> ReadMicroResources(string key, IDictionary<string, object> data, string categoryEdName = null)
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
                        categoryEdName = JsonParsing.getString(microResourceVal, "Type") ?? categoryEdName ?? MicroResourceCategory.Unknown.edname;
                        var resource = MicroResource.FromEDName(edname, fallbackName, categoryEdName);

                        var ownerId = JsonParsing.getOptionalInt(microResourceVal, "OwnerID");
                        var missionId = JsonParsing.getOptionalDecimal(microResourceVal, "MissionID");
                        var amount = JsonParsing.getInt(microResourceVal, "Count");

                        result.Add(new MicroResourceAmount(resource, ownerId, missionId, amount));
                    }
                }
            }
            return result;
        }
    }
}
