using System;
using System.Net;
using System.Net.Sockets;
using Utilities;

namespace EddiVoiceAttackService.IPC
{
    /// <summary>
    /// Automatically selects an available TCP port for IPC server.
    /// </summary>
    public static class IPCPortSelector
    {
        private const int DefaultPort = 12345;
        private const int MaxPortAttempts = 100;

        /// <summary>
        /// Selects an available port for IPC server.
        /// </summary>
        /// <returns>First available port in the search range</returns>
        /// <exception cref="InvalidOperationException">Thrown if no available port found</exception>
        public static int SelectAvailablePort ()
        {
            Logging.Info( $"Searching for available IPC port (starting from {DefaultPort})..." );

            for ( var offset = 0; offset < MaxPortAttempts; offset++ )
            {
                var candidatePort = DefaultPort + offset;
                if ( IsPortAvailable( candidatePort ) )
                {
                    Logging.Info( $"IPC Server will listen on port {candidatePort}" );
                    return candidatePort;
                }
            }

            throw new InvalidOperationException(
                $"Could not find available IPC port in range {DefaultPort}-{DefaultPort + MaxPortAttempts}" );
        }

        /// <summary>
        /// Checks if a port is available for binding.
        /// </summary>
        /// <param name="port">Port number to test</param>
        /// <returns>True if port is available; false otherwise</returns>
        private static bool IsPortAvailable ( int port )
        {
            try
            {
                using ( var listener = new TcpListener( IPAddress.Loopback, port ) )
                {
                    listener.Start();
                    listener.Stop();
                    return true;
                }
            }
            catch ( SocketException )
            {
                return false;
            }
            catch ( Exception ex )
            {
                Logging.Debug( $"Unexpected error checking port {port}: {ex.Message}" );
                return false;
            }
        }
    }
}
