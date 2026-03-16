#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EddiVoiceAttackService.Messages;
using Utilities;

namespace EddiVoiceAttackService.Server
{
    /// <summary>
    /// Default implementation of IServerEventHandler.
    /// Handles protocol-level message dispatch and responses.
    /// Integrates with the EDDI message bus for application-level handling.
    /// </summary>
    public class DefaultServerEventHandler : IServerEventHandler
    {
        private readonly IPCServer _server;

        /// <summary>
        /// Create a new default event handler for an IPC server.
        /// </summary>
        public DefaultServerEventHandler ( IPCServer server )
        {
            ArgumentNullException.ThrowIfNull( server );
            _server = server;
        }

        /// <summary>
        /// Handle client connection initialization.
        /// Validates plugin credentials and sends connection acknowledgment.
        /// </summary>
        public async Task HandleConnectAsync ( MessageEnvelope message, ConnectionContext context )
        {
            ArgumentNullException.ThrowIfNull( message );
            ArgumentNullException.ThrowIfNull( context );

            try
            {
                if ( message.Data is not Dictionary<string, object> )
                {
                    var error = MessageEnvelope.Create( MessageTypes.Error,
                        new ErrorData
                        {
                            ErrorCode = "INVALID_CONNECT_DATA",
                            Message = "Connect message data is not valid",
                            OriginalMessageId = message.Id
                        } );
                    await SendToContextAsync( context, error );
                    return;
                }

                Logging.Info( $"Client connected: {context.SessionId}" );

                // Send connection acknowledgment
                var ackData = new ConnectAckData
                {
                    EddiVersion = Constants.EDDI_VERSION.ToString(),
                    ServerName = $"{Constants.EDDI_NAME}-IPC",
                    Accepted = true,
                    Capabilities = new List<string>( ServerCapabilities.AllCapabilities ),
                    SupportedMessageTypes = new List<string>( ServerCapabilities.AllMessageTypes ),
                    SessionId = context.SessionId
                };

                var ack = MessageEnvelope.Create( MessageTypes.ConnectAck, ackData );
                await SendToContextAsync( context, ack );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error handling Connect: {ex.Message}", ex );
                var error = MessageEnvelope.Create( MessageTypes.Error,
                    new ErrorData
                    {
                        ErrorCode = "CONNECT_HANDLER_ERROR",
                        Message = $"Server error: {ex.Message}",
                        OriginalMessageId = message.Id
                    } );
                await SendToContextAsync( context, error );
            }
        }

        /// <summary>
        /// Handle client disconnection.
        /// Cleans up session resources and logs disconnection.
        /// </summary>
        public async Task HandleDisconnectAsync ( MessageEnvelope message, ConnectionContext context )
        {
            ArgumentNullException.ThrowIfNull( message );
            ArgumentNullException.ThrowIfNull( context );

            try
            {
                Logging.Info( $"Client disconnected: {context.SessionId}" );
                // Session cleanup is handled by IPCServer connection manager
                await Task.CompletedTask;
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error handling Disconnect: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Handle command execution request from client.
        /// Routes command to EDDI message bus and sends response.
        /// </summary>
        public async Task HandleCommandAsync ( MessageEnvelope message, ConnectionContext context )
        {
            ArgumentNullException.ThrowIfNull( message );
            ArgumentNullException.ThrowIfNull( context );

            try
            {
                if ( message.Data is not CommandData cmdData )
                {
                    await SendCommandErrorResponseAsync( context.SessionId, message.Id, "Invalid command data format" );
                    return;
                }

                var commandName = cmdData.Command?.ToLowerInvariant() ?? "";
                Logging.Debug( $"Command received from {context.SessionId}: {commandName}" );

                // Handle special SetResponderMode command
                if ( commandName == "setrespondermode" )
                {
                    await HandleSetResponderModeAsync( cmdData, message.Id, context );
                    return;
                }

                try
                {
                    var commandDispatcher = CommandDispatcherRegistry.CommandDispatcher;
                    if ( commandDispatcher == null )
                    {
                        await SendCommandErrorResponseAsync( context.SessionId, message.Id,
                            "No command dispatcher is registered for IPC command routing." );
                        return;
                    }

                    await commandDispatcher.DispatchAsync( commandName, cmdData.Parameters ).ConfigureAwait( false );

                    var response = MessageEnvelope.Create( MessageTypes.CommandResponse,
                        new CommandResponseData
                        {
                            CommandId = message.Id,
                            Status = "success",
                            Message = $"Command '{commandName}' executed successfully"
                        } );
                    await SendToContextAsync( context, response );
                }
                catch ( Exception ex )
                {
                    Logging.Error( $"Command execution failed for '{commandName}': {ex.Message}", ex );
                    await SendCommandErrorResponseAsync( context.SessionId, message.Id, $"Command execution failed: {ex.Message}" );
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error handling Command: {ex.Message}", ex );
                await SendCommandErrorResponseAsync( context.SessionId, message.Id, $"Server error: {ex.Message}" );
            }
        }

        /// <summary>
        /// Handle SetResponderMode command to enable/disable VoiceAttackResponder.
        /// Delegates responder mode changes to a registered handler.
        /// </summary>
        private async Task HandleSetResponderModeAsync( CommandData cmdData, string messageId, ConnectionContext context )
        {
            try
            {
                var enable = false;
                if ( cmdData.Parameters?.TryGetValue( "enable", out var enableObj ) ?? false )
                {
                    enable = enableObj is bool b && b;
                }

                var handler = ResponderModeRegistry.Handler;
                if ( handler == null )
                {
                    await SendCommandErrorResponseAsync( context.SessionId, messageId,
                        "Responder mode handler is not registered." );
                    return;
                }

                await handler( enable, CancellationToken.None ).ConfigureAwait( false );
                Logging.Info( $"Responder mode {(enable ? "enabled" : "disabled")}" );

                var response = MessageEnvelope.Create( MessageTypes.CommandResponse,
                    new CommandResponseData
                    {
                        CommandId = messageId,
                        Status = "success",
                        Message = $"Responder mode {(enable ? "enabled" : "disabled")}"
                    } );
                await SendToContextAsync( context, response );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error handling SetResponderMode: {ex.Message}", ex );
                await SendCommandErrorResponseAsync( context.SessionId, messageId, $"SetResponderMode failed: {ex.Message}" );
            }
        }

        /// <summary>
        /// Handle unexpected Event message from client.
        /// Events should only come from server to clients; log and discard.
        /// </summary>
        public async Task HandleEventAsync ( MessageEnvelope message, ConnectionContext context )
        {
            ArgumentNullException.ThrowIfNull( message );
            ArgumentNullException.ThrowIfNull( context );

            try
            {
                Logging.Warn( $"Received unexpected Event message from client {context.SessionId}: {message.Id}" );
                // Events should not be sent by clients; simply acknowledge and ignore
                await Task.CompletedTask;
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error handling Event: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Broadcast an event to all connected clients.
        /// </summary>
        public async Task BroadcastEventAsync ( MessageEnvelope eventMessage )
        {
            ArgumentNullException.ThrowIfNull( eventMessage );

            try
            {
                await _server.BroadcastAsync( eventMessage );
                Logging.Debug( $"Event broadcast to {_server.ConnectionCount} clients" );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error broadcasting event: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Send a command response to a specific client.
        /// </summary>
        public async Task SendCommandResponseAsync ( string sessionId, MessageEnvelope response )
        {
            ArgumentNullException.ThrowIfNull( sessionId );
            ArgumentNullException.ThrowIfNull( response );

            try
            {
                await _server.SendToConnectionAsync( sessionId, response );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error sending command response to {sessionId}: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Helper: Send message to specific connection context.
        /// </summary>
        private async Task SendToContextAsync ( ConnectionContext context, MessageEnvelope message )
        {
            ArgumentNullException.ThrowIfNull( context );
            ArgumentNullException.ThrowIfNull( message );

            try
            {
                await _server.SendToConnectionAsync( context.SessionId, message );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error sending message to {context.SessionId}: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Helper: Send command error response.
        /// </summary>
        private async Task SendCommandErrorResponseAsync ( string sessionId, string commandId, string errorMessage )
        {
            var response = MessageEnvelope.Create( MessageTypes.CommandResponse,
                new CommandResponseData { CommandId = commandId, Status = "error", Message = errorMessage } );
            await SendCommandResponseAsync( sessionId, response );
        }
    }
}
