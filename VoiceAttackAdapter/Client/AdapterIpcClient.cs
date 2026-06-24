#nullable enable

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EddiVoiceAttackAdapter.Client
{
    internal sealed class AdapterIpcClient : IDisposable
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<AdapterMessageEnvelope>> _pendingRequests = new();
        private readonly List<long> _responseTimes = [];

        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _receiveLoopTask;
        private bool _disposed;
        private string? _sessionId;
        private string? _serverAddress;
        private int _serverPort;
        private DateTime _connectedAt;
        private long _messagesSent;
        private long _messagesReceived;
        private DateTime _lastActivityAt;

        public bool IsConnected { get; private set; }

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ConnectionLostEventArgs>? ConnectionLost;

        public async Task ConnectAsync ( string host, int port, CancellationToken cancellationToken = default )
        {
            if ( IsConnected )
            {
                throw new InvalidOperationException( "Client is already connected" );
            }

            _tcpClient = new TcpClient();
            try
            {
                using ( var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken ) )
                {
                    timeoutCts.CancelAfter( TimeSpan.FromSeconds( 10 ) );
                    await _tcpClient.ConnectAsync( host, port, timeoutCts.Token ).ConfigureAwait( false );
                }

                _networkStream = _tcpClient.GetStream();
                IsConnected = true;
                _serverAddress = host;
                _serverPort = port;
                _sessionId = Guid.NewGuid().ToString( "D" );
                _connectedAt = DateTime.UtcNow;
                _lastActivityAt = DateTime.UtcNow;
                _messagesSent = 0;
                _messagesReceived = 0;
                _responseTimes.Clear();

                await SendConnectMessageAsync( cancellationToken ).ConfigureAwait( false );

                _cancellationTokenSource = new CancellationTokenSource();
                _receiveLoopTask = Task.Run(
                    () => ReceiveLoopAsync( _cancellationTokenSource.Token ),
                    _cancellationTokenSource.Token );
            }
            catch
            {
                IsConnected = false;
                CleanupConnection();
                throw;
            }
        }

        public async Task DisconnectAsync ( CancellationToken cancellationToken = default )
        {
            if ( !IsConnected )
            {
                return;
            }

            try
            {
                if ( _tcpClient?.Connected ?? false )
                {
                    try
                    {
                        await SendDisconnectMessageAsync( cancellationToken ).ConfigureAwait( false );
                    }
                    catch
                    {
                        // Ignore errors when sending disconnect.
                    }
                }

                IsConnected = false;

                if ( _cancellationTokenSource != null )
                {
                    _cancellationTokenSource.Cancel();
                    try
                    {
                        if ( _receiveLoopTask != null )
                        {
                            await _receiveLoopTask.WaitAsync( TimeSpan.FromSeconds( 3 ), cancellationToken )
                                .ConfigureAwait( false );
                        }
                    }
                    catch
                    {
                        // Ignore receive-loop cleanup errors.
                    }
                }
            }
            finally
            {
                CleanupConnection();
            }
        }

        public async Task<object?> SendCommandAsync ( AdapterCommandData command, CancellationToken cancellationToken = default )
        {
            if ( !IsConnected )
            {
                throw new InvalidOperationException( "Client is not connected to IPC server" );
            }

            ArgumentNullException.ThrowIfNull( command );

            var requestId = Guid.NewGuid().ToString( "D" );
            var envelope = AdapterMessageEnvelope.Create( AdapterMessageTypes.Command, command, requestId );
            var tcs = new TaskCompletionSource<AdapterMessageEnvelope>( TaskCreationOptions.RunContinuationsAsynchronously );
            _pendingRequests.TryAdd( requestId, tcs );

            try
            {
                var stopwatch = Stopwatch.StartNew();
                await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
                linkedCts.CancelAfter( TimeSpan.FromSeconds( 30 ) );
                var responseTask = await Task.WhenAny(
                    tcs.Task,
                    Task.Delay( Timeout.Infinite, linkedCts.Token ) ).ConfigureAwait( false );

                if ( responseTask == tcs.Task )
                {
                    stopwatch.Stop();
                    _responseTimes.Add( stopwatch.ElapsedMilliseconds );
                    var response = await tcs.Task.ConfigureAwait( false );
                    return response.Data;
                }

                throw new OperationCanceledException( "Command timed out or was cancelled" );
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

        public async Task SendEventAsync ( AdapterEventData eventData, CancellationToken cancellationToken = default )
        {
            if ( !IsConnected )
            {
                throw new InvalidOperationException( "Client is not connected to IPC server" );
            }

            ArgumentNullException.ThrowIfNull( eventData );

            var envelope = AdapterMessageEnvelope.Create( AdapterMessageTypes.Event, eventData );
            await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );
        }

        public Task<ConnectionStatus> GetStatusAsync ()
        {
            return Task.FromResult( new ConnectionStatus
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
            } );
        }

        private async Task SendConnectMessageAsync ( CancellationToken cancellationToken )
        {
            var envelope = AdapterMessageEnvelope.Create(
                AdapterMessageTypes.Connect,
                new AdapterConnectData
                {
                    PluginVersion = "1.0.0",
                    PluginName = "VoiceAttack IPC Plugin",
                    Capabilities = AdapterServerCapabilities.AllCapabilities,
                    SupportedMessageTypes = AdapterServerCapabilities.AllMessageTypes
                } );

            await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );
        }

        private async Task SendDisconnectMessageAsync ( CancellationToken cancellationToken )
        {
            var envelope = AdapterMessageEnvelope.Create(
                AdapterMessageTypes.Disconnect,
                new AdapterDisconnectData { Reason = "user_disconnect" } );

            await SendMessageAsync( envelope, cancellationToken ).ConfigureAwait( false );
        }

        private async Task SendMessageAsync ( AdapterMessageEnvelope envelope, CancellationToken cancellationToken = default )
        {
            if ( _networkStream == null || !( _tcpClient?.Connected ?? false ) )
            {
                throw new InvalidOperationException( "Network stream is not available" );
            }

            try
            {
                var frame = IpcMessageSerializer.SerializeToFrame( envelope );
                await _networkStream.WriteAsync( frame.Memory, cancellationToken ).ConfigureAwait( false );
                await _networkStream.FlushAsync( cancellationToken ).ConfigureAwait( false );

                _messagesSent++;
                _lastActivityAt = DateTime.UtcNow;
            }
            catch
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

            var readBuffer = ArrayPool<byte>.Shared.Rent( 4096 );
            var pendingBuffer = ArrayPool<byte>.Shared.Rent( 8192 );
            var pendingStart = 0;
            var pendingLength = 0;

            try
            {
                while ( !cancellationToken.IsCancellationRequested && ( _tcpClient?.Connected ?? false ) )
                {
                    try
                    {
                        var bytesRead = await _networkStream
                            .ReadAsync( readBuffer.AsMemory( 0, 4096 ), cancellationToken )
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
                            var messageCount = IpcMessageSerializer.DeserializeMessages(
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
                        break;
                    }
                    catch ( ObjectDisposedException )
                    {
                        break;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return( readBuffer );
                ArrayPool<byte>.Shared.Return( pendingBuffer );

                IsConnected = false;
                ConnectionLost?.Invoke( this, new ConnectionLostEventArgs( "Receive loop ended" ) );
            }
        }

        private static byte[] AppendPendingBytes (
            byte[] pendingBuffer,
            ref int pendingStart,
            ref int pendingLength,
            byte[] sourceBuffer,
            int sourceLength )
        {
            var requiredLength = pendingLength + sourceLength;

            if ( requiredLength > pendingBuffer.Length )
            {
                var newSize = Math.Max( requiredLength, pendingBuffer.Length * 2 );
                var newBuffer = ArrayPool<byte>.Shared.Rent( newSize );

                if ( pendingLength > 0 )
                {
                    Buffer.BlockCopy( pendingBuffer, pendingStart, newBuffer, 0, pendingLength );
                }

                ArrayPool<byte>.Shared.Return( pendingBuffer );
                pendingBuffer = newBuffer;
                pendingStart = 0;
            }
            else if ( pendingStart + pendingLength + sourceLength > pendingBuffer.Length )
            {
                if ( pendingLength > 0 )
                {
                    Buffer.BlockCopy( pendingBuffer, pendingStart, pendingBuffer, 0, pendingLength );
                }

                pendingStart = 0;
            }

            Buffer.BlockCopy( sourceBuffer, 0, pendingBuffer, pendingStart + pendingLength, sourceLength );
            pendingLength += sourceLength;

            return pendingBuffer;
        }

        private void ProcessReceivedMessage ( AdapterMessageEnvelope envelope )
        {
            _messagesReceived++;
            _lastActivityAt = DateTime.UtcNow;

            if ( envelope.Type == AdapterMessageTypes.CommandResponse )
            {
                var pendingRequestId = envelope.Id;
                if ( TryGetPendingRequestId( envelope.Data, "CommandId", out var commandId ) )
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

            if ( envelope.Type == AdapterMessageTypes.Error &&
                 TryGetPendingRequestId( envelope.Data, "OriginalMessageId", out var originalMessageId ) &&
                 _pendingRequests.TryRemove( originalMessageId, out var errorCompletionSource ) )
            {
                errorCompletionSource.TrySetException( new InvalidOperationException( GetErrorMessage( envelope.Data ) ) );
                return;
            }

            MessageReceived?.Invoke( this, new MessageReceivedEventArgs( envelope.Type, envelope ) );
        }

        private static bool TryGetPendingRequestId ( object data, string propertyName, out string requestId )
        {
            requestId = string.Empty;

            switch ( data )
            {
                case JsonElement json:
                    requestId = TryGetJsonString( json, propertyName ) ?? string.Empty;
                    break;
                case IDictionary<string, object> dictionary when dictionary.TryGetValue( propertyName, out var value ):
                    requestId = value?.ToString() ?? string.Empty;
                    break;
            }

            return !string.IsNullOrWhiteSpace( requestId );
        }

        private static string GetErrorMessage ( object data )
        {
            return data switch
            {
                JsonElement json => TryGetJsonString( json, "Message" ) ?? "IPC server returned an error response.",
                IDictionary<string, object> dictionary when dictionary.TryGetValue( "Message", out var value ) =>
                    value?.ToString() ?? "IPC server returned an error response.",
                _ => "IPC server returned an error response."
            };
        }

        private static string? TryGetJsonString ( JsonElement json, string propertyName )
        {
            if ( json.ValueKind != JsonValueKind.Object )
            {
                return null;
            }

            foreach ( var property in json.EnumerateObject() )
            {
                if ( string.Equals( property.Name, propertyName, StringComparison.OrdinalIgnoreCase ) )
                {
                    return property.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : property.Value.ToString();
                }
            }

            return null;
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
                    DisconnectAsync().GetAwaiter().GetResult();
                }
            }
            catch
            {
                // Ignore disposal errors.
            }

            CleanupConnection();

            foreach ( var kvp in _pendingRequests )
            {
                kvp.Value.TrySetException( new ObjectDisposedException( nameof( AdapterIpcClient ) ) );
            }

            _pendingRequests.Clear();
            GC.SuppressFinalize( this );
            _disposed = true;
        }
    }
}
