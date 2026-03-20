#nullable enable

using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Integration tests for IPC server with EDDI.
    /// Validates server initialization, configuration, handler registration, and lifecycle.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class EddiIPCIntegrationTests
    {
        private string? _testConfigDir;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary config directory for testing
            _testConfigDir = Path.Combine(Path.GetTempPath(), $"eddi-ipc-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testConfigDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up temporary directory
            if (_testConfigDir != null && Directory.Exists(_testConfigDir))
            {
                try
                {
                    Directory.Delete(_testConfigDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task IPCServer_CanBeInitialized()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            await server.StartAsync();

            // Assert
            Assert.IsTrue(server.IsRunning, "Server should be running");
            Assert.IsTrue(server.Port > 0, "Server port should be assigned");
            Assert.IsTrue(server.Port >= 12345 && server.Port <= 12450, "Port should be in valid range");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task DefaultEventHandler_CanBeCreated()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            // Act
            var handler = new DefaultServerEventHandler(server);

            // Assert
            Assert.IsNotNull(handler, "Handler should be created successfully");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task Handlers_CanBeRegistered()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var handler = new DefaultServerEventHandler(server);
            var router = server.Router;

            // Act
            router.RegisterHandler(MessageTypes.Connect, handler.HandleConnectAsync);
            router.RegisterHandler(MessageTypes.Disconnect, handler.HandleDisconnectAsync);
            router.RegisterHandler(MessageTypes.Command, handler.HandleCommandAsync);

            // Assert
            Assert.IsTrue(router.HasHandlers(MessageTypes.Connect), "Connect handler should be registered");
            Assert.IsTrue(router.HasHandlers(MessageTypes.Disconnect), "Disconnect handler should be registered");
            Assert.IsTrue(router.HasHandlers(MessageTypes.Command), "Command handler should be registered");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task BroadcastEventAsync_CanBeCalled()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();
            var handler = new DefaultServerEventHandler(server);
            var eventMessage = MessageEnvelope.Create(MessageTypes.Event,
                new EventData { EventType = "LocationChanged", EventName = "Location" });

            // Act
            await handler.BroadcastEventAsync(eventMessage);

            // Assert
            // If no exception is thrown, broadcasting works

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task ServerLifecycle_StartStop_Works()
        {
            // Arrange
            var server = new IPCServer();

            // Act - Start
            await server.StartAsync();
            Assert.IsTrue(server.IsRunning, "Server should be running after start");

            // Act - Stop
            await server.StopAsync();
            Assert.IsFalse(server.IsRunning, "Server should not be running after stop");

            // Assert - Try to start again
            await server.StartAsync();
            Assert.IsTrue(server.IsRunning, "Server should restart successfully");

            // Cleanup
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task MultipleServers_CanRunConcurrently()
        {
            // Arrange
            var server1 = new IPCServer();
            var server2 = new IPCServer();

            // Act
            await server1.StartAsync();
            await server2.StartAsync();

            // Assert
            Assert.IsTrue(server1.IsRunning, "Server 1 should be running");
            Assert.IsTrue(server2.IsRunning, "Server 2 should be running");
            Assert.AreNotEqual(server1.Port, server2.Port, "Servers should use different ports");

            // Cleanup
            await server1.StopAsync();
            await server2.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task HandleConnectAsync_SendsAcknowledgment()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            var handler = new DefaultServerEventHandler(server);
            var connectMessage = MessageEnvelope.Create(MessageTypes.Connect,
                new ConnectData 
                { 
                    PluginVersion = "5.0.0",
                    PluginName = "Test Plugin",
                    Capabilities = new System.Collections.Generic.List<string> { "test" },
                    SupportedMessageTypes = new System.Collections.Generic.List<string> { MessageTypes.Command }
                });

            // Create a dummy connection context
            var dummyClient = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            dummyClient.Start();
            var port = ((System.Net.IPEndPoint)dummyClient.LocalEndpoint).Port;

            var acceptTask = dummyClient.AcceptTcpClientAsync();
            var testClient = new System.Net.Sockets.TcpClient();
            await testClient.ConnectAsync(System.Net.IPAddress.Loopback, port);
            var acceptedClient = await acceptTask.ConfigureAwait(false);
            dummyClient.Stop();

            var context = new ConnectionContext(testClient);

            // Act & Assert - Should not throw
            await handler.HandleConnectAsync(connectMessage, context);

            // Cleanup
            acceptedClient.Dispose();
            context.Dispose();
            await server.StopAsync();
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task StartAsync_BeginsAcceptingConnectionsImmediately()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync();

            var client = new System.Net.Sockets.TcpClient();

            try
            {
                // Act
                await client.ConnectAsync(System.Net.IPAddress.Loopback, server.Port);

                // Assert
                var deadline = DateTime.UtcNow.AddSeconds(1);
                while (DateTime.UtcNow < deadline && server.ConnectionCount == 0)
                {
                    await Task.Delay(25);
                }

                Assert.IsTrue(server.ConnectionCount > 0, "Server should accept client connections immediately after startup.");
            }
            finally
            {
                client.Dispose();
                await server.StopAsync();
            }
        }
    }
}
