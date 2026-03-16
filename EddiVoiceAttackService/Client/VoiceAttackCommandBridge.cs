#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackService.Client
{
    /// <summary>
    /// Command routing gateway for VoiceAttack commands.
    /// Routes ALL commands through IPC infrastructure to EDDI.exe responder mode.
    /// This ensures centralized command management and all command logic in EDDI.exe.
    /// </summary>
    public class VoiceAttackCommandBridge
    {
        /// <summary>
        /// Routes a command through IPC to EDDI.exe responder mode.
        /// All commands must execute in EDDI.exe context for consistent state management.
        /// </summary>
        /// <param name="commandContext">The command context string (case-insensitive)</param>
        /// <param name="parameters">Optional parameters for the command</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Command execution result, or null if IPC unavailable</returns>
        /// <exception cref="ArgumentNullException">Thrown if commandContext is null</exception>
        /// <exception cref="ArgumentException">Thrown if commandContext is empty</exception>
        /// <exception cref="OperationCanceledException">Thrown if operation is cancelled</exception>
        public async Task<object?> RouteCommandAsync(
            string commandContext,
            object? parameters = null,
            CancellationToken cancellationToken = default)
        {
            // Check for cancellation early
            cancellationToken.ThrowIfCancellationRequested();

            // Validate input
            ArgumentNullException.ThrowIfNull(commandContext);
            if (string.IsNullOrWhiteSpace(commandContext))
            {
                throw new ArgumentException(@"Command context cannot be empty", nameof(commandContext));
            }

            // Normalize command context
            var normalizedContext = commandContext.ToLowerInvariant();

            try
            {
                Logging.Debug($"Routing command '{normalizedContext}' through IPC to EDDI.exe");

                var client = VoiceAttackPluginHost.Instance.Client;
                if (client == null || !client.IsConnected)
                {
                    Logging.Warn($"Command '{commandContext}' cannot execute: IPC not available (EDDI.exe not running)");
                    return null;
                }

                // Route through IPC to EDDI.exe responder
                return await DispatchThroughIpcAsync(normalizedContext, parameters, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation exceptions
                throw;
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to route command '{commandContext}' through IPC", ex);
                return null;
            }
        }

        /// <summary>
        /// Dispatches a command through the IPC infrastructure to EDDI.exe.
        /// </summary>
        /// <param name="normalizedContext">The normalized (lowercase) command context</param>
        /// <param name="parameters">Optional command parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Command execution result</returns>
        private async Task<object?> DispatchThroughIpcAsync(
            string normalizedContext,
            object? parameters,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = VoiceAttackPluginHost.Instance.Client;
                if (client == null || !client.IsConnected)
                {
                    return null;
                }

                Logging.Debug($"Dispatching command '{normalizedContext}' through IPC");

                // Send command through IPC client  
                var response = await client.SendCommandAsync(normalizedContext, parameters, cancellationToken).ConfigureAwait(false);

                Logging.Debug($"IPC command '{normalizedContext}' returned: {response != null}");

                return response;
            }
            catch (OperationCanceledException)
            {
                Logging.Debug($"IPC command '{normalizedContext}' was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logging.Error($"IPC dispatch failed for command '{normalizedContext}'", ex);
                throw;
            }
        }
    }
}
