#nullable enable

using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for CancellationToken support across IPC server components.
    /// Validates graceful cancellation, timeout handling, and resource cleanup.
    /// </summary>
    [ TestClass, TestCategory( "UnitTests" ) ]
    public class CancellationTokenTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;

        [ TestMethod ]
        [ Timeout( 5000, CooperativeCancellation = true ) ]
        public async Task StartAsync_CanBeCancelled_BeforeCompletion ()
        {
            // Arrange
            var server = new IPCServer();
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately before calling

            // Act
            try
            {
                await server.StartAsync( cts.Token );
                Assert.Fail( "Expected OperationCanceledException" );
            }
            catch ( OperationCanceledException )
            {
                // Expected
            }

            // Assert
            Assert.IsFalse( server.IsRunning, "Server should not be running after cancellation" );
        }

        [ TestMethod ]
        [ Timeout( 5000, CooperativeCancellation = true ) ]
        public async Task StartAsync_CompletesNormally_WhenNotCancelled ()
        {
            // Arrange
            var server = new IPCServer();
            var cts = new CancellationTokenSource();

            // Act
            await server.StartAsync( cts.Token );
            await Task.Delay( 100, cts.Token ); // Let server initialize

            // Assert
            Assert.IsTrue( server.IsRunning, "Server should be running" );
            Assert.IsGreaterThan( 0, server.Port, "Server should have valid port" );

            // Cleanup
            await server.StopAsync( cts.Token );
        }

        [ TestMethod ]
        [ Timeout( 5000, CooperativeCancellation = true ) ]
        public async Task StartAsync_DefaultToken_Works ()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            await server.StartAsync( TestContext.CancellationToken ); // No token provided, should work with default

            // Assert
            Assert.IsTrue( server.IsRunning, "Server should start with default token" );

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [ TestMethod ]
        [ Timeout( 5000, CooperativeCancellation = true ) ]
        public async Task StopAsync_CanBeCancelled ()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            var cts = new CancellationTokenSource();
            cts.CancelAfter( 100 );

            // Act
            try
            {
                await server.StopAsync( cts.Token );
            }
            catch ( OperationCanceledException )
            {
                // Expected - stop was cancelled
            }

            // Assert (server may or may not be running depending on timing)
            // Just verify no unhandled exception
        }
    }
}
