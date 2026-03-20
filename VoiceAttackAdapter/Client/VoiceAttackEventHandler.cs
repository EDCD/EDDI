#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackAdapter.Client
{
    /// <summary>
    /// Event routing gateway for VoiceAttack events.
    /// Routes ALL events through IPC infrastructure to VoiceAttack consumers.
    /// This ensures centralized event management and all event logic in EDDI.exe responder.
    /// </summary>
    public class VoiceAttackEventHandler
    {
        /// <summary>
        /// Routes an event through IPC to VoiceAttack consumers.
        /// All events must dispatch through IPC for consistent event handling.
        /// </summary>
        /// <param name="eventType">The type/class name of the event</param>
        /// <param name="eventName">The human-readable name of the event</param>
        /// <param name="eventPayload">Event data as dictionary</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>True if event was dispatched successfully; false if IPC unavailable</returns>
        /// <exception cref="ArgumentNullException">Thrown if eventType or eventName is null</exception>
        /// <exception cref="ArgumentException">Thrown if eventType or eventName is empty</exception>
        public async Task<bool> DispatchEventAsync(
            string eventType,
            string eventName,
            Dictionary<string, object>? eventPayload = null,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            ArgumentNullException.ThrowIfNull(eventType);
            ArgumentNullException.ThrowIfNull(eventName);
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ArgumentException(@"Event type cannot be empty", nameof(eventType));
            }
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException(@"Event name cannot be empty", nameof(eventName));
            }

            eventPayload ??= new();

            try
            {
                Logging.Debug($"Routing event '{eventType}:{eventName}' through IPC to VoiceAttack");

                // Check if IPC client is available and connected
                var client = VoiceAttackPluginHost.Instance.Client;
                if (client == null || !client.IsConnected)
                {
                    Logging.Warn($"Event '{eventType}' cannot dispatch: IPC not available (VoiceAttack plugin not connected)");
                    return false;
                }

                // Dispatch through IPC
                return await DispatchThroughIpcAsync(eventType, eventName, eventPayload, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation exceptions
                throw;
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to dispatch event '{eventType}' through IPC", ex);
                return false;
            }
        }

        /// <summary>
        /// Dispatches an event through the IPC infrastructure.
        /// </summary>
        /// <param name="eventType">The event type</param>
        /// <param name="eventName">The event name</param>
        /// <param name="eventPayload">Event payload dictionary</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successfully dispatched</returns>
        private async Task<bool> DispatchThroughIpcAsync(
            string eventType,
            string eventName,
            Dictionary<string, object> eventPayload,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = VoiceAttackPluginHost.Instance.Client;
                if (client == null || !client.IsConnected)
                {
                    return false;
                }

                // Create composite event identifier combining type and name
                var eventIdentifier = $"{eventType}:{eventName}";

                Logging.Debug($"Dispatching event '{eventIdentifier}' through IPC");

                // Send event through IPC client with payload
                await client.SendEventAsync(eventIdentifier, eventPayload, cancellationToken).ConfigureAwait(false);

                Logging.Debug($"Event '{eventIdentifier}' successfully dispatched through IPC");

                return true;
            }
            catch (OperationCanceledException)
            {
                Logging.Debug($"Event '{eventType}' dispatch was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                Logging.Error($"IPC dispatch failed for event '{eventType}'", ex);
                return false;
            }
        }
    }
}
