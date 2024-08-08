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

                // add back in any secondary properties and fields in our derived classes (other than those associated with the `FromEDName` method)
                var otherProperties = jsonObject.Values().Where(t => t.Path != "edname");
                foreach (var prop in otherProperties)
                {
                    var propInfo = result?.GetType().GetProperty(prop.Path);
                    if (propInfo != null && propInfo.CanWrite)
                    {
                        propInfo.SetValue(result, prop.ToObject(propInfo.PropertyType));
                    }
                    var fieldInfo = result?.GetType().GetField(prop.Path);
                    fieldInfo?.SetValue(result, prop.ToObject(fieldInfo.FieldType));
                }
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
    public abstract class ResourceBasedLocalizedEDName<T> : IEqualityComparer<T> where T : ResourceBasedLocalizedEDName<T>, new()
    {
        static ResourceBasedLocalizedEDName()
        {
            lock (resourceLock)
            {
                AllOfThem = new List<T>();
            }
        }

        public static List<T> AllOfThem
        {
            get { EnsureSubClassStaticConstructorHasRun(); return allOfThem; }
            private set => allOfThem = value;
        }
        private static List<T> allOfThem;

        protected static Func<string, T> missingEDNameHandler;

        // ReSharper disable StaticMemberInGenericType
        // This is as intended, with separate values for each derived type
        // rather than a single shared value across all types.
        protected static ResourceManager resourceManager;
        public static readonly object resourceLock = new object();
        // ReSharper restore StaticMemberInGenericType

        [JsonProperty]
        public readonly string edname;

        [JsonIgnore]
        public readonly string basename;

        [PublicAPI, JsonIgnore]
        public string invariantName => resourceManager.GetString(basename, CultureInfo.InvariantCulture) ?? fallbackInvariantName ?? basename;

        /// <summary>
        /// Used only for synthetic definitions derived from other object types
        /// </summary>
        [JsonIgnore]
        public string fallbackInvariantName { get; set; } = null;

        [JsonIgnore]
        public string localizedName => resourceManager.GetString(basename) ?? fallbackLocalizedName ?? basename;
        
        [JsonIgnore]
        public string fallbackLocalizedName { get; set; } = null;

        [PublicAPI, JsonIgnore, Obsolete("Please be explicit and use localizedName or invariantName")]
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
                    allOfThem.Add(this as T);
                }
            }
        }

        private static void EnsureSubClassStaticConstructorHasRun()
        {
            if (allOfThem.Count == 0)
            {
                T _ = new T();
            }
        }

        public static T FromName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (string.IsNullOrEmpty(from))
            {
                return null;
            }

            from = from.ToLowerInvariant().Trim();
            T result;
            lock (resourceLock)
            {
                result = allOfThem.FirstOrDefault(
                    v =>
                    v.localizedName.ToLowerInvariant().Trim() == from
                    || v.invariantName.ToLowerInvariant().Trim() == from);
            }
            return result;
        }

        public static T FromEDName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (string.IsNullOrEmpty(from))
            {
                return null;
            }

            string tidiedFrom = from?.Replace(";", "").Replace(" ", "").ToLowerInvariant().Trim();
            T result;
            lock (resourceLock)
            {
                result = allOfThem.FirstOrDefault(
                    v => v.edname
                    .ToLowerInvariant()
                    .Replace(";", "").Trim() == tidiedFrom);
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

        public bool Equals(T x, T y)
        {
            return x?.edname == y?.edname;
        }

        public int GetHashCode(T obj)
        {
            return obj.edname.GetHashCode();
        }
    }
}
