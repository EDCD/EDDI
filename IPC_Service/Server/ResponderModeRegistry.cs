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
        private static readonly object sync = new();
        private static Func<bool, Version?, CancellationToken, Task>? handler;

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
        /// </summary>
        /// <param name="responderModeHandler">Handler that toggles responder mode.</param>
        public static void RegisterHandler( Func<bool, Version?, CancellationToken, Task> responderModeHandler )
        {
            ArgumentNullException.ThrowIfNull( responderModeHandler );

            lock ( sync )
            {
                handler = responderModeHandler;
            }
        }

        /// <summary>
        /// Clears responder mode handler registration.
        /// </summary>
        public static void ClearHandler()
        {
            lock ( sync )
            {
                handler = null;
            }
        }
    }
}
