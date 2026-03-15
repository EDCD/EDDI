#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using EddiVoiceAttackService.Messages;
using EddiVoiceAttackService.Server;
using Utilities;

namespace EddiVoiceAttackService.Heartbeat
{
    /// <summary>
    /// Monitors connection health via bidirectional heartbeats.
    /// Runs as a background task alongside the IPC server.
    /// </summary>
    public class HeartbeatMonitor
    {
        private readonly IPCServer _server;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _monitorTask;
        private bool _isRunning;
        private readonly DateTime _serverStartTime;
        private const int HeartbeatIntervalSeconds = 3;
        private const int HeartbeatTimeoutSeconds = 10;

        /// <summary>
        /// Create a new heartbeat monitor for an IPC server.
        /// </summary>
        public HeartbeatMonitor(IPCServer server)
        {
            ArgumentNullException.ThrowIfNull(server);
            _server = server;
            _isRunning = false;
            _serverStartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Start the heartbeat monitoring loop.
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                Logging.Warn("HeartbeatMonitor is already running");
                return;
            }

            try
            {
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();
                _monitorTask = MonitorConnectionsAsync(_cancellationTokenSource.Token);

                Logging.Info("HeartbeatMonitor started");
                await Task.Yield(); // Allow monitor task to start
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to start HeartbeatMonitor: {ex.Message}", ex);
                _isRunning = false;
                throw;
            }
        }

        /// <summary>
        /// Stop the heartbeat monitoring loop gracefully.
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _cancellationTokenSource?.Cancel();
            
                if (_monitorTask != null)
                {
                    await _monitorTask.ConfigureAwait(false);
                }

                _isRunning = false;
                Logging.Info("HeartbeatMonitor stopped");
            }
            catch (Exception ex)
            {
                Logging.Error($"Error stopping HeartbeatMonitor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Background task that periodically monitors and sends heartbeats.
        /// </summary>
        private async Task MonitorConnectionsAsync(CancellationToken cancellationToken)
        {
            var heartbeatInterval = TimeSpan.FromSeconds(HeartbeatIntervalSeconds);
            using var heartbeatTimer = new PeriodicTimer(heartbeatInterval);

            try
            {
                while (await heartbeatTimer.WaitForNextTickAsync(cancellationToken))
                {
                    try
                    {
                        await SendHeartbeatsAsync();
                        await CheckTimeoutsAsync();
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"Error in heartbeat monitor: {ex.Message}", ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        /// <summary>
        /// Send heartbeat messages to all connected clients.
        /// </summary>
        private async Task SendHeartbeatsAsync()
        {
            if (_server.ConnectionCount == 0)
            {
                return;
            }

            try
            {
                var uptime = GetServerUptime();
                var heartbeat = MessageEnvelope.Create("Heartbeat",
                    new HeartbeatData { Status = "alive", UptimeMs = (long)uptime.TotalMilliseconds });

                await _server.BroadcastAsync(heartbeat).ConfigureAwait(false);
            
                Logging.Debug($"Heartbeat broadcast to {_server.ConnectionCount} clients");
            }
            catch (Exception ex)
            {
                Logging.Error($"Error broadcasting heartbeat: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check for and disconnect clients with timed-out heartbeats.
        /// </summary>
        private async Task CheckTimeoutsAsync()
        {
            // TODO: This would require access to connection list from server
            // For now, the server itself monitors heartbeats in HandleClientAsync
            await Task.CompletedTask;
        }

        /// <summary>
        /// Get the server uptime since initialization.
        /// </summary>
        private TimeSpan GetServerUptime()
        {
            return DateTime.UtcNow.Subtract(_serverStartTime);
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
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
        }
    }
}
