#nullable enable
using EddiIPC_Service.Messages;
using EddiIPC_Service.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EddiIPC_Service.Client
{
    /// <summary>
    /// Concrete implementation of IIPCClient for plugin-to-EDDI communication over TCP IPC.
    /// Handles connection management, message serialization, and request/response correlation.
    /// </summary>
    public class IPCClient : IIPCClient
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveLoopTask;
        private bool _disposed;

        // Request/response correlation tracking
        private readonly ConcurrentDictionary<string, TaskCompletionSource<MessageEnvelope>> _pendingRequests = new();

        // Connection metadata
        private string? _sessionId;
        private string? _serverAddress;
        private int _serverPort;
        private DateTime _connectedAt;
        private long _messagesSent;
        private long _messagesReceived;
        private DateTime _lastActivityAt;
        private readonly List<long> _responseTimes = [ ];

        public bool IsConnected { get; private set; }

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ConnectionLostEventArgs>? ConnectionLost;

        /// <summary>
        /// Connects to the IPC server at the specified host and port.
        /// </summary>
        public async Task ConnectAsync ( string host, int port, CancellationToken cancellationToken = default )
        {
            if ( IsConnected )
            {
                throw new InvalidOperationException( "Client is already connected" );
            }

            _tcpClient = new TcpClient();
            try
            {
                // Connect with timeout
                using ( var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) )
                {
                    timeoutCts.CancelAfter( TimeSpan.FromSeconds( 10 ) );
                    await _tcpClient.ConnectAsync( host, port, timeoutCts.Token ).ConfigureAwait( false );
                }

                _networkStream = _tcpClient.GetStream();
                IsConnected = true;
                _serverAddress = host;
                _serverPort = port;
                _sessionId = Guid.NewGuid().ToString();
                _connectedAt = DateTime.UtcNow;
                _lastActivityAt = DateTime.UtcNow;
                _messagesSent = 0;
                _messagesReceived = 0;
                _responseTimes.Clear();

                // Send Connect message
                await SendConnectMessageAsync().ConfigureAwait( false );

                // Start receive loop
                _cancellationTokenSource = new CancellationTokenSource();
                _receiveLoopTask = Task.Run( () => ReceiveLoopAsync( _cancellationTokenSource.Token ),
                    _cancellationTokenSource.Token );
            }
            catch ( Exception )
            {
                IsConnected = false;
                _tcpClient?.Dispose();
                _tcpClient = null;
                _networkStream?.Dispose();
                _networkStream = null;
                throw;
            }
        }

        /// <summary>
        /// Disconnects from the IPC server gracefully.
        /// </summary>
        public async Task DisconnectAsync ( CancellationToken cancellationToken = default )
        {
            if ( !IsConnected )
            {
                return;
            }

            try
            {
                // Send Disconnect message
                if ( _tcpClient?.Connected ?? false )
                {
                    try
                    {
                        await SendDisconnectMessageAsync( cancellationToken ).ConfigureAwait( false );
                    }
                    catch
                    {
                        // Ignore errors when sending disconnect
                    }
                }

                IsConnected = false;

                // Stop receive loop
                if ( _cancellationTokenSource != null )
                {
                    _cancellationTokenSource.Cancel();
                    try
                    {
                        if ( _receiveLoopTask != null )
                        {
                            await _receiveLoopTask.WaitAsync( TimeSpan.FromSeconds( 3 ), cancellationToken ).ConfigureAwait( false );
                        }
                    }
                    catch
                    {
                        // Ignore errors in receive loop cleanup
                    }
                }
            }
            finally
            {
                CleanupConnection();
            }
        }

        /// <summary>
        /// Sends a command to EDDI and waits for the response.
        /// </summary>
        public async Task<TResponse> SendCommandAsync<TResponse> ( CommandData command,
            CancellationToken cancellationToken = default )
            where TResponse : class
        {
            if ( !IsConnected )
            {
                throw new InvalidOperationException( "Client is not connected to IPC server" );
            }
            ArgumentNullException.ThrowIfNull( command );

            var requestId = Guid.NewGuid().ToString( "D" );
            var envelope = MessageEnvelope.Create( MessageTypes.Command, command, requestId );

            var tcs = new TaskCompletionSource<MessageEnvelope>();
            _pendingRequests.TryAdd( requestId, tcs );

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Send command
                await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );

                // Wait for response
                using ( var linkedCts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) )
                {
                    linkedCts.CancelAfter( TimeSpan.FromSeconds( 30 ) ); // 30-second default timeout
                    var responseTask = await Task.WhenAny(
                        tcs.Task,
                        Task.Delay( Timeout.Infinite, linkedCts.Token )
                    ).ConfigureAwait( false );

                    if ( responseTask == tcs.Task )
                    {
                        stopwatch.Stop();
                        _responseTimes.Add( stopwatch.ElapsedMilliseconds );
                        var response = await tcs.Task.ConfigureAwait( false );
                        return response.Data as TResponse ??
                               throw new InvalidOperationException( "Invalid response type" );
                    }

                    throw new OperationCanceledException( "Command timed out or was cancelled" );
                }
            }
            catch ( OperationCanceledException ) when ( cancellationToken.IsCancellationRequested )
            {
                throw;
            }
            catch ( Exception ex )
            {
                throw new InvalidOperationException( "Failed to send command", ex );
            }
            finally
            {
                _pendingRequests.TryRemove( requestId, out _ );
            }
        }

        /// <summary>
        /// Sends an event to EDDI (fire-and-forget).
        /// </summary>
        public async Task SendEventAsync ( EventData eventData, CancellationToken cancellationToken = default )
        {
            if ( !IsConnected )
            {
                throw new InvalidOperationException( "Client is not connected to IPC server" );
            }
            ArgumentNullException.ThrowIfNull( eventData );

            var envelope = MessageEnvelope.Create( MessageTypes.Event, eventData );

            try
            {
                await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                throw new InvalidOperationException( "Failed to send event", ex );
            }
        }

        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        public async Task<ConnectionStatus> GetStatusAsync ()
        {
            return await Task.FromResult( new ConnectionStatus
            {
                IsConnected = IsConnected,
                ServerAddress = _serverAddress,
                ServerPort = _serverPort,
                SessionId = _sessionId,
                ConnectedAt = IsConnected ? _connectedAt : null,
                MessagesSent = _messagesSent,
                MessagesReceived = _messagesReceived,
                LastActivityAt = _lastActivityAt,
                AverageResponseTimeMs = _responseTimes.Count > 0 ? _responseTimes.Average() : 0
            } ).ConfigureAwait( false );
        }

        #region Private Methods

        private async Task SendConnectMessageAsync ()
        {
            var envelope = MessageEnvelope.Create(
                MessageTypes.Connect,
                new ConnectData
                {
                    PluginVersion = "1.0.0",
                    PluginName = "VoiceAttack IPC Plugin",
                    Capabilities = ServerCapabilities.AllCapabilities,
                    SupportedMessageTypes = ServerCapabilities.AllMessageTypes
                }
            );

            await SendMessageAsync( envelope ).ConfigureAwait( false );
        }

        private async Task SendDisconnectMessageAsync ( CancellationToken cancellationToken = default )
        {
            var envelope = MessageEnvelope.Create(
                MessageTypes.Disconnect,
                new DisconnectData { Reason = "user_disconnect" }
            );

            await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );
        }

        private async Task SendMessageAsync ( MessageEnvelope envelope, CancellationToken cancellationToken = default )
        {
            if ( _networkStream == null || !( _tcpClient?.Connected ?? false ) )
            {
                throw new InvalidOperationException( "Network stream is not available" );
            }

            try
            {
                var serialized = MessageSerializer.Serialize( envelope );
                // Use ArrayPool to reduce allocations
                byte[]? rentedBuffer = null;
                try
                {
                    rentedBuffer = ArrayPool<byte>.Shared.Rent( serialized.Length * 2 );
                    var bytesWritten = Encoding.UTF8.GetBytes( serialized, 0, serialized.Length, rentedBuffer, 0 );

                    // Write data (includes length prefix and JSON)
                    await _networkStream.WriteAsync( rentedBuffer, 0, bytesWritten, cancellationToken ).ConfigureAwait( false );
                    await _networkStream.FlushAsync( cancellationToken ).ConfigureAwait( false );

                    _messagesSent++;
                    _lastActivityAt = DateTime.UtcNow;
                }
                finally
                {
                    if ( rentedBuffer != null )
                    {
                        ArrayPool<byte>.Shared.Return( rentedBuffer );
                    }
                }
            }
            catch ( Exception )
            {
                IsConnected = false;
                throw;
            }
        }

        private async Task ReceiveLoopAsync ( CancellationToken cancellationToken )
        {
            if ( _networkStream == null )
            {
                return;
            }

            var rentedBuffer = ArrayPool<byte>.Shared.Rent( 4096 );
            var bufferedBytes = new List<byte>( 8192 ); // Pre-allocate with typical message capacity
            try
            {
                while ( !cancellationToken.IsCancellationRequested && ( _tcpClient?.Connected ?? false ) )
                {
                    try
                    {
                        var bytesRead = await _networkStream
                            .ReadAsync( rentedBuffer, 0, 4096, cancellationToken )
                            .ConfigureAwait( false );

                        if ( bytesRead == 0 )
                        {
                            // Connection closed by server
                            break;
                        }

                        // More efficient: directly extend the list instead of ToArray() + AddRange()
                        bufferedBytes.AddRange( rentedBuffer.AsSpan( 0, bytesRead ) );

                        while ( bufferedBytes.Count > 0 )
                        {
                            var messageCount = MessageSerializer.DeserializeMessages(
                                CollectionsMarshal.AsSpan( bufferedBytes ), out var messages, out var bytesConsumed );

                            if ( bytesConsumed == 0 )
                            {
                                break;
                            }

                            bufferedBytes.RemoveRange( 0, bytesConsumed );

                            if ( messageCount == 0 )
                            {
                                continue;
                            }

                            foreach ( var message in messages )
                            {
                                ProcessReceivedMessage( message );
                            }
                        }
                    }
                    catch ( OperationCanceledException )
                    {
                        break;
                    }
                    catch ( IOException )
                    {
                        // Connection lost
                        break;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( rentedBuffer );
                IsConnected = false;
                ConnectionLost?.Invoke( this, new ConnectionLostEventArgs( "Receive loop ended" ) );
            }
        }

        private void ProcessReceivedMessage ( MessageEnvelope envelope )
        {
            _messagesReceived++;
            _lastActivityAt = DateTime.UtcNow;

            // Handle response messages
            if ( envelope.Type == MessageTypes.CommandResponse )
            {
                var pendingRequestId = envelope.Id;
                if ( TryGetPendingRequestId( envelope.Data, nameof( CommandResponseData.CommandId ), out var commandId ) )
                {
                    pendingRequestId = commandId;
                }

                if ( !string.IsNullOrWhiteSpace( pendingRequestId ) &&
                     _pendingRequests.TryRemove( pendingRequestId, out var responseCompletionSource ) )
                {
                    responseCompletionSource.TrySetResult( envelope );
                    return;
                }
            }

            if ( envelope.Type == MessageTypes.Error &&
                 TryGetPendingRequestId( envelope.Data, nameof( ErrorData.OriginalMessageId ), out var originalMessageId ) &&
                 _pendingRequests.TryRemove( originalMessageId, out var errorCompletionSource ) )
            {
                errorCompletionSource.TrySetException( new InvalidOperationException( GetErrorMessage( envelope.Data ) ) );
                return;
            }

            // Raise event for other messages
            MessageReceived?.Invoke( this, new MessageReceivedEventArgs( envelope.Type, envelope ) );
        }

        private static bool TryGetPendingRequestId ( object data, string propertyName, out string requestId )
        {
            requestId = string.Empty;

            switch ( data )
            {
                case CommandResponseData commandResponseData when propertyName == nameof( CommandResponseData.CommandId ):
                    requestId = commandResponseData.CommandId;
                    break;
                case ErrorData errorData when propertyName == nameof( ErrorData.OriginalMessageId ):
                    requestId = errorData.OriginalMessageId;
                    break;
                case JObject json when json.TryGetValue( propertyName, StringComparison.OrdinalIgnoreCase, out var token ):
                    requestId = token.Value<string>() ?? string.Empty;
                    break;
                case IDictionary<string, object> dictionary when dictionary.TryGetValue( propertyName, out var value ):
                    requestId = value.ToString() ?? string.Empty;
                    break;
            }

            return !string.IsNullOrWhiteSpace( requestId );
        }

        private static string GetErrorMessage ( object data )
        {
            return data switch
            {
                ErrorData errorData when !string.IsNullOrWhiteSpace( errorData.Message ) => errorData.Message,
                JObject json when json.TryGetValue( nameof( ErrorData.Message ), StringComparison.OrdinalIgnoreCase, out var token ) =>
                    token.Value<string>() ?? "IPC server returned an error response.",
                IDictionary<string, object> dictionary when dictionary.TryGetValue( nameof( ErrorData.Message ), out var value ) =>
                    value.ToString() ?? "IPC server returned an error response.",
                _ => "IPC server returned an error response."
            };
        }

        private void CleanupConnection ()
        {
            _networkStream?.Dispose();
            _networkStream = null;

            _tcpClient?.Dispose();
            _tcpClient = null;

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        public void Dispose ()
        {
            if ( _disposed )
            {
                return;
            }

            try
            {
                if ( IsConnected )
                {
                    DisconnectAsync().GetResultOrTimeout( TimeSpan.FromSeconds(2) );
                }
            }
            catch
            {
                // Ignore disposal errors
            }

            CleanupConnection();

            // Fail all pending requests
            foreach ( var kvp in _pendingRequests )
            {
                kvp.Value.TrySetException( new ObjectDisposedException( "IPCClient" ) );
            }

            _pendingRequests.Clear();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        #endregion
    }
}
