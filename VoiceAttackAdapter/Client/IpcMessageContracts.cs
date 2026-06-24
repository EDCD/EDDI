#nullable enable

using System;
using System.Collections.Generic;

namespace EddiVoiceAttackAdapter.Client
{
    internal static class AdapterMessageTypes
    {
        public const string Connect = nameof( Connect );
        public const string ConnectAck = nameof( ConnectAck );
        public const string Disconnect = nameof( Disconnect );
        public const string Event = nameof( Event );
        public const string Command = nameof( Command );
        public const string CommandResponse = nameof( CommandResponse );
        public const string Error = nameof( Error );
    }

    internal static class AdapterServerCapabilities
    {
        public const string Events = nameof( Events );
        public const string Commands = nameof( Commands );

        public static string[] AllCapabilities => [ Events, Commands ];

        public static string[] AllMessageTypes =>
        [
            AdapterMessageTypes.Connect,
            AdapterMessageTypes.Disconnect,
            AdapterMessageTypes.Command,
            AdapterMessageTypes.Event,
            AdapterMessageTypes.Error
        ];
    }

    public sealed class AdapterMessageEnvelope
    {
        public required string Type { get; set; }
        public required string Timestamp { get; set; }
        public required string Id { get; set; }
        public required object Data { get; set; }

        public static AdapterMessageEnvelope Create ( string type, object data, string? id = null )
        {
            return new AdapterMessageEnvelope
            {
                Type = type,
                Timestamp = DateTime.UtcNow.ToString( "O" ),
                Id = id ?? Guid.NewGuid().ToString( "D" ),
                Data = data
            };
        }

        public void Validate ()
        {
            if ( string.IsNullOrWhiteSpace( Type ) )
            {
                throw new ArgumentException( "Message Type is required." );
            }

            if ( string.IsNullOrWhiteSpace( Timestamp ) )
            {
                throw new ArgumentException( "Message Timestamp is required." );
            }

            if ( string.IsNullOrWhiteSpace( Id ) )
            {
                throw new ArgumentException( "Message Id is required." );
            }

            if ( Data == null )
            {
                throw new ArgumentException( "Message Data is required." );
            }
        }
    }

    internal sealed class AdapterConnectData
    {
        public required string PluginVersion { get; set; }
        public required string PluginName { get; set; }
        public required IList<string> Capabilities { get; set; }
        public required IList<string> SupportedMessageTypes { get; set; }
    }

    internal sealed class AdapterDisconnectData
    {
        public required string Reason { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    internal sealed class AdapterEventData
    {
        public required string EventType { get; set; }
        public required string EventName { get; set; }
        public Dictionary<string, object> EventPayload { get; set; } = [];
    }

    internal sealed class AdapterCommandData
    {
        public required string Command { get; set; }
        public required string Target { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = [];
    }

    public sealed class MessageReceivedEventArgs ( string messageType, AdapterMessageEnvelope messageEnvelope ) : EventArgs
    {
        public string MessageType { get; } = messageType ?? throw new ArgumentNullException( nameof( messageType ) );

        public AdapterMessageEnvelope MessageEnvelope { get; } = messageEnvelope ?? throw new ArgumentNullException( nameof( messageEnvelope ) );
    }

    public sealed class ConnectionLostEventArgs ( string reason, Exception? exception = null ) : EventArgs
    {
        public string Reason { get; } = reason ?? throw new ArgumentNullException( nameof( reason ) );

        public Exception? Exception { get; } = exception;
    }

    public sealed class ConnectionStatus
    {
        public bool IsConnected { get; set; }
        public string? ServerAddress { get; set; }
        public int? ServerPort { get; set; }
        public string? SessionId { get; set; }
        public DateTime? ConnectedAt { get; set; }
        public long MessagesSent { get; set; }
        public long MessagesReceived { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public double AverageResponseTimeMs { get; set; }
    }

    internal static class AdapterRuntimePayloadKeys
    {
        public static class EventEnvelope
        {
            public static readonly string EventType = nameof( AdapterEventData.EventType );
            public static readonly string EventName = nameof( AdapterEventData.EventName );
            public static readonly string EventPayload = nameof( AdapterEventData.EventPayload );
        }

        public static class DispatchPayload
        {
            public static readonly string CommandName = ToCamelCase( nameof( DispatchPayloadContract.CommandName ) );
            public static readonly string EventType = ToCamelCase( nameof( DispatchPayloadContract.EventType ) );
            public static readonly string ClearVariables = ToCamelCase( nameof( DispatchPayloadContract.ClearVariables ) );
            public static readonly string SetVariables = ToCamelCase( nameof( DispatchPayloadContract.SetVariables ) );
        }

        public static class VariablePayload
        {
            public static readonly string Key = ToCamelCase( nameof( VariablePayloadContract.Key ) );
            public static readonly string Type = ToCamelCase( nameof( VariablePayloadContract.Type ) );
            public static readonly string Value = ToCamelCase( nameof( VariablePayloadContract.Value ) );
        }

        public static class CommandActionPayload
        {
            public static readonly string Actions = ToCamelCase( nameof( CommandActionPayloadContract.Actions ) );
            public static readonly string Action = ToCamelCase( nameof( CommandActionPayloadContract.Action ) );
            public static readonly string Key = ToCamelCase( nameof( CommandActionPayloadContract.Key ) );
            public static readonly string Value = ToCamelCase( nameof( CommandActionPayloadContract.Value ) );
            public static readonly string Message = ToCamelCase( nameof( CommandActionPayloadContract.Message ) );
            public static readonly string Color = ToCamelCase( nameof( CommandActionPayloadContract.Color ) );
        }

        private static string ToCamelCase ( string name )
        {
            ArgumentException.ThrowIfNullOrWhiteSpace( name );

            return name.Length switch
            {
                1 => name.ToLowerInvariant(),
                _ when char.IsLower( name[ 0 ] ) => name,
                _ => char.ToLowerInvariant( name[ 0 ] ) + name[ 1.. ]
            };
        }

        private sealed class DispatchPayloadContract
        {
            public string CommandName { get; set; } = string.Empty;
            public string EventType { get; set; } = string.Empty;
            public object? ClearVariables { get; set; }
            public object? SetVariables { get; set; }
        }

        private sealed class VariablePayloadContract
        {
            public string Key { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public object? Value { get; set; }
        }

        private sealed class CommandActionPayloadContract
        {
            public object? Actions { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public object? Value { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
        }
    }
}
