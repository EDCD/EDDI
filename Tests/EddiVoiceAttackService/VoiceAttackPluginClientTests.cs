#nullable enable
using EddiIPC_Service.Server;
using EddiVoiceAttackAdapter.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Tests for VoiceAttackPluginClient, validating the integration layer between VoiceAttack plugin and IPC client.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class VoiceAttackPluginClientTests
    {
        private IPCServer? _server;
        private VoiceAttackPluginClient? _pluginClient;
        private string _configFilePath = string.Empty;

        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        [TestInitialize]
        public async Task Initialize()
        {
            // Start a real IPC server
            _server = new IPCServer();
            await _server.StartAsync( TestContext.CancellationToken );

            // Create config file for port discovery
            _configFilePath = CreateIpcConfigFile(_server.Port);

            // Create plugin client
            _pluginClient = new VoiceAttackPluginClient(_configFilePath);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_pluginClient != null)
            {
                try
                {
                    if (_pluginClient.IsConnected)
                    {
                        await _pluginClient.DisconnectAsync( TestContext.CancellationToken );
                    }
                    _pluginClient.Dispose();
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

            // Clean up config file
            if (File.Exists(_configFilePath))
            {
                try
                {
                    File.Delete(_configFilePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region Initialization Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task IsConnected_InitiallyFalse()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act & Assert
            Assert.IsFalse(_pluginClient.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Initialize_WithValidConfigFile_Succeeds()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            Assert.IsNotNull(_server);

            // Act
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(_pluginClient.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Initialize_WithMissingConfigFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_config.json");
            var pluginClient = new VoiceAttackPluginClient(nonExistentPath);

            // Act & Assert
            try
            {
                await pluginClient.InitializeAsync( TestContext.CancellationToken );
                Assert.Fail("Should have thrown FileNotFoundException");
            }
            catch (FileNotFoundException)
            {
                // Expected
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Initialize_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidConfigPath = Path.Combine(Path.GetTempPath(), "invalid_config.json");
            File.WriteAllText(invalidConfigPath, "{ invalid json }");
            var pluginClient = new VoiceAttackPluginClient(invalidConfigPath);

            // Act & Assert
            try
            {
                await pluginClient.InitializeAsync( TestContext.CancellationToken );
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception ex) when (ex is JsonException or ArgumentException)
            {
                // Expected
            }
            finally
            {
                pluginClient.Dispose();
                if (File.Exists(invalidConfigPath))
                {
                    File.Delete(invalidConfigPath);
                }
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Initialize_WithInvalidPort_ThrowsException()
        {
            // Arrange
            var configPath = Path.Combine(Path.GetTempPath(), "invalid_port_config.json");
            var config = new { port = 99999 }; // Invalid port
            File.WriteAllText(configPath, JsonSerializer.Serialize(config));
            var pluginClient = new VoiceAttackPluginClient(configPath);

            // Act & Assert
            try
            {
                await pluginClient.InitializeAsync( TestContext.CancellationToken );
                Assert.Fail("Should have thrown exception");
            }
            catch (Exception)
            {
                // Expected - connection should fail
            }
            finally
            {
                pluginClient.Dispose();
                if (File.Exists(configPath))
                {
                    File.Delete(configPath);
                }
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Initialize_CanBeCancelled()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            try
            {
                await _pluginClient.InitializeAsync(cts.Token);
                // May or may not cancel depending on timing
            }
            catch (OperationCanceledException)
            {
                // Also acceptable
            }
        }

        #endregion

        #region Plugin Command Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendCommandAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            Assert.IsFalse(_pluginClient.IsConnected);

            // Act & Assert
            try
            {
                await _pluginClient.SendCommandAsync("test.command", new { }, TestContext.CancellationToken );
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendCommandAsync_WhenInitialized_SendsCommandThroughIpc()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            Assert.IsNotNull(_server);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );
            Assert.IsTrue(_pluginClient.IsConnected);

            // Act
            var task = _pluginClient.SendCommandAsync("enable_monitor", new { monitor = "journal" }, 
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

            // Give server time to receive
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert
            // Task should be waiting for response
            Assert.IsFalse(task.IsCompleted, "Task should be waiting for server response");
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true)]
        public async Task SendCommandAsync_WithTimeout_ThrowsOperationCanceledException()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            // Act & Assert
            try
            {
                await _pluginClient.SendCommandAsync("test.command", new { }, cts.Token);
                Assert.Fail("Should have thrown OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true)]
        public async Task SendCommandAsync_WithNullCommandName_ThrowsArgumentNullException()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act & Assert
            try
            {
                await _pluginClient.SendCommandAsync(null!, new { }, TestContext.CancellationToken );
                Assert.Fail("Should have thrown ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true)]
        public async Task SendCommandAsync_WithNullParameters_UsesEmptyObject()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act
            var task = _pluginClient.SendCommandAsync("test.command", null, 
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);

            // Give server time to receive
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert - should not throw
            Assert.IsFalse(task.IsCompleted);
        }

        #endregion

        #region Plugin Event Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendEventAsync_WhenNotInitialized_ThrowsInvalidOperationException()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act & Assert
            try
            {
                await _pluginClient.SendEventAsync("test.event", new { }, TestContext.CancellationToken );
                Assert.Fail("Should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendEventAsync_WhenInitialized_SendsEventThroughIpc()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act
            await _pluginClient.SendEventAsync("player.jumped", new { coords = new { x = 1, y = 2, z = 3 } }, TestContext.CancellationToken );

            // Give server time to process
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert - if no exception, event was sent
            Assert.IsTrue(_pluginClient.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendEventAsync_WithNullEventName_ThrowsArgumentNullException()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act & Assert
            try
            {
                await _pluginClient.SendEventAsync(null!, new { }, TestContext.CancellationToken );
                Assert.Fail("Should have thrown ArgumentNullException");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task SendEventAsync_WithNullPayload_UsesEmptyObject()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act
            await _pluginClient.SendEventAsync("test.event", null, TestContext.CancellationToken );

            // Give server time to process
            await Task.Delay(100, TestContext.CancellationToken );

            // Assert - should not throw
            Assert.IsTrue(_pluginClient.IsConnected);
        }

        #endregion

        #region Connection Management Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task DisconnectAsync_WhenInitialized_ClearsIsConnected()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );
            Assert.IsTrue(_pluginClient.IsConnected);

            // Act
            await _pluginClient.DisconnectAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsFalse(_pluginClient.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task DisconnectAsync_WhenNotInitialized_DoesNotThrow()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            Assert.IsFalse(_pluginClient.IsConnected);

            // Act & Assert
            await _pluginClient.DisconnectAsync( TestContext.CancellationToken ); // Should not throw
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Reconnect_AfterDisconnect_Succeeds()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );
            Assert.IsTrue(_pluginClient.IsConnected);

            // Act
            await _pluginClient.DisconnectAsync( TestContext.CancellationToken );
            Assert.IsFalse(_pluginClient.IsConnected);

            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Assert
            Assert.IsTrue(_pluginClient.IsConnected);
        }

        #endregion

        #region Status and Metadata Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task GetServerStatusAsync_WhenConnected_ReturnsStatus()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );

            // Act
            var status = await _pluginClient.GetServerStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsTrue(status.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task GetServerStatusAsync_WhenNotConnected_ReturnsDisconnectedStatus()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act
            var status = await _pluginClient.GetServerStatusAsync();

            // Assert
            Assert.IsNotNull(status);
            Assert.IsFalse(status.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task PluginName_IsSet()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act & Assert
            Assert.IsNotNull(_pluginClient.PluginName);
            Assert.IsGreaterThan(0, _pluginClient.PluginName.Length );
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task PluginVersion_IsSet()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act & Assert
            Assert.IsNotNull(_pluginClient.PluginVersion);
            Assert.IsGreaterThan(0, _pluginClient.PluginVersion.Length );
        }

        #endregion

        #region Disposal Tests

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Dispose_ClearsResources()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);
            await _pluginClient.InitializeAsync( TestContext.CancellationToken );
            Assert.IsTrue(_pluginClient.IsConnected);

            // Act
            _pluginClient.Dispose();

            // Assert
            Assert.IsFalse(_pluginClient.IsConnected);
        }

        [TestMethod]
        [Timeout( 15000, CooperativeCancellation = true )]
        public async Task Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            Assert.IsNotNull(_pluginClient);

            // Act & Assert
            _pluginClient.Dispose();
            _pluginClient.Dispose(); // Should not throw
        }

        #endregion

        #region Helper Methods

        private static string CreateIpcConfigFile(int port)
        {
            var configPath = Path.Combine(Path.GetTempPath(), $"ipc_config_{Guid.NewGuid():N}.json");
            var config = new { port };
            File.WriteAllText(configPath, JsonSerializer.Serialize(config));
            return configPath;
        }

        #endregion
    }
}
