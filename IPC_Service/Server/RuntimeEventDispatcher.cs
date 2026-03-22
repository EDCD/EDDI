#nullable enable

using EddiIPC_Service.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Dispatches runtime payload events from the server to connected clients.
    /// </summary>
    public static class RuntimeEventDispatcher
    {
        private sealed class Registration : IDisposable
        {
            private readonly object _owner;
            private bool _disposed;

            internal Registration( object owner )
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if ( _disposed )
                {
                    return;
                }

                UnregisterDispatcher( _owner );
                _disposed = true;
            }
        }

        private static readonly object sync = new();
        private static readonly Dictionary<object, Func<EventData, CancellationToken, Task<bool>>> dispatchers = [];

        /// <summary>
        /// Register dispatcher implementation.
        /// Dispose the returned handle to unregister the dispatcher that was added by this call.
        /// </summary>
        /// <param name="runtimeDispatcher">Dispatcher callback.</param>
        /// <returns>An owned registration handle.</returns>
        public static IDisposable RegisterDispatcher( Func<EventData, CancellationToken, Task<bool>> runtimeDispatcher )
        {
            ArgumentNullException.ThrowIfNull( runtimeDispatcher );

            var owner = new object();
            lock ( sync )
            {
                dispatchers[ owner ] = runtimeDispatcher;
            }

            return new Registration( owner );
        }

        /// <summary>
        /// Dispatch runtime event payload to plugin clients.
        /// </summary>
        /// <param name="eventData">Event payload envelope data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True when at least one registered dispatch path executed successfully.</returns>
        public static async Task<bool> DispatchAsync( EventData eventData, CancellationToken cancellationToken = default )
        {
            ArgumentNullException.ThrowIfNull( eventData );

            List<Func<EventData, CancellationToken, Task<bool>>> runtimeDispatchers;
            lock ( sync )
            {
                runtimeDispatchers = dispatchers.Values.ToList();
            }

            if ( runtimeDispatchers.Count == 0 )
            {
                return false;
            }

            var dispatched = false;
            foreach ( var runtimeDispatcher in runtimeDispatchers )
            {
                cancellationToken.ThrowIfCancellationRequested();
                dispatched = await runtimeDispatcher( eventData, cancellationToken ).ConfigureAwait( false ) || dispatched;
            }

            return dispatched;
        }

        private static void UnregisterDispatcher( object owner )
        {
            lock ( sync )
            {
                dispatchers.Remove( owner );
            }
        }
    }
}
