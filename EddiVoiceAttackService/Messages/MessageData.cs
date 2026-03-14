using System.Collections.Generic;

namespace EddiVoiceAttackService.Messages;

/// <summary>
public class HeartbeatData
{
    public string Status { get; set; } = "alive";
    public long UptimeMs { get; set; }
}

/// <summary>Connection establishment message data (VA Plugin → EDDI).</summary>
public class ConnectData
{
    public required string PluginVersion { get; set; }
    public required string PluginName { get; set; }
    public required List<string> Capabilities { get; set; }
    public required List<string> SupportedMessageTypes { get; set; }
}

/// <summary>Connection acknowledgment message data (EDDI → VA Plugin).</summary>
public class ConnectAckData
{
    public required string EddiVersion { get; set; }
    public required string ServerName { get; set; }
    public bool Accepted { get; set; }
    public required List<string> Capabilities { get; set; }
    public required List<string> SupportedMessageTypes { get; set; }
    public required string SessionId { get; set; }
}

/// <summary>Disconnection message data (either direction).</summary>
public class DisconnectData
{
    public required string Reason { get; set; } // "user_shutdown", "network_error", "heartbeat_timeout"
    public string Message { get; set; } = string.Empty;
}

/// <summary>Event notification data (EDDI → VA Plugin).</summary>
public class EventData
{
    public required string EventType { get; set; }
    public required string EventName { get; set; }
    public Dictionary<string, object> EventPayload { get; set; } = new();
}

/// <summary>Command message data (VA Plugin → EDDI).</summary>
public class CommandData
{
    public required string Command { get; set; } // "enable_monitor", "disable_monitor", etc.
    public required string Target { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>Command response data (EDDI → VA Plugin).</summary>
public class CommandResponseData
{
    public required string CommandId { get; set; }
    public required string Status { get; set; } // "success" or "error"
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Result { get; set; } = new();
}

/// <summary>Error message data (either direction).</summary>
public class ErrorData
{
    public required string ErrorCode { get; set; }
    public required string Message { get; set; }
    public string OriginalMessageId { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>Query message data (VA Plugin → EDDI) - on-demand state requests.</summary>
public class QueryData
{
    public required string QueryType { get; set; } // e.g., "GetCurrentState", "GetSystemInfo"
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>Query response message data (EDDI → VA Plugin).</summary>
public class QueryResponseData
{
    public required string QueryId { get; set; }
    public required string Status { get; set; } // "success" or "error"
    public Dictionary<string, object> Result { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
