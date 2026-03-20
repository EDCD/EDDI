#nullable enable

using System;

namespace EddiIPC_Service.Server
{
    /// <summary>
    /// Registry for the active IPC command dispatcher.
    /// </summary>
    public static class CommandDispatcherRegistry
    {
        private static readonly object sync = new();
        private static ICommandDispatcher? commandDispatcher;

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
        /// </summary>
        /// <param name="dispatcher">Dispatcher instance.</param>
        public static void RegisterCommandDispatcher(ICommandDispatcher dispatcher)
        {
            ArgumentNullException.ThrowIfNull(dispatcher);

            lock ( sync )
            {
                commandDispatcher = dispatcher;
            }
        }

        /// <summary>
        /// Clears the current command dispatcher registration.
        /// </summary>
        public static void ClearCommandDispatcher()
        {
            lock ( sync )
            {
                commandDispatcher = null;
            }
        }
    }
}
