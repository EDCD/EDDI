#nullable enable

using EddiVoiceAttackAdapter.Client;
using EddiVoiceAttackAdapter.Extensions;
using EddiVoiceAttackAdapter.Logging;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace EddiVoiceAttackAdapter
{
    internal static class VoiceAttackRuntimeEventReceiver
    {
        private const string RuntimeEventType = "va_runtime";
        private const string DispatchEventName = "dispatch_event";
        private const string CommandActionEventName = "command_action";

        public static void HandleMessageReceived( object? sender, MessageReceivedEventArgs e )
        {
            HandleMessageReceivedAsync( e )
                .SafeFireAndForget( ex => AdapterLogger.Error( "Failed to process VoiceAttack runtime payload event", ex ) );
        }

        private static async Task HandleMessageReceivedAsync( MessageReceivedEventArgs e )
        {
            if ( e.MessageType != AdapterMessageTypes.Event )
            {
                return;
            }

            if ( !TryGetObjectElement( e.MessageEnvelope.Data, out var eventData ) )
            {
                return;
            }

            var eventType = GetString( eventData, AdapterRuntimePayloadKeys.EventEnvelope.EventType );
            var eventName = GetString( eventData, AdapterRuntimePayloadKeys.EventEnvelope.EventName );
            if ( !string.Equals( eventType, RuntimeEventType, StringComparison.OrdinalIgnoreCase ) )
            {
                return;
            }

            if ( !TryGetProperty( eventData, AdapterRuntimePayloadKeys.EventEnvelope.EventPayload, out var payload ) ||
                 payload.ValueKind != JsonValueKind.Object )
            {
                return;
            }

            if ( string.Equals( eventName, DispatchEventName, StringComparison.OrdinalIgnoreCase ) )
            {
                await HandleDispatchEventAsync( payload ).ConfigureAwait( false );
                return;
            }

            if ( string.Equals( eventName, CommandActionEventName, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( TryGetProperty( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Actions, out var actions ) &&
                     actions.ValueKind == JsonValueKind.Array )
                {
                    HandleCommandActions( actions );
                    return;
                }

                HandleCommandAction( payload );
            }
        }

        private static bool TryGetObjectElement ( object data, out JsonElement element )
        {
            if ( data is JsonElement { ValueKind: JsonValueKind.Object } jsonElement )
            {
                element = jsonElement;
                return true;
            }

            element = default;
            return false;
        }

        private static string? GetString( JsonElement source, string propertyName )
        {
            return TryGetProperty( source, propertyName, out var value )
                ? ParseText( value )
                : null;
        }

        private static bool TryGetProperty ( JsonElement source, string propertyName, out JsonElement value )
        {
            if ( source.ValueKind != JsonValueKind.Object )
            {
                value = default;
                return false;
            }

            foreach ( var property in source.EnumerateObject() )
            {
                if ( string.Equals( property.Name, propertyName, StringComparison.OrdinalIgnoreCase ) )
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static void HandleCommandActions( JsonElement actions )
        {
            foreach ( var payload in actions.EnumerateArray() )
            {
                if ( payload.ValueKind == JsonValueKind.Object )
                {
                    HandleCommandAction( payload );
                }
            }
        }

        private static async Task HandleDispatchEventAsync( JsonElement payload )
        {
            var commandName = GetString( payload, AdapterRuntimePayloadKeys.DispatchPayload.CommandName );
            var eddiEventType = GetString( payload, AdapterRuntimePayloadKeys.DispatchPayload.EventType );

            if ( TryGetProperty( payload, AdapterRuntimePayloadKeys.DispatchPayload.ClearVariables, out var clearVariables ) &&
                 clearVariables.ValueKind == JsonValueKind.Array )
            {
                ApplyVariables( clearVariables );
            }

            if ( !string.IsNullOrWhiteSpace( eddiEventType ) )
            {
                VoiceAttackPlugin.SetText( "EDDI event", eddiEventType );
            }

            if ( TryGetProperty( payload, AdapterRuntimePayloadKeys.DispatchPayload.SetVariables, out var setVariables ) &&
                 setVariables.ValueKind == JsonValueKind.Array )
            {
                ApplyVariables( setVariables );
            }

            if ( string.IsNullOrWhiteSpace( commandName ) )
            {
                return;
            }

            if ( VoiceAttackPlugin.CommandExists( commandName ) )
            {
                VoiceAttackPlugin.ExecuteCommand( commandName );
                await VoiceAttackPlugin.WaitForCommandExecutionAsync( commandName ).ConfigureAwait( false );
            }
            else
            {
                AdapterLogger.Debug( $"Command '{commandName}' not found." );
            }
        }

        private static void HandleCommandAction( JsonElement payload )
        {
            var action = GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Action );
            if ( string.IsNullOrWhiteSpace( action ) )
            {
                return;
            }

            switch ( action )
            {
                case "write_log":
                    VoiceAttackPlugin.WriteToLog(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Message ) ?? string.Empty,
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Color ) ?? "white" );
                    break;
                case "set_text":
                    TryGetProperty( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Value, out var textValue );
                    VoiceAttackPlugin.SetText(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        textValue.ValueKind == JsonValueKind.Undefined ? null : ParseText( textValue ) );
                    break;
                case "set_int":
                    VoiceAttackPlugin.SetInt(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseInt( GetValueOrDefault( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_decimal":
                    VoiceAttackPlugin.SetDecimal(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseDecimal( GetValueOrDefault( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_boolean":
                    VoiceAttackPlugin.SetBoolean(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseBoolean( GetValueOrDefault( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_date":
                    VoiceAttackPlugin.SetDate(
                        GetString( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseDateTime( GetValueOrDefault( payload, AdapterRuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
            }
        }

        private static void ApplyVariables( JsonElement variables )
        {
            foreach ( var variable in variables.EnumerateArray() )
            {
                if ( variable.ValueKind != JsonValueKind.Object )
                {
                    continue;
                }

                var key = GetString( variable, AdapterRuntimePayloadKeys.VariablePayload.Key );
                if ( string.IsNullOrWhiteSpace( key ) )
                {
                    continue;
                }

                var type = GetString( variable, AdapterRuntimePayloadKeys.VariablePayload.Type ) ?? "text";
                var valueToken = GetValueOrDefault( variable, AdapterRuntimePayloadKeys.VariablePayload.Value );

                switch ( type )
                {
                    case "bool":
                        VoiceAttackPlugin.SetBoolean( key, ParseBoolean( valueToken ) );
                        break;
                    case "date":
                        VoiceAttackPlugin.SetDate( key, ParseDateTime( valueToken ) );
                        break;
                    case "decimal":
                        VoiceAttackPlugin.SetDecimal( key, ParseDecimal( valueToken ) );
                        break;
                    case "int":
                        VoiceAttackPlugin.SetInt( key, ParseInt( valueToken ) );
                        break;
                    default:
                        VoiceAttackPlugin.SetText( key, ParseText( valueToken ) );
                        break;
                }
            }
        }

        private static JsonElement GetValueOrDefault ( JsonElement source, string propertyName )
        {
            return TryGetProperty( source, propertyName, out var value )
                ? value
                : default;
        }

        private static bool? ParseBoolean( JsonElement valueToken )
        {
            if ( valueToken.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null )
            {
                return null;
            }

            if ( valueToken.ValueKind is JsonValueKind.True or JsonValueKind.False )
            {
                return valueToken.GetBoolean();
            }

            if ( bool.TryParse( valueToken.ToString(), out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static DateTime? ParseDateTime( JsonElement valueToken )
        {
            if ( valueToken.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null )
            {
                return null;
            }

            if ( valueToken.ValueKind == JsonValueKind.String && valueToken.TryGetDateTime( out var typed ) )
            {
                return typed;
            }

            if ( DateTime.TryParse( valueToken.ToString(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static decimal? ParseDecimal( JsonElement valueToken )
        {
            if ( valueToken.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null )
            {
                return null;
            }

            if ( valueToken.ValueKind == JsonValueKind.Number && valueToken.TryGetDecimal( out var typed ) )
            {
                return typed;
            }

            if ( decimal.TryParse( valueToken.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static int? ParseInt( JsonElement valueToken )
        {
            if ( valueToken.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null )
            {
                return null;
            }

            if ( valueToken.ValueKind == JsonValueKind.Number && valueToken.TryGetInt32( out var typed ) )
            {
                return typed;
            }

            if ( int.TryParse( valueToken.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static string? ParseText( JsonElement valueToken )
        {
            if ( valueToken.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null )
            {
                return null;
            }

            return valueToken.ValueKind == JsonValueKind.String
                ? valueToken.GetString()
                : valueToken.ToString();
        }
    }
}
