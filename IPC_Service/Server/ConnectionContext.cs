#nullable enable

using EddiIPC_Service.Messages;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Manages per-client connection state in the IPC server.
    /// Tracks session ID, heartbeat monitoring, and message queuing.
    /// </summary>
    public class ConnectionContext : IDisposable
    {
        private readonly object _lockObj = new();
        private bool _disposed;

        /// <summary>Unique session identifier for this connection</summary>
        public string SessionId { get; }

        /// <summary>Client's TCP connection</summary>
        public TcpClient? Client { get; }

        /// <summary>Network stream for reading/writing</summary>
        public NetworkStream? Stream { get; }

        /// <summary>Whether client has completed Connect handshake</summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>Client capabilities from Connect message</summary>
        public List<string> ClientCapabilities { get; set; } = [ ];

        /// <summary>Client supported message types from Connect message</summary>
        public List<string> SupportedMessageTypes { get; set; } = [ ];

        /// <summary>Plugin version from Connect message</summary>
        public string? PluginVersion { get; set; }

        /// <summary>Plugin name from Connect message</summary>
        public string? PluginName { get; set; }

        /// <summary>Queue of outgoing messages to send to client</summary>
        private readonly Queue<MessageEnvelope> _outgoingQueue = new();

        /// <summary>
        /// Create a new connection context for an accepted client.
        /// </summary>
        public ConnectionContext(TcpClient client)
        {
            ArgumentNullException.ThrowIfNull(client);
        
            Client = client;
            Stream = client.GetStream();
            SessionId = Guid.NewGuid().ToString("D");
            IsAuthenticated = false;
        }

        /// <summary>
        /// Enqueue a message to send to the client.
        /// Thread-safe for concurrent access from multiple threads.
        /// </summary>
        public void EnqueueOutgoingMessage(MessageEnvelope message)
        {
            ArgumentNullException.ThrowIfNull(message);
        
            lock (_lockObj)
            {
                _outgoingQueue.Enqueue(message);
            }
        }

        /// <summary>
        /// Dequeue a message to send to the client (FIFO).
        /// Returns null if queue is empty.
        /// Thread-safe for concurrent access.
        /// </summary>
        public MessageEnvelope? DequeueOutgoingMessage()
        {
            lock (_lockObj)
            {
                return _outgoingQueue.Count > 0 ? _outgoingQueue.Dequeue() : null;
            }
        }

        /// <summary>
        /// Check if there are pending outgoing messages.
        /// Thread-safe check for concurrent access.
        /// </summary>
        public bool HasOutgoingMessages()
        {
            lock (_lockObj)
            {
                return _outgoingQueue.Count > 0;
            }
        }

        /// <summary>
        /// Get the number of pending outgoing messages.
        /// </summary>
        public int OutgoingMessageCount
        {
            get
            {
                lock (_lockObj)
                {
                    return _outgoingQueue.Count;
                }
            }
        }

        /// <summary>
        /// Check if connection is still alive (client not disconnected).
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    // Check if socket is still connected
                    return Client?.Connected ?? false;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Dispose of resources (close socket, stream).
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Stream?.Dispose();
                Client?.Close();
                Client?.Dispose();
            }
            catch
            {
                // Ignore errors during cleanup
            }
            GC.SuppressFinalize(this);
            _disposed = true;
        }
    }
}
