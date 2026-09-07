using EddiCore;
using EddiCore.EventHandling;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class JournalEventEnrichmentTests
    {
        private static readonly DateTime Timestamp = new( 2026, 1, 1, 0, 0, 0, DateTimeKind.Utc );

        private sealed class ParseContext : IJournalParseContext
        {
            public DataProviderService DataProvider => throw new InvalidOperationException( "Unexpected data lookup" );
            public void EnqueueEvent ( Event @event ) => Assert.Fail( "Parsing must not schedule these events" );
        }

        private sealed class Scheduler : IEventScheduler
        {
            public List<(TimeSpan delay, Func<Event> create)> Pending { get; } = [ ];
            public void Schedule ( TimeSpan delay, Func<Event> createEvent ) => Pending.Add( (delay, createEvent) );
            public void Dispose () { }
        }

        private sealed class Monitor ( Action<Event> preHandle ) : IEddiMonitor
        {
            public string MonitorName () => "Enrichment test";
            public string LocalizedMonitorName () => MonitorName();
            public string MonitorDescription () => MonitorName();
            public bool IsRequired () => false;
            public bool NeedsStart () => false;
            public void Start () { }
            public void Stop () { }
            public void Reload () { }
            public Task PreHandleAsync ( Event e ) { preHandle( e ); return Task.CompletedTask; }
            public Task PostHandleAsync ( Event e ) => Task.CompletedTask;
            public Task HandleProfileAsync ( JObject profile ) => Task.CompletedTask;
            public Task HandleStatusAsync ( Status status ) => Task.CompletedTask;
            public UserControl ConfigurationTabItem () => null;
        }

        private static string Line ( string eventName, JObject fields = null )
        {
            var data = fields ?? new JObject();
            data["timestamp"] = Timestamp;
            data["event"] = eventName;
            return data.ToString();
        }

        private static List<Event> Parse ( string eventName, JObject fields = null, bool fromLoad = false ) =>
            JournalMonitor.ParseJournalEntry( Line( eventName, fields ), new ParseContext(), fromLoad );

        private static EddiEventPipeline Pipeline ( Func<Event, Task<bool>> process ) => new(
            process, () => [ ], () => [ ], _ => null, () => true,
            () => new System.Version( 4, 0 ), new System.Version( 4, 0 ), CancellationToken.None );

        [TestMethod]
        public async Task Batch_UsesPrecedingLoadGameForVehicleAndExpansion ()
        {
            var context = TestBase.CreateEventProcessorContext();
            context.GameStateMutator.Vehicle = Constants.VEHICLE_SHIP;
            context.GameStateMutator.inOdyssey = false;
            using var processor = new EddiEventProcessor( context );
            var load = JObject.Parse( CommanderContinuedEvent.SAMPLE );
            load["Ship"] = "TestBuggy";
            load["Odyssey"] = true;
            var batch = JournalMonitor.ParseJournalEntries( [
                load.ToString(),
                Line( "HullDamage", new JObject { ["Health"] = 0.5, ["PlayerPilot"] = true } ),
                Line( "ChangeCrewRole", new JObject { ["Role"] = "Idle", ["Telepresence"] = false } )
            ], new ParseContext() );
            Assert.IsNull( ((HullDamagedEvent)batch[1]).vehicle );
            foreach ( var item in batch ) { await processor.ProcessEventAsync( item ); }
            Assert.AreEqual( Constants.VEHICLE_SRV, ((HullDamagedEvent)batch[1]).vehicle );
            Assert.IsFalse( ((CrewRoleChangedEvent)batch[2]).telepresence );
        }

        [TestMethod]
        public async Task HullDamage_PreservesPilotAndFighterInference ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            foreach ( var vehicle in new[] { Constants.VEHICLE_SHIP, Constants.VEHICLE_SRV, Constants.VEHICLE_FIGHTER } )
            foreach ( bool? pilot in new bool?[] { null, true, false } )
            foreach ( bool? fighter in new bool?[] { null, true, false } )
            {
                context.GameStateMutator.Vehicle = vehicle;
                var damage = (HullDamagedEvent)Parse( "HullDamage", new JObject
                    { ["Health"] = 0.5, ["PlayerPilot"] = pilot, ["Fighter"] = fighter } ).Single();
                await processor.ProcessEventAsync( damage );
                var expected = pilot != false ? vehicle : fighter == true ? Constants.VEHICLE_FIGHTER
                    : vehicle == Constants.VEHICLE_SHIP ? Constants.VEHICLE_SRV
                    : vehicle == Constants.VEHICLE_SRV ? Constants.VEHICLE_SHIP : vehicle;
                Assert.AreEqual( expected, damage.vehicle );
                Assert.AreEqual( 50M, damage.health );
            }
        }

        [TestMethod]
        public async Task Mapping_ResolvesAtProcessingAndMutatesOnlyMatchingBody ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var fields = new JObject { ["BodyName"] = "Test 1", ["BodyID"] = 7, ["SystemAddress"] = 1234,
                ["ProbesUsed"] = 5, ["EfficiencyTarget"] = 6 };
            var mapped = (BodyMappedEvent)Parse( "SAAScanComplete", fields ).Single();
            Assert.IsNull( mapped.body );
            Assert.AreEqual( 7L, mapped.bodyId );
            var body = new Body { bodyId = 7, bodyname = "Test 1", systemAddress = 1234, systemname = "Test" };
            var system = new StarSystem { systemAddress = 1234, systemname = "Test" };
            system.AddOrUpdateBody( body );
            context.GameStateMutator.CurrentStarSystem = system;
            Assert.IsNull( body.mappedDateTime );
            Assert.IsTrue( await processor.ProcessEventAsync( mapped ) );
            Assert.AreSame( body, mapped.body );
            Assert.AreEqual( Timestamp, body.mappedDateTime );
            Assert.AreEqual( Timestamp, body.scannedDateTime );
            Assert.IsTrue( body.mappedEfficiently );

            fields["SystemAddress"] = 9999;
            fields["ProbesUsed"] = 10;
            var mismatched = (BodyMappedEvent)Parse( "SAAScanComplete", fields ).Single();
            Assert.IsTrue( await processor.ProcessEventAsync( mismatched ) );
            Assert.IsNull( mismatched.body );
            Assert.IsTrue( body.mappedEfficiently );
        }

        [TestMethod]
        public async Task Mapping_UnknownBodiesAndRingsRemainUsable ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            foreach ( var system in new StarSystem[] { null, new() { systemAddress = 1234, systemname = "Test" } } )
            foreach ( var name in new[] { "Test 1", "Test 1 A Ring" } )
            {
                context.GameStateMutator.CurrentStarSystem = system;
                var item = Parse( "SAAScanComplete", new JObject { ["BodyName"] = name, ["BodyID"] = 7,
                    ["SystemAddress"] = 1234, ["ProbesUsed"] = 1, ["EfficiencyTarget"] = 0 } ).Single();
                Assert.IsTrue( await processor.ProcessEventAsync( item ) );
                foreach ( var property in item.GetType().GetProperties().Where( p => p.GetIndexParameters().Length == 0 ) )
                {
                    property.GetValue( item ); // Unresolved public properties must be safe to enumerate.
                }
                if ( name.EndsWith( " Ring" ) ) { Assert.IsInstanceOfType<RingMappedEvent>( item ); }
                else { Assert.IsInstanceOfType<BodyMappedEvent>( item ); }
            }
        }

        [TestMethod]
        public async Task RingMapping_ResolvesParentOnlyInMatchingSystem ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var ring = new Ring( "Test 1 A Ring", RingComposition.FromEDName( "Icy" ), 1, 2, 3 );
            var body = new Body { bodyId = 7, rings = [ ring ], bodyname = "Test 1" };
            var system = new StarSystem { systemAddress = 1234, systemname = "Test" };
            system.AddOrUpdateBody( body );
            context.GameStateMutator.CurrentStarSystem = system;
            foreach ( var address in new ulong[] { 1234, 9999 } )
            {
                var item = new RingMappedEvent( Timestamp, ring.name, null, address, 1, 0 );
                await processor.ProcessEventAsync( item );
                Assert.AreEqual( address == 1234 ? ring : null, item.ring );
                Assert.AreEqual( address == 1234 ? body : null, item.body );
            }
        }

        [TestMethod]
        public async Task MissionOrigins_AreResolvedBeforeDownstreamProcessing ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var item = (MissionAcceptedEvent)Parse( "MissionAccepted", new JObject
                { ["MissionID"] = 123, ["Name"] = "Mission_Delivery", ["Faction"] = "Test", ["Wing"] = false } ).Single();
            Assert.IsNull( item.Mission.originsystem );
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Arrival" };
            context.GameStateMutator.CurrentStation = new Station { name = "Port" };
            await processor.ProcessEventAsync( item );
            Assert.AreEqual( "Arrival", item.Mission.originsystem );
            Assert.AreEqual( "Port", item.Mission.originstation );
            var community = (MissionAcceptedEvent)Parse( "CommunityGoalJoin", new JObject
                { ["CGID"] = 12, ["Name"] = "Goal", ["System"] = "Goal System" } ).Single();
            await processor.ProcessEventAsync( community );
            Assert.AreEqual( "Goal System", community.Mission.originsystem );
        }

        [TestMethod]
        public async Task Batch_LocationThenAltruismMission_DeliversEnrichedOriginAndDestination ()
        {
            var context = TestBase.CreateEventProcessorContext();
            // Avoid data retrieval while still exercising the real location handler's transition.
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Before", systemAddress = 1 };
            context.GameStateMutator.NextStarSystem = new StarSystem { systemname = "Alpha Caeli", systemAddress = 2106438175083 };
            using var processor = new EddiEventProcessor( context );
            var batch = JournalMonitor.ParseJournalEntries( [ LocationEvent.SAMPLE,
                Line( "MissionAccepted", new JObject { ["MissionID"] = 123, ["Name"] = "Mission_AltruismCredits",
                    ["Faction"] = "Test", ["Wing"] = false } ) ], new ParseContext() );
            Assert.HasCount( 2, batch );
            var mission = (MissionAcceptedEvent)batch[1];
            Assert.IsNull( mission.Mission.originsystem );
            var delivered = false;
            var monitor = new Monitor( e =>
            {
                if ( e is not MissionAcceptedEvent accepted ) { return; }
                Assert.AreEqual( "Alpha Caeli", accepted.Mission.originsystem );
                Assert.AreEqual( "Heyerdahl Hub", accepted.Mission.originstation );
                Assert.AreEqual( accepted.Mission.originsystem, accepted.Mission.destinationsystem );
                Assert.AreEqual( accepted.Mission.originstation, accepted.Mission.destinationstation );
                delivered = true;
            } );
            var pipeline = new EddiEventPipeline( processor.ProcessEventAsync, () => [ monitor ], () => [ ],
                _ => null, () => true, () => new System.Version( 4, 0 ), new System.Version( 4, 0 ), CancellationToken.None );
            foreach ( var e in batch ) { await pipeline.HandleEventAsync( e ); }
            Assert.IsTrue( delivered );
        }

        [TestMethod]
        public async Task Transfers_ZeroAndMissingDelaysRetainExistingBehavior ()
        {
            var context = TestBase.CreateEventProcessorContext();
            var scheduler = new Scheduler();
            using var processor = new EddiEventProcessor( context, scheduler, _ => null );
            foreach ( var sample in new[] { ShipTransferInitiatedEvent.SAMPLE, ModuleTransferEvent.SAMPLE } )
            {
                var fields = JObject.Parse( sample );
                fields.Remove( "TransferTime" );
                var item = JournalMonitor.ParseJournalEntry( fields.ToString(), new ParseContext() ).Single();
                await processor.ProcessEventAsync( item );
                fields["TransferTime"] = 0;
                item = JournalMonitor.ParseJournalEntry( fields.ToString(), new ParseContext() ).Single();
                await processor.ProcessEventAsync( item );
            }
            Assert.HasCount( 2, scheduler.Pending );
            foreach ( var (delay, create) in scheduler.Pending )
            {
                Assert.AreEqual( TimeSpan.Zero, delay );
                var arrival = create();
                Assert.IsFalse( arrival.fromLoad );
                Assert.AreEqual( string.Empty, arrival.GetType().GetProperty( "system" ).GetValue( arrival ) );
                Assert.AreEqual( string.Empty, arrival.GetType().GetProperty( "station" ).GetValue( arrival ) );
            }
        }

        [TestMethod]
        public async Task StoredShips_DistanceUsesProcessingLocation ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var ship = ShipDefinitions.FromEDModel( "Adder" );
            ship.StoredLocation = new Ship.Location( new StarSystem { systemname = "Origin", x = 0, y = 0, z = 0 }, "Port", 1 );
            var item = new StoredShipsEvent( Timestamp, 2, "Other", "Destination", [ ship ] );
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Destination", x = 3, y = 4, z = 0 };
            await processor.ProcessEventAsync( item );
            Assert.AreEqual( 5M, ship.distance );
        }

        [TestMethod]
        public async Task SettlementParsing_DoesNotMutateCachedFaction_AndMergePreservesMissingFields ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var faction = new Faction { name = "Test", Government = Government.Democracy, Allegiance = Superpower.Federation };
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Test System", systemAddress = 1234, factions = [ faction ] };
            var fields = new JObject { ["Name"] = "Port", ["SystemAddress"] = 1234, ["BodyID"] = 7,
                ["StationFaction"] = new JObject { ["Name"] = "Test" } };
            var item = (SettlementApproachedEvent)Parse( "ApproachSettlement", fields ).Single();
            Assert.AreNotSame( faction, item.controllingFaction );
            Assert.IsEmpty( faction.presences );
            await processor.ProcessEventAsync( item );
            Assert.AreSame( faction, item.controllingFaction );
            Assert.AreEqual( Government.Democracy, faction.Government );
            Assert.AreEqual( Superpower.Federation, faction.Allegiance );
            Assert.AreEqual( "Test System", faction.presences.Single().systemName );

            fields["StationGovernment"] = "$government_Confederacy;";
            fields["StationAllegiance"] = "Empire";
            item = (SettlementApproachedEvent)Parse( "ApproachSettlement", fields ).Single();
            Assert.AreEqual( Government.Democracy, faction.Government );
            await processor.ProcessEventAsync( item );
            Assert.AreEqual( Government.Confederacy, faction.Government );
            Assert.AreEqual( Superpower.Empire, faction.Allegiance );
            fields["SystemAddress"] = 9999;
            fields["StationAllegiance"] = "Federation";
            item = (SettlementApproachedEvent)Parse( "ApproachSettlement", fields ).Single();
            await processor.ProcessEventAsync( item );
            Assert.AreNotSame( faction, item.controllingFaction );
            Assert.AreEqual( Superpower.Empire, faction.Allegiance );
        }

        [TestMethod]
        public async Task Messages_ResolveNpcSeedsAndFilterOnlyOrdinaryNoFireZoneEntry ()
        {
            var context = TestBase.CreateEventProcessorContext();
            using var processor = new EddiEventProcessor( context );
            var npc = new MessageReceivedEvent( Timestamp, "Port", MessageSource.Station, false, MessageChannel.NPC, "Hello" );
            var player = new MessageReceivedEvent( Timestamp, "Player", MessageSource.NPC, true, MessageChannel.Player, "Hello" );
            var playerSeed = player.seed;
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Test" };
            context.GameStateMutator.CurrentStation = new Station { name = "Port" };
            await processor.ProcessEventAsync( npc );
            await processor.ProcessEventAsync( player );
            var expected = new MessageReceivedEvent( Timestamp, "Port", MessageSource.Station, false, MessageChannel.NPC,
                "Hello", context.GameState.CurrentStarSystem, null, context.GameState.CurrentStation );
            Assert.AreEqual( expected.seed, npc.seed );
            Assert.AreEqual( playerSeed, player.seed );
            foreach ( var vehicle in new[] { Constants.VEHICLE_SHIP, Constants.VEHICLE_SRV, Constants.VEHICLE_LEGS } )
            foreach ( var deployed in new[] { false, true } )
            {
                context.GameStateMutator.Vehicle = vehicle;
                var events = Parse( "ReceiveText", new JObject { ["From"] = "Port", ["Channel"] = "npc",
                    ["Message"] = deployed ? "$STATION_NoFireZone_entered_deployed;" : "$STATION_NoFireZone_entered;",
                    ["Message_Localised"] = "No fire zone" } );
                var entry = events.OfType<StationNoFireZoneEnteredEvent>().Single();
                Assert.AreEqual( deployed || vehicle == Constants.VEHICLE_SHIP, await processor.ProcessEventAsync( entry ) );
            }
        }

        [TestMethod]
        public async Task Transfers_CaptureDestinationAndResolveShip_WithoutReplayTimers ()
        {
            var context = TestBase.CreateEventProcessorContext();
            var scheduler = new Scheduler();
            var knownShip = ShipDefinitions.FromEDModel( "Krait_MkII" );
            knownShip.LocalId = 81;
            knownShip.name = "Known ship";
            using var processor = new EddiEventProcessor( context, scheduler, id => id == 81 ? knownShip : null );
            var ship = (ShipTransferInitiatedEvent)JournalMonitor.ParseJournalEntry( ShipTransferInitiatedEvent.SAMPLE, new ParseContext() ).Single();
            var module = (ModuleTransferEvent)JournalMonitor.ParseJournalEntry( ModuleTransferEvent.SAMPLE, new ParseContext() ).Single();
            Assert.IsEmpty( scheduler.Pending );
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Arrival" };
            context.GameStateMutator.CurrentStation = new Station { name = "Port" };
            await processor.ProcessEventAsync( ship );
            await processor.ProcessEventAsync( module );
            Assert.AreSame( knownShip, ship.Ship );
            Assert.AreEqual( TimeSpan.FromSeconds( 380 ), scheduler.Pending[0].delay );
            Assert.AreEqual( TimeSpan.FromSeconds( 120 ), scheduler.Pending[1].delay );
            context.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Elsewhere" };
            context.GameStateMutator.CurrentStation = null;
            var shipArrived = (ShipArrivedEvent)scheduler.Pending[0].create();
            var moduleArrived = (ModuleArrivedEvent)scheduler.Pending[1].create();
            Assert.AreEqual( "Arrival", shipArrived.system );
            Assert.AreEqual( "Port", shipArrived.station );
            Assert.AreSame( knownShip, shipArrived.Ship );
            Assert.AreEqual( "Arrival", moduleArrived.system );
            Assert.AreEqual( "Port", moduleArrived.station );
            ship.fromLoad = true;
            module.fromLoad = true;
            await processor.ProcessEventAsync( ship );
            await processor.ProcessEventAsync( module );
            Assert.HasCount( 2, scheduler.Pending );
            Assert.IsEmpty( JournalMonitor.ParseJournalEntry( ShipTransferInitiatedEvent.SAMPLE, new ParseContext(), true ) );
            Assert.IsEmpty( JournalMonitor.ParseJournalEntry( ModuleTransferEvent.SAMPLE, new ParseContext(), true ) );
        }

        [TestMethod]
        public async Task Friends_AreReleasedInOrderAfterCommander_AndReplayIsExcluded ()
        {
            var processed = new List<Event>();
            var pipeline = Pipeline( e => { processed.Add( e ); return Task.FromResult( true ); } );
            var first = new FriendsEvent( Timestamp, "First", "Online" );
            var second = new FriendsEvent( Timestamp, "Second", "Offline" );
            await pipeline.HandleEventAsync( first );
            await pipeline.HandleEventAsync( second );
            await pipeline.HandleEventAsync( new FriendsEvent( Timestamp, "Replay", "Online" ) { fromLoad = true } );
            Assert.IsEmpty( processed );
            Assert.IsFalse( pipeline.LastEventOfType.ContainsKey( FriendsEvent.NAME ) );
            var commander = new CommanderLoadingEvent( Timestamp, "Commander", "FID" );
            await pipeline.HandleEventAsync( commander );
            CollectionAssert.AreEqual( new Event[] { commander, first, second }, processed );
            Assert.AreSame( second, pipeline.LastEventOfType[FriendsEvent.NAME] );
            var third = new FriendsEvent( Timestamp, "Third", "Online" );
            await pipeline.HandleEventAsync( third );
            Assert.AreSame( third, processed.Last() );

            var nextHeader = new FileHeaderEvent( Timestamp, "Journal.next.log", "4.0", "build" );
            var nextFriend = new FriendsEvent( Timestamp, "Next session", "Online" );
            await pipeline.HandleEventAsync( nextHeader );
            await pipeline.HandleEventAsync( nextFriend );
            Assert.AreSame( nextHeader, processed.Last() );
            var nextCommander = new CommanderLoadingEvent( Timestamp, "Next commander", "FID2" );
            await pipeline.HandleEventAsync( nextCommander );
            CollectionAssert.AreEqual( new Event[] { nextCommander, nextFriend }, processed.TakeLast( 2 ).ToArray() );
        }
    }
}
