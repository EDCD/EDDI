using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" ), DoNotParallelize]
    public class MissionMonitorCombatTests : TestBase
    {
        private static readonly DateTime Start = new( 2026, 10, 4, 12, 0, 0, DateTimeKind.Utc );
        private StarSystem originalSystem;
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize ()
        {
            MakeSafe();
            originalSystem = EDDI.Instance.GameState.CurrentStarSystem;
            EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Hunting ground" };
        }

        [TestCleanup]
        public void Cleanup () => EDDI.Instance.GameStateMutator.CurrentStarSystem = originalSystem;

        private static Mission Pirate ( ulong id, string issuer = "Issuer A", int required = 3 ) =>
            new( id, "Mission_MassacreWing", Start.AddDays( 1 ), MissionStatus.Active )
            {
                faction = issuer, targetfaction = "Pirate faction", targetTypeEDName = "Pirate", amount = required,
                destinationsystem = "Hunting ground", originsystem = "Origin", originstation = "Home"
            };

        private static BountyAwardedEvent Bounty ( int second = 10, string faction = "Pirate faction", string target = "anaconda", long reward = 100 ) =>
            new( Start.AddSeconds( second ), target, null, "Pilot", faction, reward, [ ], false );

        private static async Task<MissionMonitor> Monitor ( bool running = true, MissionMonitorConfiguration configuration = null )
        {
            var monitor = new MissionMonitor( () => Start.AddMinutes( 1 ), () => running, _ => { } );
            monitor.initializeMissionMonitor( configuration ?? new MissionMonitorConfiguration { updatedat = Start } );
            await monitor.PreHandleAsync( new FileHeaderEvent( Start, "Journal.combat.log", "test", "test" ) );
            return monitor;
        }

        private static async Task Accept ( MissionMonitor monitor, Mission mission, int second = 1, bool fromLoad = false ) =>
            await monitor.PreHandleAsync( new MissionAcceptedEvent( Start.AddSeconds( second ), mission )
                { HasKillCount = mission.amount.HasValue, fromLoad = fromLoad } );

        private static Mission Mission ( MissionMonitor monitor, ulong id ) => monitor.missions.Single( m => m.missionid == id );

        [TestMethod]
        public async Task IssuersStackButTheirOwnQueuesAdvanceOnlyOnConfirmedCompletion ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 30, required: 1 ) );
            await Accept( monitor, Pirate( 10 ), 2 ); // IDs are not acceptance order.
            await Accept( monitor, Pirate( 20, "Issuer B" ), 3 );
            await monitor.PreHandleAsync( Bounty() );
            await monitor.PreHandleAsync( Bounty( 11 ) );
            Assert.AreEqual( 2L, Mission( monitor, 30 ).estimatedKills );
            Assert.AreEqual( 0L, Mission( monitor, 10 ).estimatedKills );
            Assert.AreEqual( 2L, Mission( monitor, 20 ).estimatedKills );
            Assert.AreEqual( 100M, Mission( monitor, 30 ).estimatedProgress );
            Assert.IsFalse( Mission( monitor, 30 ).progressKnown );
            Assert.IsFalse( Mission( monitor, 30 ).claimable );
            Assert.AreEqual( MissionStatus.Active, Mission( monitor, 30 ).statusDef );
            Assert.AreEqual( Start.AddSeconds( 3 ), ConfigService.Instance.missionMonitorConfiguration.updatedat );

            await monitor.PreHandleAsync( new MissionRedirectedEvent( Start.AddSeconds( 12 ), 30, "Mission_MassacreWing",
                "Home", null, "Origin", "Hunting ground" ) { isWaypointAnnouncement = true } );
            await monitor.PreHandleAsync( Bounty( 13 ) );
            Assert.AreEqual( 3L, Mission( monitor, 30 ).estimatedKills, "Synthetic route announcements cannot advance a combat queue." );
            await monitor.PreHandleAsync( new MissionRedirectedEvent( Start.AddSeconds( 14 ), 30, "Mission_MassacreWing",
                "Elsewhere", null, "Other system", "Hunting ground" ) );
            Assert.IsFalse( Mission( monitor, 30 ).claimable, "An unrelated redirect is not final objective completion." );
            await monitor.PreHandleAsync( Bounty( 14 ) );
            Assert.AreEqual( 4L, Mission( monitor, 30 ).estimatedKills );
            Assert.AreEqual( 0L, Mission( monitor, 10 ).estimatedKills, "Changing a destination alone cannot advance a queue." );
            await monitor.PreHandleAsync( new MissionRedirectedEvent( Start.AddSeconds( 15 ), 30, "Mission_MassacreWing",
                "Home", null, "Origin", "Other system" ) );
            Assert.IsTrue( Mission( monitor, 30 ).claimable );
            Assert.AreEqual( 100M, Mission( monitor, 30 ).progress );
            await monitor.PreHandleAsync( Bounty( 16 ) );
            Assert.AreEqual( 1L, Mission( monitor, 10 ).estimatedKills );
            Assert.AreEqual( 5L, Mission( monitor, 20 ).estimatedKills );
        }

        [TestMethod]
        public async Task SameSecondAcceptanceOrderAndBountiesSurvivePersistenceWithoutReplayDoubleCounting ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 30 ) );
            await Accept( monitor, Pirate( 10 ) );
            await monitor.PreHandleAsync( Bounty() );
            await monitor.PreHandleAsync( Bounty() ); // Distinct kills can have identical one-second timestamps.
            Assert.AreEqual( 2L, Mission( monitor, 30 ).estimatedKills );
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            var replay = Bounty();
            replay.fromLoad = true;
            await reload.PreHandleAsync( replay );
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 2L, Mission( reload, 30 ).estimatedKills );
            await reload.PreHandleAsync( Bounty( 11 ) );
            Assert.AreEqual( 3L, Mission( reload, 30 ).estimatedKills );
            Assert.AreEqual( 0L, Mission( reload, 10 ).estimatedKills );
            await reload.PreHandleAsync( new FileHeaderEvent( Start, "Journal.combat.log", "test", "test" ) );
            var repeated = Bounty( 11 );
            repeated.fromLoad = true;
            await reload.PreHandleAsync( repeated );
            Assert.AreEqual( 3L, Mission( reload, 30 ).estimatedKills, "Reloading journals without recreating the monitor cannot recount previous kills." );
        }

        [TestMethod]
        [DataRow( 1 )]
        [DataRow( 2 )]
        public async Task ReplayRecoversUnpersistedSameSecondBountiesExactlyOnce ( int persistedCount )
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await Accept( monitor, Pirate( 2, "Issuer B" ) );
            for ( var i = 0; i < persistedCount; i++ ) { await monitor.PreHandleAsync( Bounty() ); }
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            var replay = Bounty();
            replay.fromLoad = true;
            for ( var i = 0; i < persistedCount; i++ )
            {
                await reload.PreHandleAsync( replay );
                Assert.IsTrue( reload.missions.All( m => m.estimatedKills == persistedCount ) );
            }
            await reload.PreHandleAsync( replay );
            Assert.IsTrue( reload.missions.All( m => m.estimatedKills == persistedCount + 1 ),
                "A distinct bounty after the persisted prefix must survive even with identical content and timestamp." );
            Assert.IsTrue( reload.missions.All( m => !m.claimable && !m.progressKnown ) );
            Assert.AreEqual( stored.updatedat, ConfigService.Instance.missionMonitorConfiguration.updatedat );

            await reload.PreHandleAsync( new FileHeaderEvent( Start, "Journal.combat.log", "test", "test" ) );
            for ( var i = 0; i <= persistedCount; i++ ) { await reload.PreHandleAsync( replay ); }
            Assert.IsTrue( reload.missions.All( m => m.estimatedKills == persistedCount + 1 ),
                "Another replay pass must skip the entire newly persisted prefix." );
        }

        [TestMethod]
        public async Task SameSecondQueueAdvancementDoesNotReattributePersistedBounties ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await Accept( monitor, Pirate( 2 ) );
            await monitor.PreHandleAsync( Bounty() );
            var redirect = new MissionRedirectedEvent( Start.AddSeconds( 10 ), 1, "Mission_MassacreWing",
                "Home", null, "Origin", "Hunting ground" );
            await monitor.PreHandleAsync( redirect );
            await monitor.PreHandleAsync( Bounty() );
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            var replay = Bounty();
            replay.fromLoad = true;
            await reload.PreHandleAsync( replay );
            redirect.fromLoad = true;
            await reload.PreHandleAsync( redirect );
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 1L, Mission( reload, 1 ).estimatedKills );
            Assert.AreEqual( 1L, Mission( reload, 2 ).estimatedKills,
                "The successor's saved position includes the bounty originally attributed to its predecessor." );
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 2L, Mission( reload, 2 ).estimatedKills );
        }

        [TestMethod]
        public async Task ReplayPositionIncludesFilteredBounties ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty( faction: "Other faction" ) );
            await monitor.PreHandleAsync( Bounty( target: "Unknown_asset" ) );
            await monitor.PreHandleAsync( Bounty() );
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            foreach ( var replay in new[] { Bounty( faction: "Other faction" ), Bounty( target: "Unknown_asset" ), Bounty() } )
            {
                replay.fromLoad = true;
                await reload.PreHandleAsync( replay );
            }
            Assert.AreEqual( 1L, Mission( reload, 1 ).estimatedKills );
            var next = Bounty();
            next.fromLoad = true;
            await reload.PreHandleAsync( next );
            Assert.AreEqual( 2L, Mission( reload, 1 ).estimatedKills );

            stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            reload = await Monitor( configuration: stored );
            await reload.PreHandleAsync( Bounty() );
            Assert.AreEqual( 3L, Mission( reload, 1 ).estimatedKills,
                "A live event still contributes an observation when replay is absent." );
            Assert.IsNull( Mission( reload, 1 ).combatBountyOrdinal,
                "Without the replay prefix, uncredited bounties may precede the live event at the same timestamp." );
            stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            reload = await Monitor( configuration: stored );
            for ( var i = 0; i < 6; i++ ) { await reload.PreHandleAsync( next ); }
            Assert.AreEqual( 3L, Mission( reload, 1 ).estimatedKills,
                "An unknown source position must retain the conservative same-second replay cutoff." );
        }

        [TestMethod]
        public async Task ReplayBountyPositionResetsWhenTheObservationTimestampAdvances ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty() );
            await monitor.PreHandleAsync( Bounty() );
            await monitor.PreHandleAsync( Bounty( 11 ) );
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            var earlier = Bounty();
            earlier.fromLoad = true;
            await reload.PreHandleAsync( earlier );
            await reload.PreHandleAsync( earlier );
            var replay = Bounty( 11 );
            replay.fromLoad = true;
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 3L, Mission( reload, 1 ).estimatedKills );
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 4L, Mission( reload, 1 ).estimatedKills );
        }

        [TestMethod]
        public async Task LegacyReplayCutoffRemainsConservativeUntilALaterObservation ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty() );
            var stored = JObject.FromObject( ConfigService.Instance.missionMonitorConfiguration );
            foreach ( var evidence in stored[ "missions" ].Select( m => (JObject)m[ "combatEvidence" ] ) )
            {
                evidence.Remove( "bountyOrdinal" );
            }
            var reload = await Monitor( configuration: stored.ToObject<MissionMonitorConfiguration>() );
            var replay = Bounty();
            replay.fromLoad = true;
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 1L, Mission( reload, 1 ).estimatedKills );
            await reload.PreHandleAsync( Bounty() );
            Assert.IsNull( Mission( reload, 1 ).combatBountyOrdinal,
                "A new live bounty at the legacy timestamp cannot establish the size of its unknown prefix." );
            await reload.PreHandleAsync( Bounty( 11 ) );
            var updated = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            reload = await Monitor( configuration: updated );
            replay = Bounty( 11 );
            replay.fromLoad = true;
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 3L, Mission( reload, 1 ).estimatedKills );
            await reload.PreHandleAsync( replay );
            Assert.AreEqual( 4L, Mission( reload, 1 ).estimatedKills );
        }

        [TestMethod]
        public async Task MissingAcceptanceProvenanceBlocksAnIssuerInsteadOfAssumingMissionIdOrder ()
        {
            var unknown = Pirate( 99 );
            var monitor = await Monitor( configuration: new MissionMonitorConfiguration { updatedat = Start, missions = [ unknown ] } );
            await Accept( monitor, Pirate( 1 ) );
            await Accept( monitor, Pirate( 2, "Issuer B" ) );
            await monitor.PreHandleAsync( Bounty() );
            Assert.IsNull( Mission( monitor, 99 ).estimatedKills );
            Assert.AreEqual( 0L, Mission( monitor, 1 ).estimatedKills );
            Assert.AreEqual( 1L, Mission( monitor, 2 ).estimatedKills );
            await Accept( monitor, Pirate( 99 ), -100, fromLoad: true );
            await monitor.PreHandleAsync( Bounty( 11 ) );
            Assert.AreEqual( 1L, Mission( monitor, 99 ).estimatedKills );
            Assert.AreEqual( 0L, Mission( monitor, 1 ).estimatedKills );
            Assert.AreEqual( Start.AddSeconds( 1 ), ConfigService.Instance.missionMonitorConfiguration.updatedat,
                "Recovering acceptance metadata must not rewind or advance the journal watermark." );
        }

        [TestMethod]
        public async Task UnrelatedFactionLocationAssetsModesAndOfflineEventsDoNotCount ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty( faction: "Other faction" ) );
            await monitor.PreHandleAsync( Bounty( faction: "$faction_Pirate;" ) );
            await monitor.PreHandleAsync( Bounty( target: "Unknown_asset" ) );
            await monitor.PreHandleAsync( Bounty( target: "tacticalsuit_class1" ) );
            await monitor.PreHandleAsync( Bounty( reward: 0 ) );
            EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Other system" };
            await monitor.PreHandleAsync( Bounty() );
            EDDI.Instance.GameStateMutator.CurrentStarSystem = new StarSystem { systemname = "Hunting ground" };
            await monitor.PreHandleAsync( new GameModeChangedEvent( Start.AddSeconds( 10 ), "Operation" ) );
            await monitor.PreHandleAsync( Bounty( 11 ) );
            await monitor.PreHandleAsync( new BondAwardedEvent( Start.AddSeconds( 11 ), "Issuer A", "Pirate faction", 100 ) );
            Assert.AreEqual( 0L, Mission( monitor, 1 ).estimatedKills );
            await monitor.PreHandleAsync( new GameModeChangedEvent( Start.AddSeconds( 12 ), "MainGame" ) );
            await monitor.PreHandleAsync( Bounty( 13 ) );
            Assert.AreEqual( 1L, Mission( monitor, 1 ).estimatedKills );
            await monitor.PreHandleAsync( new ShutdownEvent( Start.AddSeconds( 14 ) ) );
            await monitor.PreHandleAsync( Bounty( 15 ) );
            Assert.AreEqual( 1L, Mission( monitor, 1 ).estimatedKills );
            var offline = await Monitor( false, ConfigService.Instance.missionMonitorConfiguration );
            await offline.PreHandleAsync( Bounty( 16 ) );
            Assert.AreEqual( 1L, Mission( offline, 1 ).estimatedKills );
        }

        [TestMethod]
        [DataRow( "Mission_Massacre_Conflict", "Pirate" )]
        [DataRow( "Mission_OnFoot_Massacre", "Pirate" )]
        [DataRow( "Mission_Massacre", "Scout" )]
        [DataRow( "Mission_Assassinate", "PirateLord" )]
        public async Task UnsupportedObjectivesDoNotReceiveBountyEstimates ( string name, string targetType )
        {
            var mission = Pirate( 1 );
            mission.name = name;
            mission.targetTypeEDName = targetType;
            var monitor = await Monitor();
            await Accept( monitor, mission );
            await monitor.PreHandleAsync( Bounty() );
            Assert.IsNull( Mission( monitor, 1 ).estimatedKills );
        }

        [TestMethod]
        public async Task TerminalEventsSnapshotsAndDetachedRuntimeValuesRemainAuthoritative ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ) );
            await Accept( monitor, Pirate( 2 ) );
            await monitor.PreHandleAsync( Bounty( 20 ) );
            var published = (List<Mission>)monitor.GetVariableValues().ToRuntimeValueDictionary()[ "missions" ].Item2;
            await monitor.PreHandleAsync( new MissionFailedEvent( Start.AddSeconds( 19 ), 1, "Mission_MassacreWing", 0 ) );
            Assert.AreEqual( MissionStatus.Failed, Mission( monitor, 1 ).statusDef,
                "An estimate cannot suppress an older queued authoritative failure." );
            await monitor.PreHandleAsync( Bounty( 21 ) );
            Assert.AreEqual( 1L, Mission( monitor, 2 ).estimatedKills );
            Assert.AreEqual( 0L, published.Single( m => m.missionid == 2 ).estimatedKills );
            await monitor.PreHandleAsync( new MissionCompletedEvent( Start.AddSeconds( 22 ), 2, "Mission_MassacreWing",
                null, null, null, null, false, 0 ) );
            Assert.AreEqual( MissionStatus.Complete, Mission( monitor, 2 ).statusDef );
            Assert.AreEqual( 100M, Mission( monitor, 2 ).progress );
            await monitor.PostHandleAsync( new MissionCompletedEvent( Start.AddSeconds( 22 ), 2, "Mission_MassacreWing",
                null, null, null, null, false, 0 ) );
            Assert.DoesNotContain( m => m.missionid == 2, monitor.missions);
            var snapshot = (MissionsEvent)JournalMonitor.ParseJournalEntry(
                "{\"timestamp\":\"2026-10-04T12:00:23Z\",\"event\":\"Missions\",\"Active\":[],\"Failed\":[],\"Complete\":[]}" ).Single();
            await monitor.PreHandleAsync( snapshot );
            Assert.IsEmpty( monitor.missions);
        }

        [TestMethod]
        public void EstimatesAreSeparateFromMeasuredProgressAndUseLocalizedPresentation ()
        {
            var mission = Pirate( 1, required: 30 );
            Assert.IsTrue( mission.ObserveCombatAcceptance( Start, 1, 30, "Hunting ground" ) );
            Assert.IsTrue( mission.ObserveMatchingCombatBounty( Start.AddSeconds( 1 ) ) );
            Assert.AreEqual( 100M / 30M, mission.estimatedProgress );
            Assert.IsFalse( mission.progressKnown );
            Assert.AreEqual( string.Empty, mission.localizedProgress );
            Assert.IsFalse( mission.claimable );
            var changed = new List<string>();
            mission.PropertyChanged += ( _, args ) => changed.Add( args.PropertyName );
            mission.ObserveMatchingCombatBounty( Start.AddSeconds( 2 ) );
            CollectionAssert.Contains( changed, nameof( EddiDataDefinitions.Mission.estimatedKills ) );
            var converter = new MissionProgressConverter();
            var values = new object[] { mission.localizedProgress, false, false, 0L, 0, mission.estimatedKills, mission.combatRequiredKills };
            Assert.AreEqual( "Estimated 2/30 kills", converter.Convert( values, typeof( string ), null, CultureInfo.InvariantCulture ) );
            mission.SetRedirectClaimability( true );
            values[ 0 ] = mission.localizedProgress;
            Assert.AreEqual( "100%", converter.Convert( values, typeof( string ), null, CultureInfo.InvariantCulture ) );
        }

        [TestMethod]
        public void ParserUsesAmountAndInternalProvenanceForRequiredKillCount ()
        {
            const string line = "{\"timestamp\":\"2026-10-04T12:00:01Z\",\"event\":\"MissionAccepted\",\"MissionID\":1,\"Name\":\"Mission_Massacre\",\"Faction\":\"Issuer A\",\"TargetFaction\":\"Pirate faction\",\"TargetType\":\"$MissionUtil_FactionTag_Pirate;\",\"KillCount\":30,\"DestinationSystem\":\"Hunting ground\",\"Wing\":false}";
            var accepted = (MissionAcceptedEvent)JournalMonitor.ParseJournalEntry( line ).Single();
            Assert.AreEqual( 30, accepted.amount );
            Assert.IsTrue( accepted.HasKillCount );
            Assert.IsTrue( accepted.Mission.UsesPirateMassacreEstimate );
            var bounty = (BountyAwardedEvent)JournalMonitor.ParseJournalEntry( BountyAwardedEvent.SAMPLE ).Single();
            Assert.IsTrue( bounty.isShipTarget );
            var noKillCount = (MissionAcceptedEvent)JournalMonitor.ParseJournalEntry( line.Replace( "\"KillCount\":30,", "\"Count\":30," ) ).Single();
            Assert.AreEqual( 30, noKillCount.amount );
            Assert.IsFalse( noKillCount.HasKillCount, "Generic amount alone must not create a required-kill counter." );
            Assert.IsNull( typeof( MissionAcceptedEvent ).GetProperty( "killcount" ), "Do not expose a duplicate public count." );
            var contradictory = (MissionAcceptedEvent)JournalMonitor.ParseJournalEntry(
                line.Replace( "\"KillCount\":30,", "\"KillCount\":30,\"PassengerCount\":2," ) ).Single();
            Assert.IsFalse( contradictory.HasKillCount, "If another count replaced amount, it cannot be used as the kill requirement." );
        }

        [TestMethod]
        public async Task ReplayAndReloadDoNotAssumeAnUnobservedKillBaselineIsZero ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1 ), fromLoad: true );
            Assert.IsTrue( Mission( monitor, 1 ).combatEstimatePartialHistory );
            Assert.IsNull( Mission( monitor, 1 ).estimatedProgress );
            var first = Bounty();
            first.fromLoad = true;
            var second = Bounty();
            second.fromLoad = true;
            await monitor.PreHandleAsync( first );
            await monitor.PreHandleAsync( second );
            Assert.AreEqual( 2L, Mission( monitor, 1 ).estimatedKills, "Separate same-second observations are retained during a replay pass." );
            var values = new object[] { string.Empty, false, false, 0L, 0, 2L, 3, true };
            Assert.AreEqual( "Estimated 2/3 kills", new MissionProgressConverter().Convert(
                values, typeof( string ), null, CultureInfo.InvariantCulture ) );
            var stored = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(
                JsonConvert.SerializeObject( ConfigService.Instance.missionMonitorConfiguration ) );
            var reload = await Monitor( configuration: stored );
            Assert.IsTrue( Mission( reload, 1 ).combatEstimatePartialHistory );
            Assert.IsNull( Mission( reload, 1 ).estimatedProgress );
            await reload.PreHandleAsync( first );
            Assert.AreEqual( 2L, Mission( reload, 1 ).estimatedKills );
        }

        [TestMethod]
        public async Task SnapshotOnlyUnknownIssuerDoesNotAllowAQueueToJumpAhead ()
        {
            var unknown = new Mission( 99, "Mission_Massacre", null, MissionStatus.Active );
            var monitor = await Monitor( configuration: new MissionMonitorConfiguration { updatedat = Start, missions = [ unknown ] } );
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty() );
            Assert.AreEqual( 0L, Mission( monitor, 1 ).estimatedKills );
            Assert.IsNull( Mission( monitor, 99 ).estimatedKills );
        }

        [TestMethod]
        public async Task UnrelatedOnFootAndConflictMissionsDoNotBlockPirateEstimates ()
        {
            var onFoot = new Mission( 98, "Mission_OnFoot_Massacre", null, MissionStatus.Active );
            var conflict = new Mission( 99, "Mission_Massacre_Conflict", null, MissionStatus.Active );
            var monitor = await Monitor( configuration: new MissionMonitorConfiguration
                { updatedat = Start, missions = [ onFoot, conflict ] } );
            await Accept( monitor, Pirate( 1 ) );
            await monitor.PreHandleAsync( Bounty() );
            Assert.AreEqual( 1L, Mission( monitor, 1 ).estimatedKills );
            Assert.IsNull( Mission( monitor, 98 ).estimatedKills );
            Assert.IsNull( Mission( monitor, 99 ).estimatedKills );
        }

        [TestMethod]
        public async Task CombatObservationsAndDetachedPublicationsUseTheMonitorStateLock ()
        {
            var monitor = await Monitor();
            await Accept( monitor, Pirate( 1, required: 100 ) );
            await Task.WhenAll( Enumerable.Range( 0, 40 ).Select( _ => Task.Run( async () =>
            {
                await monitor.PreHandleAsync( Bounty() );
                var snapshot = (List<Mission>)monitor.GetVariableValues().ToRuntimeValueDictionary()[ "missions" ].Item2;
                var observed = snapshot.Single().estimatedKills;
                Assert.IsTrue( observed is >= 1 and <= 40 );
                monitor.writeMissions();
                Assert.AreEqual( observed, snapshot.Single().estimatedKills, "Published evidence must be detached." );
            }, TestContext.CancellationToken ) ) );
            Assert.AreEqual( 40L, Mission( monitor, 1 ).estimatedKills );
            Assert.IsFalse( Mission( monitor, 1 ).progressKnown );
            Assert.IsFalse( Mission( monitor, 1 ).claimable );
            var other = await Monitor();
            await Accept( other, Pirate( 1 ) );
            Assert.AreEqual( 0L, Mission( other, 1 ).estimatedKills );
        }
    }
}
