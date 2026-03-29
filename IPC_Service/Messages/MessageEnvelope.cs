#nullable enable

using System;

namespace EddiIPC_Service.Messages;

/// <summary>
/// Generic message envelope for all IPC communication.
/// Used for serialization/deserialization of JSON messages.
/// </summary>
public class MessageEnvelope : IMessage
{
    /// <summary>Message type identifier</summary>
    public required string Type { get; set; }

    /// <summary>ISO 8601 UTC timestamp</summary>
    public required string Timestamp { get; set; }

    /// <summary>Unique message identifier (UUID)</summary>
    public required string Id { get; set; }

    /// <summary>Message-specific payload (type depends on Type field)</summary>
    public required object Data { get; set; }

    /// <summary>
    /// Create a new message envelope with current timestamp and UUID.
    /// </summary>
    public static MessageEnvelope Create(string type, object data, string? id = null)
    {
        return new MessageEnvelope
        {
            Type = type,
            Timestamp = DateTime.UtcNow.ToString("O"), // ISO 8601 format
            Id = id ?? Guid.NewGuid().ToString("D"),
            Data = data
        };
    }

    /// <summary>Validate that all required fields are present and non-null.</summary>
    public void Validate()
    {
        if ( string.IsNullOrWhiteSpace( Type ) )
        {
            throw new ArgumentException("Message Type is required.");
        }

        if ( string.IsNullOrWhiteSpace( Timestamp ) )
        {
            throw new ArgumentException("Message Timestamp is required.");
        }

        if ( string.IsNullOrWhiteSpace( Id ) )
        {
            throw new ArgumentException("Message Id is required.");
        }

        if ( Data == null )
        {
            throw new ArgumentException("Message Data is required.");
        }
    }
}
