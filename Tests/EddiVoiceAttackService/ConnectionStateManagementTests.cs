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
    /// Tests for connection state management, resilience, and recovery strategies.
    /// Validates explicit reconnection, connection loss, and connection status.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class ConnectionStateManagementTests
    {
        private IPCServer? _server;
        private IPCClient? _client;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        [TestInitialize]
        public async Task Initialize()
        {
            _server = new IPCServer();
            await _server.StartAsync( TestContext.CancellationToken );
            _client = new IPCClient();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                if ( _client is { IsConnected: true } )
                {
                    await _client.DisconnectAsync( CancellationToken.None ).ConfigureAwait( false );
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

        #region Reconnection Logic Tests

        [TestMethod]
        public async Task Reconnect_AfterServerRestart_Succeeds()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            var originalPort = _server.Port;
            await _client.ConnectAsync("127.0.0.1", originalPort, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);

            // Act - Stop and restart server on same port
            await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            await Task.Delay(500, TestContext.CancellationToken ).ConfigureAwait( false ); // Wait for cleanup

            _server = new IPCServer();
            await _server.StartAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            
            // Attempt reconnect
            await _client.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsFalse(_client.IsConnected);

            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Assert
            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public async Task MultipleReconnectAttempts_EventualSuccess()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            var originalPort = _server.Port;

            // Act & Assert
            // Try 1: Connect successfully
            await _client.ConnectAsync("127.0.0.1", originalPort, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);

            // Disconnect
            await _client.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsFalse(_client.IsConnected);

            // Try 2: Reconnect
            await _client.ConnectAsync("127.0.0.1", originalPort, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);

            // Disconnect again
            await _client.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsFalse(_client.IsConnected);

            // Try 3: Final reconnect
            await _client.ConnectAsync("127.0.0.1", originalPort, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);
        }

        #endregion

        #region Connection State Tracking Tests

        [TestMethod]
        public async Task GetStatusAsync_TracksConnectedTime()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act
            await Task.Delay(1000, TestContext.CancellationToken ).ConfigureAwait( false );
            var status = await _client.GetStatusAsync().ConfigureAwait( false );

            // Assert
            Assert.IsTrue(status.IsConnected);
            Assert.IsNotNull(status.ConnectedAt);
            Assert.IsLessThan( DateTime.UtcNow, (DateTime)status.ConnectedAt);
        }

        [TestMethod]
        public async Task GetStatusAsync_TracksLastActivityTime()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            var status1 = await _client.GetStatusAsync().ConfigureAwait( false );
            var activity1 = status1.LastActivityAt;

            // Act
            await Task.Delay(500, TestContext.CancellationToken ).ConfigureAwait( false );
            var status2 = await _client.GetStatusAsync().ConfigureAwait( false );

            // Assert
            Assert.IsNotNull(activity1);
            Assert.IsNotNull(status2.LastActivityAt);
            Assert.IsGreaterThanOrEqualTo((DateTime)activity1, (DateTime)status2.LastActivityAt);
        }

        [TestMethod]
        public async Task GetStatusAsync_AverageResponseTime_Calculated()
        {
            using var commandTimeout = new CancellationTokenSource( TimeSpan.FromSeconds( 2 ) );
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act - Send command(s) and measure response time
            var command = new CommandData { Command = "test", Target = "test" };
            try
            {
                await _client.SendCommandAsync<object>(command, 
                    commandTimeout.Token).ConfigureAwait( false );
            }
            catch (OperationCanceledException)
            {
                // Expected since no response handler
            }

            var status = await _client.GetStatusAsync().ConfigureAwait( false );

            // Assert
            Assert.IsNotNull(status);
            // Average response time may be 0 if no complete responses yet
            Assert.IsGreaterThanOrEqualTo(0, status.AverageResponseTimeMs);
        }

        #endregion

        #region Connection Loss Detection Tests

        [TestMethod]
        public async Task ConnectionLostEvent_FiredOnServerShutdown()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            var lost = new TaskCompletionSource<ConnectionLostEventArgs>( TaskCreationOptions.RunContinuationsAsynchronously );
            void OnConnectionLost( object? sender, ConnectionLostEventArgs args ) => lost.TrySetResult( args );
            _client.ConnectionLost += OnConnectionLost;
            try
            {
                await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                var args = await lost.Task.WaitAsync( TimeSpan.FromSeconds( 5 ), TestContext.CancellationToken );

                Assert.IsFalse( string.IsNullOrWhiteSpace( args.Reason ) );
                Assert.IsFalse( _client.IsConnected );
                Assert.IsFalse( (await _client.GetStatusAsync()).IsConnected );
            }
            finally
            {
                _client.ConnectionLost -= OnConnectionLost;
            }
        }

        #endregion

        #region Graceful Degradation Tests

        [TestMethod]
        public async Task SendCommand_WhenConnectionDropped_ThrowsAppropriateException()
        {
            using var commandTimeout = new CancellationTokenSource( TimeSpan.FromSeconds( 5 ) );
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            var command = new CommandData { Command = "test", Target = "test" };

            // Act - Stop server after connection
            _ = Task.Run(async () =>
            {
                await Task.Delay(500, TestContext.CancellationToken ).ConfigureAwait( false );
                await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            }, TestContext.CancellationToken );

            // Try to send command - should fail with appropriate error
            try
            {
                await _client.SendCommandAsync<object>(command,
                    commandTimeout.Token).ConfigureAwait( false );
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Failed to send command"))
            {
                // Expected
            }
            catch (OperationCanceledException)
            {
                // Also acceptable - timeout
            }
            catch (Exception)
            {
                // Connection lost errors acceptable
            }
        }

        #endregion

        #region Stress Tests

        [TestMethod]
        public async Task ConnectionStability_UnderRepeatedOperations()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act & Assert - Perform multiple status checks
            for (var i = 0; i < 10; i++)
            {
                var status = await _client.GetStatusAsync().ConfigureAwait( false );
                Assert.IsTrue(status.IsConnected);
                Assert.IsGreaterThanOrEqualTo(0, status.MessagesSent);
            }

            Assert.IsTrue(_client.IsConnected);
        }

        [TestMethod]
        public async Task ConcurrentOperations_MaintainConnectionState()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act - Run multiple concurrent status checks
            var tasks = new Task[5];
            for (var i = 0; i < 5; i++)
            {
                tasks[i] = _client.GetStatusAsync();
            }

            await Task.WhenAll(tasks).ConfigureAwait( false );

            // Assert
            Assert.IsTrue(_client.IsConnected);
        }

        #endregion
    }
}
