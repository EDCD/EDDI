using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public partial class VoiceAttackVariables
    {
        // Event keys that shall not be written to VoiceAttack
        private static readonly string[] ignoredKeys = { "type", "raw", "fromLoad", "edName", "baseName" };

        /// <summary> Walk an object and write out all of the possible fields </summary>
        /// <param name="prefix">The prefix to add in front of the property name</param>
        /// <param name="reflectionObject">The object that we're walking. At the top level, this should be an `Event` class object</param>
        /// <param name="setVars">The list of values that we're preparing to set within VoiceAttack</param>
        /// <param name="isTopLevel">Whether we're looking at the top level of the event or at some deeper level</param>
        public static void PrepareEventVariables(string prefix, object reflectionObject, ref List<VoiceAttackVariable> setVars, bool isTopLevel = true)
        {
            var objectType = reflectionObject.GetType();
            var objectProperties = objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var objectFields = objectType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var eventProperty in objectProperties)
            {
                PrepareEventVariable(ref setVars, prefix, eventProperty.Name, eventProperty.PropertyType, eventProperty.CanRead ? eventProperty.GetValue(reflectionObject) : null);
            }

            foreach (var eventField in objectFields)
            {
                PrepareEventVariable(ref setVars, prefix, eventField.Name, eventField.FieldType, eventField.GetValue(reflectionObject));
            }

            if (isTopLevel)
            {
                // Sort the final output
                setVars = setVars.OrderBy(v => v.key).ToList();
            }
        }

        private static void PrepareEventVariable(ref List<VoiceAttackVariable> setVars, string prefix, string key, Type type, object value)
        {
            try
            {
                // We ignore some keys that are maintained for internal use only
                if (ignoredKeys.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                {
                    Logging.Debug("Ignoring key " + key);
                    return;
                }

                // Only append the child name to the current prefix if if does not repeat the prior word
                string childKey = AddSpacesToTitleCasedName(key).Replace("_", " ").ToLowerInvariant();
                string name;
                if (Regex.Match(prefix, @"(\w+)$").Value == childKey)
                {
                    name = prefix;
                }
                else
                {
                    name = prefix + " " + childKey;
                }

                // We also ignore any keys that we have already set elsewhere
                if (setVars.FirstOrDefault(v => v.key == name) != null)
                {
                    Logging.Debug("Skipping already-set key " + name);
                    return;
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Get the underlying type for nullable types
                    type = Nullable.GetUnderlyingType(type);
                }

                // Doubles, floats, and longs are all stored as decimals in VoiceAttack
                if (type == typeof(double) || type == typeof(float) || type == typeof(long))
                {
                    type = typeof(decimal);
                }

                Logging.Debug("Setting values for " + name);

                if (type == typeof(bool))
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(bool), (bool?)value));
                }
                else if (type == typeof(string))
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(string), (string)value));
                }
                else if (type == typeof(int))
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(int), (int?)value));
                }
                else if (type == typeof(decimal))
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(decimal), value is null ? null : (decimal?)Convert.ToDecimal(value)));
                }
                else if (type == typeof(DateTime))
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(DateTime), (DateTime?)value));
                }
                else if (type is null)
                {
                    setVars.Add(new VoiceAttackVariable(name, null, null));
                }
                else
                {
                    if (type.GetInterfaces().Contains(typeof(ICollection)))
                    {
                        // The object is a collection. A list, array, or similar.
                        var collection = (ICollection)value;
                        if (collection.Count > 0)
                        {
                            // Handle lists with values
                            int i = 0;
                            foreach (object item in collection)
                            {
                                Logging.Debug("Handling element " + i);
                                PrepareEventVariables(name + " " + i, item, ref setVars, false);
                                i++;
                            }
                        }
                        else
                        {
                            // TODO: Handle empty lists and arrays
                            var T = typeof(object);
                            var typeProperties = type.GetProperties();
                            foreach (var typeProperty in typeProperties)
                            {
                                if (typeProperty.Name == "Item")
                                {
                                    T = typeProperty.PropertyType;
                                }
                            }
                            var reflectionObject = T;
                        }
                        setVars.Add(new VoiceAttackVariable(name + " entries", typeof(int), collection.Count));
                    }
                    else if (type.IsClass && !type.IsGenericType)
                    {
                        Logging.Debug($"Found object '{type.Name}'");
                        PrepareEventVariables(name, value, ref setVars, false);
                    }
                    else
                    {
                        throw new InvalidDataException($"Unexpected type '{type.FullName}' cannot be set as a VoiceAttack variable.");
                    }
                }
            }
            catch (Exception ex)
            {
                var data = new Dictionary<string, object>()
                    {
                        { "Prefix", prefix },
                        { "Key", key },
                        { "Type", type },
                        { "Value", value },
                        { "Exception", ex }
                    };
                Logging.Error($"Failed to prepare VoiceAttack value.", data);
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

        public static void SetEventVariables(dynamic vaProxy, List<VoiceAttackVariable> variables)
        {
            foreach (var variable in variables)
            {
                try
                {
                    if (variable.type is null)
                    {
                        // No idea what it might have been so reset everything
                        Logging.Debug($"'{variable.key}' type is null; Unset all possible values");
                        vaProxy.SetText(variable.key, null);
                        vaProxy.SetInt(variable.key, null);
                        vaProxy.SetDecimal(variable.key, null);
                        vaProxy.SetBoolean(variable.key, null);
                        vaProxy.SetDate(variable.key, null);
                    }
                    else if (variable.type == typeof(string))
                    {
                        Logging.Debug($"Setting string value '{variable.key}' to: {(string)variable.value}");
                        vaProxy.SetText(variable.key, (string)variable.value);
                    }
                    else if (variable.type == typeof(int))
                    {
                        Logging.Debug($"Setting integer value '{variable.key}' to: {(int?)variable.value}");
                        vaProxy.SetInt(variable.key, (int?)variable.value);
                    }
                    else if (variable.type == typeof(bool))
                    {
                        Logging.Debug($"Setting boolean value '{variable.key}' to: {(bool?)variable.value}");
                        vaProxy.SetBoolean(variable.key, (bool?)variable.value);
                    }
                    else if (variable.type == typeof(decimal))
                    {
                        Logging.Debug($"Setting decimal value '{variable.key}' to: {(decimal?)variable.value}");
                        vaProxy.SetDecimal(variable.key, (decimal?)variable.value);
                    }
                    else if (variable.type == typeof(DateTime))
                    {
                        Logging.Debug($"Setting date value '{variable.key} to {(DateTime?)variable.value}");
                        vaProxy.SetDate(variable.key, (DateTime?)variable.value);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid type");
                    }
                }
                catch (Exception ex)
                {
                    var data = new Dictionary<string, object>()
                    {
                        { "Value", variable },
                        { "Exception", ex }
                    };
                    Logging.Error($"Failed to write VoiceAttack value for key '{variable.key}'", data);
                }
            }
        }
    }

    public class VoiceAttackVariable
    {
        /// <summary> The full key used to access the variable in VoiceAttack, including any applicable prefix </summary>
        public string key { get; private set; }

        /// <summary> One of "string", "int", "bool", "decimal", "double", "long", or "DateTime" </summary>
        public Type type { get; private set; }

        /// <summary> The value to write (if any) </summary>
        public object value { get; private set; }

        public VoiceAttackVariable(string key, Type type, object value = null)
        {
            this.key = key;
            this.type = type;
            this.value = value;
        }
    }
}
