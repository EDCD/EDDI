namespace EddiIPC_Service.Messages;

/// <summary>
/// Base interface for all IPC messages.
/// </summary>
public interface IMessage
{
    /// <summary>Message type identifier (e.g., "Heartbeat", "Connect", "Event")</summary>
    string Type { get; }

    /// <summary>ISO 8601 UTC timestamp when message was created</summary>
    string Timestamp { get; }

    /// <summary>Unique message identifier (UUID) for request/response matching</summary>
    string Id { get; }

    /// <summary>Message-specific payload data</summary>
    object Data { get; }
}
