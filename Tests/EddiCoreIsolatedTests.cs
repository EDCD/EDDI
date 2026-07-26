using EddiCore;
using EddiCore.EventHandling;
using EddiCore.GameState;
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

        private sealed class TestEddiEventProcessorContext : IEddiEventProcessorContext
        {
            internal EddiGameState GameStateOwner { get; } = new();
            public IEddiGameState GameState => GameStateOwner;
            public DataProviderService DataProvider { get; init; }
            public EddiEventPipeline EventPipeline { get; }
            public ConcurrentDictionary<string, Event> lastEventOfType => EventPipeline.LastEventOfType;
            public Dictionary<string, IEddiMonitor> Monitors { get; } = new();

            internal TestEddiEventProcessorContext ()
            {
                EventPipeline = CreatePipeline( getGameVersion: () => GameStateOwner.GameVersion );
            }

            public StarSystem CurrentStarSystem { get => GameStateOwner.CurrentStarSystem; set => GameStateOwner.CurrentStarSystem = value; }
            public StarSystem LastStarSystem { get => GameStateOwner.LastStarSystem; set => GameStateOwner.LastStarSystem = value; }
            public StarSystem NextStarSystem { get => GameStateOwner.NextStarSystem; set => GameStateOwner.NextStarSystem = value; }
            public StarSystem DestinationStarSystem { get => GameStateOwner.DestinationStarSystem; set => GameStateOwner.DestinationStarSystem = value; }
            public Station CurrentStation { get => GameStateOwner.CurrentStation; set => GameStateOwner.CurrentStation = value; }
            public Body CurrentStellarBody { get => GameStateOwner.CurrentStellarBody; set => GameStateOwner.CurrentStellarBody = value; }
            public FleetCarrier FleetCarrier { get => GameStateOwner.FleetCarrier; set => GameStateOwner.FleetCarrier = value; }
            public FleetCarrier SquadronCarrier { get => GameStateOwner.SquadronCarrier; set => GameStateOwner.SquadronCarrier = value; }
            public string Environment { get => GameStateOwner.Environment; set => GameStateOwner.Environment = value; }
            public string Vehicle { get => GameStateOwner.Vehicle; set => GameStateOwner.Vehicle = value; }
            public bool inTelepresence { get => GameStateOwner.inTelepresence; set => GameStateOwner.inTelepresence = value; }
            public bool inHorizons { get => GameStateOwner.inHorizons; set => GameStateOwner.inHorizons = value; }
            public bool inOdyssey { get => GameStateOwner.inOdyssey; set => GameStateOwner.inOdyssey = value; }
            public bool gameIsBeta { get => GameStateOwner.gameIsBeta; set => GameStateOwner.gameIsBeta = value; }

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
                DestinationStarSystem = null;
                return Task.CompletedTask;
            }

            public void SetGameVersionDetails ( string version, string build )
            {
                GameStateOwner.GameVersionRaw = version;
                var semanticVersion = System.Text.RegularExpressions.Regex.Match( version ?? string.Empty, @"\d+(\.\d+){1,3}" ).Value;
                GameStateOwner.GameVersion = System.Version.TryParse( semanticVersion, out var versionResult )
                    ? versionResult
                    : null;
            }
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
