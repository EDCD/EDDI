#nullable enable

using System;
using System.Collections.Generic;

namespace EddiVoiceAttackService.Client
{
    /// <summary>
    /// Interface for handling VoiceAttack events.
    /// Abstracts event transmission through IPC to VoiceAttack consumers.
    /// </summary>
    public interface IVoiceAttackEventListener
    {
        /// <summary>
        /// Handles an event received from EDDI.
        /// </summary>
        /// <param name="eventType">The type/name of the event (e.g., "BodyScannedEvent")</param>
        /// <param name="eventName">The human-readable name of the event</param>
        /// <param name="eventPayload">Event data as a dictionary of key-value pairs</param>
        /// <returns>True if event was handled successfully, false otherwise</returns>
        bool OnEventReceived(string eventType, string eventName, Dictionary<string, object> eventPayload);

        /// <summary>
        /// Handles event transmission errors or failures.
        /// </summary>
        /// <param name="eventType">The type of event that failed</param>
        /// <param name="exception">The exception that occurred</param>
        void OnEventTransmissionFailed(string eventType, Exception exception);
    }
}
