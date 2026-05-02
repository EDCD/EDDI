#nullable enable

using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

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

        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
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
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task IPCServer_CanBeInitialized()
        {
            // Arrange
            var server = new IPCServer();

            // Act
            await server.StartAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(server.IsRunning, "Server should be running");
            Assert.IsGreaterThan(0, server.Port, "Server port should be assigned");
            Assert.IsTrue(server.Port is >= 12345 and <= 12450, "Port should be in valid range");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task DefaultEventHandler_CanBeCreated()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            // Act
            var handler = new DefaultServerEventHandler(server);

            // Assert
            Assert.IsNotNull(handler, "Handler should be created successfully");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task Handlers_CanBeRegistered()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
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
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task ServerLifecycle_StartStop_Works()
        {
            // Arrange
            var server = new IPCServer();

            // Act - Start
            await server.StartAsync( TestContext.CancellationToken );
            Assert.IsTrue(server.IsRunning, "Server should be running after start");

            // Act - Stop
            await server.StopAsync( TestContext.CancellationToken );
            Assert.IsFalse(server.IsRunning, "Server should not be running after stop");

            // Assert - Try to start again
            await server.StartAsync( TestContext.CancellationToken );
            Assert.IsTrue(server.IsRunning, "Server should restart successfully");

            // Cleanup
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task MultipleServers_CanRunConcurrently()
        {
            // Arrange
            var server1 = new IPCServer();
            var server2 = new IPCServer();

            // Act
            await server1.StartAsync( TestContext.CancellationToken );
            await server2.StartAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(server1.IsRunning, "Server 1 should be running");
            Assert.IsTrue(server2.IsRunning, "Server 2 should be running");
            Assert.AreNotEqual(server1.Port, server2.Port, "Servers should use different ports");

            // Cleanup
            await server1.StopAsync( TestContext.CancellationToken );
            await server2.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task HandleConnectAsync_SendsAcknowledgment()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

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

            var acceptTask = dummyClient.AcceptTcpClientAsync(TestContext.CancellationToken);
            var testClient = new System.Net.Sockets.TcpClient();
            await testClient.ConnectAsync(System.Net.IPAddress.Loopback, port, TestContext.CancellationToken );
            var acceptedClient = await acceptTask.ConfigureAwait(false);
            dummyClient.Stop();

            var context = new ConnectionContext(testClient);

            // Act & Assert - Should not throw
            await handler.HandleConnectAsync(connectMessage, context);

            // Cleanup
            acceptedClient.Dispose();
            context.Dispose();
            await server.StopAsync( TestContext.CancellationToken );
        }

        [TestMethod]
        [Timeout(5000, CooperativeCancellation = true )]
        public async Task StartAsync_BeginsAcceptingConnectionsImmediately()
        {
            // Arrange
            var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );

            var client = new System.Net.Sockets.TcpClient();

            try
            {
                // Act
                await client.ConnectAsync(System.Net.IPAddress.Loopback, server.Port, TestContext.CancellationToken );

                // Assert
                var deadline = DateTime.UtcNow.AddSeconds(1);
                while (DateTime.UtcNow < deadline && server.ConnectionCount == 0)
                {
                    await Task.Delay(25, TestContext.CancellationToken );
                }

                Assert.IsGreaterThan(0, server.ConnectionCount, "Server should accept client connections immediately after startup.");
            }
            finally
            {
                client.Dispose();
                await server.StopAsync( TestContext.CancellationToken );
            }
        }
    }
}
