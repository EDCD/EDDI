using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace EddiVoiceAttackAdapter
{
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
            keysPath.RemoveAll(string.IsNullOrEmpty); // Remove any empty keys from the path
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
            else if ( variableType == typeof( IEnumerable<>  ))
            {
                if ( value is null )
                {
                    this.value = null;
                    this.variableType = typeof( int );
                }
                if ( value is int count )
                {
                    this.value = count;
                    this.variableType = typeof( int );
                }
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
            if (!prefix.EndsWith(' ')) { prefix += " "; }
            while (skip < childKey.Length
                   || (prefix.Skip(skip).Count() - 1) > childKey.Length
                   || (prefix.Skip(skip).Zip(childKey, (a, b) => a.Equals(b)).Any(x => !x) && skip < prefix.Length))
            {
                skip++;
            }
            return string.Concat(prefix.Take(skip).Concat(childKey));
        }

        public void Set()
        {
            // Variable type must be one of "string", "int", "bool", "decimal", "double", "long", or "DateTime"
            try
            {
                // Set final values
                if (variableType is null)
                {
                    // No idea what it might have been so reset everything
                    RuntimeSetText(key, null);
                    RuntimeSetInt(key, null);
                    RuntimeSetDecimal(key, null);
                    RuntimeSetBoolean(key, null);
                    RuntimeSetDate(key, null);
                }
                else if (variableType == typeof(string))
                {
                    RuntimeSetText(key, value as string);
                }
                else if (variableType == typeof(int))
                {
                    RuntimeSetInt(key, value as int?);
                }
                else if (variableType == typeof(bool))
                {
                    RuntimeSetBoolean(key, value as bool?);
                }
                else if (variableType == typeof(decimal))
                {
                    RuntimeSetDecimal(key, value as decimal?);
                }
                else if (variableType == typeof(DateTime))
                {
                    RuntimeSetDate(key, value as DateTime?);
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

        private static void RuntimeSetText(string key, string value)
            => DispatchRuntimeAction("set_text", key, value);

        private static void RuntimeSetInt(string key, int? value)
            => DispatchRuntimeAction("set_int", key, value);

        private static void RuntimeSetBoolean(string key, bool? value)
            => DispatchRuntimeAction("set_boolean", key, value);

        private static void RuntimeSetDecimal(string key, decimal? value)
            => DispatchRuntimeAction("set_decimal", key, value);

        private static void RuntimeSetDate(string key, DateTime? value)
            => DispatchRuntimeAction("set_date", key, value?.ToString("O"));

        private static void DispatchRuntimeAction(string action, string key, object value)
        {
            var payload = new Dictionary<string, object>
            {
                { "action", action },
                { "key", key ?? string.Empty },
                { "value", value ?? string.Empty }
            };

            try
            {
                var eventData = new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = payload
                };

                RuntimeEventDispatcher.DispatchAsync(eventData)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to dispatch runtime variable set payload", ex);
            }
        }
    }
}