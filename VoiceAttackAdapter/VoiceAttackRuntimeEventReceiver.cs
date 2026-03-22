#nullable enable

using EddiIPC_Service.Client;
using EddiIPC_Service.Messages;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Utilities;

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
                .SafeFireAndForget( ex => Logging.Error( "Failed to process VoiceAttack runtime payload event", ex ) );
        }

        private static async Task HandleMessageReceivedAsync( MessageReceivedEventArgs e )
        {
            if ( e.MessageType != MessageTypes.Event )
            {
                return;
            }

            if ( e.MessageEnvelope.Data is not JObject eventData )
            {
                return;
            }

            var eventType = GetString( eventData, RuntimePayloadKeys.EventEnvelope.EventType );
            var eventName = GetString( eventData, RuntimePayloadKeys.EventEnvelope.EventName );
            if ( !string.Equals( eventType, RuntimeEventType, StringComparison.OrdinalIgnoreCase ) )
            {
                return;
            }

            if ( GetToken( eventData, RuntimePayloadKeys.EventEnvelope.EventPayload ) is not JObject payload )
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
                if ( GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Actions ) is JArray actions )
                {
                    HandleCommandActions( actions );
                    return;
                }

                HandleCommandAction( payload );
            }
        }

        private static string? GetString( JObject source, string propertyName )
        {
            return GetToken( source, propertyName )?.Value<string>();
        }

        private static JToken? GetToken( JObject source, string propertyName )
        {
            return source.TryGetValue( propertyName, out var token )
                ? token
                : null;
        }

        private static void HandleCommandActions( JArray actions )
        {
            foreach ( var token in actions )
            {
                if ( token is not JObject payload )
                {
                    continue;
                }

                HandleCommandAction( payload );
            }
        }

        private static async Task HandleDispatchEventAsync( JObject payload )
        {
            var commandName = GetString( payload, RuntimePayloadKeys.DispatchPayload.CommandName );
            var eddiEventType = GetString( payload, RuntimePayloadKeys.DispatchPayload.EventType );

            if ( GetToken( payload, RuntimePayloadKeys.DispatchPayload.ClearVariables ) is JArray clearVariables )
            {
                ApplyVariables( clearVariables );
            }

            if ( !string.IsNullOrWhiteSpace( eddiEventType ) )
            {
                VoiceAttackPlugin.SetText( "EDDI event", eddiEventType );
            }

            if ( GetToken( payload, RuntimePayloadKeys.DispatchPayload.SetVariables ) is JArray setVariables )
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
                Logging.Debug( $"Command '{commandName}' not found." );
            }
        }

        private static void HandleCommandAction( JObject payload )
        {
            var action = GetString( payload, RuntimePayloadKeys.CommandActionPayload.Action );
            if ( string.IsNullOrWhiteSpace( action ) )
            {
                return;
            }

            switch ( action )
            {
                case "write_log":
                    VoiceAttackPlugin.WriteToLog(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Message ) ?? string.Empty,
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Color ) ?? "white" );
                    break;
                case "set_text":
                    var textValue = GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Value );
                    VoiceAttackPlugin.SetText(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        textValue?.Type == JTokenType.Null ? null : textValue?.ToString() );
                    break;
                case "set_int":
                    VoiceAttackPlugin.SetInt(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseInt( GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_decimal":
                    VoiceAttackPlugin.SetDecimal(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseDecimal( GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_boolean":
                    VoiceAttackPlugin.SetBoolean(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseBoolean( GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
                case "set_date":
                    VoiceAttackPlugin.SetDate(
                        GetString( payload, RuntimePayloadKeys.CommandActionPayload.Key ) ?? string.Empty,
                        ParseDateTime( GetToken( payload, RuntimePayloadKeys.CommandActionPayload.Value ) ) );
                    break;
            }
        }

        private static void ApplyVariables( JArray variables )
        {
            foreach ( var token in variables )
            {
                if ( token is not JObject variable )
                {
                    continue;
                }

                var key = GetString( variable, RuntimePayloadKeys.VariablePayload.Key );
                if ( string.IsNullOrWhiteSpace( key ) )
                {
                    continue;
                }

                var type = GetString( variable, RuntimePayloadKeys.VariablePayload.Type ) ?? "text";
                var valueToken = GetToken( variable, RuntimePayloadKeys.VariablePayload.Value );

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

        private static bool? ParseBoolean( JToken? valueToken )
        {
            if ( valueToken == null || valueToken.Type == JTokenType.Null )
            {
                return null;
            }

            if ( valueToken.Type == JTokenType.Boolean )
            {
                return valueToken.Value<bool>();
            }

            if ( bool.TryParse( valueToken.ToString(), out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static DateTime? ParseDateTime( JToken? valueToken )
        {
            if ( valueToken == null || valueToken.Type == JTokenType.Null )
            {
                return null;
            }

            if ( valueToken.Type == JTokenType.Date )
            {
                return valueToken.Value<DateTime>();
            }

            if ( DateTime.TryParse( valueToken.ToString(), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static decimal? ParseDecimal( JToken? valueToken )
        {
            if ( valueToken == null || valueToken.Type == JTokenType.Null )
            {
                return null;
            }

            if ( decimal.TryParse( valueToken.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static int? ParseInt( JToken? valueToken )
        {
            if ( valueToken == null || valueToken.Type == JTokenType.Null )
            {
                return null;
            }

            if ( int.TryParse( valueToken.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                    out var parsed ) )
            {
                return parsed;
            }

            return null;
        }

        private static string? ParseText( JToken? valueToken )
        {
            if ( valueToken == null || valueToken.Type == JTokenType.Null )
            {
                return null;
            }

            return valueToken.ToString();
        }
    }
}
