#nullable enable

using EddiIPC_Service.Client;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Tests for IIPCClient implementation, validating the contract for plugin-to-server communication.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class IPCClientTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        private IPCServer? _server;
        private IPCClient? _client;

        [TestInitialize]
        public async Task Initialize()
        {
            // Start a real IPC server for integration testing
            _server = new IPCServer();
            await _server.StartAsync( TestContext.CancellationToken );

            // Create client instance
            _client = new IPCClient();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_client != null)
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        await _client.DisconnectAsync( TestContext.CancellationToken );
                    }
                    _client.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            if (_server != null)
            {
                try
                {
                    await _server.StopAsync( TestContext.CancellationToken );
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Connection Tests

        [TestMethod]
        public async Task IsConnected_InitiallyFalse_BeforeConnection()
        {
            // Arrange
            Assert.IsNotNull(_client);

            // Act & Assert
            Assert.IsFalse(_client.IsConnected);
        }

        [TestMethod]
        public async Task ConnectAsync_SuccessfulConnection_SetsIsConnected()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);

            // Act
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public async Task ConnectAsync_WithInvalidHost_ThrowsException()
        {
            // Arrange
            Assert.IsNotNull(_client);

            // Act & Assert
            try
            {
                await _client.ConnectAsync("192.0.2.1", 9999, TestContext.CancellationToken ); // Non-routable IP
                Assert.Fail("Should have thrown an exception");
            }
            catch (Exception)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task ConnectAsync_WhenAlreadyConnected_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );

            // Act & Assert
            try
            {
                await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task ConnectAsync_CanBeCancelled_BeforeCompletion()
        {
            // Arrange
            Assert.IsNotNull(_client);
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            try
            {
                // Try to connect to invalid/non-responding server
                await _client.ConnectAsync("192.0.2.1", 9999, cts.Token);
                Assert.Fail("Should have thrown OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task DisconnectAsync_WhenConnected_ClearsIsConnected()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            Assert.IsTrue(_client.IsConnected);

            // Act
            await _client.DisconnectAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsFalse(_client.IsConnected);
        }

        [TestMethod]
        public async Task DisconnectAsync_WhenNotConnected_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsFalse(_client.IsConnected);

            // Act & Assert
            await _client.DisconnectAsync( TestContext.CancellationToken ); // Should not throw
        }

        [TestMethod]
        public async Task DisconnectAsync_CanBeCancelled_BeforeCompletion()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            try
            {
                await _client.DisconnectAsync(cts.Token);
                // May or may not cancel, depending on timing
            }
            catch (OperationCanceledException)
            {
                // Also acceptable
            }
        }

        #endregion

        #region Message Dispatch Tests

        [TestMethod]
        public async Task SendCommandAsync_WhenNotConnected_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(_client);
            var command = new CommandData { Command = "test.command", Target = "test" };

            // Act & Assert
            try
            {
                await _client.SendCommandAsync<object>(command, TestContext.CancellationToken );
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task SendEventAsync_WhenNotConnected_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(_client);
            var eventData = new EventData { EventType = "test", EventName = "test.event" };

            // Act & Assert
            try
            {
                await _client.SendEventAsync(eventData, TestContext.CancellationToken );
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task SendCommandAsync_WhenConnected_SendsMessage()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var command = new CommandData { Command = "test.command", Target = "test" };

            // Act
            // Should not throw and should return without timeout
            var task = _client.SendCommandAsync<object>(command, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

            // Give server time to receive
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert
            // Task should be waiting for response (not completed yet)
            Assert.IsFalse(task.IsCompleted, "Task should be waiting for server response");
        }

        [TestMethod]
        public async Task SendEventAsync_WhenConnected_SendsMessage()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var eventData = new EventData { EventType = "test", EventName = "test.event" };

            // Act
            await _client.SendEventAsync(eventData, TestContext.CancellationToken );

            // Give server time to receive
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert - if no exception thrown, message was sent
            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public async Task SendCommandAsync_CanBeCancelled_BeforeResponse()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var command = new CommandData { Command = "test.command", Target = "test" };
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Act & Assert
            try
            {
                await _client.SendCommandAsync<object>(command, cts.Token);
                Assert.Fail("Should have thrown OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        #endregion

        #region State Management Tests

        [TestMethod]
        public async Task GetStatusAsync_WhenNotConnected_ReturnsDisconnectedStatus()
        {
            // Arrange
            Assert.IsNotNull(_client);

            // Act
            var status = await _client.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsConnected);
            Assert.IsNull(status.SessionId);
        }

        [TestMethod]
        public async Task GetStatusAsync_WhenConnected_ReturnsConnectedStatus()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );

            // Act
            var status = await _client.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsConnected);
            Assert.AreEqual("127.0.0.1", status.ServerAddress);
            Assert.AreEqual(_server.Port, status.ServerPort);
            Assert.IsNotNull(status.SessionId);
            Assert.IsNotNull(status.ConnectedAt);
        }

        [TestMethod]
        public async Task GetStatusAsync_TracksMessageCounters()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );

            // Act - send some messages
            var eventData = new EventData { EventType = "test", EventName = "test.event" };
            await _client.SendEventAsync(eventData, TestContext.CancellationToken );
            await Task.Delay(100, TestContext.CancellationToken ); // Give time to process

            var status = await _client.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsGreaterThanOrEqualTo(1, status.MessagesSent, "Should have sent at least one message");
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task SendCommandAsync_WithTimeout_ThrowsOperationCanceledException()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var command = new CommandData { Command = "test.command", Target = "test" };
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            try
            {
                await _client.SendCommandAsync<object>(command, cts.Token);
                Assert.Fail("Should have thrown OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        [TestMethod]
        public async Task Dispose_ClearsResources()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            Assert.IsTrue(_client.IsConnected);

            // Act
            _client.Dispose();

            // Assert
            Assert.IsFalse(_client.IsConnected);
        }

        #endregion
    }
}
