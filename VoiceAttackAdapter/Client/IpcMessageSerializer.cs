#nullable enable

using EddiVoiceAttackAdapter.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EddiVoiceAttackAdapter.Client
{
    internal readonly record struct SerializedMessageFrame ( string MessageType, string MessageId, byte[] Bytes )
    {
        public ReadOnlyMemory<byte> Memory => Bytes;
    }

    internal static class IpcMessageSerializer
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static SerializedMessageFrame SerializeToFrame ( AdapterMessageEnvelope message )
        {
            ArgumentNullException.ThrowIfNull( message );
            message.Validate();

            var snapshot = CreateThreadSafeSnapshot( message );
            var json = JsonSerializer.Serialize( snapshot, JsonOptions );
            var payloadLength = Encoding.UTF8.GetByteCount( json );
            var header = payloadLength.ToString( CultureInfo.InvariantCulture ) + "\n";
            var headerLength = Encoding.ASCII.GetByteCount( header );
            var bytes = new byte[ headerLength + payloadLength ];

            Encoding.ASCII.GetBytes( header, 0, header.Length, bytes, 0 );
            Encoding.UTF8.GetBytes( json, 0, json.Length, bytes, headerLength );

            return new SerializedMessageFrame( message.Type, message.Id, bytes );
        }

        public static int DeserializeMessages (
            byte[] buffer,
            int offset,
            int count,
            out List<AdapterMessageEnvelope> messages,
            out int bytesConsumed )
        {
            ArgumentNullException.ThrowIfNull( buffer );

            if ( offset < 0 || count < 0 || ( offset + count ) > buffer.Length )
            {
                throw new ArgumentOutOfRangeException(
                    nameof( offset ),
                    @"The offset/count range is outside the supplied buffer." );
            }

            return DeserializeMessages(
                new ReadOnlySpan<byte>( buffer, offset, count ),
                out messages,
                out bytesConsumed );
        }

        private static int DeserializeMessages (
            ReadOnlySpan<byte> buffer,
            out List<AdapterMessageEnvelope> messages,
            out int bytesConsumed )
        {
            messages = [];
            bytesConsumed = 0;

            while ( !buffer.IsEmpty )
            {
                var newlineIndex = buffer.IndexOf( (byte)'\n' );
                if ( newlineIndex <= 0 )
                {
                    break;
                }

                var lengthPart = Encoding.ASCII.GetString( buffer[ ..newlineIndex ] );
                if ( !int.TryParse( lengthPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var declaredLength ) ||
                     declaredLength <= 0 )
                {
                    break;
                }

                var jsonStart = newlineIndex + 1;
                if ( jsonStart + declaredLength > buffer.Length )
                {
                    break;
                }

                var jsonPart = Encoding.UTF8.GetString( buffer.Slice( jsonStart, declaredLength ) );
                try
                {
                    messages.Add( DeserializePayload( jsonPart ) );
                }
                catch ( Exception ex ) when ( ex is ArgumentException or JsonException )
                {
                    AdapterLogger.Warn( $"Skipped malformed IPC message: {ex.Message}" );
                }

                var consumed = jsonStart + declaredLength;
                bytesConsumed += consumed;
                buffer = buffer[ consumed.. ];
            }

            return messages.Count;
        }

        private static AdapterMessageEnvelope DeserializePayload ( string jsonPart )
        {
            var message = JsonSerializer.Deserialize<AdapterMessageEnvelope>( jsonPart, JsonOptions ) ??
                          throw new ArgumentException( "Message payload deserialized to null." );
            message.Validate();
            return message;
        }

        private static AdapterMessageEnvelope CreateThreadSafeSnapshot ( AdapterMessageEnvelope message )
        {
            return new AdapterMessageEnvelope
            {
                Type = message.Type,
                Timestamp = message.Timestamp,
                Id = message.Id,
                Data = CopyMessageData( message.Data )
            };
        }

        private static object CopyMessageData ( object data )
        {
            return data switch
            {
                AdapterEventData eventData => new AdapterEventData
                {
                    EventType = eventData.EventType,
                    EventName = eventData.EventName,
                    EventPayload = eventData.EventPayload != null
                        ? new Dictionary<string, object>( eventData.EventPayload )
                        : []
                },
                AdapterCommandData commandData => new AdapterCommandData
                {
                    Command = commandData.Command,
                    Target = commandData.Target,
                    Parameters = commandData.Parameters != null
                        ? new Dictionary<string, object>( commandData.Parameters )
                        : []
                },
                AdapterConnectData connectData => new AdapterConnectData
                {
                    PluginVersion = connectData.PluginVersion,
                    PluginName = connectData.PluginName,
                    Capabilities = new List<string>( connectData.Capabilities ),
                    SupportedMessageTypes = new List<string>( connectData.SupportedMessageTypes )
                },
                AdapterDisconnectData disconnectData => new AdapterDisconnectData
                {
                    Reason = disconnectData.Reason,
                    Message = disconnectData.Message
                },
                _ => data
            };
        }
    }
}
