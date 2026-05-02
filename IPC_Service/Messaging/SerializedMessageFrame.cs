#nullable enable

using System;

namespace EddiIPC_Service.Messaging
{
    public sealed class SerializedMessageFrame ( string messageType, string messageId, byte[] bytes )
    {
        public string MessageType { get; } = messageType;

        public string MessageId { get; } = messageId;

        public byte[] Bytes { get; } = bytes ?? throw new ArgumentNullException( nameof( bytes ) );

        public int Length => Bytes.Length;

        public ReadOnlyMemory<byte> Memory => Bytes.AsMemory();
    }
}