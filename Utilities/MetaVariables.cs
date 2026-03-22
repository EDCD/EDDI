using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public class MetaVariables
    {
        public MetaVariables(Type reflectionObjectType, object reflectionObject = null, int? maxRecursionLevel = null)
        {
            if (reflectionObjectType is null) 
            { 
                Results = [ ]; 
            }
            else
            {
                Results = GetVariables(reflectionObjectType, maxRecursionLevel, reflectionObject);
            }
        }

        public List<MetaVariable> Results { get; private set; }

        // Some types don't need to be decomposed further - we'll stop reflecting when we hit these types
        private static readonly HashSet<Type> undecomposedTypes =
        [
            typeof(string),
            typeof(bool),
            typeof(int),
            typeof(decimal),
            typeof(long),
            typeof(ulong),
            typeof(double),
            typeof(float),
            typeof(DateTime),
            typeof(TimeSpan)
        ];

        // Apply a placeholder symbol for collection indices - to be formatted
        // differently according to the end variable type (Cottle or VoiceAttack) 
        public const string indexMarker = @"<index\>";

        /// <summary> Cache for type members to avoid reflection overhead </summary>
        private static readonly ConcurrentDictionary<Type, (PropertyInfo[], FieldInfo[])> typeCache = new();

        /// <summary> Get the properties and fields of a type, caching the results </summary>
        private static (PropertyInfo[], FieldInfo[]) GetTypeMembers(Type type)
        {
            return typeCache.GetOrAdd(type, t => (
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance),
                t.GetFields(BindingFlags.Public | BindingFlags.Instance)
            ));
        }

        /// <summary> Walk an object and write out all of the possible fields </summary>
        /// <param name="reflectionObjectType">The Type property of the object that we're walking, specified independent from the actual object in case the actual object value is null</param>
        /// <param name="maxRecursionLevel">The maximum number of recursion levels to walk while obtaining our metavariables</param>
        /// <param name="reflectionObject">(Optional) The object that we're walking to obtain values. At the top level, this should be an `Event` class object</param>
        /// <param name="keysPath">(Used internally, do not set) The path to the specific key</param>
        private List<MetaVariable> GetVariables(Type reflectionObjectType, int? maxRecursionLevel, object reflectionObject = null, List<string> keysPath = null)
        {
            if (keysPath is null) { keysPath = [ ]; }
            if (Results is null) { Results = [ ]; }

            // Some types don't need to be decomposed further.
            if (undecomposedTypes.Contains(reflectionObjectType))
            {
                GetVariable(keysPath.ToList(), "", reflectionObjectType, string.Empty, reflectionObject, maxRecursionLevel );
                return Results;
            }

            var (objectProperties, objectFields) = GetTypeMembers(reflectionObjectType);

            foreach (var eventProperty in objectProperties)
            {
                // We ignore some keys which we've marked in advance
                var passProperty = false;
                foreach (var attribute in eventProperty.GetCustomAttributes())
                {
                    if (attribute is PublicAPIAttribute publicAPIAttribute)
                    {
                        passProperty = true;
                        GetVariable(keysPath.ToList(), eventProperty.Name, eventProperty.PropertyType, publicAPIAttribute.Description, eventProperty.CanRead && reflectionObject != null ? eventProperty.GetValue(reflectionObject) : null, maxRecursionLevel );
                        break;
                    }
                }
                if (!passProperty)
                {
                    // Ignored Key
                }
            }

            foreach (var eventField in objectFields)
            {
                // We ignore some keys which we've marked in advance
                var passField = false;
                foreach (var attribute in eventField.GetCustomAttributes())
                {
                    if (attribute is PublicAPIAttribute publicAPIAttribute)
                    {
                        passField = true;
                        GetVariable(keysPath.ToList(), eventField.Name, eventField.FieldType, publicAPIAttribute.Description, reflectionObject != null ? eventField.GetValue(reflectionObject) : null, maxRecursionLevel );
                        break;
                    }
                }
                if (!passField)
                {
                    // Ignored Key
                }
            }

            return Results;
        }

        private void GetVariable(List<string> keysPath, string key, Type type, string description, object value, int? maxRecursionLevel )
        {
            try
            {
                var oldKeysPath = keysPath.ToList();
                keysPath.Add(key);

                // We ignore any key paths that we have already set elsewhere
                foreach (var v in Results)
                {
                    if (v.keysPath.SequenceEqual(keysPath))
                    {
                        return;
                    }
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Get the underlying type for nullable types
                    type = Nullable.GetUnderlyingType(type);
                }

                if (type == typeof(bool))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (bool?)value));
                }
                else if (type == typeof(string))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (string)value));
                }
                else if (type == typeof(int))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (int?)value));
                }
                else if (type == typeof(long))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (long?)value));
                }
                else if (type == typeof(ulong))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (ulong?)value));
                }
                else if (type == typeof(double))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (double?)value));
                }
                else if (type == typeof(float))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (float?)value));
                }
                else if (type == typeof(decimal))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, value is null ? null : (decimal?)Convert.ToDecimal(value)));
                }
                else if (type == typeof(DateTime))
                {
                    Results.Add(new MetaVariable(keysPath, type, description, (DateTime?)value));
                }
                else if (type is null)
                {
                    Results.Add(new MetaVariable(keysPath, null, description, null));
                }
                else if (!type.IsGenericType && type.IsEnum)
                {
                    var fieldsArray = type?.GetFields(BindingFlags.Public | BindingFlags.Static);
                    var enumName = value != null ? fieldsArray[(int)value].Name : null;
                    Results.Add(new MetaVariable(keysPath, typeof(string), description, enumName));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
                {
                    // The object is an enumerable collection. A list, array, or similar.
                    // Determine the element type
                    Type elementType = null;
                    if (type.IsArray)
                    {
                        elementType = type.GetElementType();
                    }
                    else if (type.IsGenericType && type.GetGenericArguments().Length > 0)
                    {
                        elementType = type.GetGenericArguments().Last();
                    }
                    else
                    {
                        elementType = typeof(object);
                    }

                    int? i = 0;
                    if (value != null)
                    {
                        // Handle filled collections
                        foreach ( var item in (IEnumerable)value)
                        {
                            i++;
                            var elementKeysPath = keysPath.ToList();
                            elementKeysPath.Add(i.ToString());
                            if (maxRecursionLevel is null || keysPath.Count < maxRecursionLevel)
                            {
                                GetVariables(elementType, maxRecursionLevel, item, elementKeysPath);
                            }
                        }
                    }
                    else
                    {
                        // Handle empty collections (for example when we're generating wiki documentation)
                        // Get the current list element's underlying variable data
                        var elementKeysPath = keysPath.ToList();
                        elementKeysPath.Add(indexMarker);
                        if (maxRecursionLevel is null || keysPath.Count < maxRecursionLevel)
                        {
                            GetVariables(elementType, maxRecursionLevel, null, elementKeysPath);
                        }
                        // Set i to null so that no value is written to the wiki documentation
                        i = null;
                    }

                    // Write the root element name with (if available) the number of associated entries from the collection
                    var entriesPath = keysPath.ToList();
                    Results.Add(new MetaVariable(entriesPath, typeof(IEnumerable<>), description, i));
                }
                else
                {
                    if (undecomposedTypes.Contains(type)) { return; }

                    if ((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) || 
                        type.GetInterfaces().Contains(typeof(IDictionary)))
                    {
                        if (value != null)
                        {
                            foreach (DictionaryEntry kvp in (IDictionary)value)
                            {
                                if ( kvp.Value != null )
                                {
                                    GetVariable( oldKeysPath, kvp.Key.ToString(), kvp.Value.GetType(), description, kvp.Value, maxRecursionLevel );
                                }
                            }
                        }
                    }
                    else if ((type.IsClass || type.IsInterface) && !type.IsGenericType)
                    {
                        // Add an object to represent the root name for the object in our docs
                        Results.Add(new MetaVariable(keysPath, typeof(object), description));

                        // Get the object's child properties
                        if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                        {
                            GetVariables( type, maxRecursionLevel, value, keysPath );
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected variable type '{type.FullName}'.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to obtain variable metadata by reflection.", ex);
            }
        }
    }

    public class MetaVariable ( List<string> keysPath, Type type, string description = null, object value = null )
    {
        /// <summary> The full path to access the key </summary>
        public List<string> keysPath { get; set; } = keysPath;

        /// <summary> The variable's type </summary>
        public Type type { get; } = type;

        /// <summary> The variable's description (if any) </summary>
        public string description { get; } = description;

        /// <summary> The variable's value (if any) </summary>
        public object value { get; set;  } = value;
    }

    public class CottleVariable
    {
        /// <summary> The full key used to access the variable </summary>
        public string key { get; }

        /// <summary> The variable's description (if any) </summary>
        public string description { get; }

        /// <summary> The value to write (if any) </summary>
        public object value { get; }

        public CottleVariable(List<string> keysPath, string description, object value)
        {
            keysPath.RemoveAll(k => k == ""); // Remove any empty keys from the path
            this.key = string
                .Join(".", keysPath) // Format separators as points
                .Replace($".{MetaVariables.indexMarker}", @"[\<index\>]"); // Format index values for Cottle
            this.description = description;
            this.value = value;
        }
    }

    [UsedImplicitly]
    public static class MetaVariablesExtensions
    {
        public static List<CottleVariable> AsCottleVariables(this List<MetaVariable> source)
        {
            return source
                .Where(v => v.keysPath.Last() != MetaVariables.indexMarker) // Exclude index values in Cottle
                .Select(v => new CottleVariable(v.keysPath, v.description, v.value))
                .ToList();
        }
    }
}
