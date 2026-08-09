#nullable enable

using EddiVoiceAttackAdapter.Annotations;
using EddiVoiceAttackAdapter.Client;
using EddiVoiceAttackAdapter.Extensions;
using EddiVoiceAttackAdapter.Logging;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiVoiceAttackAdapter
{
    [UsedImplicitly]
    public class VoiceAttackPlugin
    {
        private static bool? _supportsVariableSaveToProfile;

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static string VA_DisplayName() => $"EDDI {AdapterVersionProvider.GetDisplayVersion()}";

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static string VA_DisplayInfo() => $"EDDI\r\nVersion {AdapterVersionProvider.GetDisplayVersion()}";

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static Guid VA_Id() => new("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");

        internal static dynamic? VaProxy;

        private static Version? VaVersion
        {
            get
            {
                lock ( vaProxyLock )
                {
                    if ( VaProxy == null )
                    {
                        return null;
                    }

                    try
                    {
                        return VaProxy.VAVersion as Version;
                    }
                    catch ( RuntimeBinderException )
                    {
                        var vaVersionProperty = VaProxy.GetType().GetProperty(
                            "VAVersion",
                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static );

                        return vaVersionProperty?.GetValue(
                            vaVersionProperty.GetMethod?.IsStatic == true
                                ? null
                                : VaProxy ) as Version;
                    }
                }
            }
        }

        private static readonly object vaProxyLock = new();
        private static bool _isShuttingDown;
        private static bool _runtimeReceiverSubscribed;
        private static VoiceAttackPluginClient? _runtimeReceiverClient;
        private static CancellationTokenSource? _shutdownCancellationTokenSource;

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Init1(dynamic vaProxy)
        {
            _isShuttingDown = false;
            _shutdownCancellationTokenSource?.Dispose();
            _shutdownCancellationTokenSource = new CancellationTokenSource();

            // Store VA proxy for variable read/write access
            if (vaProxy != null)
            {
                lock (vaProxyLock)
                {
                    VaProxy = vaProxy;
                }
            }

            // Launch EDDI process and establish IPC connection immediately
            // (no need for callback; plugin is already running in VoiceAttack)
            var shutdownToken = _shutdownCancellationTokenSource.Token;
            Task.Run(async () => await LaunchEddiAndInitializeIpcAsync(shutdownToken).ConfigureAwait(false))
                .SafeFireAndForget(ex => AdapterLogger.Error("Failed to initialize VoiceAttack plugin", ex));
        }

        /// <summary>
        /// Launch EDDI process and establish IPC connection for responder mode.
        /// Called on background thread to avoid blocking VoiceAttack.
        /// </summary>
        private static async Task LaunchEddiAndInitializeIpcAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                AdapterLogger.Info("VoiceAttack plugin: Launching EDDI process");

                // Launch EDDI as separate process or connect to existing instance
                var launchSuccess = await EddiProcessLauncher.LaunchEddiIfNeededAsync(true, VaVersion, cancellationToken).ConfigureAwait(false);
                if (!launchSuccess)
                {
                    AdapterLogger.Warn("Failed to launch or connect to EDDI standalone process");
                    WriteToLog("Warning: EDDI standalone process could not be launched. Plugin may operate with reduced functionality.", "orange");
                    return;
                }

                // Initialize IPC client for command/query/event dispatch
                try
                {
                    await VoiceAttackPluginHost.Instance.InitializeAsync( cancellationToken ).ConfigureAwait(false);
                    AdapterLogger.Debug("IPC client initialized");

                    RegisterRuntimeEventReceiver();

                    // Send responder mode handshake to EDDI.exe
                    // This sets EDDI.FromVA = true and triggers VoiceAttackResponderMode initialization in EDDI.exe
                    var responderModeEnabled = await VoiceAttackPluginHost.Instance.SendSetResponderModeAsync(true, VaVersion, cancellationToken ).ConfigureAwait(false);
                    if (!responderModeEnabled)
                    {
                        AdapterLogger.Warn("VoiceAttack responder mode handshake was not acknowledged");
                        WriteToLog("Warning: EDDI IPC connection is available, but responder mode could not be enabled.", "orange");
                        return;
                    }

                    AdapterLogger.Info("VoiceAttack responder mode handshake sent");
                    WriteToLog( "The EDDI plugin is connected. VoiceAttack variables are syncing.", "green" );
                    
                    // Start background task to monitor for EDDI crashes we launched ourselves
                    if ( EddiProcessLauncher.HasManagedEddiProcess() )
                    {
                        _ = MonitorEddiProcessAsync(cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    AdapterLogger.Warn($"Failed to initialize IPC client: {ex.Message}");
                }
            }
            catch ( OperationCanceledException )
            {
                AdapterLogger.Debug( "VoiceAttack plugin initialization was cancelled" );
            }
            catch (Exception e)
            {
                AdapterLogger.Error("Failed to launch EDDI or initialize IPC", e);
                WriteToLog("Unable to launch EDDI process. Plugin functions may be limited.", "red");
            }
        }

        /// <summary>
        /// Monitor EDDI process for unexpected crashes and automatically restart if needed.
        /// Runs in background until intentional shutdown is signaled.
        /// </summary>
        private static async Task MonitorEddiProcessAsync(CancellationToken cancellationToken)
        {
            const int checkIntervalMs = 2000;
            const int maxRestartAttempts = 3;
            var restartAttempts = 0;

            while (!_isShuttingDown && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(checkIntervalMs, cancellationToken).ConfigureAwait(false);

                    // Check if EDDI process has exited unexpectedly
                    if (EddiProcessLauncher.HasEddiProcessExited())
                    {
                        if (_isShuttingDown)
                        {
                            // User initiated shutdown, don't restart
                            AdapterLogger.Debug("EDDI process exited during intentional shutdown");
                            break;
                        }

                        // EDDI crashed unexpectedly
                        AdapterLogger.Warn("EDDI process has crashed unexpectedly");
                        WriteToLog("EDDI has crashed unexpectedly. Attempting to restart...", "red");

                        // Attempt restart if within limit
                        if (restartAttempts < maxRestartAttempts)
                        {
                            restartAttempts++;
                            AdapterLogger.Info($"Attempting EDDI restart (attempt {restartAttempts}/{maxRestartAttempts})");

                            try
                            {
                                // Relaunch EDDI and reinitialize IPC
                                var restartSuccess = await EddiProcessLauncher.LaunchEddiIfNeededAsync(true, VaVersion, cancellationToken).ConfigureAwait(false);
                                if (restartSuccess)
                                {
                                    await VoiceAttackPluginHost.Instance.InitializeAsync( cancellationToken ).ConfigureAwait(false);
                                    RegisterRuntimeEventReceiver();
                                    var responderModeEnabled = await VoiceAttackPluginHost.Instance.SendSetResponderModeAsync(true, VaVersion, cancellationToken ).ConfigureAwait(false);
                                    if (!responderModeEnabled)
                                    {
                                        AdapterLogger.Warn("EDDI restart completed but responder mode handshake was not acknowledged");
                                        WriteToLog("EDDI restarted, but responder mode could not be re-enabled.", "orange");
                                        continue;
                                    }

                                    AdapterLogger.Info("EDDI process restarted successfully");
                                    WriteToLog("EDDI has been restarted successfully.", "green");
                                    restartAttempts = 0; // Reset counter on successful restart
                                }
                                else
                                {
                                    AdapterLogger.Warn($"Failed to restart EDDI (attempt {restartAttempts})");
                                    WriteToLog($"Failed to restart EDDI (attempt {restartAttempts}/{maxRestartAttempts}). Will retry...", "orange");
                                }
                            }
                            catch ( OperationCanceledException )
                            {
                                AdapterLogger.Debug( "EDDI restart monitoring was cancelled" );
                                break;
                            }
                            catch (Exception restartEx)
                            {
                                AdapterLogger.Warn($"Exception during EDDI restart: {restartEx.Message}");
                                WriteToLog($"Error restarting EDDI: {restartEx.Message}", "orange");
                            }
                        }
                        else
                        {
                            AdapterLogger.Error("EDDI restart attempts exceeded maximum");
                            WriteToLog("EDDI restart attempts exceeded. Plugin may operate with reduced functionality.", "red");
                            break;
                        }
                    }
                }
                catch ( OperationCanceledException ) when ( cancellationToken.IsCancellationRequested )
                {
                    break;
                }
                catch (Exception ex)
                {
                    AdapterLogger.Error($"Error in crash monitoring: {ex.Message}", ex);
                }
            }

            AdapterLogger.Debug("EDDI crash monitoring stopped");
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Exit1( dynamic _ )
        {
            // Signal intentional shutdown to stop crash monitoring and prevent restart
            _isShuttingDown = true;
            _shutdownCancellationTokenSource?.Cancel();

            AdapterLogger.Info("EDDI VoiceAttack plugin exiting");

            // Disable responder mode in EDDI.exe through IPC
            try
            {
                var sent = VoiceAttackPluginHost.Instance.SendSetResponderModeAsync(false).GetAwaiter().GetResult();
                if (!sent)
                {
                    AdapterLogger.Warn("SetResponderMode(false) command was not acknowledged during plugin shutdown");
                }
            }
            catch (Exception ex)
            {
                AdapterLogger.Warn($"Error sending responder mode shutdown command: {ex.Message}");
            }

            UnregisterRuntimeEventReceiver();

            // Disconnect IPC client gracefully
            try
            {
                VoiceAttackPluginHost.Instance.DisconnectAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                AdapterLogger.Warn($"Error disconnecting IPC client during shutdown: {ex.Message}");
            }

            // Give background tasks a moment to gracefully shut down
            Thread.Sleep( 500 );
            _shutdownCancellationTokenSource?.Dispose();
            _shutdownCancellationTokenSource = null;

            // Dispose of plugin host
            try
            {
                VoiceAttackPluginHost.Instance.Dispose();
            }
            catch (Exception ex)
            {
                AdapterLogger.Warn($"Error disposing plugin host during shutdown: {ex.Message}");
            }

            // Shutdown the EDDI process if it was launched by the plugin
            EddiProcessLauncher.ShutdownEddiProcess();

            // EDDI runs out-of-process in plugin mode; the shim has no WPF application to shut down.
        }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_StopCommand()
        { }

        [UsedImplicitly( Reason = "VoiceAttack Interface Member" )]
        public static void VA_Invoke1(dynamic vaProxy)
        {
            lock ( vaProxyLock )
            {
                VaProxy = vaProxy;
            }

            var commandContext = vaProxy.Context as string;
            if ( string.Equals( commandContext, "initialize eddi", StringComparison.OrdinalIgnoreCase ) )
            {
                var shutdownToken = _shutdownCancellationTokenSource?.Token ?? CancellationToken.None;
                Task.Run( async () => await LaunchEddiAndInitializeIpcAsync(shutdownToken).ConfigureAwait(false), shutdownToken )
                    .SafeFireAndForget( ex => AdapterLogger.Error( "Failed to run initialize eddi bootstrap", ex ) );
                return;
            }

            if ( string.IsNullOrWhiteSpace( commandContext ) )
            {
                AdapterLogger.Warn( "VoiceAttack plugin invocation skipped because command context was null or empty" );
                return;
            }

            var commandPayload = BuildInvocationPayload();

            // Route command through bridge to EDDI.exe responder via IPC
            var task = VoiceAttackCommandBridge.RouteCommandAsync( commandContext, commandPayload );
            task.SafeFireAndForget( LogException );
        }

        private static System.Collections.Generic.Dictionary<string, object> BuildInvocationPayload()
        {
            var payload = new System.Collections.Generic.Dictionary<string, object>( StringComparer.OrdinalIgnoreCase )
            {
                ["Script"] = GetText( "Script" ) ?? string.Empty,
                ["Priority"] = GetInt( "Priority" )?.ToString() ?? string.Empty,
                ["Voice"] = GetText( "Voice" ) ?? string.Empty,
                ["Volume"] = GetInt( "Volume" )?.ToString() ?? string.Empty,
                ["Name"] = GetText( "Name" ) ?? string.Empty,
                ["Personality"] = GetText( "Personality" ) ?? string.Empty,
                ["State variable"] = GetText( "State variable" ) ?? string.Empty,
                ["EDDI open uri in browser"] = GetBoolean( "EDDI open uri in browser" )?.ToString() ?? string.Empty,
                ["EDDI use clipboard"] = GetBoolean( "EDDI use clipboard" )?.ToString() ?? string.Empty,
                ["EDDI system comment"] = GetText( "EDDI system comment" ) ?? string.Empty,
                ["Type variable"] = GetText( "Type variable" ) ?? string.Empty,
                ["System variable"] = GetText( "System variable" ) ?? string.Empty,
                ["System variable 2"] = GetText( "System variable 2" ) ?? string.Empty,
                ["Station variable"] = GetText( "Station variable" ) ?? string.Empty,
                ["Numeric variable"] = GetDecimal( "Numeric variable" )?.ToString() ?? string.Empty,
                ["Boolean variable"] = GetBoolean( "Boolean variable" )?.ToString() ?? string.Empty
            };

            var stateVariableName = GetText( "State variable" );
            if ( !string.IsNullOrWhiteSpace( stateVariableName ) )
            {
                payload["State variable text value"] = GetText( stateVariableName ) ?? string.Empty;
                payload["State variable int value"] = GetInt( stateVariableName )?.ToString() ?? string.Empty;
                payload["State variable bool value"] = GetBoolean( stateVariableName )?.ToString() ?? string.Empty;
                payload["State variable decimal value"] = GetDecimal( stateVariableName )?.ToString() ?? string.Empty;
            }

            return payload;
        }

        private static void LogException(Exception ex)
        {
            AdapterLogger.Error(ex.Message, ex);
        }

        private static void RegisterRuntimeEventReceiver()
        {
            var client = VoiceAttackPluginHost.Instance.Client;
            if ( ReferenceEquals( _runtimeReceiverClient, client ) && _runtimeReceiverSubscribed )
            {
                return;
            }

            if ( _runtimeReceiverClient != null )
            {
                _runtimeReceiverClient.MessageReceived -= VoiceAttackRuntimeEventReceiver.HandleMessageReceived;
                _runtimeReceiverClient = null;
                _runtimeReceiverSubscribed = false;
            }

            if (client == null)
            {
                return;
            }

            client.MessageReceived += VoiceAttackRuntimeEventReceiver.HandleMessageReceived;
            _runtimeReceiverClient = client;
            _runtimeReceiverSubscribed = true;
        }

        private static void UnregisterRuntimeEventReceiver()
        {
            if (!_runtimeReceiverSubscribed || _runtimeReceiverClient == null)
            {
                _runtimeReceiverSubscribed = false;
                _runtimeReceiverClient = null;
                return;
            }

            _runtimeReceiverClient.MessageReceived -= VoiceAttackRuntimeEventReceiver.HandleMessageReceived;
            _runtimeReceiverClient = null;
            _runtimeReceiverSubscribed = false;
        }

        private static bool IsVaVersionSameOrNewer ( Version minVersion )
        {
            lock ( vaProxyLock )
            {
                return VaVersion?.CompareTo( minVersion ) >= 0;
            }
        }

        #region Command Interactions

        // If running VoiceAttack version 1.7.4 or later then we should use the more modern command API endpoints
        private static readonly Version commandApiVaVersion = new( 1, 7, 4 );

        public static async Task WaitForCommandExecutionAsync ( string commandName )
        {
            var isCommandExecuting = true;
            while ( isCommandExecuting )
            {
                await Task.Delay( 25 ).ConfigureAwait(false);
                lock ( vaProxyLock )
                {
                    isCommandExecuting = IsVaVersionSameOrNewer( commandApiVaVersion )
                        ? VaProxy?.Command.Active( commandName )
                        : VaProxy?.CommandActive( commandName );
                }
            }
        }

        public static bool CommandExists ( string commandName )
        {
            lock ( vaProxyLock )
            {
                return IsVaVersionSameOrNewer( commandApiVaVersion )
                    ? VaProxy?.Command.Exists( commandName )
                    : VaProxy?.CommandExists( commandName );
            }
        }

        public static void ExecuteCommand ( string commandName )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( commandApiVaVersion ) )
                {
                    VaProxy?.Command.Execute( commandName );
                }
                else
                {
                    // Use the legacy endpoint
                    VaProxy?.ExecuteCommand( commandName );
                }
            }
        }

        #endregion

        #region Log Interactions

        public static void WriteToLog ( string message, string color )
        {
            lock ( vaProxyLock )
            {
                VaProxy?.WriteToLog( message, color );
            }
        }

        #endregion

        #region Variable Interactions

        // If running VoiceAttack version 1.10.4 or later then we should use the more modern variable API endpoints
        private static readonly Version variableApiVaVersion = new( 1, 10, 4 );

        public static bool? GetBoolean ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy?.GetBoolean( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy?.GetBoolean( key );
            }
        }

        public static DateTime? GetDate ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy?.GetDate( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy?.GetDate( key );
            }
        }

        public static decimal? GetDecimal ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy?.GetDecimal( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy?.GetDecimal( key );
            }
        }

        public static int? GetInt ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy?.GetInt( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy?.GetInt( key );
            }
        }

        public static string? GetText ( string key, bool retrieveFromProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        return VaProxy?.GetText( key, retrieveFromProfile );
                    }
                    catch ( RuntimeBinderException )
                    {
                        // We'll need to use the legacy endpoint
                    }
                }

                // Use the legacy endpoint
                return VaProxy?.GetText( key );
            }
        }

        public static void SetBoolean ( string key, bool? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( _supportsVariableSaveToProfile != false &&
                     IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy?.SetBoolean( key, value, saveToProfile );
                        _supportsVariableSaveToProfile = true;
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        _supportsVariableSaveToProfile = false;
                    }
                }

                // Use the legacy endpoint
                VaProxy?.SetBoolean( key, value );
            }
        }

        public static void SetDate ( string key, DateTime? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( _supportsVariableSaveToProfile != false &&
                     IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy?.SetDate( key, value, saveToProfile );
                        _supportsVariableSaveToProfile = true;
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        _supportsVariableSaveToProfile = false;
                    }
                }

                // Use the legacy endpoint
                VaProxy?.SetDate( key, value );
            }
        }

        public static void SetDecimal ( string key, decimal? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( _supportsVariableSaveToProfile != false &&
                     IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy?.SetDecimal( key, value, saveToProfile );
                        _supportsVariableSaveToProfile = true;
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        _supportsVariableSaveToProfile = false;
                    }
                }

                // Use the legacy endpoint
                VaProxy?.SetDecimal( key, value );
            }
        }

        public static void SetInt ( string key, int? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( _supportsVariableSaveToProfile != false &&
                     IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy?.SetInt( key, value, saveToProfile );
                        _supportsVariableSaveToProfile = true;
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        _supportsVariableSaveToProfile = false;
                    }
                }

                // Use the legacy endpoint
                VaProxy?.SetInt( key, value );
            }
        }

        public static void SetText ( string key, string? value, bool saveToProfile = false )
        {
            lock ( vaProxyLock )
            {
                if ( _supportsVariableSaveToProfile != false &&
                     IsVaVersionSameOrNewer( variableApiVaVersion ) )
                {
                    try
                    {
                        VaProxy?.SetText( key, value, saveToProfile );
                        _supportsVariableSaveToProfile = true;
                        return;
                    }
                    catch ( RuntimeBinderException )
                    {
                        _supportsVariableSaveToProfile = false;
                    }
                }

                // Use the legacy endpoint
                VaProxy?.SetText( key, value );
            }
        }

        #endregion
    }
}
