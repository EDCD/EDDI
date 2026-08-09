#nullable enable

using EddiVoiceAttackAdapter.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EddiVoiceAttackAdapter
{
    /// <summary>
    /// Manages launching and lifecycle of the separate EDDI process when running as a VoiceAttack plugin.
    /// The plugin communicates with the standalone EDDI process via IPC.
    /// </summary>
    public static class EddiProcessLauncher
    {
        private const int LaunchTimeoutMs = 15000;  // 15 seconds to wait for EDDI to launch and be ready
        private const int ConnectionRetryIntervalMs = 500;  // Check for IPC server every 500ms during initial startup
        private const int BackgroundConnectionRetryIntervalMs = 3000;  // Continue polling every 3 seconds if a connection isn't established within the initial startup window
        private static Process? _eddiProcess;
        private static bool _managedEddiProcess;

        /// <summary>
        /// Attempts to launch the standalone EDDI process if it's not already running.
        /// Waits for the IPC server to be ready before returning.
        /// </summary>
        /// <param name="fromVoiceAttack">True when launch is initiated by VoiceAttack plugin</param>
        /// <param name="voiceAttackVersion">Optional VoiceAttack host application version</param>
        /// <returns>True if EDI process launched successfully or was already running; False if launch failed</returns>
        public static async Task<bool> LaunchEddiIfNeededAsync(bool fromVoiceAttack = true,
            Version? voiceAttackVersion = null, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Try to connect to existing EDDI server first. If successful, take ownership of the process.
                AdapterLogger.Debug("Attempting to connect to existing EDDI server...");
                var isConnected = await AttemptServerConnectionAsync(cancellationToken).ConfigureAwait(false);
                if (isConnected)
                {
                    _eddiProcess = Process.GetProcessesByName( "Eddi" ).FirstOrDefault();
                    _managedEddiProcess = _eddiProcess != null;
                    AdapterLogger.Info("Connected to existing EDDI standalone instance");
                    return true;
                }

                // No server running, launch EDDI.exe
                AdapterLogger.Info("No existing EDDI server found; launching EDDI.exe as separate process");
                return await LaunchEddiProcessAsync(fromVoiceAttack, voiceAttackVersion, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                AdapterLogger.Debug("EDDI process launch was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                AdapterLogger.Error("Failed to launch EDDI process", ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to connect to an existing EDDI IPC server.
        /// </summary>
        /// <returns>True if server is reachable; False if no server is running</returns>
        private static async Task<bool> AttemptServerConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
                cts.CancelAfter( TimeSpan.FromSeconds( 3 ) );
                await Client.VoiceAttackPluginHost.Instance
                    .InitializeAsync(cts.Token)
                    .ConfigureAwait(false);

                // If initialization succeeds without throwing, check if we're connected
                return Client.VoiceAttackPluginHost.Instance.Client != null;
            }
            catch (OperationCanceledException) when ( !cancellationToken.IsCancellationRequested )
            {
                AdapterLogger.Debug("Server connection attempt timed out");
                return false;
            }
            catch (Exception ex)
            {
                AdapterLogger.Debug($"Server connection attempt failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Launches the EDDI.exe process and waits for it to be ready.
        /// </summary>
        /// <returns>True if process launched and became ready; False otherwise</returns>
        private static async Task<bool> LaunchEddiProcessAsync(bool fromVoiceAttack, Version? voiceAttackVersion,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var eddiPath = GetEddiExecutablePath();
                if (!File.Exists(eddiPath))
                {
                    AdapterLogger.Error($"EDDI executable not found at {eddiPath}");
                    return false;
                }

                AdapterLogger.Info($"Launching EDDI from {eddiPath}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = eddiPath,
                    UseShellExecute = true,
                    CreateNoWindow = false  // Allow the EDDI window to display
                };

                if (fromVoiceAttack)
                {
                    startInfo.ArgumentList.Add("--voice-attack-plugin");
                    if (voiceAttackVersion != null)
                    {
                        startInfo.ArgumentList.Add("--voice-attack-version");
                        startInfo.ArgumentList.Add(voiceAttackVersion.ToString());
                    }
                }

                _eddiProcess = Process.Start(startInfo);
                if (_eddiProcess == null)
                {
                    _managedEddiProcess = false;
                    AdapterLogger.Error("Failed to start EDDI process");
                    return false;
                }

                _managedEddiProcess = true;
                AdapterLogger.Info($"EDDI process started with PID {_eddiProcess.Id}");

                // Wait for the IPC server to be ready
                return await WaitForServerReadyAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                AdapterLogger.Debug("Waiting for the EDDI IPC server was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                AdapterLogger.Error("Exception while launching EDDI process", ex);
                return false;
            }
        }

        /// <summary>
        /// Waits for the EDDI IPC server to be ready and accessible.
        /// Uses exponential backoff to avoid overwhelming the server during startup.
        /// </summary>
        /// <returns>True if server became ready; False if timeout occurred</returns>
        private static async Task<bool> WaitForServerReadyAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var attemptCount = 0;
            var backgroundPollingLogged = false;

            while ( true )
            {
                cancellationToken.ThrowIfCancellationRequested();
                attemptCount++;

                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource( cancellationToken );
                    cts.CancelAfter( TimeSpan.FromSeconds( 2 ) );
                    await Client.VoiceAttackPluginHost.Instance
                        .InitializeAsync(cts.Token)
                        .ConfigureAwait(false);

                    var client = Client.VoiceAttackPluginHost.Instance.Client;
                    if (client != null)
                    {
                        AdapterLogger.Info($"EDDI IPC server ready after {stopwatch.ElapsedMilliseconds}ms ({attemptCount} attempts)");
                        return true;
                    }
                }
                catch (OperationCanceledException) when ( !cancellationToken.IsCancellationRequested )
                {
                    AdapterLogger.Debug($"Server ready check timed out (attempt {attemptCount})");
                }
                catch (Exception ex)
                {
                    AdapterLogger.Debug($"Server ready check failed (attempt {attemptCount}): {ex.Message}");
                }

                if ( _eddiProcess?.HasExited ?? false )
                {
                    _managedEddiProcess = false;
                    AdapterLogger.Warn($"EDDI process exited before the IPC server became ready ({attemptCount} attempts)");
                    return false;
                }

                var stillInInitialStartupWindow = stopwatch.ElapsedMilliseconds < LaunchTimeoutMs;
                if ( !stillInInitialStartupWindow && !backgroundPollingLogged )
                {
                    AdapterLogger.Info($"EDDI IPC server was not ready within the initial {LaunchTimeoutMs}ms window ({attemptCount} attempts); continuing low-frequency polling");
                    backgroundPollingLogged = true;
                }

                await Task.Delay(
                        stillInInitialStartupWindow ? ConnectionRetryIntervalMs : BackgroundConnectionRetryIntervalMs,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the path to the EDDI executable.
        /// Release installs resolve through the installer/app-maintained locator first;
        /// same-folder fallbacks are retained for local development layouts.
        /// </summary>
        /// <returns>Full path to EDDI.exe</returns>
        private static string GetEddiExecutablePath()
        {
            var pluginDir = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
            var eddiPath = EddiInstallLocator.ResolveExecutablePath(
                pluginDir,
                baseDirectory: AppDomain.CurrentDomain.BaseDirectory );
            if ( eddiPath != null )
            {
                return eddiPath;
            }

            // Preserve the previous error-path behavior so callers log the expected candidate.
            return Path.Combine( pluginDir ?? string.Empty, "EDDI.exe" );
        }

        /// <summary>
        /// Checks if the EDDI process has exited unexpectedly.
        /// </summary>
        /// <returns>True if process has exited; False if still running</returns>
        public static bool HasEddiProcessExited()
        {
            return _managedEddiProcess && (_eddiProcess == null || _eddiProcess.HasExited);
        }

        internal static bool HasManagedEddiProcess()
        {
            return _managedEddiProcess;
        }

        /// <summary>
        /// Cleanly shuts down the EDDI process if it was launched by the plugin.
        /// </summary>
        public static void ShutdownEddiProcess()
        {
            try
            {
                if ( !_managedEddiProcess || _eddiProcess == null || _eddiProcess.HasExited )
                {
                    _managedEddiProcess = false;
                    _eddiProcess = null;
                    return;
                }

                AdapterLogger.Info("Shutting down EDDI process");
                _eddiProcess.CloseMainWindow();

                // Give it 3 seconds to close gracefully
                if (!_eddiProcess.WaitForExit(3000))
                {
                    AdapterLogger.Warn("EDDI process did not exit gracefully; forcing termination");
                    _eddiProcess.Kill();
                }

                _eddiProcess.Dispose();
                _eddiProcess = null;
                _managedEddiProcess = false;
            }
            catch (Exception ex)
            {
                AdapterLogger.Warn("Exception while shutting down EDDI process", ex);
            }
        }
    }
}
