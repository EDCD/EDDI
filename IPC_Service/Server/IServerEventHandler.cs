#nullable enable

using EddiIPC_Service.Messages;
using System.Threading.Tasks;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Handler interface for processing specific message types in the IPC server.
    /// Implementations handle events, commands, and other protocol messages.
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
        /// Handle a Command message (request to execute action in EDDI).
        /// </summary>
        /// <param name="message">The Command message</param>
        /// <param name="context">The client connection context</param>
        Task HandleCommandAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Handle an Event message (should not be sent to server, but handle gracefully if received).
        /// </summary>
        /// <param name="message">The Event message</param>
        /// <param name="context">The client connection context</param>
        Task HandleEventAsync(MessageEnvelope message, ConnectionContext context);

        /// <summary>
        /// Send a command response to a specific client.
        /// </summary>
        /// <param name="sessionId">The target client session ID</param>
        /// <param name="response">The CommandResponse message</param>
        Task SendCommandResponseAsync(string sessionId, MessageEnvelope response);
    }
}
