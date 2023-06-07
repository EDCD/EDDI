﻿using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public class MetaVariables
    {
        public MetaVariables(Type reflectionObjectType, object reflectionObject = null, int? maxRecursionLevel = null)
        {
            if (reflectionObjectType is null) 
            { 
                Results = new List<MetaVariable>(); 
            }
            else
            {
                Results = GetVariables(reflectionObjectType, maxRecursionLevel, reflectionObject);
            }
        }

        public List<MetaVariable> Results { get; private set; }

        // Some types don't need to be decomposed further - we'll stop reflecting when we hit these types
        private static readonly Type[] undecomposedTypes =
        {
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
        };

        // Apply a placeholder symbol for collection indices - to be formatted
        // differently according to the end variable type (Cottle or VoiceAttack) 
        public const string indexMarker = @"<index\>";

        /// <summary> Walk an object and write out all of the possible fields </summary>
        /// <param name="reflectionObjectType">The Type property of the object that we're walking, specified independent from the actual object in case the actual object value is null</param>
        /// <param name="maxRecursionLevel">The maximum number of recursion levels to walk while obtaining our metavariables</param>
        /// <param name="reflectionObject">(Optional) The object that we're walking to obtain values. At the top level, this should be an `Event` class object</param>
        /// <param name="keysPath">(Used internally, do not set) The path to the specific key</param>
        private List<MetaVariable> GetVariables(Type reflectionObjectType, int? maxRecursionLevel, object reflectionObject = null, List<string> keysPath = null)
        {
            if (keysPath is null) { keysPath = new List<string>(); }
            if (Results is null) { Results = new List<MetaVariable>(); }

            // Some types don't need to be decomposed further.
            if (undecomposedTypes.Contains(reflectionObjectType))
            {
                GetVariable(keysPath.Copy(), "", reflectionObjectType, string.Empty, reflectionObject, maxRecursionLevel );
                return Results;
            }

            var objectProperties = reflectionObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var objectFields = reflectionObjectType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var eventProperty in objectProperties)
            {
                // We ignore some keys which we've marked in advance
                bool passProperty = false;
                foreach (var attribute in eventProperty.GetCustomAttributes())
                {
                    if (attribute is PublicAPIAttribute publicAPIAttribute)
                    {
                        passProperty = true;
                        GetVariable(keysPath.Copy(), eventProperty.Name, eventProperty.PropertyType, publicAPIAttribute.Description, eventProperty.CanRead && reflectionObject != null ? eventProperty.GetValue(reflectionObject) : null, maxRecursionLevel );
                        break;
                    }
                }
                if (!passProperty)
                {
                    Logging.Debug("Ignoring key " + eventProperty.Name);
                }
            }

            foreach (var eventField in objectFields)
            {
                // We ignore some keys which we've marked in advance
                bool passField = false;
                foreach (var attribute in eventField.GetCustomAttributes())
                {
                    if (attribute is PublicAPIAttribute publicAPIAttribute)
                    {
                        passField = true;
                        GetVariable(keysPath.Copy(), eventField.Name, eventField.FieldType, publicAPIAttribute.Description, reflectionObject != null ? eventField.GetValue(reflectionObject) : null, maxRecursionLevel );
                        break;
                    }
                }
                if (!passField)
                {
                    Logging.Debug("Ignoring key " + eventField.Name);
                }
            }

            return Results;
        }

        private void GetVariable(List<string> keysPath, string key, Type type, string description, object value, int? maxRecursionLevel )
        {
            try
            {
                var oldKeysPath = keysPath.Copy();
                keysPath.Add(key);

                // We ignore any key paths that we have already set elsewhere
                if (Results.FirstOrDefault(v => v.keysPath.DeepEquals(keysPath)) != null)
                {
                    Logging.Debug("Skipping already-set key " + string.Join("/", keysPath));
                    return;
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Get the underlying type for nullable types
                    type = Nullable.GetUnderlyingType(type);
                }

                Logging.Debug($"Handling {type?.Name ?? "<null>"} key {key} in path {string.Join("/", keysPath)}", value);

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
                else
                {
                    if (undecomposedTypes.Contains(type)) { return; }
                    else if ((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) || 
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
                    else if (type == typeof(IEnumerable) || type.GetInterfaces().Contains(typeof(IEnumerable)))
                    {
                        // The object is an enumerable collection. A list, array, or similar.

                        // Get the underlying type. If there is more than one, the last will correspond to the value type.
                        var underlyingType = type.GetGenericArguments().Last();

                        int? i = 0;
                        if (value != null)
                        {
                            foreach (object item in (IEnumerable)value)
                            {
                                // Handle filled collections
                                Logging.Debug("Handling element " + i++);
                                var elementKeysPath = keysPath.Copy();
                                elementKeysPath.Add(i.ToString());
                                if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                                {
                                    GetVariables( underlyingType, maxRecursionLevel, item, elementKeysPath );
                                }
                            }
                        }
                        else
                        {
                            // Handle empty collections (for example when we're generating wiki documentation)

                            // Get the current list element's underlying variable data
                            var elementKeysPath = keysPath.Copy();
                            elementKeysPath.Add(indexMarker);
                            if ( maxRecursionLevel is null || keysPath.Count < maxRecursionLevel )
                            {
                                GetVariables( underlyingType, maxRecursionLevel, null, elementKeysPath );
                            }

                            // Set i to null so that no value is written to the wiki documentation
                            i = null;
                        }

                        // Write the root element name with (if available) the number of associated entries from the collection
                        var entriesPath = keysPath.Copy();
                        Results.Add(new MetaVariable(entriesPath, typeof( IEnumerable<> ), description, i));
                    }
                    else if ((type.IsClass || type.IsInterface) && !type.IsGenericType)
                    {
                        Logging.Debug($"Found object '{type.Name}'");

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
            return;
        }
    }

    public class MetaVariable
    {
        /// <summary> The full path to access the key </summary>
        public List<string> keysPath { get; set; }

        /// <summary> The variable's type </summary>
        public Type type { get; }

        /// <summary> The variable's description (if any) </summary>
        public string description { get; }

        /// <summary> The variable's value (if any) </summary>
        public object value { get; set;  }

        public MetaVariable(List<string> keysPath, Type type, string description = null, object value = null)
        {
            this.keysPath = keysPath;
            this.type = type;
            this.description = description;
            this.value = value;
        }
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

    public class VoiceAttackVariable
    {
        public string eventType { get; }

        /// <summary> The full key used to access the variable in VoiceAttack, including any applicable prefix </summary>
        public string key { get; }

        /// <summary> The variable type </summary>
        public Type variableType { get; set; }

        /// <summary> The variable's description (if any) </summary>
        public string description { get; }

        /// <summary> The value to write (if any) </summary>
        public object value { get; set; }

        public VoiceAttackVariable(string startingPrefix, string eventType, List<string> keysPath, Type variableType, string description, object value = null)
        {
            // Build the full key
            this.key = startingPrefix; // Set our starting prefix
            keysPath = keysPath.Prepend(eventType?.ToLowerInvariant()).ToList();
            keysPath.RemoveAll(k => string.IsNullOrEmpty(k)); // Remove any empty keys from the path
            foreach (var keySegment in keysPath)
            {
                // Generate a variable name from the prefix and key. 
                var childKey = AddSpacesToTitleCasedName(keySegment).Replace("_", " ").ToLowerInvariant();

                // Only append the portion of the formatted key which isn't redundant with the current prefix.
                this.key = ConcatOverlappingNames(this.key, childKey);
            }
            this.key = key
                .Replace(MetaVariables.indexMarker, @"\<index\>"); // Format index values for VoiceAttack

            this.eventType = eventType;
            this.description = description;

            // Convert doubles, floats, and longs to decimals
            if (value is null && (variableType == typeof(double) || variableType == typeof(float) || variableType == typeof(long) || variableType == typeof(ulong)))
            {
                this.value = null;
                this.variableType = typeof(decimal);
            }
            else if ( variableType == typeof( IEnumerable<> ) && value is int count )
            {
                this.value = count;
                this.variableType = typeof( int );
            }
            else if (value is double d)
            {
                this.value = Convert.ToDecimal(d);
                this.variableType = typeof(decimal);
            }
            else if (value is float f)
            {
                this.value = Convert.ToDecimal(f);
                this.variableType = typeof(decimal);
            }
            else if (value is long l)
            {
                this.value = Convert.ToDecimal(l);
                this.variableType = typeof(decimal);
            }
            else if (value is ulong ul)
            {
                this.value = Convert.ToDecimal(ul);
                this.variableType = typeof(decimal);
            }
            else
            {
                this.value = value;
                this.variableType = variableType;
            }
        }

        private static string AddSpacesToTitleCasedName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ' && !char.IsUpper(text[i - 1]))
                {
                    newText.Append(' ');
                }
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        private static string ConcatOverlappingNames(string prefix, string childKey)
        {
            // For a prefix of "AA BB CC" and a childKey of "BB CC DD", return "AA BB CC DD"
            var skip = 0;
            if (!prefix.EndsWith(" ")) { prefix += " "; }
            while (skip < childKey.Length
                || (prefix.Skip(skip).Count() - 1) > childKey.Length
                || (prefix.Skip(skip).Zip(childKey, (a, b) => a.Equals(b)).Any(x => !x) && skip < prefix.Length))
            {
                skip++;
            }
            return string.Concat(prefix.Take(skip).Concat(childKey));
        }

        public void Set(dynamic vaProxy)
        {
            // Variable type must be one of "string", "int", "bool", "decimal", "double", "long", or "DateTime"
            try
            {
                // Set final values
                if (variableType is null)
                {
                    // No idea what it might have been so reset everything
                    vaProxy.SetText(key, null);
                    vaProxy.SetInt(key, null);
                    vaProxy.SetDecimal(key, null);
                    vaProxy.SetBoolean(key, null);
                    vaProxy.SetDate(key, null);
                }
                else if (variableType == typeof(string))
                {
                    vaProxy.SetText(key, (string)value);
                }
                else if (variableType == typeof(int))
                {
                    vaProxy.SetInt(key, (int?)value);
                }
                else if (variableType == typeof(bool))
                {
                    vaProxy.SetBoolean(key, (bool?)value);
                }
                else if (variableType == typeof(decimal))
                {
                    vaProxy.SetDecimal(key, (decimal?)value);
                }
                else if (variableType == typeof(DateTime))
                {
                    vaProxy.SetDate(key, (DateTime?)value);
                }
                else
                {
                    throw new ArgumentException("Invalid type");
                }
            }
            catch (Exception ex)
            {
                Logging.Error($@"Failed to write VoiceAttack value for {(variableType is null ? "<null type>" : variableType.ToString())} key '{key}' with value {JsonConvert.SerializeObject(value)}", ex);
            }
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

        public static List<VoiceAttackVariable> AsVoiceAttackVariables(this List<MetaVariable> source, string startingPrefix, string eventType = null)
        {
            return source
                .Where(v => ( v.type != typeof(object) ) )
                .Select(v => new VoiceAttackVariable(startingPrefix, eventType, v.keysPath, v.type, v.description, v.value))
                .ToList();
        }
    }
}
