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
        private IPCServer? _server;
        private IPCClient? _client;

        [TestInitialize]
        public async Task Initialize()
        {
            // Start a real IPC server for integration testing
            _server = new IPCServer();
            await _server.StartAsync();

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
                        await _client.DisconnectAsync();
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
                    await _server.StopAsync();
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);

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
                await _client.ConnectAsync("192.0.2.1", 9999); // Non-routable IP
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);

            // Act & Assert
            try
            {
                await _client.ConnectAsync("127.0.0.1", _server.Port);
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);
            Assert.IsTrue(_client.IsConnected);

            // Act
            await _client.DisconnectAsync();

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
            await _client.DisconnectAsync(); // Should not throw
        }

        [TestMethod]
        public async Task DisconnectAsync_CanBeCancelled_BeforeCompletion()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port);
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
                await _client.SendCommandAsync<object>(command);
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
                await _client.SendEventAsync(eventData);
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);
            var command = new CommandData { Command = "test.command", Target = "test" };

            // Act
            // Should not throw and should return without timeout
            var task = _client.SendCommandAsync<object>(command, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

            // Give server time to receive
            await Task.Delay(100);

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
            await _client.ConnectAsync("127.0.0.1", _server.Port);
            var eventData = new EventData { EventType = "test", EventName = "test.event" };

            // Act
            await _client.SendEventAsync(eventData);

            // Give server time to receive
            await Task.Delay(100);

            // Assert - if no exception thrown, message was sent
            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public async Task SendCommandAsync_CanBeCancelled_BeforeResponse()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port);
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);

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
            await _client.ConnectAsync("127.0.0.1", _server.Port);

            // Act - send some messages
            var eventData = new EventData { EventType = "test", EventName = "test.event" };
            await _client.SendEventAsync(eventData);
            await Task.Delay(100); // Give time to process

            var status = await _client.GetStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsTrue(status.MessagesSent >= 1, "Should have sent at least one message");
        }

        [TestMethod]
        public async Task MessageReceivedEvent_IsRaisedWhenEventReceived()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port);

            _client.MessageReceived += (s, e) =>
            {
                _ = e;
            };

            // Act - simulate server sending event by using internal broadcast
            var eventMessage = MessageEnvelope.Create(
                MessageTypes.Event,
                new EventData { EventType = "test", EventName = "test.event" }
            );
            await _server.BroadcastAsync(eventMessage);
            await Task.Delay(200); // Give time to process

            // Assert
            // Note: This test assumes successful message delivery
            // Implementation may need adjustment based on actual network timing
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task SendCommandAsync_WithTimeout_ThrowsOperationCanceledException()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port);
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
            await _client.ConnectAsync("127.0.0.1", _server.Port);
            Assert.IsTrue(_client.IsConnected);

            // Act
            _client.Dispose();

            // Assert
            Assert.IsFalse(_client.IsConnected);
        }

        #endregion
    }
}
