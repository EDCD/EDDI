#nullable enable

using EddiIPC_Service.Client;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
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
            try
            {
                if ( _client is { IsConnected: true } )
                {
                    await _client.DisconnectAsync( CancellationToken.None );
                }
            }
            finally
            {
                try
                {
                    _client?.Dispose();
                }
                finally
                {
                    _server?.Dispose();
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
        public async Task ConnectAsync_WithRefusedLoopbackConnection_LeavesClientDisconnected()
        {
            Assert.IsNotNull(_client);
            using var socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
            socket.Bind( new IPEndPoint( IPAddress.Loopback, 0 ) );
            var port = ((IPEndPoint)socket.LocalEndPoint!).Port;

            var exception = await Assert.ThrowsExactlyAsync<SocketException>( () =>
                _client.ConnectAsync( "127.0.0.1", port, TestContext.CancellationToken ) );

            Assert.AreEqual( SocketError.ConnectionRefused, exception.SocketErrorCode );
            Assert.IsFalse( _client.IsConnected );
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
            Assert.IsNotNull( _server );
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>( () =>
                _client.ConnectAsync( "127.0.0.1", _server.Port, cts.Token ) );

            Assert.IsFalse( _client.IsConnected );
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
        public async Task DisconnectAsync_WithCancelledToken_StillCleansUpConnection()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await _client.DisconnectAsync( cts.Token );

            Assert.IsFalse( _client.IsConnected );
            var status = await _client.GetStatusAsync();
            Assert.IsFalse( status.IsConnected );
            Assert.IsNull( status.ConnectedAt );
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
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            var received = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _server.Router.UnregisterHandlers( MessageTypes.Command );
            _server.Router.RegisterHandler( MessageTypes.Command, ( _, _ ) =>
            {
                received.TrySetResult();
                return Task.CompletedTask;
            } );
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken );
            var command = new CommandData { Command = "test.command", Target = "test" };
            using var cts = new CancellationTokenSource();
            var pendingCommand = _client.SendCommandAsync<object>( command, cts.Token );

            try
            {
                await received.Task.WaitAsync( TimeSpan.FromSeconds( 5 ), TestContext.CancellationToken );
                Assert.IsFalse( pendingCommand.IsCompleted, "Command must wait for a response before cancellation." );
                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>( () =>
                    pendingCommand.WaitAsync( TimeSpan.FromSeconds( 5 ), TestContext.CancellationToken ) );
                Assert.IsTrue( pendingCommand.IsCanceled );
                Assert.IsTrue( _client.IsConnected );
            }
            finally
            {
                cts.Cancel();
                // Observe the pending operation even when an earlier assertion fails.
                try { await pendingCommand.WaitAsync( TimeSpan.FromSeconds( 5 ) ); }
                catch ( OperationCanceledException ) { }
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
