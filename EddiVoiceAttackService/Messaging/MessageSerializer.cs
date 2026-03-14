using System;
using System.Collections.Generic;
using EddiVoiceAttackService.Messages;
using Newtonsoft.Json;
using Utilities;

namespace EddiVoiceAttackService.Messaging;

/// <summary>
/// Serializes and deserializes IPC messages using length-prefixed JSON encoding.
/// 
/// Format: [LENGTH]\n[PAYLOAD]
/// - LENGTH: UTF-8 byte count of JSON payload (unsigned int)
/// - PAYLOAD: UTF-8 encoded JSON string
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
        if ( message == null )
        {
            throw new ArgumentNullException( nameof(message) );
        }

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
            var json = JsonConvert.SerializeObject( message, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "O", // ISO 8601
                Formatting = Formatting.None
            } );

            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( json );
            var length = jsonBytes.Length;

            return $"{length}\n{json}";
        }
        catch ( JsonException ex )
        {
            Logging.Error( $"Failed to serialize message of type {message.Type}: {ex.Message}" );
            throw new ArgumentException( $"Message serialization failed: {ex.Message}", ex );
        }
    }

    /// <summary>
    /// Deserialize a length-prefixed JSON message.
    /// </summary>
    /// <param name="serialized">Length-prefixed JSON string: "[LENGTH]\n[PAYLOAD]"</param>
    /// <returns>Deserialized MessageEnvelope</returns>
    /// <exception cref="ArgumentException">If format is invalid or payload is malformed</exception>
    public static MessageEnvelope Deserialize ( string serialized )
    {
        if ( string.IsNullOrEmpty( serialized ) )
        {
            throw new ArgumentException( @"Serialized message cannot be null or empty.", nameof(serialized) );
        }

        try
        {
            // Split on first newline
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

            var jsonPart = serialized.Substring( newlineIndex + 1 );

            // Verify byte count matches declared length
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes( jsonPart );
            if ( jsonBytes.Length != declaredLength )
            {
                throw new ArgumentException( $"Length mismatch: declared {declaredLength} bytes but payload is {jsonBytes.Length} bytes." );
            }

            // Deserialize JSON
            var message = JsonConvert.DeserializeObject<MessageEnvelope>( jsonPart, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DateFormatString = "O" } );

            if ( message is null )
            {
                throw new ArgumentException( "JSON deserialized to null." );
            }

            // Validate envelope
            message.Validate();

            Logging.Debug( $"Deserialized message: type={message.Type}, id={message.Id}" );
            return message;
        }
        catch ( JsonException ex )
        {
            Logging.Error( $"Failed to deserialize message: {ex.Message}" );
            throw new ArgumentException( $"Message payload is not valid JSON: {ex.Message}", ex );
        }
        catch ( ArgumentException )
        {
            throw; // Re-throw validation/format errors
        }
        catch ( Exception ex )
        {
            Logging.Error( $"Unexpected error during deserialization: {ex.Message}" );
            throw new ArgumentException( $"Deserialization failed: {ex.Message}", ex );
        }
    }

    /// <summary>
    /// Deserialize multiple length-prefixed messages from a stream.
    /// Handles partial messages and provides remaining unprocessed data.
    /// </summary>
    /// <param name="buffer">Buffer containing one or more messages</param>
    /// <param name="messages">Output list of successfully deserialized messages</param>
    /// <param name="remaining">Output: unprocessed data (partial message, etc.)</param>
    /// <returns>Number of complete messages deserialized</returns>
    public static int DeserializeMessages ( string buffer, out List<MessageEnvelope> messages, out string remaining )
    {
        messages = new List<MessageEnvelope>();
        remaining = buffer;

        while ( !string.IsNullOrEmpty( remaining ) )
        {
            var newlineIndex = remaining.IndexOf( '\n' );
            if ( newlineIndex <= 0 )
            {
                break; // No complete length prefix yet
            }

            var lengthPart = remaining.Substring( 0, newlineIndex );
            if ( !int.TryParse( lengthPart, out var declaredLength ) || declaredLength <= 0 )
            {
                break; // Invalid length, stop processing
            }

            var jsonStart = newlineIndex + 1;
            if ( ( jsonStart + declaredLength ) > remaining.Length )
            {
                break; // Not enough data yet for complete payload
            }

            var jsonPart = remaining.Substring( jsonStart, declaredLength );
            try
            {
                var message = JsonConvert.DeserializeObject<MessageEnvelope>( jsonPart );
                if ( message != null )
                {
                    message.Validate();
                    messages.Add( message );
                }
            }
            catch
            {
                // Skip malformed messages, continue processing
                Logging.Warn( "Skipped malformed message during batch deserialization." );
            }

            remaining = remaining.Substring( jsonStart + declaredLength );
        }

        return messages.Count;
    }
}
