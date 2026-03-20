#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackAdapter
{
    /// <summary>
    /// Manages launching and lifecycle of the separate EDDI process when running as a VoiceAttack plugin.
    /// The plugin communicates with the standalone EDDI process via IPC.
    /// </summary>
    public static class EddiProcessLauncher
    {
        private const int LaunchTimeoutMs = 15000;  // 15 seconds to wait for EDDI to launch and be ready
        private const int ConnectionRetryIntervalMs = 500;  // Check for IPC server every 500ms
        private static Process? _eddiProcess;

        /// <summary>
        /// Attempts to launch the standalone EDDI process if it's not already running.
        /// Waits for the IPC server to be ready before returning.
        /// </summary>
        /// <param name="fromVoiceAttack">True when launch is initiated by VoiceAttack plugin</param>
        /// <param name="voiceAttackVersion">Optional VoiceAttack host application version</param>
        /// <returns>True if EDI process launched successfully or was already running; False if launch failed</returns>
        public static async Task<bool> LaunchEddiIfNeededAsync(bool fromVoiceAttack = true,
            System.Version? voiceAttackVersion = null)
        {
            try
            {
                // Try to connect to existing EDDI server first
                Logging.Debug("Attempting to connect to existing EDDI server...");
                var isConnected = await AttemptServerConnectionAsync().ConfigureAwait(false);
                if (isConnected)
                {
                    Logging.Info("Connected to existing EDDI standalone instance");
                    return true;
                }

                // No server running, launch EDDI.exe
                Logging.Info("No existing EDDI server found; launching EDDI.exe as separate process");
                return await LaunchEddiProcessAsync(fromVoiceAttack, voiceAttackVersion).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to launch EDDI process", ex);
                return false;
            }
        }

        /// <summary>
        /// Attempts to connect to an existing EDDI IPC server.
        /// </summary>
        /// <returns>True if server is reachable; False if no server is running</returns>
        private static async Task<bool> AttemptServerConnectionAsync()
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await Client.VoiceAttackPluginHost.Instance
                    .InitializeAsync(cts.Token)
                    .ConfigureAwait(false);

                // If initialization succeeds without throwing, check if we're connected
                return Client.VoiceAttackPluginHost.Instance.Client != null;
            }
            catch (OperationCanceledException)
            {
                Logging.Debug("Server connection attempt timed out");
                return false;
            }
            catch (Exception ex)
            {
                Logging.Debug($"Server connection attempt failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Launches the EDDI.exe process and waits for it to be ready.
        /// </summary>
        /// <returns>True if process launched and became ready; False otherwise</returns>
        private static async Task<bool> LaunchEddiProcessAsync(bool fromVoiceAttack, System.Version? voiceAttackVersion)
        {
            try
            {
                var eddiPath = GetEddiExecutablePath();
                if (!File.Exists(eddiPath))
                {
                    Logging.Error($"EDDI executable not found at {eddiPath}");
                    return false;
                }

                Logging.Info($"Launching EDDI from {eddiPath}");

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
                    Logging.Error("Failed to start EDDI process");
                    return false;
                }

                Logging.Info($"EDDI process started with PID {_eddiProcess.Id}");

                // Wait for the IPC server to be ready
                return await WaitForServerReadyAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logging.Error("Exception while launching EDDI process", ex);
                return false;
            }
        }

        /// <summary>
        /// Waits for the EDDI IPC server to be ready and accessible.
        /// Uses exponential backoff to avoid overwhelming the server during startup.
        /// </summary>
        /// <returns>True if server became ready; False if timeout occurred</returns>
        private static async Task<bool> WaitForServerReadyAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            int attemptCount = 0;

            while (stopwatch.ElapsedMilliseconds < LaunchTimeoutMs)
            {
                attemptCount++;

                try
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await Client.VoiceAttackPluginHost.Instance
                        .InitializeAsync(cts.Token)
                        .ConfigureAwait(false);

                    var client = Client.VoiceAttackPluginHost.Instance.Client;
                    if (client != null)
                    {
                        Logging.Info($"EDDI IPC server ready after {stopwatch.ElapsedMilliseconds}ms ({attemptCount} attempts)");
                        return true;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected during early startup attempts
                }
                catch (Exception ex)
                {
                    Logging.Debug($"Server ready check failed (attempt {attemptCount}): {ex.Message}");
                }

                // Wait before retrying
                await Task.Delay(ConnectionRetryIntervalMs).ConfigureAwait(false);
            }

            Logging.Warn($"EDDI IPC server did not become ready within {LaunchTimeoutMs}ms ({attemptCount} attempts)");
            return false;
        }

        /// <summary>
        /// Gets the path to the EDDI executable.
        /// First checks the same directory as the current assembly, then checks Program Files.
        /// </summary>
        /// <returns>Full path to EDDI.exe</returns>
        private static string GetEddiExecutablePath()
        {
            // First, try the directory where the VoiceAttack plugin is loaded from
            var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pluginDirPath = Path.Combine(pluginDir ?? "", "EDDI.exe");
            if (File.Exists(pluginDirPath))
            {
                return pluginDirPath;
            }

            // Try the application base directory (where the assembly loaded from)
            var baseDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EDDI.exe");
            if (File.Exists(baseDirPath))
            {
                return baseDirPath;
            }

            // Fallback: assume it's in the same directory as the plugin DLL
            return pluginDirPath;
        }

        /// <summary>
        /// Checks if the EDDI process has exited unexpectedly.
        /// </summary>
        /// <returns>True if process has exited; False if still running</returns>
        public static bool HasEddiProcessExited()
        {
            return _eddiProcess == null || _eddiProcess.HasExited;
        }

        /// <summary>
        /// Cleanly shuts down the EDDI process if it was launched by the plugin.
        /// </summary>
        public static void ShutdownEddiProcess()
        {
            try
            {
                if (_eddiProcess == null || _eddiProcess.HasExited)
                {
                    return;
                }

                Logging.Info("Shutting down EDDI process");
                _eddiProcess.CloseMainWindow();

                // Give it 3 seconds to close gracefully
                if (!_eddiProcess.WaitForExit(3000))
                {
                    Logging.Warn("EDDI process did not exit gracefully; forcing termination");
                    _eddiProcess.Kill();
                }

                _eddiProcess.Dispose();
                _eddiProcess = null;
            }
            catch (Exception ex)
            {
                Logging.Warn("Exception while shutting down EDDI process", ex);
            }
        }
    }
}
