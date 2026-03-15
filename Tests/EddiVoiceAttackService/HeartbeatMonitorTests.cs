#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackService.Server;
using EddiVoiceAttackService.Heartbeat;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for HeartbeatMonitor.
    /// </summary>
    [TestClass]
    public class HeartbeatMonitorTests
    {
        [TestMethod]
        public async Task StartAsync_StartsMonitorSuccessfully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var monitor = new HeartbeatMonitor(server);

            // Act
            await monitor.StartAsync();
            await Task.Delay(100); // Allow monitor to start

            // Assert
            // If it doesn't throw, it started successfully
        
            // Cleanup
            await monitor.StopAsync();
            await server.StopAsync();
        }

        [TestMethod]
        public async Task StopAsync_StopsMonitorSuccessfully()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var monitor = new HeartbeatMonitor(server);
            await monitor.StartAsync();
            await Task.Delay(50);

            // Act
            await monitor.StopAsync();

            // Assert
            // If it doesn't throw, it stopped successfully
        
            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public async Task StartAsync_Idempotent_WarnsIfAlreadyRunning()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var monitor = new HeartbeatMonitor(server);
            await monitor.StartAsync();
            await Task.Delay(50);

            // Act
            await monitor.StartAsync(); // Call again
            await Task.Delay(50);

            // Assert
            // If it doesn't throw, it handled idempotency gracefully
        
            // Cleanup
            await monitor.StopAsync();
            await server.StopAsync();
        }

        [TestMethod]
        public async Task StopAsync_Idempotent_WarnsIfNotRunning()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var monitor = new HeartbeatMonitor(server);

            // Act & Assert - should not throw
            await monitor.StopAsync();

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        public void Constructor_ThrowsOnNullServer()
        {
            // Act & Assert
            try
            {
                var monitor = new HeartbeatMonitor(null!);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var server = new IPCServer();
            var monitor = new HeartbeatMonitor(server);

            // Act & Assert
            monitor.Dispose();

            // Cleanup
            server.Dispose();
        }

        [TestMethod]
        public async Task MultipleStartStopCycles_WorkCorrectly()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act & Assert
            for (int i = 0; i < 3; i++)
            {
                var monitor = new HeartbeatMonitor(server);
                await monitor.StartAsync();
                await Task.Delay(100);
            
                await monitor.StopAsync();
                monitor.Dispose();
            }

            // Cleanup
            await server.StopAsync();
        }
    }
}
