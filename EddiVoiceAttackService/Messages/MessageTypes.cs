#nullable enable

namespace EddiVoiceAttackService.Messages;

/// <summary>
/// Standard message type identifiers for IPC protocol.
/// Using constants enables compile-time type safety and refactoring support.
/// </summary>
public static class MessageTypes
{
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

    /// <summary>All supported capabilities</summary>
    public static string[] AllCapabilities => [ Events, Commands ];

    /// <summary>All supported message types</summary>
    public static string[] AllMessageTypes =>
    [
        MessageTypes.Connect,
        MessageTypes.Disconnect,
        MessageTypes.Command,
        MessageTypes.Event,
        MessageTypes.Error
    ];
}
