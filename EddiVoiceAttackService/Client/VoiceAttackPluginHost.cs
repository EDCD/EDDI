#nullable enable

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackService.Client
{
    /// <summary>
    /// Manages the lifecycle of the VoiceAttack plugin's IPC client connection.
    /// Provides singleton access to the plugin client with automatic initialization
    /// and error recovery.
    /// </summary>
    public class VoiceAttackPluginHost
    {
        private static VoiceAttackPluginHost? _instance;
        private static readonly object _instanceLock = new();

        private VoiceAttackPluginClient? _pluginClient;
        private bool _initialized;
        private bool _disposed;

        /// <summary>
        /// Prevents direct instantiation. Use the <see cref="Instance"/> property instead.
        /// </summary>
        private VoiceAttackPluginHost()
        { }

        /// <summary>
        /// Gets the singleton instance of the plugin host.
        /// </summary>
        public static VoiceAttackPluginHost Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        _instance ??= new VoiceAttackPluginHost();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the plugin client if connected; otherwise returns null.
        /// </summary>
        public VoiceAttackPluginClient? Client => _pluginClient?.IsConnected ?? false ? _pluginClient : null;

        /// <summary>
        /// Gets a value indicating whether the host has attempted initialization.
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Initializes the plugin client by reading the IPC configuration file
        /// and establishing connection to the IPC server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the initialization operation</param>
        /// <returns>A task representing the asynchronous initialization operation</returns>
        /// <remarks>
        /// If initialization fails, logs the error but does not throw. This allows
        /// the plugin to continue operating with reduced functionality.
        /// </remarks>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            try
            {
                var configPath = DetermineConfigFilePath();
                
                if (!File.Exists(configPath))
                {
                    Logging.Warn($"IPC configuration file not found at {configPath}. Plugin will operate without IPC connectivity.");
                    return;
                }

                _pluginClient = new VoiceAttackPluginClient(configPath);
                await _pluginClient.InitializeAsync(cancellationToken).ConfigureAwait(false);
                
                Logging.Info("VoiceAttack plugin IPC client initialized successfully");
            }
            catch (OperationCanceledException)
            {
                Logging.Warn("VoiceAttack plugin IPC client initialization was cancelled");
            }
            catch (Exception ex)
            {
                Logging.Warn($"Failed to initialize VoiceAttack plugin IPC client: {ex.Message}");
                _pluginClient?.Dispose();
                _pluginClient = null;
            }
        }

        /// <summary>
        /// Gracefully disconnects the plugin client from the IPC server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for the disconnection</param>
        /// <returns>A task representing the asynchronous disconnection operation</returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (_pluginClient == null || !_pluginClient.IsConnected)
            {
                return;
            }

            try
            {
                await _pluginClient.DisconnectAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Warn($"Error disconnecting VoiceAttack plugin IPC client: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends a responder mode handshake to the EDDI.exe server.
        /// This tells EDDI to set FromVA flag and activate the VoiceAttackResponder.
        /// </summary>
        /// <param name="enable">true to enable responder mode; false to disable</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A task representing the asynchronous operation; true if successful</returns>
        public async Task<bool> SendSetResponderModeAsync(bool enable, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_pluginClient == null || !_pluginClient.IsConnected)
                {
                    Logging.Warn("Cannot send responder mode - IPC client not connected");
                    return false;
                }

                var parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "enable", enable }
                };

                await _pluginClient.SendCommandAsync("setrespondermode", parameters, cancellationToken).ConfigureAwait(false);
                Logging.Info($"Sent SetResponderMode({enable}) handshake to EDDI.exe");
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn($"Failed to send SetResponderMode: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Releases all resources used by the plugin host.
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

            _pluginClient?.Dispose();
            _pluginClient = null;

            _disposed = true;
        }

        /// <summary>
        /// Determines the path to the IPC configuration file.
        /// First checks the EDDI application data directory, then checks the plugin directory.
        /// </summary>
        /// <returns>The path to the IPC configuration file</returns>
        private static string DetermineConfigFilePath()
        {
            // Check EDDI application data directory first
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.EDDI_NAME,
                "ipc_config.json"
            );

            if (File.Exists(appDataPath))
            {
                return appDataPath;
            }

            // Check current directory as fallback
            var currentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ipc_config.json");
            return currentPath;
        }
    }
}
