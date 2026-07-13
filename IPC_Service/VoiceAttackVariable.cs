using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiIPC_Service
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

        public VoiceAttackVariable ( string startingPrefix, string eventType, VariableDescriptor descriptor )
            : this(
                startingPrefix,
                eventType,
                descriptor.KeysPath.ToList(),
                descriptor.VariableType,
                descriptor.Description,
                descriptor.Value )
        { }

        public VoiceAttackVariable(string startingPrefix, string eventType, List<string> keysPath, Type variableType, string description, object value = null)
        {
            this.key = VariablePathFormatter.RenderVoiceAttackName( startingPrefix, eventType, keysPath );
            this.eventType = eventType;
            this.description = description;

            // Convert doubles, floats, and longs to decimals
            if (value is null && (variableType == typeof(double) || variableType == typeof(float) || variableType == typeof(long) || variableType == typeof(ulong) || variableType == typeof(uint)))
            {
                this.value = null;
                this.variableType = typeof(decimal);
            }
            else if ( variableType != typeof( string ) &&
                      typeof( System.Collections.IEnumerable ).IsAssignableFrom( variableType ) )
            {
                this.value = value switch
                {
                    null => null,
                    int count => count,
                    System.Collections.ICollection collection => collection.Count,
                    _ => null
                };
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
            else if (value is uint ui)
            {
                this.value = Convert.ToDecimal(ui);
                this.variableType = typeof(decimal);
            }
            else
            {
                this.value = value;
                this.variableType = variableType;
            }
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
            ArgumentNullException.ThrowIfNull( action );
            ArgumentNullException.ThrowIfNull( key );
            
            var payload = new Dictionary<string, object>
            {
                { "action", action },
                { "key", key },
                { "value", value }
            };

            try
            {
                var eventData = new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = payload
                };

                var dispatched = RuntimeEventDispatcher.DispatchAsync(eventData)
                    .GetAwaiter()
                    .GetResult();

                if ( !dispatched )
                {
                    Logging.Debug(
                        $"VoiceAttack variable '{key}' could not be dispatched because no runtime dispatcher is available." );
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to dispatch runtime variable set payload", ex);
            }
        }
    }
}
