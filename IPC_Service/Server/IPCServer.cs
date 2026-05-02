#nullable enable

using EddiIPC_Service.Messages;
using EddiIPC_Service.Messaging;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiIPC_Service.Server
{
    /// <summary>
    /// IPC Server using TCP sockets with length-prefixed JSON messages.
    /// Manages client connections, message I/O, and routing to handlers.
    /// Designed for low-latency AppDomain communication.
    /// </summary>
    public class IPCServer : IDisposable
    {
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _acceptConnectionsTask;
        private readonly ConcurrentDictionary<string, ConnectionContext> _connections = [];
        private DefaultServerEventHandler? _ipcHandler;
        private IDisposable? _runtimeEventDispatcherRegistration;

        /// <summary>Server port (auto-selected from range 12345-12450)</summary>
        public int Port { get; private set; }

        /// <summary>Is server currently running and listening</summary>
        public bool IsRunning { get; private set; }

        /// <summary>Number of connected clients</summary>
        public int ConnectionCount => _connections.Count;

        /// <summary>
        /// Create a new IPC server instance.
        /// </summary>
        public IPCServer ()
        {
            Router = new MessageRouter();
            Port = 0;
            IsRunning = false;
        }

        /// <summary>
        /// Get the message router for registering handlers.
        /// </summary>
        public MessageRouter Router { get; }

        /// <summary>
        /// Initialize IPC server for plugin mode communication.
        /// </summary>
        public void InitializeIpcServer ()
        {
            _ipcHandler = new DefaultServerEventHandler( this );
            RegisterRuntimeEventDispatcher();

            // Register all message handlers
            Router.RegisterHandler( MessageTypes.Connect, _ipcHandler.HandleConnectAsync );
            Router.RegisterHandler( MessageTypes.Disconnect, _ipcHandler.HandleDisconnectAsync );
            Router.RegisterHandler( MessageTypes.Command, _ipcHandler.HandleCommandAsync );
            Router.RegisterHandler( MessageTypes.Event, _ipcHandler.HandleEventAsync );

            // Start server (blocking call acceptable during initialization)
            StartAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Start the server on first available port in range 12345-12450.
        /// Runs asynchronously to accept connections.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for shutdown</param>
        public async Task StartAsync ( CancellationToken cancellationToken = default )
        {
            if ( IsRunning )
            {
                Logging.Warn( "IPCServer is already running" );
                return;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                ( _listener, Port ) = StartListener( 12345, 12450 );
                IsRunning = true;
                Logging.Info( $"Starting IPC server on port {Port}" );

                WritePortToConfig( Port );

                // Start background task to accept connections with linked cancellation token
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
                _acceptConnectionsTask = AcceptConnectionsAsync( _cancellationTokenSource.Token );

                // Ensure task is scheduled before returning
                await Task.Yield();

                Logging.Info( $"IPC Server started successfully on port {Port}" );
            }
            catch ( OperationCanceledException )
            {
                Logging.Info( "IPC Server startup cancelled" );
                IsRunning = false;
                throw;
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Failed to start IPC Server: {ex.Message}", ex );
                IsRunning = false;
                throw;
            }
        }

        /// <summary>
        /// Stop the server gracefully, closing all client connections.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for timeout</param>
        public async Task StopAsync ( CancellationToken cancellationToken = default )
        {
            if ( !IsRunning )
            {
                Logging.Warn( "IPCServer is not running" );
                return;
            }

            try
            {
                Logging.Info( "Stopping IPC server" );

                cancellationToken.ThrowIfCancellationRequested();

                // Signal cancellation
                _cancellationTokenSource?.Cancel();

                // Wait for accept task to complete (with timeout)
                if ( _acceptConnectionsTask != null )
                {
                    try
                    {
                        await _acceptConnectionsTask.WaitAsync( TimeSpan.FromSeconds( 5 ), cancellationToken ).ConfigureAwait( false );
                    }
                    catch ( TimeoutException )
                    {
                        Logging.Warn( "Server shutdown timeout during accept task" );
                    }
                    catch ( OperationCanceledException )
                    {
                        // Expected during normal cancellation
                    }
                }

                // Close all client connections
                await CloseAllConnectionsAsync();

                // Dispose listener
                _listener?.Stop();
                _listener?.Dispose();

                _runtimeEventDispatcherRegistration?.Dispose();
                _runtimeEventDispatcherRegistration = null;

                IsRunning = false;
                Logging.Info( "IPC Server stopped successfully" );
            }
            catch ( OperationCanceledException )
            {
                Logging.Info( "IPC Server stop cancelled" );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error stopping IPC Server: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Send a message to a specific client connection.
        /// </summary>
        /// <param name="sessionId">Target session ID</param>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        public async Task SendToConnectionAsync ( string sessionId, MessageEnvelope message,
            CancellationToken cancellationToken = default )
        {
            ArgumentNullException.ThrowIfNull( sessionId );
            ArgumentNullException.ThrowIfNull( message );

            _connections.TryGetValue( sessionId, out var context );
            if ( context is null )
            {
                Logging.Warn( $"Connection not found for session {sessionId}" );
                return;
            }

            if ( context.Stream is null )
            {
                Logging.Warn( $"No stream available for session {sessionId}" );
                return;
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var frame = MessageSerializer.SerializeToFrame( message );

                await context.Stream
                    .WriteAsync( frame.Memory, cancellationToken )
                    .ConfigureAwait( false );

                await context.Stream
                    .FlushAsync( cancellationToken )
                    .ConfigureAwait( false );

                Logging.Debug( $"Sent {frame.MessageType} message to session {sessionId}: {frame.Length} bytes." );
            }
            catch ( OperationCanceledException )
            {
                Logging.Debug( $"Send operation cancelled for session {sessionId}" );
                throw;
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error sending message to session {sessionId}: {ex.Message}", ex );
                await DisconnectAsync( sessionId, "send_error" );
            }
        }

        /// <summary>
        /// Broadcast a message to all connected clients.
        /// </summary>
        /// <param name="frame">Message to broadcast, in bytes</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        public async Task<bool> BroadcastAsync ( SerializedMessageFrame frame, CancellationToken cancellationToken = default )
        {
            ArgumentNullException.ThrowIfNull( frame );

            if ( !IsRunning )
            {
                Logging.Debug( "Broadcast skipped because IPC server is not running." );
                return false;
            }

            var sessionIds = _connections.Keys.ToList();

            if ( sessionIds.Count == 0 )
            {
                Logging.Debug( $"Broadcast skipped for {frame.MessageType}: no connected IPC sessions." );
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            var tasks = sessionIds
                .Select( sessionId => SendToConnectionAsyncWithFrame(
                    sessionId,
                    frame,
                    cancellationToken ) )
                .ToArray();

            var results = await Task.WhenAll( tasks ).ConfigureAwait( false );
            return results.Any( delivered => delivered );
        }

        private async Task<bool> SendToConnectionAsyncWithFrame (
            string sessionId,
            SerializedMessageFrame frame,
            CancellationToken cancellationToken )
        {
            if ( !_connections.TryGetValue( sessionId, out var context ) ||
                 context.Stream == null )
            {
                return false;
            }

            try
            {
                await context.Stream
                    .WriteAsync( frame.Memory, cancellationToken )
                    .ConfigureAwait( false );

                await context.Stream
                    .FlushAsync( cancellationToken )
                    .ConfigureAwait( false );

                Logging.Debug(
                    $"Sent {frame.MessageType} message to session {sessionId}: {frame.Length} bytes." );

                return true;
            }
            catch ( Exception ex )
            {
                Logging.Warn(
                    $"Failed to send {frame.MessageType} message to session {sessionId}: {ex.Message}" );

                try
                {
                    await DisconnectAsync( sessionId ).ConfigureAwait( false );
                }
                catch ( Exception disconnectEx )
                {
                    Logging.Debug(
                        $"Error disconnecting failed IPC session {sessionId}: {disconnectEx.Message}" );
                }

                return false;
            }
        }

        /// <summary>
        /// Disconnect a client connection gracefully.
        /// </summary>
        public async Task DisconnectAsync ( string sessionId, string reason = "client_disconnect" )
        {
            ArgumentNullException.ThrowIfNull( sessionId );

            _connections.Remove( sessionId, out var context );
            if ( context?.Stream is null )
            {
                return;
            }

            try
            {
                // Send disconnect message
                var disconnect = MessageEnvelope.Create( "Disconnect",
                    new DisconnectData { Reason = reason, Message = "Server closing connection" } );

                try
                {
                    var serialized = MessageSerializer.Serialize( disconnect );
                    var bytes = Encoding.UTF8.GetBytes( serialized );
                    await context.Stream.WriteAsync( bytes.AsMemory( 0, bytes.Length ) ).ConfigureAwait( false );
                    await context.Stream.FlushAsync().ConfigureAwait( false );
                }
                catch
                {
                    // Ignore if send fails
                }

                context.Dispose();
                Logging.Info( $"Disconnected session {sessionId} ({reason})" );
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Error disconnecting session {sessionId}: {ex.Message}", ex );
            }
        }

        /// <summary>
        /// Find the first available TCP port in the given range.
        /// </summary>
        private static (TcpListener listener, int port) StartListener ( int startPort, int endPort )
        {
            for ( var port = startPort; port <= endPort; port++ )
            {
                TcpListener? listener = null;
                try
                {
                    listener = new TcpListener( IPAddress.Loopback, port );
                    listener.Start();
                    return ( listener, port );
                }
                catch ( SocketException )
                {
                    listener?.Stop();
                    listener?.Dispose();
                }
            }

            throw new InvalidOperationException( $"No available ports in range {startPort}-{endPort}" );
        }

        internal void RegisterRuntimeEventDispatcher ()
        {
            _runtimeEventDispatcherRegistration?.Dispose();

            _runtimeEventDispatcherRegistration = RuntimeEventDispatcher.RegisterDispatcher(
                async ( eventData, cancellationToken ) =>
                {
                    var message = MessageEnvelope.Create( MessageTypes.Event, eventData );
                    var frame = MessageSerializer.SerializeToFrame( message );

                    return await BroadcastAsync( frame, cancellationToken )
                        .ConfigureAwait( false );
                } );
        }

        /// <summary>
        /// Write the server port to ipc_config.json for plugin discovery.
        /// </summary>
        private static void WritePortToConfig ( int port )
        {
            try
            {
                var configPath = Path.Combine(
                    Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                    Constants.EDDI_NAME,
                    "ipc_config.json" );

                var directory = Path.GetDirectoryName( configPath );
                if ( directory != null && !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                var json = $"{{\"port\":{port}}}";
                File.WriteAllText( configPath, json, Encoding.UTF8 );

                Logging.Debug( $"IPC port configuration written to {configPath}" );
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to write IPC config: {ex.Message}" );
            }
        }

        /// <summary>
        /// Background task to accept incoming client connections.
        /// </summary>
        private async Task AcceptConnectionsAsync ( CancellationToken cancellationToken )
        {
            while ( !cancellationToken.IsCancellationRequested )
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync( cancellationToken ).ConfigureAwait( false );
                    _ = HandleClientAsync( client, cancellationToken );
                }
                catch ( OperationCanceledException )
                {
                    // Expected during shutdown
                    break;
                }
                catch ( Exception ex )
                {
                    Logging.Error( $"Error accepting connection: {ex.Message}", ex );
                }
            }
        }

        /// <summary>
        /// Handle a connected client - receive messages and route to handlers.
        /// </summary>
        private async Task HandleClientAsync (
            TcpClient client,
            CancellationToken cancellationToken )
        {
            var context = new ConnectionContext( client );
            _connections[ context.SessionId ] = context;

            Logging.Info( $"Client connected: session {context.SessionId}" );

            byte[]? readBuffer = null;
            byte[]? pendingBuffer = null;

            var pendingStart = 0;
            var pendingLength = 0;

            try
            {
                readBuffer = ArrayPool<byte>.Shared.Rent( 65536 );
                pendingBuffer = ArrayPool<byte>.Shared.Rent( 131072 );

                while ( !cancellationToken.IsCancellationRequested && context.IsConnected )
                {
                    try
                    {
                        ArgumentNullException.ThrowIfNull(
                            context.Stream,
                            "Client stream is null" );

                        var bytesRead = await context.Stream
                    .ReadAsync(
                        readBuffer.AsMemory( 0, 65536 ),
                        cancellationToken )
                    .ConfigureAwait( false );

                        if ( bytesRead == 0 )
                        {
                            break;
                        }

                        pendingBuffer = AppendPendingBytes(
                            pendingBuffer,
                            ref pendingStart,
                            ref pendingLength,
                            readBuffer,
                            bytesRead );

                        while ( pendingLength > 0 )
                        {
                            var messageCount = MessageSerializer.DeserializeMessages(
                        pendingBuffer,
                        pendingStart,
                        pendingLength,
                        out var messages,
                        out var bytesConsumed );

                            if ( bytesConsumed == 0 )
                            {
                                break;
                            }

                            pendingStart += bytesConsumed;
                            pendingLength -= bytesConsumed;

                            if ( pendingLength == 0 )
                            {
                                pendingStart = 0;
                            }

                            if ( messageCount == 0 )
                            {
                                continue;
                            }

                            foreach ( var message in messages )
                            {
                                try
                                {
                                    message.Validate();

                                    await Router
                                        .RouteAsync( message, context )
                                        .ConfigureAwait( false );
                                }
                                catch ( Exception ex )
                                {
                                    Logging.Error(
                                        $"Error processing message: {ex.Message}",
                                        ex );

                                    var errorMsg = MessageEnvelope.Create(
                                "Error",
                                new ErrorData
                                {
                                    ErrorCode = "PROCESSING_ERROR",
                                    Message = ex.Message,
                                    OriginalMessageId = message.Id
                                } );

                                    await SendToConnectionAsync(
                                            context.SessionId,
                                            errorMsg,
                                            cancellationToken )
                                        .ConfigureAwait( false );
                                }
                            }
                        }
                    }
                    catch ( OperationCanceledException )
                    {
                        break;
                    }
                    catch ( IOException )
                    {
                        break;
                    }
                }
            }
            finally
            {
                if ( readBuffer != null )
                {
                    ArrayPool<byte>.Shared.Return( readBuffer );
                }

                if ( pendingBuffer != null )
                {
                    ArrayPool<byte>.Shared.Return( pendingBuffer );
                }

                _connections.Remove( context.SessionId, out _ );
                context.Dispose();

                Logging.Info( $"Client disconnected: session {context.SessionId}" );
            }
        }

        private static byte[] AppendPendingBytes (
            byte[] pendingBuffer,
            ref int pendingStart,
            ref int pendingLength,
            byte[] sourceBuffer,
            int sourceLength )
        {
            if ( sourceLength <= 0 )
            {
                return pendingBuffer;
            }

            var requiredLength = pendingLength + sourceLength;

            if ( requiredLength > pendingBuffer.Length )
            {
                var newSize = Math.Max( requiredLength, pendingBuffer.Length * 2 );
                var newBuffer = ArrayPool<byte>.Shared.Rent( newSize );

                if ( pendingLength > 0 )
                {
                    Buffer.BlockCopy(
                        pendingBuffer,
                        pendingStart,
                        newBuffer,
                        0,
                        pendingLength );
                }

                ArrayPool<byte>.Shared.Return( pendingBuffer );

                pendingBuffer = newBuffer;
                pendingStart = 0;
            }
            else if ( pendingStart + pendingLength + sourceLength > pendingBuffer.Length )
            {
                if ( pendingLength > 0 )
                {
                    Buffer.BlockCopy(
                        pendingBuffer,
                        pendingStart,
                        pendingBuffer,
                        0,
                        pendingLength );
                }

                pendingStart = 0;
            }

            Buffer.BlockCopy(
                sourceBuffer,
                0,
                pendingBuffer,
                pendingStart + pendingLength,
                sourceLength );

            pendingLength += sourceLength;

            return pendingBuffer;
        }

        /// <summary>
        /// Close all client connections gracefully.
        /// </summary>
        private async Task CloseAllConnectionsAsync ()
        {
            var sessionIds = _connections.Keys.ToList();

            var tasks = sessionIds.Select( id => DisconnectAsync( id, "server_shutdown" ) );
            await Task.WhenAll( tasks ).ConfigureAwait( false );
        }

        /// <summary>
        /// Dispose of server resources.
        /// </summary>
        public void Dispose ()
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during cleanup
            }

            _cancellationTokenSource?.Dispose();
            GC.SuppressFinalize( this );
        }
    }
}
