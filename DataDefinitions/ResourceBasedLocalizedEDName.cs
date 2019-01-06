using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using Utilities;

namespace EddiDataDefinitions
{
    // A JsonConverter that correctly initialises ResourceBasedLocalizedEDName<T> instances using their static FromEDName() method.
    // Unfortunately we cannot make this a generic type as they are not allowed as parameters for JsonConverterAttribute (or any attribute for that matter),
    // so instead we have to access FromEDName() via the reflection API.
    public class JsonConverterFromEDName : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ResourceBasedLocalizedEDName<>).IsAssignableFrom(objectType);
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                // bail early on known null cases
                switch (reader.TokenType)
                {
                    case JsonToken.None:
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                    case JsonToken.EndObject:
                    case JsonToken.EndArray:
                        return null;
                    default:
                        break;
                }

                // get the edname
                JObject jsonObject = JObject.Load(reader);
                bool success = jsonObject.TryGetValue("edname", out JToken token);
                if (!success)
                {
                    return null;
                }
                string edname = token.Value<string>();

                // get the FromEDName() method
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
                Type[] argumentTypes = new Type[] { typeof(string) };
                MethodInfo method = objectType.GetMethod("FromEDName", bindingFlags, binder: null, types: argumentTypes, modifiers: null);

                // invoke the method
                object[] parameters = new object[] { edname };
                object result = method?.Invoke(null, parameters);
                return result;
            }
            catch (Exception)
            {
                // let the Json.Net machinery handle the exception
                throw;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [JsonObject(MemberSerialization.OptIn), JsonConverter(typeof(JsonConverterFromEDName))]
    public class ResourceBasedLocalizedEDName<T> where T : ResourceBasedLocalizedEDName<T>, new()
    {
        static ResourceBasedLocalizedEDName()
        {
            lock (resourceLock)
            {
                AllOfThem = new List<T>();
            }
        }
        public static readonly List<T> AllOfThem;
        protected static ResourceManager resourceManager;
        protected static Func<string, T> missingEDNameHandler;
        public static readonly object resourceLock = new object();

        [JsonProperty]
        public readonly string edname;

        [JsonIgnore]
        public readonly string basename;

        [JsonIgnore]
        public string invariantName => resourceManager.GetString(basename, CultureInfo.InvariantCulture) ?? basename;

        [JsonIgnore]
        public string fallbackLocalizedName { get; set; } = null;

        [JsonIgnore]
        public string localizedName => resourceManager.GetString(basename) ?? fallbackLocalizedName ?? basename;

        [JsonIgnore]
        [Obsolete("Please be explicit and use localizedName or invariantName")]
        public string name => localizedName;

        public override string ToString()
        {
            return localizedName;
        }

        protected ResourceBasedLocalizedEDName(string edname, string basename)
        {
            this.edname = edname;
            this.basename = basename;

            if (!string.IsNullOrEmpty(edname))
            {
                lock (resourceLock)
                {
                    AllOfThem.Add(this as T);
                }
            }
        }

        private static void EnsureSubClassStaticConstructorHasRun()
        {
            if (AllOfThem.Count == 0)
            {
                T dummy = new T();
            }
        }

        public static T FromName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (from == null || from == string.Empty)
            {
                return null;
            }

            from = from.ToLowerInvariant();
            T result;
            lock (resourceLock)
            {
                result = AllOfThem.FirstOrDefault(
                    v =>
                    v.localizedName.ToLowerInvariant() == from
                    || v.invariantName.ToLowerInvariant() == from);
            }
            return result;
        }

        public static T FromEDName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (from == null || from == string.Empty)
            {
                return null;
            }

            string tidiedFrom = from?.Replace(";", "").Replace(" ", "").ToLowerInvariant();
            T result;
            lock (resourceLock)
            {
                result = AllOfThem.FirstOrDefault(
                    v => v.edname
                    .ToLowerInvariant()
                    .Replace(";", "") == tidiedFrom);
            }
            if (result == null)
            {
                Logging.Info($"Unknown ED name {from} in resource {resourceManager.BaseName}");
                if (missingEDNameHandler != null)
                {
                    result = missingEDNameHandler(from);
                }
            }
            return result;
        }
    }
}
