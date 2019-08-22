using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class VolcanismConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Volcanism));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                // This is an object - use standard deserialization
                return serializer.Deserialize<Volcanism>(reader);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                // This is a simple string, from EDDB.
                return Volcanism.FromName(reader.Value.ToString());
            }
            else
            {
                // Unsupported
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
