using System.Collections.Generic;

namespace EddiIPC_Service.Messages
{
    /// <summary>Connection establishment message data (VA Plugin → EDDI).</summary>
    public class ConnectData
    {
        public required string PluginVersion { get; set; }
        public required string PluginName { get; set; }
        public required IList<string> Capabilities { get; set; }
        public required IList<string> SupportedMessageTypes { get; set; }
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
        public required string Reason { get; set; } // "user_shutdown", "network_error"
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>Event notification data (EDDI → VA Plugin).</summary>
    public class EventData
    {
        public required string EventType { get; set; }
        public required string EventName { get; set; }
        public Dictionary<string, object> EventPayload { get; set; } = [];
    }

    /// <summary>Command message data (VA Plugin → EDDI).</summary>
    public class CommandData
    {
        public required string Command { get; set; } // "enable_monitor", "disable_monitor", etc.
        public required string Target { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = [];
    }

    /// <summary>Command response data (EDDI → VA Plugin).</summary>
    public class CommandResponseData
    {
        public required string CommandId { get; set; }
        public required string Status { get; set; } // "success" or "error"
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Result { get; set; } = [];
    }

    /// <summary>Error message data (either direction).</summary>
    public class ErrorData
    {
        public required string ErrorCode { get; set; }
        public required string Message { get; set; }
        public string OriginalMessageId { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = [];
    }
}
