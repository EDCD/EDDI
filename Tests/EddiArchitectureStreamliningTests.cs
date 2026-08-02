using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.EventHandling;
using EddiCore.PluginHosting;
using EddiDataDefinitions;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EddiArchitectureStreamliningTests : TestBase
    {
        private sealed class HostTestMonitor ( string name, bool required = false, bool needsStart = false ) : IEddiMonitor
        {
            private readonly ManualResetEventSlim _started = new();
            private readonly ManualResetEventSlim _stopped = new();

            public int StartCount { get; private set; }
            public int StopCount { get; private set; }
            public int ReloadCount { get; private set; }
            public int StatusCount { get; private set; }
            public int ProfileCount { get; private set; }

            public string MonitorName () => name;
            public string LocalizedMonitorName () => name;
            public string MonitorDescription () => "Test monitor";
            public bool IsRequired () => required;
            public bool NeedsStart () => needsStart;

            public void Start ()
            {
                StartCount++;
                _started.Set();
                if ( needsStart )
                {
                    _stopped.Wait();
                }
            }

            public void Stop ()
            {
                StopCount++;
                _stopped.Set();
            }

            public void Reload () => ReloadCount++;
            public Task PreHandleAsync ( Event @event ) => Task.CompletedTask;
            public Task PostHandleAsync ( Event @event ) => Task.CompletedTask;
            public Task HandleProfileAsync ( JObject profile )
            {
                ProfileCount++;
                return Task.CompletedTask;
            }

            public Task HandleStatusAsync ( Status status )
            {
                StatusCount++;
                return Task.CompletedTask;
            }

            public UserControl ConfigurationTabItem () => null;
            public bool WaitForStart ( TimeSpan timeout ) => _started.Wait( timeout );
        }

        private sealed class HostTestResponder ( string name, bool startResult = true ) : IEddiResponder
        {
            public int StartCount { get; private set; }
            public int StopCount { get; private set; }
            public int ReloadCount { get; private set; }
            public int StatusCount { get; private set; }

            public string ResponderName () => name;
            public string LocalizedResponderName () => name;
            public string ResponderDescription () => "Test responder";
            public bool Start ()
            {
                StartCount++;
                return startResult;
            }

            public void Stop () => StopCount++;
            public void Reload () => ReloadCount++;
            public Task HandleAsync ( Event @event ) => Task.CompletedTask;
            public Task HandleStatusAsync ( Status status )
            {
                StatusCount++;
                return Task.CompletedTask;
            }

            public UserControl ConfigurationTabItem () => null;
        }

        [TestMethod]
        public async Task EddiPluginHost_DiscoverAsync_HandlesDiscoveryFailures ()
        {
            var host = new EddiPluginHost(
                () => true,
                () => true,
                findMonitors: () => throw new InvalidOperationException( "monitor discovery failed" ),
                findResponders: () => throw new InvalidOperationException( "responder discovery failed" ),
                appCancellationToken: CancellationToken.None );

            await host.DiscoverAsync( CancellationToken.None );

            Assert.IsEmpty( host.Monitors);
            Assert.IsEmpty( host.Responders);
        }

        [TestMethod]
        public void EddiPluginHost_Start_RespectsEnabledStateAndRequiredMonitors ()
        {
            var optionalMonitor = new HostTestMonitor( "Optional monitor" );
            var requiredMonitor = new HostTestMonitor( "Required monitor", required: true );
            var enabledResponder = new HostTestResponder( "Enabled responder" );
            var disabledResponder = new HostTestResponder( "Disabled responder" );
            var host = new EddiPluginHost(
                () => true,
                () => true,
                [ optionalMonitor, requiredMonitor ],
                [ enabledResponder, disabledResponder ],
                appCancellationToken: CancellationToken.None );
            var configuration = new EDDIConfiguration
            {
                Plugins = new Dictionary<string, bool>
                {
                    [ optionalMonitor.MonitorName() ] = false,
                    [ requiredMonitor.MonitorName() ] = false,
                    [ enabledResponder.ResponderName() ] = true,
                    [ disabledResponder.ResponderName() ] = false
                }
            };

            host.Start( configuration );

            Assert.IsFalse( host.ActiveMonitors.Contains( optionalMonitor ) );
            Assert.IsTrue( host.ActiveMonitors.Contains( requiredMonitor ) );
            Assert.IsTrue( host.ActiveResponders.Contains( enabledResponder ) );
            Assert.IsFalse( host.ActiveResponders.Contains( disabledResponder ) );
        }

        [TestMethod]
        public async Task EddiPluginHost_EnableDisableReloadAndFanOut_DelegateToActivePlugins ()
        {
            var monitor = new HostTestMonitor( "Monitor" );
            var responder = new HostTestResponder( "Responder" );
            var host = new EddiPluginHost( () => true, () => true, [ monitor ], [ responder ], appCancellationToken: CancellationToken.None );

            host.EnableMonitor( "Monitor" );
            host.EnableResponder( "Responder" );
            host.Reload();
            await host.HandleStatusAsync( new Status() );
            await host.HandleProfileAsync( new JObject() );
            host.DisableMonitor( monitor );
            host.DisableResponder( responder );

            Assert.AreEqual( 1, monitor.ReloadCount );
            Assert.AreEqual( 1, monitor.StatusCount );
            Assert.AreEqual( 1, monitor.ProfileCount );
            Assert.AreEqual( 1, monitor.StopCount );
            Assert.AreEqual( 1, responder.StartCount );
            Assert.AreEqual( 1, responder.ReloadCount );
            Assert.AreEqual( 1, responder.StatusCount );
            Assert.AreEqual( 1, responder.StopCount );
            Assert.IsEmpty( host.ActiveMonitors);
            Assert.IsEmpty( host.ActiveResponders);
        }

        [TestMethod]
        public void EddiPluginHost_DisableMonitor_CancelsKeepAlive ()
        {
            var monitor = new HostTestMonitor( "KeepAlive monitor", needsStart: true );
            var host = new EddiPluginHost( () => true, () => true, [ monitor ], appCancellationToken: CancellationToken.None );

            host.EnableMonitor( monitor );
            Assert.IsTrue( monitor.WaitForStart( TimeSpan.FromSeconds( 2 ) ) );

            host.DisableMonitor( monitor );

            Assert.IsEmpty( host.ActiveMonitors);
            Assert.AreEqual( 1, monitor.StopCount );
        }

        [TestMethod]
        public async Task EddiLocationStateService_UpdateCurrentSystem_UsesNextSystemAndClearsReachedDestination ()
        {
            var context = CreateEventProcessorContext();
            var oldSystem = new StarSystem
            {
                systemname = "Old",
                systemAddress = 1,
                signalSources = ImmutableList.Create( SignalSource.FromEDName( "Old signal" ) )
            };
            var nextSystem = new StarSystem { systemname = "Next", systemAddress = 2 };
            context.GameStateMutator.CurrentStarSystem = oldSystem;
            context.GameStateMutator.NextStarSystem = nextSystem;
            context.GameStateMutator.DestinationStarSystem = nextSystem;
            using var service = new EddiLocationStateService( context );

            await service.UpdateCurrentSystemAsync( "Next", 2 );

            Assert.AreSame( nextSystem, context.GameState.CurrentStarSystem );
            Assert.AreSame( oldSystem, context.GameState.LastStarSystem );
            Assert.IsNull( context.GameState.NextStarSystem );
            Assert.IsNull( context.GameState.DestinationStarSystem );
            Assert.IsEmpty( oldSystem.signalSources);
        }

        [TestMethod]
        public async Task EddiLocationStateService_UpdateCurrentSystem_IgnoresEmptyAndSameSystemUpdates ()
        {
            var context = CreateEventProcessorContext();
            var currentSystem = new StarSystem { systemname = "Current", systemAddress = 7 };
            context.GameStateMutator.CurrentStarSystem = currentSystem;
            using var service = new EddiLocationStateService( context );

            await service.UpdateCurrentSystemAsync( "", 0 );
            await service.UpdateCurrentSystemAsync( "Current", 7 );

            Assert.AreSame( currentSystem, context.GameState.CurrentStarSystem );
            Assert.IsNull( context.GameState.LastStarSystem );
        }

        [TestMethod]
        public async Task EddiLocationStateService_UpdateCurrentStellarBody_AddsPlaceholderBody ()
        {
            var context = CreateEventProcessorContext();
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Sol", systemAddress = 42 };
            using var service = new EddiLocationStateService( context );

            await service.UpdateCurrentStellarBodyAsync( "Sol 1", 1, "Sol", 42 );

            Assert.AreEqual( "Sol 1", context.GameState.CurrentStellarBody.bodyname );
            Assert.AreEqual( 1, context.GameState.CurrentStellarBody.bodyId );
            Assert.HasCount( 1, context.GameState.CurrentStarSystem.bodies);
        }

        [TestMethod]
        public async Task OrganicSamplingTracker_EnqueuesDistanceEventsOnlyOnNearFarTransitions ()
        {
            var emittedEvents = new List<Event>();
            var tracker = new OrganicSamplingTracker(
                CreateIsolatedTestDataProvider( out _, out _ ),
                emittedEvents.Add );
            const ulong systemAddress = 12345;
            const int bodyId = 3;
            var organic = new Organic( OrganicVariant.Clypeus_02_A );

            tracker.TrackLocationEvent( CreateLocationEvent( systemAddress, bodyId ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 0, 0 ) );
            await tracker.TrackScanOrganicAsync( new ScanOrganicEvent(
                DateTime.UtcNow,
                systemAddress,
                bodyId,
                "Sample",
                2,
                organic ) );

            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 0M, 0M ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 0.02M, 0.02M ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 1M, 1M ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 1.01M, 1.01M ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 0M, 0M ) );
            await tracker.TrackScanOrganicAsync( new ScanOrganicEvent(
                DateTime.UtcNow,
                systemAddress,
                bodyId,
                "Analyse",
                4,
                organic ) );
            await tracker.HandleStatusAsync( CreateNearSurfaceStatus( 1M, 1M ) );

            var distanceEvents = emittedEvents.OfType<ScanOrganicDistanceEvent>().ToList();
            Assert.HasCount( 2, distanceEvents);
            Assert.IsTrue( distanceEvents[ 0 ].scanready );
            Assert.IsFalse( distanceEvents[ 1 ].scanready );
        }

        private static Status CreateNearSurfaceStatus ( decimal latitude, decimal longitude ) =>
            new( Status.Flags.HasLatLong )
            {
                latitude = latitude,
                longitude = longitude,
                planetradius = 1_000_000M
            };

        private static EmbarkEvent CreateLocationEvent ( ulong systemAddress, int bodyId ) =>
            new(
                DateTime.UtcNow,
                false,
                false,
                false,
                1,
                "Test system",
                systemAddress,
                "Test system 1",
                bodyId,
                true,
                false,
                null,
                null,
                null );
    }
}
