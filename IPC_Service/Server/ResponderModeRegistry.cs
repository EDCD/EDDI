#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Registry for responder mode handling used by IPC SetResponderMode commands.
    /// </summary>
    public static class ResponderModeRegistry
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

                UnregisterHandler( _owner );
                _disposed = true;
            }
        }

        private static readonly object sync = new();
        private static Func<bool, Version?, CancellationToken, Task>? handler;
        private static object? owner;

        /// <summary>
        /// Gets the registered responder mode handler.
        /// </summary>
        public static Func<bool, Version?, CancellationToken, Task>? Handler
        {
            get
            {
                lock ( sync )
                {
                    return handler;
                }
            }
        }

        /// <summary>
        /// Registers responder mode handler.
        /// Dispose the returned handle to clear the handler registered by this call if it is still active.
        /// </summary>
        /// <param name="responderModeHandler">Handler that toggles responder mode.</param>
        /// <returns>An owned registration handle.</returns>
        public static IDisposable RegisterHandler( Func<bool, Version?, CancellationToken, Task> responderModeHandler )
        {
            ArgumentNullException.ThrowIfNull( responderModeHandler );

            var registrationOwner = new object();
            lock ( sync )
            {
                handler = responderModeHandler;
                owner = registrationOwner;
            }

            return new Registration( registrationOwner );
        }

        private static void UnregisterHandler( object registrationOwner )
        {
            lock ( sync )
            {
                if ( !ReferenceEquals( owner, registrationOwner ) )
                {
                    return;
                }

                handler = null;
                owner = null;
            }
        }
    }
}
