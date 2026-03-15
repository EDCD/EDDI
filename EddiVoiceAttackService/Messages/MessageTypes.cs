#nullable enable

namespace EddiVoiceAttackService.Messages;

/// <summary>
/// Standard message type identifiers for IPC protocol.
/// Using constants enables compile-time type safety and refactoring support.
/// </summary>
public static class MessageTypes
{
    /// <summary>Heartbeat keep-alive message (bidirectional)</summary>
    public const string Heartbeat = nameof(Heartbeat);

    /// <summary>Connection initialization message (VA Plugin → EDDI)</summary>
    public const string Connect = nameof(Connect);

    /// <summary>Connection acknowledgment message (EDDI → VA Plugin)</summary>
    public const string ConnectAck = nameof(ConnectAck);

    /// <summary>Disconnection message (either direction)</summary>
    public const string Disconnect = nameof(Disconnect);

    /// <summary>Event notification message (EDDI → VA Plugin)</summary>
    public const string Event = nameof(Event);

    /// <summary>Command execution request message (VA Plugin → EDDI)</summary>
    public const string Command = nameof(Command);

    /// <summary>Command response message (EDDI → VA Plugin)</summary>
    public const string CommandResponse = nameof(CommandResponse);

    /// <summary>State query request message (VA Plugin → EDDI)</summary>
    public const string Query = nameof(Query);

    /// <summary>Query response message (EDDI → VA Plugin)</summary>
    public const string QueryResponse = nameof(QueryResponse);

    /// <summary>Error message (either direction)</summary>
    public const string Error = nameof(Error);
}

/// <summary>
/// Server capabilities and supported message types.
/// </summary>
public static class ServerCapabilities
{
    /// <summary>Server supports event broadcasting</summary>
    public const string Events = nameof(Events);

    /// <summary>Server supports command execution</summary>
    public const string Commands = nameof(Commands);

    /// <summary>Server supports state queries</summary>
    public const string Queries = nameof(Queries);

    /// <summary>All supported capabilities</summary>
    public static string[] AllCapabilities => new[] { Events, Commands, Queries };

    /// <summary>All supported message types</summary>
    public static string[] AllMessageTypes => new[]
    {
        MessageTypes.Connect,
        MessageTypes.Disconnect,
        MessageTypes.Heartbeat,
        MessageTypes.Command,
        MessageTypes.Query,
        MessageTypes.Event,
        MessageTypes.Error
    };
}
