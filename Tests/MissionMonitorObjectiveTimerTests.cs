using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiEvents;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class MissionMonitorObjectiveTimerTests : TestBase
    {
        private static readonly DateTime Start = new( 2026, 10, 5, 12, 0, 0, DateTimeKind.Utc );

        [TestInitialize]
        public void Initialize () => MakeSafe();

        private static Mission Objective ( string name ) => new( 1, name, Start.AddMinutes( 2 ), MissionStatus.Active )
            { originsystem = "Origin", originstation = "Home", destinationsystem = "Target", amount = 2 };

        private static async Task<MissionMonitor> Monitor ( Func<DateTime> clock, List<Event> events, Mission mission )
        {
            var monitor = new MissionMonitor( clock, () => true, events.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ], updatedat = Start, missionWarning = 1 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( Start, "Journal.timer.log", "test", "test" ) );
            monitor.ArmMissionTimer( monitor.missions.Single() );
            return monitor;
        }

        private static MissionRedirectedEvent Redirect ( DateTime timestamp, string name, bool final = true ) =>
            new( timestamp, 1, name, final ? "home" : "Elsewhere", null, final ? "origin" : "Target", null );

        [TestMethod]
        [DataRow( "Mission_Massacre", false )]
        [DataRow( "Mission_MassacreWing_Legal_CivilUnrest", false )]
        [DataRow( "Mission_Assassinate_Legal_Corporate", false )]
        [DataRow( "Mission_Salvage", false )]
        [DataRow( "Mission_Scan", false )]
        [DataRow( "Mission_OnFoot_Massacre", true )]
        [DataRow( "Mission_OnFoot_Reboot", true )]
        public async Task FinalOriginReturnStopsShipTimersButNotOnFootTimers ( string name, bool onFoot )
        {
            var now = Start;
            var events = new List<Event>();
            var monitor = await Monitor( () => now, events, Objective( name ) );
            var mission = monitor.missions.Single();
            var deadline = mission.expiry;
            var changed = new HashSet<string>();
            mission.PropertyChanged += ( _, args ) => changed.Add( args.PropertyName );
            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( Redirect( now, name ) );
            Assert.IsTrue( mission.claimable );
            Assert.AreEqual( 100M, mission.progress );
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            Assert.AreEqual( deadline, mission.expiry );
            Assert.AreEqual( onFoot, mission.expirationTimerApplicable );
            Assert.AreEqual( onFoot, mission.timeRemaining.HasValue );
            CollectionAssert.Contains( changed.ToList(), nameof( Mission.timeRemaining ) );
            now = Start.AddSeconds( 61 );
            monitor.ProcessMissionTimers();
            now = Start.AddMinutes( 2 );
            monitor.ProcessMissionTimers();
            monitor.ProcessMissionTimers();
            Assert.AreEqual( onFoot ? 1 : 0, events.OfType<MissionWarningEvent>().Count() );
            Assert.AreEqual( onFoot ? 1 : 0, events.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task PositiveExpirySnapshotsCannotRearmACompletedShipObjective ()
        {
            var now = Start;
            var events = new List<Event>();
            var monitor = await Monitor( () => now, events, Objective( "Mission_Assassinate" ) );
            await monitor.PreHandleAsync( Redirect( now.AddSeconds( 1 ), "Mission_Assassinate" ) );
            now = now.AddSeconds( 2 );
            var deadline = Start.AddMinutes( 4 );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 1, "Mission_Assassinate", deadline, MissionStatus.Active ) ] ) );
            Assert.AreEqual( deadline, monitor.missions.Single().expiry );
            Assert.IsNull( monitor.missions.Single().timeRemaining );
            now = deadline.AddSeconds( -30 );
            monitor.ProcessMissionTimers();
            now = deadline;
            monitor.ProcessMissionTimers();
            Assert.IsEmpty( events);
        }

        [TestMethod]
        public async Task RetractingTheFinalRedirectRearmsOnlyThePreservedFutureDeadline ()
        {
            var now = Start;
            var events = new List<Event>();
            var monitor = await Monitor( () => now, events, Objective( "Mission_Assassinate" ) );
            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( Redirect( now, "Mission_Assassinate" ) );
            monitor.ProcessMissionTimers();
            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( Redirect( now, "Mission_Assassinate", final: false ) );
            Assert.IsTrue( monitor.missions.Single().expirationTimerApplicable );
            Assert.AreEqual( Start.AddMinutes( 2 ), monitor.missions.Single().expiry );
            now = Start.AddSeconds( 61 );
            monitor.ProcessMissionTimers();
            now = Start.AddMinutes( 2 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, events.OfType<MissionWarningEvent>().Count() );
            Assert.AreEqual( 1, events.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task SyntheticAndUnqualifiedRedirectsDoNotStopTimers ()
        {
            var now = Start;
            var events = new List<Event>();
            var monitor = await Monitor( () => now, events, Objective( "Mission_Assassinate" ) );
            var synthetic = new MissionRedirectedEvent( now, 1, "Mission_Assassinate", "Home", null, "Origin", null )
                { isWaypointAnnouncement = true };
            await monitor.PreHandleAsync( synthetic );
            Assert.IsTrue( monitor.missions.Single().expirationTimerApplicable );
            await monitor.PreHandleAsync( Redirect( now.AddSeconds( 1 ), "Mission_Assassinate", final: false ) );
            Assert.IsTrue( monitor.missions.Single().expirationTimerApplicable );
            now = Start.AddMinutes( 2 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, events.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public async Task PartialAndFullDeliveryClaimabilityDoNotStopTimers ( int delivered )
        {
            var now = Start;
            var events = new List<Event>();
            var delivery = Objective( "Mission_Delivery" );
            delivery.delivered = delivered;
            var monitor = await Monitor( () => now, events, delivery );
            Assert.IsTrue( monitor.missions.Single().claimable );
            await monitor.PreHandleAsync( Redirect( now.AddSeconds( 1 ), delivery.name ) );
            Assert.IsTrue( monitor.missions.Single().expirationTimerApplicable );
            now = Start.AddMinutes( 2 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, events.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task FullCargoAndEstimatedQuotaAloneDoNotStopTimers ()
        {
            var now = Start;
            var events = new List<Event>();
            var salvage = Objective( "Mission_Salvage" );
            salvage.ObserveOnboardCargo( now, 2 );
            var monitor = await Monitor( () => now, events, salvage );
            Assert.AreEqual( 100M, monitor.missions.Single().progress );
            Assert.IsTrue( monitor.missions.Single().expirationTimerApplicable );
            now = Start.AddMinutes( 2 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, events.OfType<MissionExpiredEvent>().Count() );
            var combat = Objective( "Mission_Massacre" );
            combat.faction = "Issuer";
            combat.targetfaction = "Pirates";
            combat.targetTypeEDName = "Pirate";
            combat.ObserveCombatAcceptance( Start, 1, 2, "Target" );
            combat.ObserveMatchingCombatBounty( Start );
            combat.ObserveMatchingCombatBounty( Start );
            Assert.AreEqual( 100M, combat.estimatedProgress );
            Assert.IsTrue( combat.expirationTimerApplicable );
            Assert.IsFalse( combat.claimable );
        }

        [TestMethod]
        public async Task CargoLossDoesNotUndoTheObservedTimerStopAndReloadPreservesTheDeadline ()
        {
            var now = Start;
            var monitor = await Monitor( () => now, [ ], Objective( "Mission_Salvage" ) );
            await monitor.PreHandleAsync( Redirect( now.AddSeconds( 1 ), "Mission_Salvage" ) );
            var mission = monitor.missions.Single();
            mission.ObserveOnboardCargo( now.AddSeconds( 2 ), 2 );
            Assert.IsTrue( mission.claimable );
            mission.ObserveOnboardCargo( now.AddSeconds( 3 ), 1 );
            Assert.IsFalse( mission.claimable );
            Assert.IsFalse( mission.expirationTimerApplicable );
            var stored = JsonConvert.DeserializeObject<Mission>( JsonConvert.SerializeObject( mission ) );
            Assert.AreEqual( Start.AddMinutes( 2 ), stored.expiry );
            Assert.IsNull( stored.timeRemaining );
            Assert.IsFalse( stored.expirationTimerApplicable );
        }
    }
}
