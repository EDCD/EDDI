#nullable enable

using EddiIPC_Service.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Dispatches runtime payload events from the server to connected clients.
    /// </summary>
    public static class RuntimeEventDispatcher
    {
        private static readonly object sync = new();
        private static Func<EventData, CancellationToken, Task<bool>>? dispatcher;

        /// <summary>
        /// Register dispatcher implementation.
        /// </summary>
        /// <param name="runtimeDispatcher">Dispatcher callback.</param>
        public static void RegisterDispatcher( Func<EventData, CancellationToken, Task<bool>> runtimeDispatcher )
        {
            ArgumentNullException.ThrowIfNull( runtimeDispatcher );

            lock ( sync )
            {
                dispatcher = runtimeDispatcher;
            }
        }

        /// <summary>
        /// Clear dispatcher registration.
        /// </summary>
        public static void ClearDispatcher()
        {
            lock ( sync )
            {
                dispatcher = null;
            }
        }

        /// <summary>
        /// Dispatch runtime event payload to plugin clients.
        /// </summary>
        /// <param name="eventData">Event payload envelope data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when dispatch path is registered and executed.</returns>
        public static async Task<bool> DispatchAsync( EventData eventData, CancellationToken cancellationToken = default )
        {
            ArgumentNullException.ThrowIfNull( eventData );

            Func<EventData, CancellationToken, Task<bool>>? runtimeDispatcher;
            lock ( sync )
            {
                runtimeDispatcher = dispatcher;
            }

            if ( runtimeDispatcher == null )
            {
                return false;
            }

            return await runtimeDispatcher( eventData, cancellationToken ).ConfigureAwait( false );
        }
    }
}
