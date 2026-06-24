#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EddiVoiceAttackAdapter.Client
{
    /// <summary>
    /// Integration layer between VoiceAttack plugin and IPC server.
    /// Handles plugin-level initialization, command/query/event dispatch through IPC client,
    /// and lifecycle management specific to VoiceAttack plugin mode.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="VoiceAttackPluginClient"/> class.
    /// </remarks>
    /// <param name="configFilePath">Path to the IPC configuration file containing the server port</param>
    public class VoiceAttackPluginClient ( string configFilePath ) : IDisposable
    {
        private readonly string _configFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
        private AdapterIpcClient? _ipcClient;
        private bool _disposed;
        private int _port;

        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ConnectionLostEventArgs>? ConnectionLost;

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string PluginName { get; } = "VoiceAttack IPC Plugin";

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        public string PluginVersion { get; } = "1.0.0";

        /// <summary>
        /// Gets a value indicating whether the plugin is connected to the IPC server.
        /// </summary>
        public bool IsConnected => _ipcClient?.IsConnected ?? false;

        /// <summary>
        /// Initializes the plugin client by reading the config file and connecting to the IPC server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the initialization operation</param>
        /// <returns>A task representing the asynchronous initialization operation</returns>
        /// <exception cref="FileNotFoundException">If the config file does not exist</exception>
        /// <exception cref="JsonException">If the config file contains invalid JSON</exception>
        /// <exception cref="Exception">If connection to the server fails</exception>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_configFilePath))
            {
                throw new FileNotFoundException($"IPC configuration file not found: {_configFilePath}");
            }

            try
            {
                // Read and parse config file
                var json = await File.ReadAllTextAsync(_configFilePath, cancellationToken ).ConfigureAwait(false);
                using (var doc = JsonDocument.Parse(json))
                {
                    if (!doc.RootElement.TryGetProperty("port", out var portElement))
                    {
                        throw new ArgumentException("Configuration file must contain 'port' property");
                    }

                    if (!portElement.TryGetInt32(out _port) || _port <= 0 || _port > 65535)
                    {
                        throw new ArgumentException($"Invalid port number: {portElement}");
                    }
                }

                // Create and initialize IPC client
                _ipcClient = new AdapterIpcClient();
                _ipcClient.MessageReceived += OnIpcClientMessageReceived;
                _ipcClient.ConnectionLost += OnIpcClientConnectionLost;
                await _ipcClient.ConnectAsync("127.0.0.1", _port, cancellationToken).ConfigureAwait(false);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (JsonException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _ipcClient?.Dispose();
                _ipcClient = null;
                throw new InvalidOperationException($"Failed to initialize plugin client: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a command through the IPC client to EDDI.
        /// </summary>
        /// <param name="commandName">The name/identifier of the command</param>
        /// <param name="parameters">The command parameters object</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A task representing the asynchronous operation with the command result</returns>
        /// <exception cref="InvalidOperationException">If the client is not connected</exception>
        /// <exception cref="ArgumentNullException">If commandName is null</exception>
        /// <exception cref="OperationCanceledException">If the operation is cancelled or times out</exception>
        public async Task<object?> SendCommandAsync(string commandName, object? parameters = null, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Plugin client is not connected to IPC server. Call InitializeAsync first.");
            }

            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentNullException(nameof(commandName));
            }

            var command = new AdapterCommandData
            {
                Command = commandName,
                Target = "va_plugin",
                Parameters = parameters as Dictionary<string, object> ?? new Dictionary<string, object>()
            };

            try
            {
                var response = await _ipcClient!.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Command '{commandName}' failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends an event through the IPC client to EDDI (fire-and-forget).
        /// </summary>
        /// <param name="eventName">The name/identifier of the event</param>
        /// <param name="payload">The event payload object</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="InvalidOperationException">If the client is not connected</exception>
        /// <exception cref="ArgumentNullException">If eventName is null</exception>
        /// <exception cref="OperationCanceledException">If the operation is cancelled or times out</exception>
        public async Task SendEventAsync(string eventName, object? payload = null, CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Plugin client is not connected to IPC server. Call InitializeAsync first.");
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            var eventData = new AdapterEventData
            {
                EventType = "plugin_event",
                EventName = eventName,
                EventPayload = payload as Dictionary<string, object> ?? new Dictionary<string, object>()
            };

            try
            {
                await _ipcClient!.SendEventAsync(eventData, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Event '{eventName}' failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the current status of the server connection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation with the connection status</returns>
        public async Task<ConnectionStatus> GetServerStatusAsync()
        {
            if (_ipcClient == null)
            {
                return new ConnectionStatus { IsConnected = false };
            }

            return await _ipcClient.GetStatusAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Disconnects from the IPC server gracefully.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the disconnection</param>
        /// <returns>A task representing the asynchronous disconnection operation</returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_ipcClient == null || !IsConnected)
            {
                return;
            }

            try
            {
                await _ipcClient.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Ignore disconnection errors
            }
        }

        /// <summary>
        /// Releases all resources used by the plugin client.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore disposal errors
            }

            if ( _ipcClient != null )
            {
                _ipcClient.MessageReceived -= OnIpcClientMessageReceived;
                _ipcClient.ConnectionLost -= OnIpcClientConnectionLost;
            }

            _ipcClient?.Dispose();
            _ipcClient = null;
            GC.SuppressFinalize( this );
            _disposed = true;
        }

        private void OnIpcClientMessageReceived( object? sender, MessageReceivedEventArgs e )
        {
            MessageReceived?.Invoke( this, e );
        }

        private void OnIpcClientConnectionLost( object? sender, ConnectionLostEventArgs e )
        {
            ConnectionLost?.Invoke( this, e );
        }
    }
}
