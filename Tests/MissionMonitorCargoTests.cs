using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class MissionMonitorCargoTests : TestBase
    {
        private static readonly DateTime Start = new( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc );

        [TestInitialize]
        public void Initialize () => MakeSafe();

        private static Mission CargoMission ( ulong id = 1067632524, string name = "MISSION_Salvage_CivilUnrest", int? required = 2 ) =>
            new( id, name, Start.AddDays( 4 ), MissionStatus.Active )
            {
                originsystem = "Laksak", originstation = "Sekelj Port", destinationsystem = "HIP 89272",
                destinationstation = "Sanger Survey", commodity = "Black Box", amount = required
            };

        private static async Task<MissionMonitor> LiveMonitor ( params Mission[] missions )
        {
            var monitor = new MissionMonitor( () => Start, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ .. missions ], updatedat = Start } );
            await monitor.PreHandleAsync( new FileHeaderEvent( Start, "Journal.cargo.log", "test", "test" ) );
            return monitor;
        }

        private static CargoEvent Manifest ( DateTime timestamp, ulong id, int quantity,
            DateTime? inventoryTimestamp = null ) =>
            new( timestamp, true, Constants.VEHICLE_SHIP,
                quantity == 0 ? [ ] : [ new CargoInfoItem( "USSCargoBlackBox", id, quantity, 0 ) ],
                quantity, inventoryTimestamp );

        private static MissionRedirectedEvent ReturnRedirect ( DateTime timestamp, Mission mission ) =>
            new( timestamp, mission.missionid, mission.name, "Sekelj Port", "Sanger Survey", "Laksak", "HIP 89272" );

        [TestMethod]
        public async Task PartialCargoAcrossFsdCanLaterBecomeClaimableAndComplete ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 1 ), mission.missionid, 1 ) );
            Assert.AreEqual( 50M, mission.progress );
            Assert.IsFalse( mission.claimable );

            var departure = JournalMonitor.ParseJournalEntry(
                """{ "timestamp":"2026-10-04T12:00:02Z", "event":"StartJump", "JumpType":"Hyperspace", "StarSystem":"Laksak", "SystemAddress":4305444669811, "StarClass":"G" }""" ).Single();
            await monitor.PreHandleAsync( departure );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            Assert.AreEqual( 50M, mission.progress );

            await monitor.PreHandleAsync( ReturnRedirect( Start.AddSeconds( 3 ), mission ) );
            Assert.IsFalse( mission.claimable );
            Assert.AreEqual( 50M, mission.progress );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 4 ), mission.missionid, 2 ) );
            Assert.AreEqual( 100M, mission.progress );
            Assert.IsTrue( mission.claimable );
            await monitor.PreHandleAsync( departure );
            Assert.IsTrue( mission.claimable );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );

            var completion = new MissionCompletedEvent( Start.AddSeconds( 5 ), mission.missionid, mission.name,
                null, null, null, null, false, 0 );
            await monitor.PreHandleAsync( completion );
            Assert.AreEqual( MissionStatus.Complete, mission.statusDef );
            Assert.IsFalse( mission.claimable );
            await monitor.PostHandleAsync( completion );
            Assert.IsEmpty( monitor.missions);
        }

        [TestMethod]
        [DataRow( "MISSION_Salvage_CivilUnrest" )]
        [DataRow( "Mission_Rescue_Planet_Expansion" )]
        [DataRow( "Mission_Smuggle_Famine" )]
        public async Task CargoLossAndRestorationUpdateReadinessWithoutFailure ( string name )
        {
            var monitor = await LiveMonitor( CargoMission( name: name ) );
            var mission = monitor.missions.Single();
            // The smuggling type has no origin-return tag. Full observed depot collection supplies its objective evidence.
            if ( name.Contains( "Smuggle" ) )
            {
                await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 1 ), mission.missionid,
                    "Collect", mission.CommodityDefinition, 2, 1, 2, 2, 0, 2 ) );
            }
            else
            {
                await monitor.PreHandleAsync( ReturnRedirect( Start.AddSeconds( 1 ), mission ) );
            }
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 2 ), mission.missionid, 2 ) );
            Assert.IsTrue( mission.claimable );
            var changed = new HashSet<string>();
            mission.PropertyChanged += ( _, args ) => changed.Add( args.PropertyName );

            await monitor.PreHandleAsync( new CommodityEjectedEvent( Start.AddSeconds( 3 ),
                mission.CommodityDefinition, 1, (long)mission.missionid, false ) );
            Assert.IsTrue( mission.claimable, "Incremental loss waits for a fresh full manifest." );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 4 ), mission.missionid, 1 ) );
            Assert.AreEqual( 50M, mission.progress );
            Assert.IsFalse( mission.claimable );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            Assert.Contains( nameof( Mission.claimable ), changed );
            Assert.Contains( nameof( Mission.localizedProgress ), changed );

            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 5 ), mission.missionid, 2 ) );
            Assert.IsTrue( mission.claimable );
            Assert.AreEqual( 100M, mission.progress );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
        }

        [TestMethod]
        public async Task FullCargoRequiresObjectiveEvidence ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 1 ), mission.missionid, 2 ) );
            Assert.AreEqual( 100M, mission.progress );
            Assert.IsFalse( mission.claimable );
            await monitor.PreHandleAsync( ReturnRedirect( Start.AddSeconds( 2 ), mission ) );
            Assert.IsTrue( mission.claimable );
        }

        [TestMethod]
        [DataRow( 2, false, 40 )]
        [DataRow( 3, true, 60 )]
        [DataRow( 5, true, 100 )]
        public async Task DeliveryCargoLossPreservesCreditedProgress ( int delivered, bool claimable, int percent )
        {
            var monitor = await LiveMonitor( CargoMission( name: "Mission_Delivery_Wing", required: 5 ) );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 1 ), mission.missionid, "WingUpdate",
                mission.CommodityDefinition, null, 1, 2, 5, delivered, 5 ) );
            Assert.AreEqual( percent, (int)mission.progress );
            Assert.AreEqual( claimable, mission.claimable );
            Assert.IsTrue( mission.shared );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 2 ), mission.missionid, 0 ) );
            Assert.AreEqual( percent, (int)mission.progress );
            Assert.AreEqual( claimable, mission.claimable );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
        }

        [TestMethod]
        public async Task MiningArchiveDepotUpdatesBecomeClaimableAfterHalfDelivery ()
        {
            var now = new DateTime( 2026, 10, 12, 8, 23, 23, DateTimeKind.Utc );
            var deadline = new DateTime( 2026, 10, 19, 8, 21, 33, DateTimeKind.Utc );
            var mission = new Mission( 1068224618, "Mission_Mining", deadline, MissionStatus.Active )
            {
                originsystem = "Olgrea", originstation = "Warner Station", destinationsystem = "Olgrea",
                destinationstation = "Warner Station", commodity = "Methane Clathrate", amount = 36
            };
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ], updatedat = now } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.mining.log", "test", "test" ) );
            mission = monitor.missions.Single();

            // Minimized CargoDepot records from Journal.2026-10-11T142400.01.log.
            var depotRecords = new[]
            {
                """{ "timestamp":"2026-10-12T09:17:51Z", "event":"CargoDepot", "MissionID":1068224618, "UpdateType":"Deliver", "CargoType":"MethaneClathrate", "Count":17, "StartMarketID":0, "EndMarketID":3229533440, "ItemsCollected":0, "ItemsDelivered":17, "TotalItemsToDeliver":36, "Progress":0.000000 }""",
                """{ "timestamp":"2026-10-12T10:20:59Z", "event":"CargoDepot", "MissionID":1068224618, "UpdateType":"Deliver", "CargoType":"MethaneClathrate", "Count":6, "StartMarketID":0, "EndMarketID":3229533440, "ItemsCollected":0, "ItemsDelivered":23, "TotalItemsToDeliver":36, "Progress":0.000000 }""",
                """{ "timestamp":"2026-10-12T10:21:38Z", "event":"CargoDepot", "MissionID":1068224618, "UpdateType":"Deliver", "CargoType":"MethaneClathrate", "Count":13, "StartMarketID":0, "EndMarketID":3229533440, "ItemsCollected":0, "ItemsDelivered":36, "TotalItemsToDeliver":36, "Progress":0.000000 }"""
            };
            var expectedDelivered = new[] { 17, 23, 36 };
            for ( var index = 0; index < depotRecords.Length; index++ )
            {
                var depot = (CargoDepotEvent)JournalMonitor.ParseJournalEntry( depotRecords[ index ] ).Single();
                now = depot.timestamp;
                await monitor.PreHandleAsync( depot );

                Assert.AreEqual( 100M * expectedDelivered[ index ] / 36, mission.progress );
                Assert.AreEqual( expectedDelivered[ index ] >= 18, mission.claimable );
                Assert.AreEqual( MissionStatus.Active, mission.statusDef );
                Assert.AreEqual( deadline, mission.expiry );
                Assert.IsTrue( mission.expirationTimerApplicable, "Partial claimability must not stop the timer." );
            }

            var completion = new MissionCompletedEvent( now.AddSeconds( 7 ), mission.missionid, mission.name,
                null, null, null, null, false, 0 );
            await monitor.PreHandleAsync( completion );
            Assert.AreEqual( MissionStatus.Complete, mission.statusDef );
            Assert.IsFalse( mission.claimable );
            await monitor.PostHandleAsync( completion );
            Assert.IsEmpty( monitor.missions);
        }

        [TestMethod]
        public async Task InvalidDepotCountersPreserveLastConfirmedDeliveryProgress ()
        {
            var monitor = await LiveMonitor( CargoMission( name: "Mission_Delivery", required: 5 ) );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 1 ), mission.missionid, "Deliver",
                mission.CommodityDefinition, 3, 1, 2, 5, 3, 5 ) );
            foreach ( var (required, collected, delivered) in new[] { (0, 5, 3), (5, -1, 3), (5, 2, 3), (5, 5, 6), (5, 6, 3) } )
            {
                await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 3 ), mission.missionid, "Deliver",
                    mission.CommodityDefinition, 1, 1, 2, collected, delivered, required ) );
                Assert.AreEqual( 60M, mission.progress );
                Assert.IsTrue( mission.claimable );
                Assert.AreEqual( 5, mission.amount );
            }
            await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 3 ), mission.missionid, "Deliver",
                mission.CommodityDefinition, 6, 1, 2, 5, 5, 5 ) );
            Assert.AreEqual( 60M, mission.progress, "An out-of-range transaction quantity is contradictory evidence." );
            // A rejected event cannot move the watermark or suppress a valid, slightly older update.
            await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( 2 ), mission.missionid, "Deliver",
                mission.CommodityDefinition, 2, 1, 2, 5, 5, 5 ) );
            Assert.AreEqual( 100M, mission.progress );
        }

        [TestMethod]
        public async Task OldDepotReplayCannotRegressUntimestampedPersistedCounters ()
        {
            var storedMission = CargoMission( name: "Mission_Delivery", required: 5 );
            storedMission.collected = 5;
            storedMission.delivered = 3;
            var monitor = await LiveMonitor( storedMission );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( new CargoDepotEvent( Start.AddSeconds( -1 ), mission.missionid, "Collect",
                mission.CommodityDefinition, 1, 1, 2, 1, 0, 5 ) { fromLoad = true } );
            Assert.AreEqual( 60M, mission.progress );
            Assert.IsTrue( mission.claimable );
        }

        [TestMethod]
        public async Task StaleUnknownAndContradictoryCargoPreserveConfirmedReadiness ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( ReturnRedirect( Start.AddSeconds( 1 ), mission ) );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 2 ), mission.missionid, 2 ) );
            await monitor.PreHandleAsync( new CommodityEjectedEvent( Start.AddSeconds( 4 ),
                mission.CommodityDefinition, 1, (long)mission.missionid, false ) );
            var invalidManifests = new[]
            {
                Manifest( Start.AddSeconds( 3 ), mission.missionid, 0 ),
                Manifest( Start.AddSeconds( 5 ), mission.missionid, 0, Start.AddSeconds( 3 ) ),
                new CargoEvent( Start.AddSeconds( 5 ), true, "Ship", null, 0 ),
                new CargoEvent( Start.AddSeconds( 5 ), true, "Ship", [ ], 2 ),
                Manifest( Start.AddSeconds( 5 ), mission.missionid, -1 ),
                Manifest( Start.AddSeconds( 5 ), mission.missionid, 3 ),
                new CargoEvent( Start.AddSeconds( 5 ), true, "Ship", [ new CargoInfoItem( "Gold", mission.missionid, 1, 0 ) ], 1 ),
                new CargoEvent( Start.AddSeconds( 5 ), true, "SRV", [ ], 0 )
            };
            foreach ( var manifest in invalidManifests )
            {
                await monitor.PreHandleAsync( manifest );
                Assert.AreEqual( 100M, mission.progress );
                Assert.IsTrue( mission.claimable );
                Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            }
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 6 ), mission.missionid, 0 ) );
            Assert.AreEqual( 0M, mission.progress );
            Assert.IsFalse( mission.claimable );
        }

        [TestMethod]
        public async Task CargoDoesNotAdvanceWatermarkOrSuppressAuthoritativeFailure ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 10 ), mission.missionid, 1 ) );
            Assert.AreEqual( Start, ConfigService.Instance.missionMonitorConfiguration.updatedat );
            await monitor.PreHandleAsync( new MissionFailedEvent( Start.AddSeconds( 9 ), mission.missionid, mission.name, 0 ) );
            Assert.AreEqual( MissionStatus.Failed, mission.statusDef );
            Assert.AreEqual( 50M, mission.progress );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 11 ), mission.missionid, 2 ) );
            Assert.IsFalse( mission.claimable );
            Assert.AreEqual( 50M, mission.progress );
            Assert.AreEqual( MissionStatus.Failed, mission.statusDef );
        }

        [TestMethod]
        public async Task EvidenceSurvivesReloadAndOfflineStartup ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( ReturnRedirect( Start.AddSeconds( 1 ), mission ) );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 2 ), mission.missionid, 1 ) );
            var configuration = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var offlineMonitor = new MissionMonitor( () => Start.AddDays( 5 ), () => false, _ => { } );
            offlineMonitor.initializeMissionMonitor( configuration );
            await offlineMonitor.PreHandleAsync( new FileHeaderEvent( Start, "Journal.offline.log", "test", "test" ) );
            await offlineMonitor.PreHandleAsync( Manifest( Start.AddSeconds( 10 ), mission.missionid, 0 ) );
            offlineMonitor.ProcessMissionTimers();
            Assert.AreEqual( 50M, offlineMonitor.missions.Single().progress );
            Assert.IsFalse( offlineMonitor.missions.Single().claimable );
            Assert.AreEqual( MissionStatus.Active, offlineMonitor.missions.Single().statusDef );
        }

        [TestMethod]
        public async Task ReplayRepairsRejectedBlackBoxRedirectUsingActualJournalEvidence ()
        {
            var blackBox = CargoMission( required: 1 );
            blackBox.destinationsystem = blackBox.originsystem;
            blackBox.destinationstation = blackBox.originstation;
            var samples = CargoMission( 1067633353, "MISSION_Salvage_Expansion", 1 );
            samples.commodity = "Commercial Samples";
            samples.originsystem = "Scorpii Sector IC-V b2-8";
            samples.originstation = "182 One World Ev";
            samples.destinationsystem = "Obambojas";
            samples.destinationstation = "Wheeler Hub";
            var monitor = await LiveMonitor( blackBox, samples );
            const string oldRedirect = """
                { "timestamp":"2026-10-04T11:53:57Z", "event":"MissionRedirected", "MissionID":1067632524, "Name":"MISSION_Salvage_CivilUnrest", "NewDestinationStation":"Sekelj Port", "NewDestinationSystem":"Laksak", "OldDestinationStation":"", "OldDestinationSystem":"HIP 89272" }
                """;
            const string newRedirect = """
                { "timestamp":"2026-10-04T12:08:46Z", "event":"MissionRedirected", "MissionID":1067633353, "Name":"MISSION_Salvage_Expansion", "NewDestinationStation":"182 One World Ev", "NewDestinationSystem":"Scorpii Sector IC-V b2-8", "OldDestinationStation":"", "OldDestinationSystem":"Obambojas" }
                """;
            var replayedEvent = JournalMonitor.ParseJournalEntry( oldRedirect ).Single();
            replayedEvent.fromLoad = true;
            await monitor.PreHandleAsync( replayedEvent );
            await monitor.PreHandleAsync( JournalMonitor.ParseJournalEntry( newRedirect ).Single() );
            await monitor.PreHandleAsync( new CargoEvent( Start.AddMinutes( 8 ).AddSeconds( 47 ), true, "Ship",
                [ new CargoInfoItem( "USSCargoBlackBox", blackBox.missionid, 1, 0 ),
                  new CargoInfoItem( "ComercialSamples", samples.missionid, 1, 0 ),
                  new CargoInfoItem( "Drones", null, 30, 0 ) ], 32 ) );
            Assert.IsTrue( monitor.missions.All( m => m.claimable && m.progress == 100M && m.statusDef == MissionStatus.Active ) );
        }

        [TestMethod]
        public async Task MatchingDestinationAloneCannotInventObjectiveCompletion ()
        {
            var mission = CargoMission( required: 1 );
            mission.destinationsystem = mission.originsystem;
            mission.destinationstation = mission.originstation;
            var monitor = await LiveMonitor( mission );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 1 ), mission.missionid, 1 ) );
            Assert.AreEqual( 100M, monitor.missions.Single().progress );
            Assert.IsFalse( monitor.missions.Single().claimable );
        }

        [TestMethod]
        public async Task TerminalRemovalAndSnapshotOmissionSupersedeCargoEvidence ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 1 ), mission.missionid, 1 ) );
            await monitor.PreHandleAsync( new MissionsEvent( Start.AddSeconds( 2 ), [ ] ) );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 3 ), mission.missionid, 2 ) );
            Assert.IsEmpty( monitor.missions);

            monitor = await LiveMonitor( CargoMission() );
            mission = monitor.missions.Single();
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 1 ), mission.missionid, 1 ) );
            var abandonment = new MissionAbandonedEvent( Start.AddSeconds( 2 ), mission.missionid, mission.name, 0 );
            await monitor.PreHandleAsync( abandonment );
            Assert.AreEqual( MissionStatus.Failed, mission.statusDef );
            Assert.AreEqual( 50M, mission.progress );
            Assert.IsFalse( mission.claimable );
            await monitor.PostHandleAsync( abandonment );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 3 ), mission.missionid, 2 ) );
            Assert.IsEmpty( monitor.missions);
        }

        [TestMethod]
        public async Task SidecarObservationTimeIsPreservedAndRuntimeSnapshotsAreDetached ()
        {
            var monitor = await LiveMonitor( CargoMission() );
            var mission = monitor.missions.Single();
            await monitor.PreHandleAsync( new CommodityEjectedEvent( Start.AddSeconds( 3 ),
                mission.CommodityDefinition, 1, (long)mission.missionid, false ) );
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 2 ), mission.missionid, 1, Start.AddSeconds( 4 ) ) );
            Assert.AreEqual( 50M, mission.progress );
            var snapshots = (List<Mission>)monitor.GetVariableValues().ToRuntimeValueDictionary()[ "missions" ].Item2;
            await monitor.PreHandleAsync( Manifest( Start.AddSeconds( 5 ), mission.missionid, 2 ) );
            Assert.AreEqual( 50M, snapshots.Single().progress );
            Assert.AreEqual( 100M, mission.progress );
        }
    }
}
