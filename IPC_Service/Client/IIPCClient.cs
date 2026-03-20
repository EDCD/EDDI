#nullable enable

using EddiIPC_Service.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EddiIPC_Service.Client
{
    /// <summary>
    /// Defines the contract for an IPC client that communicates with EDDI's IPC server.
    /// Provides connection management, message dispatch, and event handling for plugin-to-EDDI communication.
    /// </summary>
    public interface IIPCClient : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether the client is currently connected to the IPC server.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the IPC server at the specified host and port.
        /// </summary>
        /// <param name="host">The host address of the IPC server (typically "127.0.0.1")</param>
        /// <param name="port">The port number of the IPC server</param>
        /// <param name="cancellationToken">Cancellation token to cancel the connection attempt</param>
        /// <returns>A task representing the asynchronous connection operation</returns>
        /// <exception cref="InvalidOperationException">If already connected</exception>
        /// <exception cref="OperationCanceledException">If the connection is cancelled</exception>
        Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects from the IPC server gracefully.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the disconnection</param>
        /// <returns>A task representing the asynchronous disconnection operation</returns>
        Task DisconnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a command to EDDI and waits for the response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response expected from the command</typeparam>
        /// <param name="command">The command data to send</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous command operation, with the response</returns>
        /// <exception cref="InvalidOperationException">If not connected</exception>
        /// <exception cref="OperationCanceledException">If the operation is cancelled or times out</exception>
        Task<TResponse> SendCommandAsync<TResponse>(CommandData command, CancellationToken cancellationToken = default) 
            where TResponse : class;

        /// <summary>
        /// Sends an event to EDDI (fire-and-forget, no response expected).
        /// </summary>
        /// <param name="eventData">The event data to send</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task representing the asynchronous event operation</returns>
        /// <exception cref="InvalidOperationException">If not connected</exception>
        /// <exception cref="OperationCanceledException">If the operation is cancelled or times out</exception>
        Task SendEventAsync(EventData eventData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Occurs when a message is received from the IPC server.
        /// </summary>
        event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// Occurs when the connection is lost unexpectedly.
        /// </summary>
        event EventHandler<ConnectionLostEventArgs>? ConnectionLost;

        /// <summary>
        /// Gets the current connection status and metadata.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, with connection status</returns>
        Task<ConnectionStatus> GetStatusAsync();
    }

    /// <summary>
    /// Event arguments for when a message is received from the server.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the type of message received.
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Gets the message envelope containing the full message details.
        /// </summary>
        public MessageEnvelope MessageEnvelope { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReceivedEventArgs"/> class.
        /// </summary>
        public MessageReceivedEventArgs(string messageType, MessageEnvelope messageEnvelope)
        {
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            MessageEnvelope = messageEnvelope ?? throw new ArgumentNullException(nameof(messageEnvelope));
        }
    }

    /// <summary>
    /// Event arguments for when the connection is lost.
    /// </summary>
    public class ConnectionLostEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the reason the connection was lost.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the exception that caused the connection loss, if any.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionLostEventArgs"/> class.
        /// </summary>
        public ConnectionLostEventArgs(string reason, Exception? exception = null)
        {
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Exception = exception;
        }
    }

    /// <summary>
    /// Represents the status of an IPC client connection.
    /// </summary>
    public class ConnectionStatus
    {
        /// <summary>
        /// Gets a value indicating whether the client is connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets the server address the client is connected to (or attempting to connect to).
        /// </summary>
        public string? ServerAddress { get; set; }

        /// <summary>
        /// Gets the server port the client is connected to (or attempting to connect to).
        /// </summary>
        public int? ServerPort { get; set; }

        /// <summary>
        /// Gets the unique session ID for this connection.
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Gets the time the connection was established.
        /// </summary>
        public DateTime? ConnectedAt { get; set; }

        /// <summary>
        /// Gets the number of messages sent to the server.
        /// </summary>
        public long MessagesSent { get; set; }

        /// <summary>
        /// Gets the number of messages received from the server.
        /// </summary>
        public long MessagesReceived { get; set; }

        /// <summary>
        /// Gets the time of the last message sent or received.
        /// </summary>
        public DateTime? LastActivityAt { get; set; }

        /// <summary>
        /// Gets the average response time for command/query operations in milliseconds.
        /// </summary>
        public double AverageResponseTimeMs { get; set; }
    }
}
