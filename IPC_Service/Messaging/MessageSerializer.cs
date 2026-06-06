#nullable enable

using EddiIPC_Service.Messages;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Utilities;

namespace EddiIPC_Service.Messaging;

/// <summary>
/// Serializes and deserializes IPC messages using length-prefixed JSON encoding.
/// 
/// Format: [LENGTH]\n[PAYLOAD]
/// - LENGTH: UTF-8 byte count of JSON payload (unsigned int)
/// - PAYLOAD: JSON string
/// 
/// Example: "147\n{\"type\":\"Heartbeat\",...}"
/// </summary>
public static class MessageSerializer
{
    /// <summary>
    /// Serialize a message envelope to length-prefixed JSON.
    /// </summary>
    /// <param name="message">Message to serialize</param>
    /// <returns>Length-prefixed JSON string: "[LENGTH]\n[PAYLOAD]"</returns>
    /// <exception cref="ArgumentNullException">If message is null</exception>
    /// <exception cref="ArgumentException">If message fails validation</exception>
    public static string Serialize ( MessageEnvelope message )
    {
        var frame = SerializeToFrame( message );
        return Encoding.UTF8.GetString( frame.Bytes );
    }

    /// <summary>
    /// Deserialize a length-prefixed JSON message.
    /// </summary>
    /// <param name="serialized">Length-prefixed JSON string: "[LENGTH]\n[PAYLOAD]"</param>
    /// <returns>Deserialized MessageEnvelope</returns>
    /// <exception cref="ArgumentException">If format is invalid or payload is malformed</exception>
    public static MessageEnvelope Deserialize ( string serialized )
    {
        if ( string.IsNullOrWhiteSpace( serialized ) )
        {
            throw new ArgumentException( @"Serialized message cannot be null or empty.", nameof(serialized) );
        }

        try
        {
            var newlineIndex = serialized.IndexOf( '\n' );
            if ( newlineIndex <= 0 )
            {
                throw new ArgumentException( "Message does not contain length prefix (no newline found)." );
            }

            var lengthPart = serialized.Substring( 0, newlineIndex );
            if ( !int.TryParse( lengthPart, out var declaredLength ) || declaredLength <= 0 )
            {
                throw new ArgumentException( $"Invalid length prefix: '{lengthPart}' is not a positive integer." );
            }

            if ( !TryReadPayload( serialized.AsSpan( newlineIndex + 1 ), declaredLength, out var jsonPart,
                    out var charsConsumed ) )
            {
                var actualLength = Encoding.UTF8.GetByteCount( serialized.AsSpan( newlineIndex + 1 ) );
                throw new ArgumentException(
                    $"Length mismatch: declared {declaredLength} bytes but payload is {actualLength} bytes." );
            }

            if ( (newlineIndex + 1 + charsConsumed) != serialized.Length )
            {
                throw new ArgumentException( "Message contains trailing data beyond the declared payload length." );
            }

            var message = DeserializePayload( jsonPart );
            Logging.Debug( $"Deserialized message: type={message.Type}, id={message.Id}" );
            return message;
        }
        catch ( JsonException ex )
        {
            Logging.Error( $"Failed to deserialize message: {ex.Message}" );
            throw new ArgumentException( $"Message payload is not valid JSON: {ex.Message}", ex );
        }
    }

    public static int DeserializeMessages ( ReadOnlySpan<byte> buffer, out List<MessageEnvelope> messages, out int bytesConsumed )
    {
        messages = [ ];
        bytesConsumed = 0;

        while ( !buffer.IsEmpty )
        {
            var newlineIndex = buffer.IndexOf( (byte)'\n' );
            if ( newlineIndex <= 0 )
            {
                break;
            }

            var lengthPart = Encoding.ASCII.GetString( buffer.Slice( 0, newlineIndex ) );
            if ( !int.TryParse( lengthPart, out var declaredLength ) || declaredLength <= 0 )
            {
                break;
            }

            var jsonStart = newlineIndex + 1;
            if ( ( jsonStart + declaredLength ) > buffer.Length )
            {
                break;
            }

            var jsonPart = Encoding.UTF8.GetString( buffer.Slice( jsonStart, declaredLength ) );
            try
            {
                messages.Add( DeserializePayload( jsonPart ) );
            }
            catch ( ArgumentException ex )
            {
                Logging.Warn( $"Skipped malformed message during batch deserialization: {ex.Message}" );
            }

            var consumed = jsonStart + declaredLength;
            bytesConsumed += consumed;
            buffer = buffer.Slice( consumed );
        }

        return messages.Count;
    }

    public static int DeserializeMessages (
        byte[] buffer,
        int offset,
        int count,
        out List<MessageEnvelope> messages,
        out int bytesConsumed )
    {
        ArgumentNullException.ThrowIfNull( buffer );

        if ( offset < 0 || count < 0 || (offset + count) > buffer.Length )
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

    private static MessageEnvelope DeserializePayload ( string jsonPart )
    {
        try
        {
            var message = JsonConvert.DeserializeObject<MessageEnvelope>( jsonPart, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "O"
            } );

            ArgumentNullException.ThrowIfNull( message ); // JSON deserialized to null.

            message.Validate();
            return message;
        }
        catch ( JsonException ex )
        {
            throw new ArgumentException( $"Message payload is not valid JSON: {ex.Message}", ex );
        }
    }

    private static bool TryReadPayload ( ReadOnlySpan<char> buffer, int declaredLength, out string payload,
        out int charsConsumed )
    {
        payload = string.Empty;
        charsConsumed = 0;

        var byteCount = 0;
        while ( charsConsumed < buffer.Length && byteCount < declaredLength )
        {
            var status = Rune.DecodeFromUtf16( buffer.Slice( charsConsumed ), out var rune, out var runeCharsConsumed );
            if ( status != OperationStatus.Done )
            {
                return false;
            }

            var runeLength = rune.Utf8SequenceLength;
            if ( (byteCount + runeLength) > declaredLength )
            {
                return false;
            }

            byteCount += runeLength;
            charsConsumed += runeCharsConsumed;
        }

        if ( byteCount != declaredLength )
        {
            return false;
        }

        payload = buffer.Slice( 0, charsConsumed ).ToString();
        return true;
    }

        public static SerializedMessageFrame SerializeToFrame ( MessageEnvelope message )
        {
            ArgumentNullException.ThrowIfNull( message );

            try
            {
                message.Validate();
            }
            catch ( ArgumentException ex )
            {
                Logging.Error( $"Message validation failed: {ex.Message}" );
                throw;
            }

            try
            {
                // Create a snapshot of the message with deep-copied mutable collections
                // to prevent concurrent modification exceptions during serialization.
                var snapshot = CreateThreadSafeSnapshot( message );

                // Serialization is the freeze point. After this returns, nested mutations cannot affect transport.
                var json = JsonConvert.SerializeObject( snapshot, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "O",
                    Formatting = Formatting.None
                } );

                var payloadLength = Encoding.UTF8.GetByteCount( json );
                var header = payloadLength.ToString( CultureInfo.InvariantCulture ) + "\n";
                var headerLength = Encoding.ASCII.GetByteCount( header );

                var bytes = new byte[ headerLength + payloadLength ];

                Encoding.ASCII.GetBytes(
                    header,
                    0,
                    header.Length,
                    bytes,
                    0 );

                Encoding.UTF8.GetBytes(
                    json,
                    0,
                    json.Length,
                    bytes,
                    headerLength );

                return new SerializedMessageFrame(
                    message.Type,
                    message.Id,
                    bytes );
            }
            catch ( JsonException ex )
            {
                Logging.Error( $"Failed to serialize message of type {message.Type}: {ex.Message}" );
                throw new ArgumentException( $"Message serialization failed: {ex.Message}", ex );
            }
            catch ( InvalidOperationException ex )
            {
                Logging.Error( $"Failed to serialize message of type {message.Type}: {ex.Message}" );
                throw new ArgumentException( $"Message serialization failed: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Creates a thread-safe snapshot of the message by deep-copying mutable collections.
        /// This prevents InvalidOperationException when concurrent modifications occur during serialization.
        /// </summary>
        private static MessageEnvelope CreateThreadSafeSnapshot ( MessageEnvelope message )
        {
            var snapshot = new MessageEnvelope
            {
                Type = message.Type,
                Timestamp = message.Timestamp,
                Id = message.Id,
                Data = CopyMessageData( message.Data )
            };

            return snapshot;
        }

        /// <summary>
        /// Recursively copies message data, creating new instances of mutable dictionaries
        /// to avoid concurrent modification exceptions.
        /// </summary>
        private static object CopyMessageData ( object data )
        {
            return data switch
            {
                EventData eventData => new EventData
                {
                    EventType = eventData.EventType,
                    EventName = eventData.EventName,
                    EventPayload = eventData.EventPayload != null
                        ? new Dictionary<string, object>( eventData.EventPayload )
                        : new Dictionary<string, object>()
                },
                CommandData commandData => new CommandData
                {
                    Command = commandData.Command,
                    Target = commandData.Target,
                    Parameters = commandData.Parameters != null
                        ? new Dictionary<string, object>( commandData.Parameters )
                        : new Dictionary<string, object>()
                },
                ConnectData connectData => new ConnectData
                {
                    PluginVersion = connectData.PluginVersion,
                    PluginName = connectData.PluginName,
                    Capabilities = new List<string>( connectData.Capabilities ),
                    SupportedMessageTypes = new List<string>( connectData.SupportedMessageTypes )
                },
                ConnectAckData connectAckData => new ConnectAckData
                {
                    EddiVersion = connectAckData.EddiVersion,
                    ServerName = connectAckData.ServerName,
                    Accepted = connectAckData.Accepted,
                    Capabilities = new List<string>( connectAckData.Capabilities ),
                    SupportedMessageTypes = new List<string>( connectAckData.SupportedMessageTypes ),
                    SessionId = connectAckData.SessionId
                },
                DisconnectData disconnectData => new DisconnectData
                {
                    Reason = disconnectData.Reason,
                    Message = disconnectData.Message
                },
                CommandResponseData commandResponseData => new CommandResponseData
                {
                    CommandId = commandResponseData.CommandId,
                    Status = commandResponseData.Status,
                    Message = commandResponseData.Message,
                    Result = commandResponseData.Result != null
                        ? new Dictionary<string, object>( commandResponseData.Result )
                        : new Dictionary<string, object>()
                },
                _ => data
            };
        }
    }
