#nullable enable

using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Smoke tests for IPCServer lifecycle and basic operations.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class IPCServerTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        [TestMethod]
        public async Task StartAsync_StartsServerSuccessfully()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            await server.StartAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(server.IsRunning, "Server should be running");
            Assert.IsTrue(server.Port is >= 12345 and <= 12450, "Port should be in valid range");
            Assert.AreEqual(0, server.ConnectionCount, "Should have no connections initially");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public async Task StopAsync_StopsServerSuccessfully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            Assert.IsTrue(server.IsRunning, "Server should be running");

            // Act
            await server.StopAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsFalse(server.IsRunning, "Server should not be running after stop");
        }

        [TestMethod]
        public async Task StartAsync_Idempotent_WarnsIfAlreadyRunning()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            var port1 = server.Port;

            // Act
            await server.StartAsync( TestContext.CancellationToken ); // Call again
            var port2 = server.Port;

            // Assert
            Assert.AreEqual(port1, port2, "Port should not change on second start");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public async Task StopAsync_Idempotent_WarnsIfNotRunning()
        {
            // Arrange
            var server = new IPCServer();

            // Act & Assert - should not throw
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public async Task RouterIsAccessible()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            var router = server.Router;

            // Assert
            Assert.IsNotNull(router, "Router should be accessible");
        }

        [TestMethod]
        public async Task ConnectionCount_StartsAtZero()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            // Act
            var count = server.ConnectionCount;

            // Assert
            Assert.AreEqual(0, count, "Initial connection count should be 0");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var server = new IPCServer();

            // Act & Assert - should not throw
            server.Dispose();
        }

        [TestMethod]
        public async Task SendToConnectionAsync_HandlesInvalidSessionIdGracefully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            var message = MessageEnvelope.Create("Test", new DisconnectData { Reason = "user_shutdown" } );

            // Act & Assert - should not throw
            await server.SendToConnectionAsync("invalid-session-id", message, TestContext.CancellationToken );

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public async Task SendToConnectionAsync_ThrowsOnNullSessionId()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            var message = MessageEnvelope.Create("Test", new DisconnectData { Reason = "user_shutdown" } );

            // Act & Assert
            try
            {
                await server.SendToConnectionAsync(null!, message, TestContext.CancellationToken );
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync( TestContext.CancellationToken );
            }
        }

        [TestMethod]
        public async Task SendToConnectionAsync_ThrowsOnNullMessage()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            // Act & Assert
            try
            {
                await server.SendToConnectionAsync("test-session", null!, TestContext.CancellationToken );
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync( TestContext.CancellationToken );
            }
        }

        [TestMethod]
        public async Task BroadcastAsync_ThrowsOnNullMessage()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            // Act & Assert
            try
            {
                await server.BroadcastAsync(null!, TestContext.CancellationToken );
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync( TestContext.CancellationToken );
            }
        }

        [TestMethod]
        public async Task DisconnectAsync_HandlesInvalidSessionIdGracefully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            // Act & Assert - should not throw
            await server.DisconnectAsync("invalid-session-id");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        public async Task MultipleStartStopCycles_WorkCorrectly()
        {
            // Arrange & Act & Assert
            for (var i = 0; i < 3; i++)
            {
                var server = new IPCServer();
                await server.StartAsync( TestContext.CancellationToken );
            
                Assert.IsTrue(server.IsRunning, $"Cycle {i}: Server should be running");
            
                await server.StopAsync( TestContext.CancellationToken );
            
                Assert.IsFalse(server.IsRunning, $"Cycle {i}: Server should be stopped");
            
                server.Dispose();
            }
        }
    }
}
