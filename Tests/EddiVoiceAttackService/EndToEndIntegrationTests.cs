#nullable enable

using EddiIPC_Service.Client;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiVoiceAttackAdapter.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// End-to-end integration tests validating complete plugin-to-EDDI communication flow.
    /// Tests the entire stack: VoiceAttackPluginClient → IPCClient → IPCServer → Message Handlers.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class EndToEndIntegrationTests
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        private sealed class TestCommandDispatcher ( Action<string, IReadOnlyDictionary<string, object>?> onDispatch )
            : ICommandDispatcher
        {
            public Task DispatchAsync(string commandName, IReadOnlyDictionary<string, object>? parameters = null,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                onDispatch( commandName, parameters );
                return Task.CompletedTask;
            }
        }

        private IPCServer? _server;
        private DefaultServerEventHandler? _eventHandler;
        private string? _configFilePath;
        private readonly List<( string Command, IReadOnlyDictionary<string, object>? Parameters )> _dispatchedCommands = [];
        private readonly object _dispatchLock = new();
        private ManualResetEventSlim? _dispatchSignal;
        private ManualResetEventSlim? _responderModeSignal;
        private bool? _responderModeEnabled;
        private System.Version? _responderModeVersion;
        private IDisposable? _commandDispatcherRegistration;
        private IDisposable? _responderModeRegistration;

        [TestInitialize]
        public async Task Initialize()
        {
            // Start IPC server
            _server = new IPCServer();
            await _server.StartAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            _server.RegisterRuntimeEventDispatcher();
            
            _dispatchedCommands.Clear();
            _dispatchSignal = new ManualResetEventSlim( false );
            _responderModeSignal = new ManualResetEventSlim( false );
            _responderModeEnabled = null;
            _responderModeVersion = null;

            // Create event handler
            _eventHandler = new DefaultServerEventHandler(_server);

            // Register handlers
            _server.Router.RegisterHandler(MessageTypes.Connect, _eventHandler.HandleConnectAsync);
            _server.Router.RegisterHandler(MessageTypes.Disconnect, _eventHandler.HandleDisconnectAsync);
            _server.Router.RegisterHandler(MessageTypes.Command, _eventHandler.HandleCommandAsync);
            _server.Router.RegisterHandler(MessageTypes.Event, _eventHandler.HandleEventAsync);

            _commandDispatcherRegistration = CommandDispatcherRegistry.RegisterCommandDispatcher(new TestCommandDispatcher((commandName, parameters) =>
            {
                lock ( _dispatchLock )
                {
                    _dispatchedCommands.Add( ( commandName, parameters ) );
                }
                _dispatchSignal?.Set();
            }));

            _responderModeRegistration = ResponderModeRegistry.RegisterHandler( ( enable, voiceAttackVersion, cancellationToken ) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                _responderModeEnabled = enable;
                _responderModeVersion = voiceAttackVersion;
                _responderModeSignal?.Set();
                return Task.CompletedTask;
            } );

            // Create config file
            _configFilePath = CreateConfigFile(_server.Port);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            _commandDispatcherRegistration?.Dispose();
            _commandDispatcherRegistration = null;
            _responderModeRegistration?.Dispose();
            _responderModeRegistration = null;
            _dispatchSignal?.Dispose();
            _responderModeSignal?.Dispose();

            try
            {
                await VoiceAttackPluginHost.Instance.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            }
            catch
            {
                // Ignore
            }

            try
            {
                VoiceAttackPluginHost.Instance.Dispose();
            }
            catch
            {
                // Ignore
            }

            DeleteHostConfigFile();

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

            if (!string.IsNullOrEmpty(_configFilePath) && File.Exists(_configFilePath))
            {
                try
                {
                    File.Delete(_configFilePath);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        #region Full Integration Flow Tests

        [TestMethod, DoNotParallelize]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_PluginClient_ConnectToServer()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);

            try
            {
                // Act
                await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

                // Assert
                Assert.IsTrue(pluginClient.IsConnected);
                Assert.AreEqual("VoiceAttack IPC Plugin", pluginClient.PluginName);
                Assert.AreEqual("1.0.0", pluginClient.PluginVersion);
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_PluginClient_SendCommand_ReceiveResponse()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            try
            {
                // Act
                var response = await pluginClient.SendCommandAsync(
                    "test.command",
                    new Dictionary<string, object> { ["param1"] = "value1" },
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait( false );

                // Assert
                Assert.IsTrue(pluginClient.IsConnected);
                Assert.IsTrue( _dispatchSignal?.Wait( 3000, TestContext.CancellationToken ) ?? false, "Expected command dispatch within 3 seconds" );

                (string Command, IReadOnlyDictionary<string, object>? Parameters) dispatch;
                lock ( _dispatchLock )
                {
                    Assert.HasCount( 1, _dispatchedCommands, "Expected a single dispatched command" );
                    dispatch = _dispatchedCommands[ 0 ];
                }

                Assert.AreEqual( "test.command", dispatch.Command );
                Assert.IsNotNull( dispatch.Parameters );
                Assert.IsTrue( dispatch.Parameters!.TryGetValue( "param1", out var param1 ) );
                Assert.AreEqual( "value1", param1?.ToString() );

                Assert.IsTrue( TryReadCommandResponseStatus( response, out var status, out var message ),
                    "Expected command response payload to be readable" );
                Assert.AreEqual( "success", status );
                Assert.Contains( "executed successfully", message );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod, DoNotParallelize]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_RuntimeEventDispatcher_Broadcast_ReachesPluginClient()
        {
            // Arrange
            Assert.IsNotNull( _configFilePath );
            var pluginClient = new VoiceAttackPluginClient( _configFilePath );
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            Debug.Assert( _server != null, $"{nameof( _server )} must not be null" );
            Assert.IsGreaterThan(
                0,
                _server.ConnectionCount,
                "Expected at least one connected IPC client before runtime broadcast." );
            
            var eventReceived = new TaskCompletionSource<MessageEnvelope>( TaskCreationOptions.RunContinuationsAsynchronously );
            pluginClient.MessageReceived += ( _, args ) =>
            {
                if ( args.MessageType == MessageTypes.Event )
                {
                    eventReceived.TrySetResult( args.MessageEnvelope );
                }
            };

            try
            {
                // Act
                var dispatched = await RuntimeEventDispatcher.DispatchAsync( new EventData
                {
                    EventType = "va_runtime",
                    EventName = "command_action",
                    EventPayload = new Dictionary<string, object>
                    {
                        [ "actions" ] = new List<Dictionary<string, object>>
                        {
                            new()
                            {
                                [ "action" ] = "set_text",
                                [ "key" ] = "EDDI state 1",
                                [ "value" ] = "test"
                            },
                            new()
                            {
                                [ "action" ] = "set_boolean",
                                [ "key" ] = "EDDI state enabled",
                                [ "value" ] = true
                            }
                        }
                    }
                }, TestContext.CancellationToken ).ConfigureAwait( false );

                // Assert
                Assert.IsTrue( dispatched, "Runtime dispatcher should be registered" );

                var completed = await Task.WhenAny( eventReceived.Task, Task.Delay( 3000, TestContext.CancellationToken ) ).ConfigureAwait( false );
                Assert.AreSame( eventReceived.Task, completed, "Expected runtime event broadcast within 3 seconds" );

                var envelope = await eventReceived.Task.ConfigureAwait( false );
                Assert.AreEqual( MessageTypes.Event, envelope.Type );

                Assert.IsTrue( TryReadEventData( envelope.Data, out var eventData ),
                    "Expected event payload to deserialize to EventData" );
                Assert.AreEqual( "va_runtime", eventData!.EventType );
                Assert.AreEqual( "command_action", eventData.EventName );
                Assert.IsTrue( eventData.EventPayload.TryGetValue( "actions", out var actions ) );
                Assert.IsInstanceOfType( actions, typeof( JArray ) );
                Assert.AreEqual( 2, ((JArray)actions).Count );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_PluginClient_SendEvent_NoResponse()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            try
            {
                // Act
                await pluginClient.SendEventAsync("player.docked", 
                    new { station = "Jameson Station", system = "Sol" }, TestContext.CancellationToken ).ConfigureAwait( false );

                // Give server time to process
                await Task.Delay(200, TestContext.CancellationToken ).ConfigureAwait( false );

                // Assert
                Assert.IsTrue(pluginClient.IsConnected);
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_MultiplePluginClients_ConnectSimultaneously()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var clients = new List<VoiceAttackPluginClient>();

            try
            {
                // Act
                var tasks = new Task[3];
                for (var i = 0; i < 3; i++)
                {
                    var client = new VoiceAttackPluginClient(_configFilePath);
                    clients.Add(client);
                    tasks[i] = client.InitializeAsync( TestContext.CancellationToken );
                }

                await Task.WhenAll(tasks).ConfigureAwait( false );

                // Assert
                foreach (var client in clients)
                {
                    Assert.IsTrue(client.IsConnected);
                }
            }
            finally
            {
                foreach (var client in clients)
                {
                    client.Dispose();
                }
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_PluginClient_SendMultipleMessages()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            try
            {
                // Act & Assert
                for (var i = 0; i < 5; i++)
                {
                    await pluginClient.SendEventAsync($"test.event.{i}", new { index = i }, TestContext.CancellationToken ).ConfigureAwait( false );
                    await Task.Delay(100, TestContext.CancellationToken ).ConfigureAwait( false );
                }

                // Verify connection still active
                var status = await pluginClient.GetServerStatusAsync().ConfigureAwait( false );
                Assert.IsTrue(status.IsConnected);
                Assert.IsGreaterThanOrEqualTo(5, status.MessagesSent );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        #endregion

        #region Protocol Compliance Tests

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_MessageProtocol_ConnectAck_Contains_ServerCapabilities()
        {
            // Arrange
            Assert.IsNotNull(_server);
            var client = new IPCClient();

            try
            {
                // Act
                await client.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

                // Assert
                var status = await client.GetStatusAsync().ConfigureAwait( false );
                Assert.IsTrue(status.IsConnected);
                Assert.IsNotNull(status.SessionId);
            }
            finally
            {
                client.Dispose();
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_MessageProtocol_SessionId_Unique_PerConnection()
        {
            // Arrange
            Assert.IsNotNull(_server);
            var client1 = new IPCClient();
            var client2 = new IPCClient();

            try
            {
                // Act
                await client1.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );
                await client2.ConnectAsync("127.0.0.1", _server.Port, TestContext.CancellationToken ).ConfigureAwait( false );

                var status1 = await client1.GetStatusAsync().ConfigureAwait( false );
                var status2 = await client2.GetStatusAsync().ConfigureAwait( false );

                // Assert
                Assert.IsNotNull(status1.SessionId);
                Assert.IsNotNull(status2.SessionId);
                Assert.AreNotEqual(status1.SessionId, status2.SessionId);
            }
            finally
            {
                client1.Dispose();
                client2.Dispose();
            }
        }

        #endregion

        #region Lifecycle Tests

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_CompleteLifecycle_Initialize_Use_Disconnect()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);

            // Act & Assert
            // 1. Initialize
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsTrue(pluginClient.IsConnected);

            // 2. Use (send messages)
            await pluginClient.SendEventAsync("lifecycle.test", new { phase = "active" }, TestContext.CancellationToken ).ConfigureAwait( false );
            await Task.Delay(100, TestContext.CancellationToken ).ConfigureAwait( false );

            var status = await pluginClient.GetServerStatusAsync().ConfigureAwait( false );
            Assert.IsTrue(status.IsConnected);

            // 3. Disconnect
            await pluginClient.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsFalse(pluginClient.IsConnected);

            // 4. Verify cleanup
            status = await pluginClient.GetServerStatusAsync().ConfigureAwait( false );
            Assert.IsFalse(status.IsConnected);

            pluginClient.Dispose();
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_Reconnection_AfterDisconnect()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);

            try
            {
                // Act & Assert
                // 1. First connection
                await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                var status1 = await pluginClient.GetServerStatusAsync().ConfigureAwait( false );
                Assert.IsTrue(status1.IsConnected);

                // 2. Disconnect
                await pluginClient.DisconnectAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                Assert.IsFalse(pluginClient.IsConnected);

                // 3. Reconnect
                await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                var status2 = await pluginClient.GetServerStatusAsync().ConfigureAwait( false );
                Assert.IsTrue(status2.IsConnected);

                // Sessions should be different
                Assert.AreNotEqual(status1.SessionId, status2.SessionId);
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_InvalidConfigFile_ProperError()
        {
            // Arrange
            var invalidPath = Path.Combine(Path.GetTempPath(), $"invalid_config_{Guid.NewGuid():N}.json");
            File.WriteAllText(invalidPath, "invalid json content");
            var pluginClient = new VoiceAttackPluginClient(invalidPath);

            // Act & Assert
            try
            {
                await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                Assert.Fail("Should have thrown JsonException or ArgumentException");
            }
            catch (JsonException)
            {
                // Expected - invalid JSON
            }
            catch (ArgumentException)
            {
                // Also acceptable - missing required properties
            }
            finally
            {
                pluginClient.Dispose();
                try { File.Delete(invalidPath); } catch { /* Ignore */ }
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_ServerNotAvailable_ProperError()
        {
            // Arrange
            var configPath = Path.Combine(Path.GetTempPath(), "e2e_config.json");
            var config = new { port = 54321 }; // Port that won't respond
            File.WriteAllText(configPath, JsonSerializer.Serialize(config));

            var pluginClient = new VoiceAttackPluginClient(configPath);

            try
            {
                // Act & Assert
                try
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await pluginClient.InitializeAsync(cts.Token).ConfigureAwait( false );
                    Assert.Fail("Should have thrown exception");
                }
                catch (Exception ex) when (ex is InvalidOperationException or OperationCanceledException)
                {
                    // Expected
                }
            }
            finally
            {
                pluginClient.Dispose();
                File.Delete(configPath);
            }
        }

        #endregion

        #region Performance Tests

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_ResponseTime_Acceptable()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            try
            {
                // Act
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // Send multiple events and measure time
                for (var i = 0; i < 10; i++)
                {
                    await pluginClient.SendEventAsync($"perf.test.{i}", new { index = i }, TestContext.CancellationToken ).ConfigureAwait( false );
                }

                stopwatch.Stop();

                // Assert
                // 10 events should complete in reasonable time (< 2 seconds)
                Assert.IsLessThan( 2000, stopwatch.ElapsedMilliseconds, $"10 events took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms" );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        #endregion

        #region Helper Methods

        private static bool TryReadCommandResponseStatus( object? response, out string status, out string message )
        {
            status = string.Empty;
            message = string.Empty;

            switch ( response )
            {
                case CommandResponseData typed:
                    status = typed.Status;
                    message = typed.Message;
                    return true;
                case JObject json:
                    status = json[ nameof( CommandResponseData.Status ) ]?.ToString() ?? string.Empty;
                    message = json[ nameof( CommandResponseData.Message ) ]?.ToString() ?? string.Empty;
                    return !string.IsNullOrWhiteSpace( status );
                case IDictionary<string, object> dictionary:
                    status = dictionary.TryGetValue( nameof( CommandResponseData.Status ), out var statusObj )
                        ? statusObj.ToString() ?? string.Empty
                        : string.Empty;
                    message = dictionary.TryGetValue( nameof( CommandResponseData.Message ), out var messageObj )
                        ? messageObj.ToString() ?? string.Empty
                        : string.Empty;
                    return !string.IsNullOrWhiteSpace( status );
                default:
                    return false;
            }
        }

        private static bool TryReadEventData( object? data, out EventData? eventData )
        {
            eventData = data switch
            {
                EventData typed => typed,
                JObject json => json.ToObject<EventData>(),
                IDictionary<string, object> dictionary => JObject.FromObject( dictionary ).ToObject<EventData>(),
                _ => null
            };

            return eventData != null;
        }

        private static string CreateConfigFile(int port)
        {
            var configPath = Path.Combine(Path.GetTempPath(), $"e2e_config_{Guid.NewGuid():N}.json");
            var config = new { port };
            File.WriteAllText(configPath, JsonSerializer.Serialize(config));
            return configPath;
        }

        #endregion

        #region Responder Mode Tests

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_SetResponderMode_Command_InvokesRegisteredHandler()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

            try
            {
                _responderModeEnabled = null;
                _responderModeVersion = null;
                _responderModeSignal?.Reset();

                // Act
                var response = await pluginClient.SendCommandAsync(
                    "setrespondermode",
                    new Dictionary<string, object>
                    {
                        ["enable"] = true,
                        ["voiceAttackVersion"] = "2.1.0"
                    },
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait( false );

                // Assert
                Assert.IsTrue( _responderModeSignal?.Wait( 3000, TestContext.CancellationToken ) ?? false,
                    "Expected responder-mode handler invocation within 3 seconds" );
                Assert.IsTrue( _responderModeEnabled );
                Assert.AreEqual( new System.Version( 2, 1, 0 ), _responderModeVersion );

                Assert.IsTrue( TryReadCommandResponseStatus( response, out var status, out var message ),
                    "Expected command response payload to be readable" );
                Assert.AreEqual( "success", status );
                Assert.Contains( message, "Responder mode enabled" );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        [TestMethod]
        [Timeout(15000, CooperativeCancellation = true )]
        public async Task E2E_SetResponderMode_Command_WhenHandlerMissing_ReturnsErrorStatus()
        {
            // Arrange
            Assert.IsNotNull(_configFilePath);
            var pluginClient = new VoiceAttackPluginClient(_configFilePath);
            await pluginClient.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            _responderModeRegistration?.Dispose();
            _responderModeRegistration = null;

            try
            {
                // Act
                var response = await pluginClient.SendCommandAsync(
                    "setrespondermode",
                    new Dictionary<string, object>
                    {
                        ["enable"] = false
                    },
                    new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait( false );

                // Assert
                Assert.IsTrue( TryReadCommandResponseStatus( response, out var status, out var message ),
                    "Expected command response payload to be readable" );
                Assert.AreEqual( "error", status );
                Assert.Contains( "not registered", message );
            }
            finally
            {
                pluginClient.Dispose();
            }
        }

        #endregion

        [TestMethod]
        [Timeout(20000, CooperativeCancellation = true )]
        public async Task E2E_PluginHost_ReconnectsAfterConfigPortReplacement()
        {
            // Arrange
            Assert.IsNotNull( _server );
            WriteHostConfigFile( _server.Port );

            var host = VoiceAttackPluginHost.Instance;
            await host.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            Assert.IsNotNull( host.Client );

            var initialClient = host.Client;
            var initialStatus = await initialClient!.GetServerStatusAsync().ConfigureAwait( false );
            Assert.AreEqual( _server.Port, initialStatus.ServerPort );

            var replacementServer = new IPCServer();
            DefaultServerEventHandler? replacementEventHandler = null;

            try
            {
                await replacementServer.StartAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                replacementEventHandler = new DefaultServerEventHandler( replacementServer );
                replacementServer.Router.RegisterHandler( MessageTypes.Connect, replacementEventHandler.HandleConnectAsync );
                replacementServer.Router.RegisterHandler( MessageTypes.Disconnect, replacementEventHandler.HandleDisconnectAsync );
                replacementServer.Router.RegisterHandler( MessageTypes.Command, replacementEventHandler.HandleCommandAsync );
                replacementServer.Router.RegisterHandler( MessageTypes.Event, replacementEventHandler.HandleEventAsync );

                WriteHostConfigFile( replacementServer.Port );
                await _server.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
                _server = null;

                await WaitForConditionAsync( () => host.Client is null, "Expected plugin host to observe original server disconnect." )
                    .ConfigureAwait( false );

                // Act
                await host.InitializeAsync( TestContext.CancellationToken ).ConfigureAwait( false );

                // Assert
                Assert.IsNotNull( host.Client );
                Assert.AreNotSame( initialClient, host.Client );

                var replacementStatus = await host.Client!.GetServerStatusAsync().ConfigureAwait( false );
                Assert.IsTrue( replacementStatus.IsConnected );
                Assert.AreEqual( replacementServer.Port, replacementStatus.ServerPort );
            }
            finally
            {
                await replacementServer.StopAsync( TestContext.CancellationToken ).ConfigureAwait( false );
            }
        }

        private static async Task WaitForConditionAsync( Func<bool> condition, string failureMessage,
            int timeoutMs = 5000, int pollIntervalMs = 100 )
        {
            var deadline = DateTime.UtcNow.AddMilliseconds( timeoutMs );
            while ( DateTime.UtcNow < deadline )
            {
                if ( condition() )
                {
                    return;
                }

                await Task.Delay( pollIntervalMs ).ConfigureAwait( false );
            }

            Assert.Fail( failureMessage );
        }

        private static void WriteHostConfigFile( int port )
        {
            var configPath = GetHostConfigFilePath();
            var directory = Path.GetDirectoryName( configPath );
            if ( !string.IsNullOrEmpty( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            File.WriteAllText( configPath, JsonSerializer.Serialize( new { port } ) );
        }

        private static void DeleteHostConfigFile()
        {
            var configPath = GetHostConfigFilePath();
            if ( File.Exists( configPath ) )
            {
                File.Delete( configPath );
            }
        }

        private static string GetHostConfigFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ),
                Constants.EDDI_NAME,
                "ipc_config.json" );
        }
    }
}
