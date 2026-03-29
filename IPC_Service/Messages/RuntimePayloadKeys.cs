#nullable enable

using System;

namespace EddiIPC_Service.Messages
{
    /// <summary>
    /// Canonical key names for dynamic VoiceAttack runtime payloads transported over IPC.
    /// </summary>
    public static class RuntimePayloadKeys
    {
        public static class EventEnvelope
        {
            public static readonly string EventType = nameof(EventData.EventType);
            public static readonly string EventName = nameof(EventData.EventName);
            public static readonly string EventPayload = nameof(EventData.EventPayload);
        }

        public static class DispatchPayload
        {
            public static readonly string CommandName = ToCamelCase(nameof(DispatchPayloadContract.CommandName));
            public static readonly string EventType = ToCamelCase(nameof(DispatchPayloadContract.EventType));
            public static readonly string ClearVariables = ToCamelCase(nameof(DispatchPayloadContract.ClearVariables));
            public static readonly string SetVariables = ToCamelCase(nameof(DispatchPayloadContract.SetVariables));
        }

        public static class VariablePayload
        {
            public static readonly string Key = ToCamelCase(nameof(VariablePayloadContract.Key));
            public static readonly string Type = ToCamelCase(nameof(VariablePayloadContract.Type));
            public static readonly string Value = ToCamelCase(nameof(VariablePayloadContract.Value));
        }

        public static class CommandActionPayload
        {
            public static readonly string Actions = ToCamelCase(nameof(CommandActionPayloadContract.Actions));
            public static readonly string Action = ToCamelCase(nameof(CommandActionPayloadContract.Action));
            public static readonly string Key = ToCamelCase(nameof(CommandActionPayloadContract.Key));
            public static readonly string Value = ToCamelCase(nameof(CommandActionPayloadContract.Value));
            public static readonly string Message = ToCamelCase(nameof(CommandActionPayloadContract.Message));
            public static readonly string Color = ToCamelCase(nameof(CommandActionPayloadContract.Color));
        }

        private static string ToCamelCase(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return name.Length switch
            {
                1 => name.ToLowerInvariant(),
                _ when char.IsLower(name[0]) => name,
                _ => char.ToLowerInvariant(name[0]) + name[1..]
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
