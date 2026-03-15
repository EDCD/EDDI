#nullable enable

using System.Threading.Tasks;
using EddiVoiceAttackService.Messages;

namespace EddiVoiceAttackService.Server
{
    /// <summary>
    /// Handler interface for processing specific message types in the IPC server.
    /// Implementations handle events, commands, queries, and other protocol messages.
    /// </summary>
    public interface IServerEventHandler
    {
        /// <summary>
        /// Handle a Connect message (client connection initialization).
        /// </summary>
        /// <param name="message">The Connect message</param>
        /// <param name="context">The client connection context</param>
        Task HandleConnectAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle a Disconnect message (client disconnection).
        /// </summary>
        /// <param name="message">The Disconnect message</param>
        /// <param name="context">The client connection context</param>
        Task HandleDisconnectAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle a Heartbeat message (keep-alive).
        /// </summary>
        /// <param name="message">The Heartbeat message</param>
        /// <param name="context">The client connection context</param>
        Task HandleHeartbeatAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle a Command message (request to execute action in EDDI).
        /// </summary>
        /// <param name="message">The Command message</param>
        /// <param name="context">The client connection context</param>
        Task HandleCommandAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle a Query message (request for state or data from EDDI).
        /// </summary>
        /// <param name="message">The Query message</param>
        /// <param name="context">The client connection context</param>
        Task HandleQueryAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle an Event message (should not be sent to server, but handle gracefully if received).
        /// </summary>
        /// <param name="message">The Event message</param>
        /// <param name="context">The client connection context</param>
        Task HandleEventAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Broadcast an event to all connected clients.
        /// </summary>
        /// <param name="eventMessage">The Event message to broadcast</param>
        Task BroadcastEventAsync(MessageEnvelope eventMessage);

        /// <summary>
        /// Send a command response to a specific client.
        /// </summary>
        /// <param name="sessionId">The target client session ID</param>
        /// <param name="response">The CommandResponse message</param>
        Task SendCommandResponseAsync(string sessionId, MessageEnvelope response);

        /// <summary>
        /// Send a query response to a specific client.
        /// </summary>
        /// <param name="sessionId">The target client session ID</param>
        /// <param name="response">The QueryResponse message</param>
        Task SendQueryResponseAsync(string sessionId, MessageEnvelope response);
    }
}
