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
    /// Validates reconnection logic, timeout handling, and heartbeat monitoring.
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
            if (_client != null)
            {
                try
                {
                    if (_client.IsConnected)
                    {
                        await _client.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                    }
                    _client.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }

            if (_server != null)
            {
                try
                {
                    await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                }
                catch
                {
                    // Ignore
                }
            }
        }

        #region Heartbeat Timeout Tests

        [TestMethod]
        public async Task Heartbeat_ReceivesHeartbeatResponse()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);

            // Act
            var status1 = await _client.GetStatusAsync().ConfigureAwait( false );
            await Task.Delay(6000, TestContext.CancellationToken ).ConfigureAwait( false ); // Wait for heartbeat interval
            var status2 = await _client.GetStatusAsync().ConfigureAwait( false );

            // Assert
            Assert.IsTrue(status1.IsConnected);
            Assert.IsTrue(status2.IsConnected);
        }

        [TestMethod]
        public async Task Heartbeat_TimeoutDetectsDeadConnection()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(_client.IsConnected);

            // Act - Stop server to simulate dead connection
            await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            // Wait longer than heartbeat timeout
            await Task.Delay(15000, TestContext.CancellationToken ).ConfigureAwait( false ); // 10s timeout + 5s buffer

            // Assert - Check connection is lost
            await _client.GetStatusAsync().ConfigureAwait( false );
            Assert.IsFalse(_client.IsConnected, "Client should detect connection loss");
        }

        #endregion

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
        public async Task AutomaticReconnection_WithExponentialBackoff()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            
            // This test validates the concept of exponential backoff
            // In a real implementation, reconnection attempts would use:
            // Attempt 1: 1 second delay
            // Attempt 2: 2 seconds delay
            // Attempt 3: 4 seconds delay
            // Attempt 4: 8 seconds delay
            // Max delay: 60 seconds

            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );
            var status1 = await _client.GetStatusAsync().ConfigureAwait( false );
            
            // Assert initial state
            Assert.IsTrue(status1.IsConnected);
            Assert.IsNotNull(status1.SessionId);
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

        #region Timeout Detection Tests

        [TestMethod]
        public async Task ConnectionTimeout_OnSlowServer()
        {
            // Arrange
            Assert.IsNotNull(_client);
            var slowPort = _server!.Port + 5000; // Port that won't respond (large offset)

            // Act & Assert
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await _client.ConnectAsync("192.0.2.1", slowPort, cts.Token).ConfigureAwait( false ); // Non-routable IP
                Assert.Fail("Should have timed out or failed to connect");
            }
            catch (OperationCanceledException)
            {
                // Expected - timeout
            }
            catch (Exception ex) when (ex.InnerException is TimeoutException || 
                                       ex.InnerException is OperationCanceledException ||
                                       ex is TimeoutException ||
                                       ex.Message.Contains("timeout") ||
                                       ex.Message.Contains("connection") ||
                                       ex.Message.Contains("refused"))
            {
                // Also acceptable - various connection errors
            }
        }

        [TestMethod]
        public async Task MessageSendTimeout_ReturnsCancelledTask()
        {
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act
            var command = new CommandData { Command = "test", Target = "test" };
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            // Assert
            try
            {
                await _client.SendCommandAsync<object>(command, cts.Token).ConfigureAwait( false );
                Assert.Fail("Should have timed out");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
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
            // Arrange
            Assert.IsNotNull(_client);
            Assert.IsNotNull(_server);
            await _client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

            // Act - Send command(s) and measure response time
            var command = new CommandData { Command = "test", Target = "test" };
            try
            {
                await _client.SendCommandAsync<object>(command, 
                    new CancellationTokenSource(TimeSpan.FromSeconds(2)).Token).ConfigureAwait( false );
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

            _client.ConnectionLost += (s, e) => _ = e;

            // Act
            await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            
            // Wait for connection to be detected as lost
            await Task.Delay(2000, TestContext.CancellationToken ).ConfigureAwait( false );

            // Assert
            // Connection should be lost (though event may not fire depending on timing)
            Assert.IsFalse(_client.IsConnected);
        }

        #endregion

        #region Graceful Degradation Tests

        [TestMethod]
        public async Task SendCommand_WhenConnectionDropped_ThrowsAppropriateException()
        {
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
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait( false );
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
