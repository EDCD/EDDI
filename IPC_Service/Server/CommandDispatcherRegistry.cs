#nullable enable

using System;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Registry for the active IPC command dispatcher.
    /// </summary>
    public static class CommandDispatcherRegistry
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

                UnregisterCommandDispatcher( _owner );
                _disposed = true;
            }
        }

        private static readonly object sync = new();
        private static ICommandDispatcher? commandDispatcher;
        private static object? owner;

        /// <summary>
        /// Gets the current command dispatcher.
        /// </summary>
        public static ICommandDispatcher? CommandDispatcher
        {
            get
            {
                lock ( sync )
                {
                    return commandDispatcher;
                }
            }
        }

        /// <summary>
        /// Registers the command dispatcher used by the IPC server.
        /// Dispose the returned handle to clear the dispatcher registered by this call if it is still active.
        /// </summary>
        /// <param name="dispatcher">Dispatcher instance.</param>
        /// <returns>An owned registration handle.</returns>
        public static IDisposable RegisterCommandDispatcher( ICommandDispatcher dispatcher )
        {
            ArgumentNullException.ThrowIfNull( dispatcher );

            var registrationOwner = new object();
            lock ( sync )
            {
                commandDispatcher = dispatcher;
                owner = registrationOwner;
            }

            return new Registration( registrationOwner );
        }

        private static void UnregisterCommandDispatcher( object registrationOwner )
        {
            lock ( sync )
            {
                if ( !ReferenceEquals( owner, registrationOwner ) )
                {
                    return;
                }

                commandDispatcher = null;
                owner = null;
            }
        }
    }
}
