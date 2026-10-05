using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class MissionMonitorProgressTests : TestBase
    {
        private static readonly DateTime Start = new( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc );
        private StarSystem originalSystem;

        [TestInitialize]
        public void Initialize ()
        {
            MakeSafe();
            originalSystem = EDDI.Instance.GameState.CurrentStarSystem;
        }

        [TestCleanup]
        public void Cleanup () => EDDI.Instance.GameStateMutator.CurrentStarSystem = originalSystem;

        private static Mission Tour ( ulong id = 1 ) => new( id, "Mission_Sightseeing", Start.AddDays( 1 ), MissionStatus.Active )
        {
            originsystem = "Origin", originstation = "Home", destinationsystem = "First",
            destinationsystems = [ new NavWaypoint( "First", 1, 0, 0, 0 ), new NavWaypoint( "Second", 2, 1, 0, 0 ),
                new NavWaypoint( "Third", 3, 2, 0, 0 ) ]
        };

        private static void At ( string name, ulong address ) =>
            EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = name, systemAddress = address };

        private static DataScannedEvent Beacon ( int seconds ) =>
            new( Start.AddSeconds( seconds ), DataScan.FromEDName( "TouristBeacon" ) );

        private static async Task<MissionMonitor> Monitor ( List<Event> events, bool running = true, params Mission[] missions )
        {
            var monitor = new MissionMonitor( () => Start.AddMinutes( 1 ), () => running, events.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ .. missions ], updatedat = Start } );
            await monitor.PreHandleAsync( new FileHeaderEvent( Start, "Journal.progress.log", "test", "test" ) );
            return monitor;
        }

        [TestMethod]
        [DataRow( "Mission_Courier" )]
        [DataRow( "Mission_Scan" )]
        [DataRow( "Mission_Assassinate" )]
        [DataRow( "Mission_OnFoot_Reboot" )]
        public void DiscreteObjectivesDisplayZeroUntilCompletionEvidence ( string name )
        {
            var mission = new Mission( 1, name, null, MissionStatus.Active );
            Assert.IsTrue( mission.progressKnown );
            Assert.AreEqual( "0%", mission.localizedProgress );
            Assert.IsFalse( mission.claimable );
            mission.SetRedirectClaimability( true );
            Assert.AreEqual( "100%", mission.localizedProgress );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            mission.SetRedirectClaimability( false );
            Assert.AreEqual( "0%", mission.localizedProgress );
            mission.statusDef = MissionStatus.Complete;
            Assert.AreEqual( "100%", mission.localizedProgress );
            Assert.IsFalse( mission.claimable );
        }

        [TestMethod]
        [DataRow( "Mission_None" )]
        [DataRow( "Mission_Massacre" )]
        [DataRow( "Mission_UnknownFutureObjective" )]
        public void UnmeasurableObjectivesStayBlankUntilAuthoritativeCompletion ( string name )
        {
            var mission = new Mission( 1, name, null, MissionStatus.Active );
            Assert.IsFalse( mission.progressKnown );
            Assert.AreEqual( 0M, mission.progress, "Keep the existing decimal runtime API." );
            Assert.AreEqual( string.Empty, mission.localizedProgress );
            mission.statusDef = MissionStatus.Complete;
            Assert.IsTrue( mission.progressKnown );
            Assert.AreEqual( "100%", mission.localizedProgress );
        }

        [TestMethod]
        public async Task BeaconScansUpdateAllMatchingMissionsWithoutInventingClaimability ()
        {
            var events = new List<Event>();
            var monitor = await Monitor( events, true, Tour(), Tour( 2 ) );
            Assert.IsTrue( monitor.missions.All( m => m.localizedProgress == "0%" ) );
            At( "First", 1 );
            await monitor.PreHandleAsync( Beacon( 1 ) );
            Assert.IsTrue( monitor.missions.All( m => m.progress == 100M / 3M && m.destinationsystem == "Second" ) );
            Assert.AreEqual( 2, events.Count );
            Assert.AreEqual( Start, ConfigService.Instance.missionMonitorConfiguration.updatedat );
            foreach ( var announcement in events.ToArray() ) { await monitor.PreHandleAsync( announcement ); }
            Assert.AreEqual( Start, ConfigService.Instance.missionMonitorConfiguration.updatedat );
            await monitor.PreHandleAsync( Beacon( 2 ) );
            Assert.AreEqual( 2, events.Count, "Repeated scans in the same system do not count twice." );
            At( "Third", 3 );
            await monitor.PreHandleAsync( Beacon( 3 ) );
            Assert.IsTrue( monitor.missions.All( m => m.progress == 100M / 3M ), "An out-of-order stop is not credited." );
            At( "Second", 2 );
            await monitor.PreHandleAsync( Beacon( 4 ) );
            At( "Third", 3 );
            await monitor.PreHandleAsync( Beacon( 5 ) );
            Assert.IsTrue( monitor.missions.All( m => m.localizedProgress == "100%" && !m.claimable && m.statusDef == MissionStatus.Active ) );
            await monitor.PreHandleAsync( new MissionRedirectedEvent( Start.AddSeconds( 6 ), 1, "Mission_Sightseeing",
                "Home", null, "Origin", "Third" ) );
            Assert.IsTrue( monitor.missions.Single( m => m.missionid == 1 ).claimable );
            Assert.IsFalse( monitor.missions.Single( m => m.missionid == 2 ).claimable );
        }

        [TestMethod]
        public async Task ArrivalUnrelatedScansOfflineAndTerminalMissionsDoNotAdvanceWaypoints ()
        {
            var monitor = await Monitor( [ ], true, Tour() );
            var mission = monitor.missions.Single();
            At( "First", 1 );
            Assert.AreEqual( "0%", mission.localizedProgress, "Merely arriving is not a sightseeing stop." );
            await monitor.PreHandleAsync( new DataScannedEvent( Start.AddSeconds( 1 ), DataScan.FromEDName( "DataLink" ) ) );
            At( "Unrelated", 99 );
            await monitor.PreHandleAsync( Beacon( 2 ) );
            At( "First", 99 );
            await monitor.PreHandleAsync( Beacon( 3 ) );
            Assert.AreEqual( "0%", mission.localizedProgress );
            At( "First", 1 );
            await monitor.PreHandleAsync( new MissionFailedEvent( Start.AddSeconds( 4 ), 1, mission.name, 0 ) );
            await monitor.PreHandleAsync( Beacon( 5 ) );
            Assert.AreEqual( "0%", mission.localizedProgress );

            monitor = await Monitor( [ ], false, Tour() );
            await monitor.PreHandleAsync( Beacon( 6 ) );
            Assert.AreEqual( "0%", monitor.missions.Single().localizedProgress );
        }

        [TestMethod]
        public async Task WaypointEvidenceSurvivesReloadAndDetachedPublication ()
        {
            var monitor = await Monitor( [ ], true, Tour() );
            At( "First", 1 );
            await monitor.PreHandleAsync( Beacon( 1 ) );
            var published = (List<Mission>)monitor.GetVariableValues().ToRuntimeValueDictionary()[ "missions" ].Item2;
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = new MissionMonitor( () => Start.AddDays( 2 ), () => false, _ => { } );
            reload.initializeMissionMonitor( stored );
            Assert.AreEqual( 100M / 3M, reload.missions.Single().progress );
            At( "Second", 2 );
            await monitor.PreHandleAsync( Beacon( 2 ) );
            Assert.AreEqual( 100M / 3M, published.Single().progress );
            Assert.AreEqual( 200M / 3M, monitor.missions.Single().progress );
            Assert.IsFalse( monitor.missions.Single().ObserveSightseeingStop( Start, "Third", 3 ) );
        }

        [TestMethod]
        public void JournalOrderAndUnresolvedStopsPreserveTheProgressDenominator ()
        {
            var accepted = new MissionAcceptedEvent( Start, Tour() ) { JournalDestinationSystems = [ "First", "Second", "Third" ] };
            accepted.Mission.destinationsystems.Clear();
            accepted.Mission.InitializeSightseeingWaypoints( accepted.JournalDestinationSystems );
            accepted.ResolveDestinations( [ new NavWaypoint( "Third", 3, 2, 0, 0 ), new NavWaypoint( "First", 1, 0, 0, 0 ) ] );
            CollectionAssert.AreEqual( new[] { "First", "Third" }, accepted.Mission.destinationsystems.Select( s => s.systemName ).ToArray() );
            Assert.IsTrue( accepted.Mission.ObserveSightseeingStop( Start, "First", 1 ) );
            Assert.AreEqual( 100M / 3M, accepted.Mission.progress );
            Assert.IsTrue( accepted.Mission.ObserveSightseeingStop( Start.AddSeconds( 1 ), "Second", 2 ) );
            Assert.AreEqual( 200M / 3M, accepted.Mission.progress );
        }

        [TestMethod]
        public void AmbiguousRepeatedSystemsCannotManufactureMeasuredProgress ()
        {
            var mission = Tour();
            mission.InitializeSightseeingWaypoints( [ "First", "First" ] );
            Assert.IsFalse( mission.progressKnown );
            Assert.AreEqual( string.Empty, mission.localizedProgress );
            Assert.IsFalse( mission.ObserveSightseeingStop( Start, "First", 1 ) );
            Assert.IsFalse( mission.claimable );
        }

        [TestMethod]
        public void CommunityGoalPresentationUsesContributionAndReportedBand ()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo( "fr-FR" );
                var mission = new Mission( 1, "Mission_CommunityGoal", null, MissionStatus.Active ) { communal = true };
                var converter = new MissionProgressConverter();
                Assert.IsFalse( mission.communityGoalProgressKnown );
                mission.ObserveCommunityGoalProgress( 12345, 25 );
                var output = (string)converter.Convert( [ mission.localizedProgress, true, mission.communityGoalProgressKnown,
                    mission.communalContribution, mission.communalPercentileBand ], typeof( string ), null, CultureInfo.CurrentCulture );
                Assert.Contains( 12345L.ToString( "N0", CultureInfo.CurrentCulture ), output );
                Assert.Contains( "25%", output );
                Assert.DoesNotContain( "75%", output );
                Assert.IsFalse( mission.claimable );
                mission.ObserveCommunityGoalProgress( -1, 101 );
                Assert.AreEqual( 12345L, mission.communalContribution );
                var stored = JsonConvert.DeserializeObject<Mission>( JsonConvert.SerializeObject( mission ) );
                Assert.IsTrue( stored.communityGoalProgressKnown );
                Assert.AreEqual( 25, stored.communalPercentileBand );
            }
            finally { CultureInfo.CurrentCulture = originalCulture; }
        }
    }
}
