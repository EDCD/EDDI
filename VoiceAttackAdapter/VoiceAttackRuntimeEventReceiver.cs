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

            var eventType = eventData[ "eventType" ]?.Value<string>();
            var eventName = eventData[ "eventName" ]?.Value<string>();
            if ( !string.Equals( eventType, RuntimeEventType, StringComparison.OrdinalIgnoreCase ) )
            {
                return;
            }

            if ( eventData[ "eventPayload" ] is not JObject payload )
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
                HandleCommandAction( payload );
            }
        }

        private static async Task HandleDispatchEventAsync( JObject payload )
        {
            var commandName = payload[ "commandName" ]?.Value<string>();
            var eddiEventType = payload[ "eventType" ]?.Value<string>();

            if ( payload[ "clearVariables" ] is JArray clearVariables )
            {
                ApplyVariables( clearVariables );
            }

            if ( !string.IsNullOrWhiteSpace( eddiEventType ) )
            {
                VoiceAttackPlugin.SetText( "EDDI event", eddiEventType );
            }

            if ( payload[ "setVariables" ] is JArray setVariables )
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
            var action = payload[ "action" ]?.Value<string>();
            if ( string.IsNullOrWhiteSpace( action ) )
            {
                return;
            }

            switch ( action )
            {
                case "write_log":
                    VoiceAttackPlugin.WriteToLog(
                        payload[ "message" ]?.Value<string>() ?? string.Empty,
                        payload[ "color" ]?.Value<string>() ?? "white" );
                    break;
                case "set_text":
                    VoiceAttackPlugin.SetText( payload[ "key" ]?.Value<string>() ?? string.Empty,
                        payload[ "value" ]?.Type == JTokenType.Null ? null : payload[ "value" ]?.ToString() );
                    break;
                case "set_int":
                    VoiceAttackPlugin.SetInt( payload[ "key" ]?.Value<string>() ?? string.Empty,
                        ParseInt( payload[ "value" ] ) );
                    break;
                case "set_decimal":
                    VoiceAttackPlugin.SetDecimal( payload[ "key" ]?.Value<string>() ?? string.Empty,
                        ParseDecimal( payload[ "value" ] ) );
                    break;
                case "set_boolean":
                    VoiceAttackPlugin.SetBoolean( payload[ "key" ]?.Value<string>() ?? string.Empty,
                        ParseBoolean( payload[ "value" ] ) );
                    break;
                case "set_date":
                    VoiceAttackPlugin.SetDate( payload[ "key" ]?.Value<string>() ?? string.Empty,
                        ParseDateTime( payload[ "value" ] ) );
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

                var key = variable[ "key" ]?.Value<string>();
                if ( string.IsNullOrWhiteSpace( key ) )
                {
                    continue;
                }

                var type = variable[ "type" ]?.Value<string>() ?? "text";
                var valueToken = variable[ "value" ];

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
