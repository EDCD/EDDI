using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public partial class VoiceAttackVariables
    {
        // Event keys that shall not be written to VoiceAttack
        private static readonly string[] ignoredKeys = { "type", "raw", "fromLoad" };

        /// <summary> Set keys for values that we describe within the Event Sub-Class </summary>
        /// <param name="prefix">The prefix to add in front of the property name</param>
        /// <param name="theEvent">The event with any associated values</param>
        /// <param name="setVars">The list of values that we're preparing to set within VoiceAttack</param>
        public static void PrepareEventVariables(string prefix, Event theEvent, ref List<VoiceAttackVariable> setVars)
        {
            foreach (string key in Events.VARIABLES[theEvent.type].Keys)
            {
                try
                {
                    // Obtain the value by name.  Actually looking for a method get_<name>
                    System.Reflection.MethodInfo method = theEvent.GetType().GetMethod("get_" + key);
                    if (method != null)
                    {
                        Type returnType = method.ReturnType;
                        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            returnType = Nullable.GetUnderlyingType(returnType);
                        }

                        string varname = prefix + " " + key;
                        Logging.Debug("Setting values for " + varname);

                        if (returnType == typeof(string))
                        {
                            setVars.Add(new VoiceAttackVariable(varname, typeof(string), (string)method.Invoke(theEvent, null)));
                        }
                        else if (returnType == typeof(int))
                        {
                            setVars.Add(new VoiceAttackVariable(varname, typeof(int), (int?)method.Invoke(theEvent, null)));
                        }
                        else if (returnType == typeof(bool))
                        {
                            setVars.Add(new VoiceAttackVariable(varname, typeof(bool), (bool?)method.Invoke(theEvent, null)));
                        }
                        else if (returnType == typeof(decimal))
                        {
                            setVars.Add(new VoiceAttackVariable(varname, typeof(decimal), (decimal?)method.Invoke(theEvent, null)));
                        }
                        else if (returnType == typeof(double))
                        {
                            // Doubles are stored as decimals
                            setVars.Add(new VoiceAttackVariable(varname, typeof(decimal), (decimal?)(double?)method.Invoke(theEvent, null)));
                        }
                        else if (returnType == typeof(long))
                        {
                            // Longs are stored as decimals
                            setVars.Add(new VoiceAttackVariable(varname, typeof(decimal), (decimal?)(long?)method.Invoke(theEvent, null)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var data = new Dictionary<string, object>()
                    {
                        { "Event", theEvent },
                        { "Exception", ex }
                    };
                    Logging.Error($"Failed to prepare VoiceAttack value for key '{key}' in {theEvent.type}", data);
                }
            }
        }

        /// <summary> Walk a JSON object and write out all of the possible fields </summary>
        public static void PrepareExtendedEventVariables(string prefix, dynamic json, ref List<VoiceAttackVariable> setVars)
        {
            foreach (JProperty child in json)
            {
                // We ignore some keys that are maintained for internal use only
                if (ignoredKeys.Contains(child.Name))
                {
                    Logging.Debug("Ignoring key " + child.Name);
                    continue;
                }

                // Only append the child name to the current prefix if if does not repeat the prior word
                string childName = AddSpacesToTitleCasedName(child.Name).Replace("_", " ").ToLowerInvariant();
                string name;
                if (Regex.Match(prefix, @"(\w+)$").Value == childName)
                {
                    name = prefix;
                }
                else
                {
                    name = prefix + " " + childName;
                }

                // We also ignore any keys that we have already set elsewhere
                if (setVars.FirstOrDefault(v => v.key == name) != null)
                {
                    Logging.Debug("Skipping already-set key " + name);
                    continue;
                }

                if (child.Value == null)
                {
                    // No idea what it might have been so reset everything
                    setVars.Add(new VoiceAttackVariable(name, null));
                    continue;
                }
                if (child.Value.Type == JTokenType.Boolean)
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(bool), (bool?)child.Value));
                }
                else if (child.Value.Type == JTokenType.String)
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(string), (string)child.Value));
                }
                else if (child.Value.Type == JTokenType.Float)
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(decimal), (decimal?)(double?)child.Value));
                }
                else if (child.Value.Type == JTokenType.Integer)
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(decimal), (decimal?)(long?)child.Value));
                }
                else if (child.Value.Type == JTokenType.Date)
                {
                    setVars.Add(new VoiceAttackVariable(name, typeof(DateTime), (DateTime?)child.Value));
                }
                else if (child.Value.Type == JTokenType.Array)
                {
                    var arrayChildren = child.Value.Children();
                    int i = 0;

                    foreach (JToken arrayChild in arrayChildren)
                    {
                        Logging.Debug("Handling element " + i);
                        childName = name + " " + i;

                        if (arrayChild.Type == JTokenType.Boolean)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, typeof(bool), arrayChild.Value<bool?>()));
                        }
                        else if (arrayChild.Type == JTokenType.String)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, typeof(string), arrayChild.Value<string>()));
                        }
                        else if (arrayChild.Type == JTokenType.Float)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, typeof(decimal), arrayChild.Value<decimal?>()));
                        }
                        else if (arrayChild.Type == JTokenType.Integer)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, typeof(decimal), arrayChild.Value<decimal?>()));
                        }
                        else if (arrayChild.Type == JTokenType.Date)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, typeof(DateTime), arrayChild.Value<DateTime?>()));
                        }
                        else if (arrayChild.Type == JTokenType.Null)
                        {
                            setVars.Add(new VoiceAttackVariable(childName, null));
                        }
                        else if (arrayChild.Type == JTokenType.Object) 
                        {
                            PrepareExtendedEventVariables(childName, arrayChild, ref setVars);
                        }
                        i++;
                    }
                    setVars.Add(new VoiceAttackVariable(name + " entries", typeof(int), i));
                }
                else if (child.Value.Type == JTokenType.Object)
                {
                    Logging.Debug("Found object");
                    PrepareExtendedEventVariables(name, child.Value, ref setVars);
                }
                else if (child.Value.Type == JTokenType.Null)
                {
                    setVars.Add(new VoiceAttackVariable(name, null));
                }
                else
                {
                    Logging.Warn(child.Value.Type + ": " + child.Name + "=" + child.Value);
                }
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
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
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
                        Logging.Debug($"'{variable.key}' is null; Unset all possible values");
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
                    else if (variable.type == typeof(double))
                    {
                        // Doubles are stored as decimals
                        Logging.Debug($"Setting decimal value '{variable.key}' to: {(decimal?)variable.value}");
                        vaProxy.SetDecimal(variable.key, (decimal?)variable.value);
                    }
                    else if (variable.type == typeof(long))
                    {
                        // Longs are stored as decimals
                        Logging.Debug($"Setting decimal value '{variable.key}' to: {(decimal?)variable.value}");
                        vaProxy.SetDecimal(variable.key, (decimal?)variable.value);
                    }
                    else if (variable.type == typeof(DateTime))
                    {
                        Logging.Debug($"Setting date value '{variable.key} to {variable.value}");
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
