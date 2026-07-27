using EddiCore;
using EddiCore.EventHandling;
using EddiCore.GameState;
using EddiCore.RuntimeVariables;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EddiCoreIsolatedTests
    {
        private sealed class PipelineTestMonitor ( string name, IList<string> calls ) : IEddiMonitor
        {
            public string MonitorName () => name;
            public string LocalizedMonitorName () => name;
            public string MonitorDescription () => "Test monitor for event pipeline";
            public bool IsRequired () => false;
            public bool NeedsStart () => false;
            public void Start () { }
            public void Stop () { }
            public void Reload () { }
            public Task PreHandleAsync ( Event @event )
            {
                calls.Add( $"pre:{name}" );
                return Task.CompletedTask;
            }

            public Task PostHandleAsync ( Event @event )
            {
                calls.Add( $"post:{name}" );
                return Task.CompletedTask;
            }

            public Task HandleProfileAsync ( JObject profile ) => Task.CompletedTask;
            public Task HandleStatusAsync ( Status status ) => Task.CompletedTask;
            public UserControl ConfigurationTabItem () => null;
        }

        private sealed class PipelineTestResponder ( string name, IList<string> calls ) : IEddiResponder
        {
            public string ResponderName () => name;
            public string LocalizedResponderName () => name;
            public string ResponderDescription () => "Test responder for event pipeline";
            public bool Start () => true;
            public void Stop () { }
            public void Reload () { }
            public Task HandleAsync ( Event @event )
            {
                calls.Add( $"respond:{name}" );
                return Task.CompletedTask;
            }

            public Task HandleStatusAsync ( Status status ) => Task.CompletedTask;
            public UserControl ConfigurationTabItem () => null;
        }

        private static EddiEventPipeline CreatePipeline (
            Func<Event, Task<bool>> processEventAsync = null,
            IEnumerable<IEddiMonitor> monitors = null,
            IEnumerable<IEddiResponder> responders = null,
            Func<string, IEddiResponder> obtainResponder = null,
            Func<System.Version> getGameVersion = null )
        {
            return new EddiEventPipeline(
                processEventAsync ?? ( _ => Task.FromResult( true ) ),
                () => monitors ?? [ ],
                () => responders ?? [ ],
                obtainResponder ?? ( _ => null ),
                () => true,
                getGameVersion ?? ( () => new System.Version( 4, 0 ) ),
                new System.Version( 4, 0 ),
                CancellationToken.None );
        }

        private static EddiGameStateService CreateGameStateService (
            EddiGameState gameState,
            (decimal? x, decimal? y, decimal? z)? homeSystemCoordinates = null,
            Action<Ship> setCurrentShip = null,
            Action<string> sayLegacyGameVersionWarning = null,
            Action<System.Version, string, string> setStarMapGameVersion = null )
        {
            return new EddiGameStateService(
                gameState,
                () => homeSystemCoordinates ?? ( null, null, null ),
                setCurrentShip,
                sayLegacyGameVersionWarning,
                setStarMapGameVersion,
                new System.Version( 4, 0 ) );
        }

        private sealed class TestEddiEventProcessorContext : IEddiEventProcessorContext
        {
            internal EddiGameState GameStateOwner { get; } = new();
            private readonly EddiGameStateService _gameStateService;
            public IEddiGameState GameState => GameStateOwner;
            public IEddiGameStateMutator GameStateMutator => _gameStateService;
            public DataProviderService DataProvider { get; init; }
            public EddiEventPipeline EventPipeline { get; }
            public ConcurrentDictionary<string, Event> lastEventOfType => EventPipeline.LastEventOfType;
            public Dictionary<string, IEddiMonitor> Monitors { get; } = new();

            internal TestEddiEventProcessorContext ()
            {
                _gameStateService = CreateGameStateService( GameStateOwner );
                EventPipeline = CreatePipeline( getGameVersion: () => GameStateOwner.GameVersion );
            }

            public IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase )
            {
                return Monitors.FirstOrDefault( kvp => kvp.Key.Equals( invariantName, stringComparison ) ).Value;
            }

            public Task conditionallyRefreshStationProfileAsync (
                string expectedSystemName,
                long expectedLastMarketID,
                bool forceUpdate = false,
                JObject profileJson = null ) => Task.CompletedTask;

            public Task updateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null )
            {
                GameStateMutator.DestinationStarSystem = null;
                return Task.CompletedTask;
            }
        }

        private sealed class TestRuntimeVariableContext : IRuntimeVariableContext
        {
            public TestRuntimeVariableContext ( IEddiGameState gameState )
            {
                GameState = gameState;
            }

            public IEddiGameState GameState { get; }
            public bool FromVA { get; init; }
            public bool CapiActive { get; init; }
            public bool IcaoActive { get; init; }
            public bool IpaActive { get; init; }
        }

        [TestMethod]
        public void EddiGameState_AssigningProperty_RaisesPropertyChanged ()
        {
            var gameState = new EddiGameState();
            var propertyNames = new List<string>();
            gameState.PropertyChanged += ( _, args ) => propertyNames.Add( args.PropertyName );

            gameState.Environment = Constants.ENVIRONMENT_NORMAL_SPACE;

            Assert.Contains( nameof( EddiGameState.Environment ), propertyNames );
            Assert.AreEqual( Constants.ENVIRONMENT_NORMAL_SPACE, gameState.Environment );
        }

        [TestMethod]
        public void TopLevelRuntimeVariableValues_BuildsFromExplicitContext ()
        {
            var gameState = new EddiGameState();
            var gameStateService = CreateGameStateService( gameState );
            gameStateService.DestinationDistanceLy = 12.34M;
            gameStateService.Environment = Constants.ENVIRONMENT_SUPERCRUISE;
            gameStateService.inHorizons = true;
            gameStateService.inOdyssey = false;
            gameStateService.SearchDistanceLy = 56.78M;
            gameStateService.Vehicle = Constants.VEHICLE_SHIP;
            var context = new TestRuntimeVariableContext( gameState )
            {
                FromVA = true,
                CapiActive = true,
                IcaoActive = false,
                IpaActive = true
            };

            var values = TopLevelRuntimeVariableValues.Build( context )
                .ToDictionary( value => value.Name );

            Assert.AreEqual( true, values[ RuntimeVariableCatalog.CapiActiveVariable ].Value );
            Assert.AreEqual( 12.34M, values[ RuntimeVariableCatalog.DestinationDistanceLyVariable ].Value );
            Assert.AreEqual( Constants.ENVIRONMENT_SUPERCRUISE, values[ RuntimeVariableCatalog.EnvironmentVariable ].Value );
            Assert.AreEqual( true, values[ RuntimeVariableCatalog.HorizonsVariable ].Value );
            Assert.AreEqual( false, values[ RuntimeVariableCatalog.IcaoActiveVariable ].Value );
            Assert.AreEqual( true, values[ RuntimeVariableCatalog.IpaActiveVariable ].Value );
            Assert.AreEqual( false, values[ RuntimeVariableCatalog.OdysseyVariable ].Value );
            Assert.AreEqual( 56.78M, values[ RuntimeVariableCatalog.SearchDistanceLyVariable ].Value );
            Assert.AreEqual( true, values[ RuntimeVariableCatalog.VaActiveVariable ].Value );
            Assert.AreEqual( Constants.VEHICLE_SHIP, values[ RuntimeVariableCatalog.VehicleVariable ].Value );
            Assert.AreEqual( Constants.EDDI_VERSION.ShortString, values[ RuntimeVariableCatalog.VersionVariable ].Value );
            Assert.AreEqual( Constants.EDDI_VERSION.ToString(), values[ RuntimeVariableCatalog.VersionVariable ].GetVoiceAttackValue() );
        }

        [TestMethod]
        public void EddiGameState_ReplacingChild_UnsubscribesPreviousChild ()
        {
            var gameState = new EddiGameState();
            var oldSystem = new StarSystem { systemname = "Old", systemAddress = 1 };
            var newSystem = new StarSystem { systemname = "New", systemAddress = 2 };
            var propertyNames = new List<string>();
            gameState.PropertyChanged += ( _, args ) => propertyNames.Add( args.PropertyName );

            gameState.CurrentStarSystem = oldSystem;
            gameState.CurrentStarSystem = newSystem;
            propertyNames.Clear();

            oldSystem.totalbodies = 1;
            Assert.DoesNotContain( nameof( EddiGameState.CurrentStarSystem ), propertyNames );

            newSystem.totalbodies = 2;
            Assert.Contains( nameof( EddiGameState.CurrentStarSystem ), propertyNames );
        }

        [TestMethod]
        public void EddiGameState_ChildChange_RaisesOwningPropertyName ()
        {
            var gameState = new EddiGameState();
            var ship = new Ship();
            var propertyNames = new List<string>();
            gameState.CurrentShip = ship;
            gameState.PropertyChanged += ( _, args ) => propertyNames.Add( args.PropertyName );

            ship.value = 42;

            Assert.Contains( nameof( EddiGameState.CurrentShip ), propertyNames );
        }

        [TestMethod]
        public void EddiGameStateService_SettingCurrentLastNextSystem_AppliesHomeDistance ()
        {
            var gameState = new EddiGameState();
            var gameStateService = CreateGameStateService(
                gameState,
                ( 0M, 0M, 0M ) );
            var currentSystem = new StarSystem { systemname = "Current", x = 3M, y = 4M, z = 0M };
            var lastSystem = new StarSystem { systemname = "Last", x = 0M, y = 0M, z = 12M };
            var nextSystem = new StarSystem { systemname = "Next", x = 8M, y = 0M, z = 15M };

            gameStateService.CurrentStarSystem = currentSystem;
            gameStateService.LastStarSystem = lastSystem;
            gameStateService.NextStarSystem = nextSystem;

            Assert.AreEqual( 5M, currentSystem.distancefromhome );
            Assert.AreEqual( 12M, lastSystem.distancefromhome );
            Assert.AreEqual( 17M, nextSystem.distancefromhome );
        }

        [TestMethod]
        public void EddiGameStateService_SettingCurrentSystem_UpdatesDestinationDistance ()
        {
            var gameState = new EddiGameState();
            var gameStateService = CreateGameStateService( gameState );
            gameStateService.DestinationStarSystem = new StarSystem { systemname = "Destination", x = 0M, y = 0M, z = 0M };

            gameStateService.CurrentStarSystem = new StarSystem { systemname = "Current", x = 0M, y = 3M, z = 4M };

            Assert.AreEqual( 5M, gameState.DestinationDistanceLy );
        }

        [TestMethod]
        public void EddiGameStateService_ChangingDestinationSystem_RecalculatesCurrentSystemDestinationDistance ()
        {
            var gameState = new EddiGameState();
            var gameStateService = CreateGameStateService( gameState );
            gameStateService.CurrentStarSystem = new StarSystem { systemname = "Current", x = 0M, y = 0M, z = 0M };

            gameStateService.DestinationStarSystem = new StarSystem { systemname = "Destination 1", x = 3M, y = 4M, z = 0M };
            Assert.AreEqual( 5M, gameState.DestinationDistanceLy );

            gameStateService.DestinationStarSystem = new StarSystem { systemname = "Destination 2", x = 0M, y = 0M, z = 12M };
            Assert.AreEqual( 12M, gameState.DestinationDistanceLy );

            gameStateService.DestinationStarSystem = null;
            Assert.AreEqual( 0M, gameState.DestinationDistanceLy );
        }

        [TestMethod]
        public void EddiGameStateService_SettingCurrentShip_InvokesDelegateOnceAndSkipsDuplicate ()
        {
            var gameState = new EddiGameState();
            var delegateCallCount = 0;
            Ship delegateShip = null;
            var gameStateService = CreateGameStateService(
                gameState,
                setCurrentShip: ship =>
                {
                    delegateCallCount++;
                    delegateShip = ship;
                } );
            var ship = new Ship();

            gameStateService.CurrentShip = ship;
            gameStateService.CurrentShip = ship;

            Assert.AreEqual( 1, delegateCallCount );
            Assert.AreSame( ship, delegateShip );
            Assert.AreSame( ship, gameState.CurrentShip );
        }

        [TestMethod]
        public void EddiGameStateService_SetGameVersionDetails_ParsesLegacyVersionAndInvokesDelegates ()
        {
            var gameState = new EddiGameState();
            var warningCount = 0;
            System.Version reportedVersion = null;
            string reportedRawVersion = null;
            string reportedBuild = null;
            var gameStateService = CreateGameStateService(
                gameState,
                sayLegacyGameVersionWarning: _ => warningCount++,
                setStarMapGameVersion: ( version, rawVersion, build ) =>
                {
                    reportedVersion = version;
                    reportedRawVersion = rawVersion;
                    reportedBuild = build;
                } );

            gameStateService.SetGameVersionDetails( "3.8.0.0 Beta", "r123/r0" );

            Assert.AreEqual( "3.8.0.0 Beta", gameState.GameVersionRaw );
            Assert.AreEqual( new System.Version( 3, 8, 0, 0 ), gameState.GameVersion );
            Assert.AreEqual( 1, warningCount );
            Assert.AreEqual( new System.Version( 3, 8, 0, 0 ), reportedVersion );
            Assert.AreEqual( "3.8.0.0 Beta", reportedRawVersion );
            Assert.AreEqual( "r123/r0", reportedBuild );
        }

        [TestMethod, DoNotParallelize]
        public async Task EddiEventProcessor_FileHeader_UpdatesVersionState ()
        {
            var originalCapiGameIsBeta = EddiCompanionAppService.CompanionAppService.Instance.gameIsBeta;
            var context = new TestEddiEventProcessorContext();
            var processor = new EddiEventProcessor( context );

            try
            {
                var @event = new FileHeaderEvent(
                    DateTime.UtcNow,
                    "Journal.250725000000.01.log",
                    "4.2.1.0 Beta",
                    "r123/r0" );

                var passEvent = await processor.ProcessEventAsync( @event ).ConfigureAwait( false );

                Assert.IsTrue( passEvent );
                Assert.IsTrue( context.GameStateOwner.gameIsBeta );
                Assert.AreEqual( new System.Version( 4, 2, 1, 0 ), context.GameStateOwner.GameVersion );
            }
            finally
            {
                EddiCompanionAppService.CompanionAppService.Instance.gameIsBeta = originalCapiGameIsBeta;
            }
        }

        [TestMethod]
        public async Task EddiEventProcessor_Died_ClearsDeployedVessels ()
        {
            var context = new TestEddiEventProcessorContext();
            var processor = new EddiEventProcessor( context );
            context.GameStateOwner.DeployedVessels[ 1 ] = VesselDefinition.Fighter_Federation;

            var passEvent = await processor
                .ProcessEventAsync( new DiedEvent( DateTime.UtcNow, [ ] ) )
                .ConfigureAwait( false );

            Assert.IsTrue( passEvent );
            Assert.IsEmpty( context.GameStateOwner.DeployedVessels );
        }

        [TestMethod]
        public void EddiEventPipeline_EnqueueNull_IsIgnored ()
        {
            var pipeline = CreatePipeline();

            pipeline.Enqueue( null );

            Assert.IsFalse( pipeline.HasQueuedSignalDetectedEvents() );
            Assert.IsEmpty( pipeline.LastEventOfType );
        }

        [TestMethod]
        public async Task EddiEventPipeline_HandleEventAsync_RecordsAcceptedEvent ()
        {
            var pipeline = CreatePipeline();
            var @event = new FileHeaderEvent(
                DateTime.UtcNow,
                "Journal.250725000000.01.log",
                "4.2.1.0",
                "r123/r0" );

            await pipeline.HandleEventAsync( @event ).ConfigureAwait( false );

            Assert.IsTrue( pipeline.LastEventOfType.TryGetValue( FileHeaderEvent.NAME, out var recordedEvent ) );
            Assert.AreSame( @event, recordedEvent );
        }

        [TestMethod]
        public async Task EddiEventPipeline_HandleEventAsync_SuppressesLegacyGameVersionEvents ()
        {
            var processCalled = false;
            var pipeline = CreatePipeline(
                processEventAsync: _ =>
                {
                    processCalled = true;
                    return Task.FromResult( true );
                },
                getGameVersion: () => new System.Version( 3, 8 ) );

            await pipeline.HandleEventAsync( new DiedEvent( DateTime.UtcNow, [ ] ) ).ConfigureAwait( false );

            Assert.IsFalse( processCalled );
            Assert.IsEmpty( pipeline.LastEventOfType );
        }

        [TestMethod]
        public async Task EddiEventPipeline_HandleEventAsync_FansOutAcceptedEventsInOrder ()
        {
            var calls = new List<string>();
            var monitor = new PipelineTestMonitor( "monitor", calls );
            var responder = new PipelineTestResponder( "responder", calls );
            var pipeline = CreatePipeline(
                monitors: [ monitor ],
                responders: [ responder ] );

            await pipeline.HandleEventAsync( new FileHeaderEvent(
                DateTime.UtcNow,
                "Journal.250725000000.01.log",
                "4.2.1.0",
                "r123/r0" ) ).ConfigureAwait( false );

            CollectionAssert.AreEqual(
                new List<string> { "pre:monitor", "respond:responder", "post:monitor" },
                calls );
        }

        [TestMethod]
        public async Task EddiEventPipeline_HandleEventAsync_SendsFallbackResponderEventsWhenProcessorThrows ()
        {
            var calls = new List<string>();
            var fallbackResponders = new Dictionary<string, IEddiResponder>
            {
                [ "EDDN Responder" ] = new PipelineTestResponder( "EDDN Responder", calls ),
                [ "EDSM Responder" ] = new PipelineTestResponder( "EDSM Responder", calls ),
                [ "Inara Responder" ] = new PipelineTestResponder( "Inara Responder", calls )
            };
            var pipeline = CreatePipeline(
                processEventAsync: _ => throw new InvalidOperationException( "test failure" ),
                obtainResponder: name => fallbackResponders[ name ] );

            await pipeline.HandleEventAsync( new FileHeaderEvent(
                DateTime.UtcNow,
                "Journal.250725000000.01.log",
                "4.2.1.0",
                "r123/r0" ) ).ConfigureAwait( false );

            CollectionAssert.AreEqual(
                new List<string> { "respond:EDDN Responder", "respond:EDSM Responder", "respond:Inara Responder" },
                calls );
        }
    }
}
