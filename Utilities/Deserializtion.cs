using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public class Deserializtion
    {
        public static IDictionary<string, object> DeserializeData(string data)
        {
            if (data == null)
            {
                return new Dictionary<string, object>();
            }

            Logging.Debug("Deserializing " + data);
            Dictionary<string, object> values;
            try
            {
                values = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            }
            catch (JsonReaderException e)
            {
                Logging.Error($"Unable to read from json string: {data}", e);
                throw;
            }

            return DeserializeData(values);
        }

        private static IDictionary<string, object> DeserializeData(IDictionary<string, object> data)
        {
            if (data is null) { return new Dictionary<string, object>(); }

            foreach (var key in data.Keys.ToArray())
            {
                var value = data[key];

                if (value is JObject)
                    data[key] = DeserializeData(value as JObject);

                if (value is JArray)
                    data[key] = DeserializeData(value as JArray);
            }

            return data;
        }

        private static IDictionary<string, object> DeserializeData(JObject data)
        {
            var dict = data.ToObject<Dictionary<string, object>>();
            if (dict != null)
            {
                return DeserializeData(dict);
            }
            else
            {
                return null;
            }
        }

        private static IList<object> DeserializeData(JArray data)
        {
            var list = data.ToObject<List<object>>();

            for (int i = 0; i < list.Count; i++)
            {
                var value = list[i];

                if (value is JObject)
                    list[i] = DeserializeData(value as JObject);

                if (value is JArray)
                    list[i] = DeserializeData(value as JArray);
            }
            return list;
        }
    }
}
