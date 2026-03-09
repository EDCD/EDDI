using JetBrains.Annotations;
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
            // bail early on known null cases
            switch (reader.TokenType)
            {
                case JsonToken.None:
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    return null;
            }

            // get the edname
            var jsonObject = JObject.Load(reader);
            var success = jsonObject.TryGetValue("edname", out var token);
            if (!success)
            {
                return null;
            }
            var edname = token.Value<string>();

            // get the FromEDName() method
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            var argumentTypes = new[] { typeof(string) };
            var method = objectType.GetMethod("FromEDName", bindingFlags, binder: null, types: argumentTypes, modifiers: null);

            // Invoke the FromEDName() method to get the base instance
            var parameters = new object[] { edname };
            var baseInstance = method?.Invoke(null, parameters);
            if ( baseInstance == null )
            {
                return null; // If no instance is found, return null
            }

            // Clone the base instance into a new instance
            var result = Activator.CreateInstance(objectType, nonPublic: true);
            foreach ( var prop in objectType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
            {
                if ( prop.CanWrite )
                {
                    prop.SetValue( result, prop.GetValue( baseInstance ) );
                }
            }
            foreach ( var field in objectType.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic ) )
            {
                field.SetValue( result, field.GetValue( baseInstance ) );
            }

            // Populate additional properties and fields from the JSON object
            var otherProperties = jsonObject.Properties().Where(p => p.Name != "edname");
            foreach ( var prop in otherProperties )
            {
                var propInfo = result.GetType().GetProperty(prop.Name);
                if ( propInfo != null && propInfo.CanWrite )
                {
                    propInfo.SetValue( result, prop.Value.ToObject( propInfo.PropertyType ) );
                }
                var fieldInfo = result.GetType().GetField(prop.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo?.SetValue( result, prop.Value.ToObject( fieldInfo.FieldType ) );
            }
            return result;
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
        public static readonly object resourceLock = new();
        // ReSharper restore StaticMemberInGenericType

        [JsonProperty]
        public readonly string edname;

        [JsonIgnore]
        public readonly string basename;

        [Utilities.PublicAPI, JsonIgnore]
        public string invariantName => resourceManager.GetString(basename, CultureInfo.InvariantCulture) ?? fallbackInvariantName ?? basename;

        /// <summary>
        /// Used only for synthetic definitions derived from other object types
        /// </summary>
        [JsonIgnore]
        public string fallbackInvariantName { get; set; }

        [JsonIgnore]
        public string localizedName => resourceManager.GetString(basename) ?? fallbackLocalizedName ?? basename;
        
        [JsonIgnore]
        public string fallbackLocalizedName { get; set; }

        [Utilities.PublicAPI, JsonIgnore, Obsolete("Please be explicit and use localizedName or invariantName")]
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
                _ = new T();
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

        [CanBeNull]
        public static T FromEDName(string from)
        {
            EnsureSubClassStaticConstructorHasRun();
            if (string.IsNullOrEmpty(from))
            {
                return null;
            }

            var tidiedFrom = from.Replace(";", "").Replace(" ", "").ToLowerInvariant().Trim();
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
