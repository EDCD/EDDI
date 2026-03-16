#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackService.Server;
using EddiVoiceAttackService.Messages;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Smoke tests for IPCServer lifecycle and basic operations.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class IPCServerTests
    {
        [TestMethod]
        public async Task StartAsync_StartsServerSuccessfully()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            await server.StartAsync();

            // Assert
            Assert.IsTrue(server.IsRunning, "Server should be running");
            Assert.IsTrue(server.Port >= 12345 && server.Port <= 12450, "Port should be in valid range");
            Assert.AreEqual(0, server.ConnectionCount, "Should have no connections initially");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task StopAsync_StopsServerSuccessfully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            Assert.IsTrue(server.IsRunning, "Server should be running");

            // Act
            await server.StopAsync();

            // Assert
            Assert.IsFalse(server.IsRunning, "Server should not be running after stop");
        }

        [TestMethod]
        public async Task StartAsync_Idempotent_WarnsIfAlreadyRunning()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var port1 = server.Port;

            // Act
            await server.StartAsync(); // Call again
            var port2 = server.Port;

            // Assert
            Assert.AreEqual(port1, port2, "Port should not change on second start");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task StopAsync_Idempotent_WarnsIfNotRunning()
        {
            // Arrange
            var server = new IPCServer();

            // Act & Assert - should not throw
            await server.StopAsync();
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
        public async Task BroadcastAsync_WorksWithNoConnections()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

            // Act & Assert - should not throw
            await server.BroadcastAsync(message);

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task ConnectionCount_StartsAtZero()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act
            var count = server.ConnectionCount;

            // Assert
            Assert.AreEqual(0, count, "Initial connection count should be 0");

            // Cleanup
            await server.StopAsync();
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
            await server.StartAsync();
            var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

            // Act & Assert - should not throw
            await server.SendToConnectionAsync("invalid-session-id", message);

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task SendToConnectionAsync_ThrowsOnNullSessionId()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var message = MessageEnvelope.Create("Test", new HeartbeatData { Status = "alive", UptimeMs = 0 });

            // Act & Assert
            try
            {
                await server.SendToConnectionAsync(null!, message);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync();
            }
        }

        [TestMethod]
        public async Task SendToConnectionAsync_ThrowsOnNullMessage()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act & Assert
            try
            {
                await server.SendToConnectionAsync("test-session", null!);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync();
            }
        }

        [TestMethod]
        public async Task BroadcastAsync_ThrowsOnNullMessage()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act & Assert
            try
            {
                await server.BroadcastAsync(null!);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
            finally
            {
                // Cleanup
                await server.StopAsync();
            }
        }

        [TestMethod]
        public async Task DisconnectAsync_HandlesInvalidSessionIdGracefully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act & Assert - should not throw
            await server.DisconnectAsync("invalid-session-id");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task MultipleStartStopCycles_WorkCorrectly()
        {
            // Arrange & Act & Assert
            for (int i = 0; i < 3; i++)
            {
                var server = new IPCServer();
                await server.StartAsync();
            
                Assert.IsTrue(server.IsRunning, $"Cycle {i}: Server should be running");
            
                await server.StopAsync();
            
                Assert.IsFalse(server.IsRunning, $"Cycle {i}: Server should be stopped");
            
                server.Dispose();
            }
        }
    }
}
