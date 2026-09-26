using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiMissionMonitor
{
    /**
     * Monitor missions for the commander
     */
    public class MissionMonitor : IEddiMonitor
    {
        // Keep track of status
        private bool running;

        // Observable collection for us to handle changes
        public ObservableCollection<Mission> missions { get; private set; }

        private DateTime updateDat;
        public int? missionWarning;

        private readonly Func<DateTime> utcNow;
        private readonly Func<bool> isEliteRunning;
        private readonly Action<Event> enqueueEvent;
        private readonly Dictionary<ulong, MissionTimerState> missionTimerStates = [ ];
        private readonly Dictionary<ulong, MissionSnapshotReconciliationState> missionSnapshotReconciliations = [ ];
        private bool liveSessionReady;

        private static readonly object missionsLock = new();
        private readonly object missionTimerLock = new();
        [UsedImplicitly] public event EventHandler MissionUpdatedEvent;

        public string MonitorName()
        {
            return "Mission monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.MissionMonitor.mission_monitor_name;
        }

        public string MonitorDescription()
        {
            return Properties.MissionMonitor.mission_monitor_desc;
        }

        public bool IsRequired()
        {
            return false;
        }

        public MissionMonitor() : this( () => DateTime.UtcNow, Processes.IsEliteRunning,
            @event => EDDI.Instance.enqueueEvent( @event ) )
        { }

        internal MissionMonitor ( Func<DateTime> utcNow, Func<bool> isEliteRunning, Action<Event> enqueueEvent )
        {
            this.utcNow = utcNow ?? throw new ArgumentNullException( nameof( utcNow ) );
            this.isEliteRunning = isEliteRunning ?? throw new ArgumentNullException( nameof( isEliteRunning ) );
            this.enqueueEvent = enqueueEvent ?? throw new ArgumentNullException( nameof( enqueueEvent ) );
            missions = [ ];
            BindingOperations.CollectionRegistering += Missions_CollectionRegistering;
            initializeMissionMonitor();
        }

        public void initializeMissionMonitor(MissionMonitorConfiguration configuration = null)
        {
            readMissions(configuration);
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void Missions_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(missions, missionsLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(missions, missionsLock); });
            }
        }
        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            running = true;

            while ( running )
            {
                try
                {
                    ProcessMissionTimers();
                    Thread.Sleep( 5000 );
                }
                catch ( Exception e )
                {
                    Logging.Error("Failed to respond appropriately for an expiring mission", e);
                }
            }
        }

        internal void ProcessMissionTimers ()
        {
            var now = utcNow();
            var gameIsRunning = isEliteRunning();
            var events = new List<Event>();
            var missionStateUpdated = false;
            List<Mission> missionsList;
            lock ( missionsLock )
            {
                missionsList = missions.ToList();
            }

            lock ( missionTimerLock )
            {
                if ( !gameIsRunning )
                {
                    ResetLiveSession();
                }

                foreach ( var mission in missionsList )
                {
                    if ( mission.statusDef != MissionStatus.Active )
                    {
                        missionTimerStates.Remove( mission.missionid );
                        missionSnapshotReconciliations.Remove( mission.missionid );
                        mission.expiring = false;
                        mission.UpdateTimeRemaining();
                        continue;
                    }

                    if ( !mission.communal && liveSessionReady && mission.expiry is DateTime deadline &&
                         missionTimerStates.TryGetValue( mission.missionid, out var timerState ) &&
                         timerState.deadline == deadline )
                    {
                        if ( now >= deadline )
                        {
                            mission.expiring = false;
                            if ( !timerState.expirationEmitted )
                            {
                                timerState.expirationEmitted = true;
                                events.Add( new MissionExpiredEvent( now, mission.missionid, mission.name ) );
                            }
                        }
                        else if ( missionWarning > 0 &&
                                  deadline < now.AddMinutes( (double)missionWarning ) )
                        {
                            if ( !timerState.warningEmitted )
                            {
                                timerState.warningEmitted = true;
                                mission.expiring = true;
                                events.Add( new MissionWarningEvent( now, mission.missionid, mission.name,
                                    (int)( deadline - now ).TotalMinutes ) );
                            }
                        }
                        else if ( mission.expiring )
                        {
                            mission.expiring = false;
                        }
                    }

                    mission.UpdateTimeRemaining();
                }

                foreach ( var ( missionId, reconciliation ) in missionSnapshotReconciliations.ToList() )
                {
                    if ( now < reconciliation.reconcileAt )
                    {
                        continue;
                    }

                    var mission = missionsList.FirstOrDefault( m => m.missionid == missionId );
                    missionSnapshotReconciliations.Remove( missionId );
                    if ( mission is null || mission.communal || mission.statusDef != MissionStatus.Active )
                    {
                        continue;
                    }

                    var knownDeadline = mission.expiry;
                    mission.statusDef = MissionStatus.Claim;
                    mission.expiry = knownDeadline;
                    mission.expiring = false;
                    missionTimerStates.Remove( missionId );
                    missionStateUpdated = true;
                }
            }

            if ( missionStateUpdated )
            {
                writeMissions();
            }

            foreach ( var @event in events )
            {
                enqueueEvent( @event );
            }
        }

        public void Stop()
        {
            running = false;
        }

        public void Reload()
        {
            readMissions();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public Task HandleProfileAsync(JObject profile)
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            return Task.CompletedTask;
        }
        
        public Task PreHandleAsync ( Event @event )
        {
            // Handle the events that we care about
            if ( @event is FileHeaderEvent )
            {
                ObserveFileHeader();
            }
            else if ( @event is ShutdownEvent )
            {
                lock ( missionTimerLock )
                {
                    ResetLiveSession();
                }
            }
            else if ( @event is CommodityCollectedEvent commodityCollectedEvent )
            {
                handleCommodityCollectedEvent( commodityCollectedEvent );
            }
            else if ( @event is CommodityPurchasedEvent commodityPurchasedEvent )
            {
                handleCommodityPurchasedEvent( commodityPurchasedEvent );
            }
            else if ( @event is CommodityRefinedEvent commodityRefinedEvent )
            {
                handleCommodityRefinedEvent( commodityRefinedEvent );
            }
            else if ( @event is CargoDepotEvent cargoDepotEvent )
            {
                handleCargoDepotEvent( cargoDepotEvent );
            }
            else if ( @event is CommunityGoalsEvent communityGoalsEvent )
            {
                handleCommunityGoalsEvent( communityGoalsEvent );
            }
            else if ( @event is DataScannedEvent dataScannedEvent )
            {
                handleDataScannedEvent( dataScannedEvent );
            }
            if ( @event is FSDEngagedEvent fsdEngagedEvent )
            {
                handleFsdEngagedEvent( fsdEngagedEvent );
            }
            else if ( @event is MissionAcceptedEvent missionAcceptedEvent )
            {
                if ( ( !missionAcceptedEvent.communal || IsLiveSessionReady() ) &&
                     handleMissionAcceptedEvent( missionAcceptedEvent ) )
                {
                    ArmMissionTimer( missionAcceptedEvent.Mission );
                }
            }
            else if ( @event is MissionRedirectedEvent missionRedirectedEvent )
            {
                handleMissionRedirectedEvent( missionRedirectedEvent );
            }
            else if ( @event is MissionsEvent missionsEvent )
            {
                handleMissionsEvent( missionsEvent );
            }
            else if ( @event is PassengersEvent passengersEvent )
            {
                handlePassengersEvent( passengersEvent );
            }

            // Change the mission status here, remove the missions after the events resolve using the post-handler
            else if ( @event is MissionAbandonedEvent missionAbandonedEvent )
            {
                if ( !IsCommunityGoalMission( missionAbandonedEvent.missionid ) || IsLiveSessionReady() )
                {
                    handleMissionAbandonedEvent( missionAbandonedEvent );
                    CancelMissionSnapshotReconciliation( missionAbandonedEvent.missionid );
                }
            }
            else if ( @event is MissionCompletedEvent missionCompletedEvent )
            {
                if ( !missionCompletedEvent.communal || IsLiveSessionReady() )
                {
                    handleMissionCompletedEvent( missionCompletedEvent );
                    CancelMissionSnapshotReconciliation( missionCompletedEvent.missionid );
                }
            }
            else if ( @event is MissionFailedEvent missionFailedEvent )
            {
                handleMissionFailedEvent( missionFailedEvent );
                CancelMissionSnapshotReconciliation( missionFailedEvent.missionid );
            }

            return Task.CompletedTask;
        }

        internal void ObserveFileHeader ()
        {
            lock ( missionTimerLock )
            {
                missionTimerStates.Clear();
                liveSessionReady = isEliteRunning();
            }
        }

        private void ResetLiveSession ()
        {
            missionTimerStates.Clear();
            missionSnapshotReconciliations.Clear();
            liveSessionReady = false;
            lock ( missionsLock )
            {
                foreach ( var mission in missions )
                {
                    mission.expiring = false;
                }
            }
        }

        private bool IsLiveSessionReady ()
        {
            lock ( missionTimerLock )
            {
                return liveSessionReady && isEliteRunning();
            }
        }

        internal void ArmMissionTimersFromSnapshot ( MissionsEvent @event )
        {
            lock ( missionTimerLock )
            {
                ArmMissionTimersFromSnapshotLocked( @event );
            }
        }

        private void ArmMissionTimersFromSnapshotLocked ( MissionsEvent @event )
        {
            if ( !isEliteRunning() )
            {
                ResetLiveSession();
                return;
            }
            if ( !liveSessionReady )
            {
                return;
            }

            var now = utcNow();
            var snapshotMissionIds = @event.missions.Where( m => !m.communal )
                .Select( m => m.missionid ).ToHashSet();
            foreach ( var missionId in missionTimerStates.Keys.Where( id => !snapshotMissionIds.Contains( id ) ).ToList() )
            {
                missionTimerStates.Remove( missionId );
            }
            foreach ( var missionId in missionSnapshotReconciliations.Keys
                         .Where( id => !snapshotMissionIds.Contains( id ) ).ToList() )
            {
                missionSnapshotReconciliations.Remove( missionId );
            }

            foreach ( var mission in @event.missions )
            {
                if ( !mission.communal && mission.statusDef == MissionStatus.Active &&
                     @event.zeroExpiryMissionIds.Contains( mission.missionid ) )
                {
                    missionTimerStates.Remove( mission.missionid );
                    missionSnapshotReconciliations[ mission.missionid ] =
                        new MissionSnapshotReconciliationState( now.AddSeconds( 10 ) );
                }
                else
                {
                    missionSnapshotReconciliations.Remove( mission.missionid );
                    ArmMissionTimerLocked( mission );
                }
            }
        }

        internal void ArmMissionTimer ( Mission mission )
        {
            lock ( missionTimerLock )
            {
                if ( liveSessionReady && isEliteRunning() )
                {
                    ArmMissionTimerLocked( mission );
                }
            }
        }

        private void ArmMissionTimerLocked ( Mission mission )
        {
            missionSnapshotReconciliations.Remove( mission.missionid );
            if ( mission.communal || mission.statusDef != MissionStatus.Active ||
                 mission.expiry is not DateTime deadline || deadline <= utcNow() )
            {
                missionTimerStates.Remove( mission.missionid );
                return;
            }

            if ( !missionTimerStates.TryGetValue( mission.missionid, out var timerState ) ||
                 timerState.deadline != deadline )
            {
                missionTimerStates[ mission.missionid ] = new MissionTimerState( deadline );
                mission.expiring = false;
            }
        }

        private void CancelMissionSnapshotReconciliation ( ulong missionId )
        {
            lock ( missionTimerLock )
            {
                missionSnapshotReconciliations.Remove( missionId );
            }
        }

        public Task PostHandleAsync ( Event @event )
        {
            // Use the post-handler to persist or remove missions only after we have reacted to terminal events.
            if ( @event is MissionAbandonedEvent missionAbandonedEvent )
            {
                if ( !IsCommunityGoalMission( missionAbandonedEvent.missionid ) || IsLiveSessionReady() )
                {
                    postHandleMissionAbandonedEvent( missionAbandonedEvent );
                }
            }
            else if ( @event is MissionCompletedEvent missionCompletedEvent )
            {
                if ( !missionCompletedEvent.communal || IsLiveSessionReady() )
                {
                    postHandleMissionCompletedEvent( missionCompletedEvent );
                }
            }
            else if ( @event is MissionFailedEvent missionFailedEvent )
            {
                postHandleMissionFailedEvent( missionFailedEvent );
            }

            return Task.CompletedTask;
        }

        private void handleCommodityCollectedEvent ( CommodityCollectedEvent @event )
        {
            if ( @event.timestamp > updateDat )
            {
                updateDat = @event.timestamp;
                if ( _handleCommodityCollectedEvent( @event ) )
                {
                    writeMissions();
                }
            }
        }

        private bool _handleCommodityCollectedEvent ( CommodityCollectedEvent @event )
        {
            var mission = missions.FirstOrDefault( h => h.missionid == @event.missionid );
            if ( mission != null )
            {
                mission.sourcesystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance.GameState.CurrentStellarBody?.bodyname;
            }
            return true;
        }

        private void handleCommodityPurchasedEvent ( CommodityPurchasedEvent @event )
        {
            if ( @event.timestamp > updateDat )
            {
                updateDat = @event.timestamp;
                if ( _handleCommodityPurchasedEvent( @event ) )
                {
                    writeMissions();
                }
            }
        }

        private bool _handleCommodityPurchasedEvent ( CommodityPurchasedEvent @event )
        {
            var collectMissions = missions.Where( m => m.CommodityDefinition?.edname == @event.commodityDefinition.edname && m.tagsList.Contains( MissionType.Collect ) ).ToList();
            foreach ( var mission in collectMissions )
            {
                mission.sourcesystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance.GameState.CurrentStation?.name;
            }
            return collectMissions.Count > 0;
        }

        private void handleCommodityRefinedEvent ( CommodityRefinedEvent @event )
        {
            if ( @event.timestamp > updateDat )
            {
                updateDat = @event.timestamp;
                if ( _handleCommodityRefinedEvent( @event ) )
                {
                    writeMissions();
                }
            }
        }

        private bool _handleCommodityRefinedEvent ( CommodityRefinedEvent @event )
        {
            var miningMissions = missions.Where( m => m.CommodityDefinition?.edname == @event.commodityDefinition.edname && m.tagsList.Contains( MissionType.Mining ) ).ToList();
            foreach ( var mission in miningMissions )
            {
                mission.sourcesystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance.GameState.CurrentStation?.name;
            }
            return miningMissions.Count > 0;
        }

        private void handleDataScannedEvent(DataScannedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleDataScannedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleDataScannedEvent(DataScannedEvent @event)
        {
            if ( DataScan.FromName( @event.datalinktype ).edname == "TouristBeacon")
            {
                foreach (var mission in missions.ToList())
                {
                    // A `MissionRedirected` journal event isn't written for each waypoint in multi-destination passenger missions, so we handle those here.
                    if ( mission.tagsList.Contains(MissionType.SightSeeing) )
                    {
                        var system = mission.destinationsystems.FirstOrDefault(s => s.systemAddress == EDDI.Instance.GameState.CurrentStarSystem?.systemAddress);
                        if ( system != null )
                        {
                            system.visited = true;
                            var waypointSystemName = mission.destinationsystems?
                                .FirstOrDefault(s => !s.visited)?.systemName;
                            if ( !string.IsNullOrEmpty( waypointSystemName ) )
                            {
                                // Set destination system to next in chain & trigger a 'Mission redirected' event
                                EDDI.Instance.enqueueEvent( new MissionRedirectedEvent( DateTime.UtcNow,
                                    mission.missionid, mission.name, null, null, waypointSystemName,
                                    EDDI.Instance.GameState.CurrentStarSystem?.systemname ) );
                            }

                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void handleMissionsEvent(MissionsEvent @event)
        {
            var update = false;
            lock ( missionTimerLock )
            {
                if ( @event.timestamp >= updateDat )
                {
                    updateDat = @event.timestamp;
                    update = _handleMissionsEvent( @event );

                    // Apply snapshot state and cancel or arm its reconciliation while holding the same lock
                    // used by ProcessMissionTimers, so a positive expiry cannot race a due zero-expiry window.
                    ArmMissionTimersFromSnapshotLocked( @event );
                }
            }

            if ( update )
            {
                writeMissions();
            }
        }

        public bool _handleMissionsEvent(MissionsEvent @event)
        {
            var update = false;
            foreach (var mission in @event.missions)
            {
                var missionEntry = missions.FirstOrDefault(m => m.missionid == mission.missionid);

                // If the mission exists in the log, update status
                if (missionEntry != null)
                {
                    if ( missionEntry.name != mission.name )
                    {
                        missionEntry.name = mission.name;
                        update = true;
                    }
                    var localisedName = string.IsNullOrEmpty(mission.localisedname)
                        ? mission.name
                        : mission.localisedname;
                    if ( missionEntry.localisedname != localisedName )
                    {
                        missionEntry.localisedname = localisedName;
                        update = true;
                    }

                    var expiryIsZero = @event.zeroExpiryMissionIds.Contains( mission.missionid );
                    if ( mission.statusDef == MissionStatus.Active )
                    {
                        // An Active snapshot is authoritative, including repair of legacy failures inferred from
                        // local timer expiration. A zero-expiry snapshot starts reconciliation separately and must
                        // not overwrite a previously observed contractual deadline.
                        if ( missionEntry.statusDef != MissionStatus.Active )
                        {
                            missionEntry.statusDef = MissionStatus.Active;
                            update = true;
                        }
                        if ( !expiryIsZero && missionEntry.expiry != mission.expiry )
                        {
                            missionEntry.expiry = mission.expiry;
                            update = true;
                        }
                        if ( !expiryIsZero && UpdateRedirectStatus( missionEntry ) )
                        {
                            update = true;
                        }
                    }
                    else if ( missionEntry.statusDef != mission.statusDef )
                    {
                        missionEntry.statusDef = mission.statusDef;
                        update = true;
                    }

                    // Add our destination for origin return missions
                    if (mission.originreturn && string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        mission.destinationsystem = mission.originsystem;
                        mission.destinationstation = mission.originstation;
                    }
                }

                // Add missions to mission log
                else
                {
                    // Starter zone missions have no consistent 'accepted' or 'completed' events, so exclude them from the mission log
                    if (!mission.edTags.Contains("StartZone", StringComparer.InvariantCultureIgnoreCase))
                    {
                        AddMission(mission);
                        update = true;
                    }
                }
            }

            // Remove strays from the mission log
            foreach (var missionEntry in missions.ToList())
            {
                // Standard mission snapshots do not own community goal membership or state.
                if ( missionEntry.communal )
                {
                    continue;
                }
                
                var mission = @event.missions.FirstOrDefault(m => m.missionid == missionEntry.missionid);
                if (mission == null || mission.name.Contains("StartZone"))
                {
                    // Strip out stray and 'StartZone' missions from the log
                    RemoveMissionWithMissionId(missionEntry.missionid);
                    update = true;
                }
            }
            return update;
        }

        private void handlePassengersEvent(PassengersEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                _handlePassengersEvent(@event);
                writeMissions();
            }
        }

        public void _handlePassengersEvent(PassengersEvent @event)
        {
            foreach (var passenger in @event.passengers)
            {
                var mission = missions.FirstOrDefault(m => m.missionid == passenger.missionid);
                if (mission != null)
                {
                    mission.passengertypeEDName = passenger.type;
                    mission.passengervips = passenger.vip;
                    mission.passengerwanted = passenger.wanted;
                    mission.amount = passenger.amount;
                }
                else
                {
                    // Dummy mission to populate 'Passengers' parameters
                    // 'Missions' event will populate 'name', 'status', 'type' & 'expiry'
                    var status = MissionStatus.Active;
                    mission = new Mission(passenger.missionid, "Mission_None", DateTime.UtcNow.AddDays(1), status)
                    {
                        passengertypeEDName = passenger.type,
                        passengervips = passenger.vip,
                        passengerwanted = passenger.wanted,
                        amount = passenger.amount
                    };
                    AddMission(mission);
                }
            }
        }

        private void handleCommunityGoalsEvent(CommunityGoalsEvent @event)
        {
            if ( !IsLiveSessionReady() )
            {
                return;
            }

            if ( @event.timestamp > updateDat )
            {
                updateDat = @event.timestamp;
            }
            _handleCommunityGoalsEvent(@event);
            writeMissions();
        }

        public void _handleCommunityGoalsEvent(CommunityGoalsEvent @event)
        {
            var currentGoalIds = @event.goals.Select( goal => goal.cgid ).ToHashSet();

            // A current CommunityGoal snapshot is authoritative for community goal membership.
            foreach ( var missionId in missions.Where( mission => mission.communal )
                         .Select( mission => mission.missionid ).Where( id => !currentGoalIds.Contains( id ) ).ToList() )
            {
                RemoveMissionWithMissionId( missionId );
            }

            foreach ( var goal in @event.goals )
            {
                var mission = missions.FirstOrDefault( m => m.communal && m.missionid == goal.cgid );

                // Completed goals without a contribution have no reward to claim.
                if ( goal.iscomplete && goal.contribution <= 0 )
                {
                    if ( mission != null )
                    {
                        RemoveMissionWithMissionId( mission.missionid );
                    }
                    continue;
                }

                if ( mission == null )
                {
                    mission = new Mission( goal.cgid, "MISSION_CommunityGoal", goal.expiryDateTime,
                        MissionStatus.Active ) { communal = true };
                    AddMission( mission );
                }

                if ( !@event.fromLoad )
                {
                    // Raise events for the notable changes in community goal status.
                    var cgUpdates = new List<CGUpdate>();

                    // Check for community goal tier changes
                    if ( mission.communalTier < goal.tier )
                    {
                        cgUpdates.Add( new CGUpdate( "Tier", "Increase",
                            mission.communalTier, goal.tier ) );
                    }

                    // Check for player contribution changes
                    if ( goal.contribution > mission.communalContribution )
                    {
                        cgUpdates.Add( new CGUpdate( "Contribution", "Increase",
                            mission.communalContribution, goal.contribution ) );
                    }

                    // Check for player percentile band changes
                    if ( goal.contribution > 0 )
                    {
                        if ( mission.communalPercentileBand > goal.percentileband )
                        {
                            cgUpdates.Add( new CGUpdate( "Percentile", "Increase",
                                mission.communalPercentileBand, goal.percentileband ) );
                        }
                        if ( mission.communalPercentileBand < goal.percentileband )
                        {
                            cgUpdates.Add( new CGUpdate( "Percentile", "Decrease",
                                mission.communalPercentileBand, goal.percentileband ) );
                        }
                    }

                    if ( cgUpdates.Count > 0 )
                    {
                        enqueueEvent( new CommunityGoalEvent( utcNow(), cgUpdates, goal ) );
                    }
                }

                mission.localisedname = goal.name;
                mission.originsystem = goal.system;
                mission.originstation = goal.station;
                mission.destinationsystem = goal.system;
                mission.destinationstation = goal.station;
                mission.reward = goal.contribution > 0 ? goal.tierreward : 0;
                mission.communal = true;
                mission.communalPercentileBand = goal.percentileband;
                mission.communalTier = goal.tier;
                mission.communalContribution = goal.contribution;
                mission.statusDef = goal.iscomplete ? MissionStatus.Claim : MissionStatus.Active;
                mission.expiry = goal.expiryDateTime;
            }
        }

        private void handleFsdEngagedEvent ( FSDEngagedEvent @event )
        {
            if ( @event.timestamp > updateDat )
            {
                updateDat = @event.timestamp;
                _handleFsdEngagedEvent();
            }
        }

        private void _handleFsdEngagedEvent ()
        {
            var cargoInventory = ConfigService.Instance.cargoMonitorConfiguration;
            foreach ( var mission in missions )
            {
                var cargo = cargoInventory.cargo.FirstOrDefault( c => c.missionCargo.ContainsKey( mission.missionid ) );
                if ( cargo is null ) { continue; }

                if ( mission.tagsList.Contains( MissionType.Delivery ) )
                {
                    // Delivery missions allow up to 50% of cargo to be lost before failing the mission
                    var onboard = cargo.missionCargo.Where( c => c.Key == mission.missionid ).Sum( c => c.Value );
                    var lost = mission.collected - mission.delivered - onboard;
                    if ( lost > ( Convert.ToDecimal( mission.amount ) / 2 ) )
                    {
                        mission.statusDef = MissionStatus.Failed;
                    }
                }
                else if ( mission.tagsList.Any( tag =>
                             tag == MissionType.Rescue || tag == MissionType.Salvage ||
                             tag == MissionType.Smuggle ) )
                {
                    // If we leave the instance after having lost irreplaceable mission cargo then the mission is failed.
                    mission.statusDef = MissionStatus.Failed;
                }
            }
        }

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                _handleCargoDepotEvent(@event);
                updateDat = @event.timestamp;
                writeMissions();
            }
        }

        public void _handleCargoDepotEvent ( CargoDepotEvent @event )
        {
            // Retrieve or create the relevant mission
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            if ( mission is null )
            {
                var type = @event.startmarketid == 0 ? "MISSION_Collect" : "MISSION_Delivery";
                mission = new Mission( @event.missionid, type, null, MissionStatus.Active )
                {
                    amount = @event.totaltodeliver,
                    CommodityDefinition = @event.commodityDefinition,
                    startmarketid = @event.startmarketid,
                    endmarketid = @event.endmarketid
                };
                AddMission( mission );
            }

            // Update mission details and generate wing update events where appropriate
            if ( @event.updatetype == "Collect" )
            {
                mission.sourcesystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance.GameState.CurrentStation?.name;
                mission.originsystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.originstation = EDDI.Instance.GameState.CurrentStation?.name;
            }

            if ( @event.updatetype == "Deliver" )
            {
                mission.originsystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
                mission.originstation = EDDI.Instance.GameState.CurrentStation?.name;
            }
            else if ( @event.updatetype == "WingUpdate" )
            {
                mission.shared = true;
                if ( !mission.tagsList.Contains(MissionType.Wing) )
                {
                    mission.name += "_Wing";
                }

                // Generate a derived event when a wing-mate collects or delivers cargo for a wing mission
                var updatetype = @event.collected > mission.collected ? "Collect" : "Deliver";
                var wingCollected = @event.collected - mission.collected;
                var wingDelivered = @event.delivered - mission.delivered;
                var wingAmount = Math.Max( wingCollected, wingDelivered );
                if ( wingAmount > 0 )
                {
                    mission.wingCollected += wingCollected;
                    mission.wingCollected -= wingDelivered;
                    EDDI.Instance.enqueueEvent( new CargoWingUpdateEvent( DateTime.UtcNow, mission.missionid,
                        updatetype, mission.CommodityDefinition, wingAmount, @event.collected, @event.delivered,
                        @event.totaltodeliver ) );
                }
            }

            mission.collected = @event.collected;
            mission.delivered = @event.delivered;

            // If we've delivered the full quantity then set the mission status to 'Claim'
            if ( ( @event.totaltodeliver - @event.delivered ) == 0 )
            {
                mission.statusDef = MissionStatus.Claim;
            }
        }

        public void handleMissionAbandonedEvent ( MissionAbandonedEvent @event )
        {
            if ( @event.timestamp >= updateDat )
            {
                updateDat = @event.timestamp;
                var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                if ( mission != null )
                {
                    mission.statusDef = MissionStatus.Failed;
                }
            }
        }

        private void postHandleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (_postHandleMissionAbandonedEvent(@event))
            {
                writeMissions();
            }
        }

        public bool _postHandleMissionAbandonedEvent ( MissionAbandonedEvent @event )
        {
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            if ( mission != null )
            {
                RemoveMissionWithMissionId( @event.missionid );
                return true;
            }

            return false;
        }

        private bool handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionAcceptedEvent(@event))
                {
                    writeMissions();
                }
                return true;
            }
            return false;
        }

        public bool _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            var update = false;

            // Protect against duplicates and empty strings
            var exists = missions?.Any(m => m.missionid == @event.missionid) ?? false;
            var valid = !string.IsNullOrEmpty(@event.name) && !@event.name.Contains("StartZone");
            if (!exists && valid)
            {
                AddMission(@event.Mission);
                update = true;
            }
            return update;
        }

        private bool IsCommunityGoalMission ( ulong missionId ) =>
            missions.Any( mission => mission.missionid == missionId && mission.communal );

        public void handleMissionCompletedEvent ( MissionCompletedEvent @event )
        {
            if ( @event.timestamp >= updateDat )
            {
                updateDat = @event.timestamp;
                var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                if ( mission != null )
                {
                    mission.statusDef = MissionStatus.Complete;
                }
            }
        }

        private void postHandleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (_postHandleMissionCompletedEvent(@event))
            {
                writeMissions();
            }
        }

        public bool _postHandleMissionCompletedEvent ( MissionCompletedEvent @event )
        {
            var update = false;

            try
            {
                Mission mission;
                lock ( missionsLock )
                {
                    mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                }
                if ( mission != null )
                {
                    RemoveMissionWithMissionId( @event.missionid );
                    update = true;
                }
            }
            catch ( Exception e )
            {
                Logging.Error( e.Message, e );
                throw;
            }

            return update;
        }

        public void handleMissionFailedEvent ( MissionFailedEvent @event )
        {
            if ( @event.timestamp >= updateDat )
            {
                updateDat = @event.timestamp;
                var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                if ( mission != null )
                {
                    mission.statusDef = MissionStatus.Failed;
                }
            }
        }

        private void postHandleMissionFailedEvent(MissionFailedEvent @event)
        {
            if (_postHandleMissionFailedEvent(@event))
            {
                writeMissions();
            }
        }

        public bool _postHandleMissionFailedEvent ( MissionFailedEvent @event )
        {
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            return mission?.statusDef == MissionStatus.Failed;
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionRedirectedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionRedirectedEvent ( MissionRedirectedEvent @event )
        {
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            if ( mission != null )
            {
                mission.destinationsystem = @event.newdestinationsystem;
                mission.destinationstation = @event.newdestinationstation;
                return UpdateRedirectStatus( mission );
            }

            return false;
        }

        [Utilities.PublicAPI( "The number of community goals you are engaged in." )]
        public static RuntimeVariableDefinition GoalsCountVariable => new( "goalsCount", typeof(int) );

        [Utilities.PublicAPI( "A list of tracked missions." )]
        public static RuntimeVariableDefinition MissionsVariable => new( "missions", typeof(List<Mission>) );

        [Utilities.PublicAPI( "The number of active missions." )]
        public static RuntimeVariableDefinition MissionsCountVariable => new( "missionsCount", typeof(int) );

        [Utilities.PublicAPI( "You will receive a warning when a mission is within this many minutes from expiring." )]
        public static RuntimeVariableDefinition MissionWarningVariable => new( "missionWarning", typeof(int) );

        public IReadOnlyList<RuntimeVariableDeclaration> GetVariableDeclarations () =>
            RuntimeVariableDefinitionExtensions.DiscoverDeclarations( typeof(MissionMonitor) );

        public IReadOnlyList<RuntimeVariableValue> GetVariableValues ()
        {
            lock (missionsLock)
            {
                return
                [
                    GoalsCountVariable.WithValue( missions.Count( m => m.communal ) ),
                    MissionsVariable.WithValue( missions.ToList() ),
                    MissionsCountVariable.WithValue( missions.Count( IsCountedAsActive ) ),
                    MissionWarningVariable.WithValue( missionWarning )
                ];
            }
        }

        public void writeMissions()
        {
            lock (missionsLock)
            {
                // Write bookmarks configuration with current list
                var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
                missionsConfig.missions = missions.ToList();
                missionsConfig.goalsCount = missions.Count(m => m.communal);
                missionsConfig.missionsCount = missions.Count( IsCountedAsActive );
                missionsConfig.missionWarning = missionWarning;
                missionsConfig.updatedat = updateDat;
                ConfigService.Instance.missionMonitorConfiguration = missionsConfig;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(MissionUpdatedEvent, missions);
        }

        private void readMissions(MissionMonitorConfiguration configuration = null)
        {
            lock (missionsLock)
            {
                // Obtain current missions log from configuration
                var missionsConfig = configuration ?? ConfigService.Instance.missionMonitorConfiguration;
                missionWarning = missionsConfig.missionWarning;
                updateDat = missionsConfig.updatedat;

                // Build a new missions log
                var newMissions = new List<Mission>();

                // Start with the missions we have in the log
                foreach (var mission in missionsConfig.missions)
                {
                    newMissions.Add(mission);
                }

                // Now order the list by mission id
                newMissions = newMissions.OrderBy(m => m.missionid).ToList();

                // Update the missions log 
                missions.Clear();
                foreach (var mission in newMissions)
                {
                    AddMission(mission);
                }
            }
        }

        private static bool IsCountedAsActive ( Mission mission ) =>
            !mission.communal && ( mission.statusDef == MissionStatus.Active || mission.statusDef == MissionStatus.Claim );

        private void AddMission(Mission mission)
        {
            if (mission == null || missions.Any(m => m.missionid == mission.missionid))
            {
                return;
            }

            lock (missionsLock)
            {
                missions.Add(mission);
            }
        }

        private void RemoveMission(Mission mission)
        {
            RemoveMissionWithMissionId(mission.missionid);
        }

        private void RemoveMissionWithMissionId(ulong missionid)
        {
            lock (missionsLock)
            {
                for (var i = 0; i < missions.Count; i++)
                {
                    if (missions[i].missionid == missionid)
                    {
                        missions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private static bool UpdateRedirectStatus(Mission mission)
        {
            if ( mission.originreturn && mission.originsystem == mission.destinationsystem
                && mission.originstation == mission.destinationstation)
            {
                // Mission is claimed at a cargo depot - do not redirect until all cargo has been delivered.
                if ( mission.cargodepot &&
                     mission.delivered < mission.amount )
                { return false; }

                // Redirect based on origin and destination info.
                if ( mission.tagsList.Any( t => t.ClaimAtOrigin ) )
                {
                    if ( mission.statusDef != MissionStatus.Claim )
                    {
                        mission.statusDef = MissionStatus.Claim;
                        return true;
                    }
                }
            }
            return false;
        }

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                var uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                if (uiSyncContext == null)
                {
                    handler(sender, EventArgs.Empty);
                }
                else
                {
                    uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
                }
            }
        }
    }
}
