using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json;
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
        private int? _missionWarning;
        public int? missionWarning
        {
            get
            {
                lock ( stateLock ) { return _missionWarning; }
            }
            set
            {
                lock ( stateLock ) { _missionWarning = value; }
            }
        }

        private readonly Func<DateTime> utcNow;
        private readonly Func<bool> isEliteRunning;
        private readonly Action<Event> enqueueEvent;
        private readonly Dictionary<ulong, MissionTimerState> missionTimerStates = [ ];
        private readonly Dictionary<ulong, MissionSnapshotReconciliationState> missionSnapshotReconciliations = [ ];
        private bool liveSessionReady;

        private readonly object stateLock = new();
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
            if ( !ReferenceEquals( e.Collection, missions ) )
            {
                return;
            }

            BindingOperations.CollectionRegistering -= Missions_CollectionRegistering;
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(missions, stateLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(missions, stateLock); });
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
            lock ( stateLock )
            {
                if ( !gameIsRunning )
                {
                    ResetLiveSessionLocked();
                }

                foreach ( var mission in missions )
                {
                    if ( mission.statusDef != MissionStatus.Active )
                    {
                        missionTimerStates.Remove( mission.missionid );
                        missionSnapshotReconciliations.Remove( mission.missionid );
                        mission.UpdateTimeRemaining();
                        continue;
                    }

                    if ( !mission.communal && liveSessionReady && mission.expiry is DateTime deadline &&
                         missionTimerStates.TryGetValue( mission.missionid, out var timerState ) &&
                         timerState.deadline == deadline )
                    {
                        if ( now >= deadline )
                        {
                            if ( !timerState.expirationEmitted )
                            {
                                timerState.expirationEmitted = true;
                                events.Add( new MissionExpiredEvent( now, mission.missionid, mission.name ) );
                            }
                        }
                        else if ( _missionWarning > 0 &&
                                  deadline < now.AddMinutes( (double)_missionWarning ) )
                        {
                            if ( !timerState.warningEmitted )
                            {
                                timerState.warningEmitted = true;
                                events.Add( new MissionWarningEvent( now, mission.missionid, mission.name,
                                    (int)( deadline - now ).TotalMinutes ) );
                            }
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

                    var mission = missions.FirstOrDefault( m => m.missionid == missionId );
                    missionSnapshotReconciliations.Remove( missionId );
                    if ( mission is null || mission.communal || mission.statusDef != MissionStatus.Active )
                    {
                        continue;
                    }

                    mission.SetZeroExpiryClaimability( true );
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
                lock ( stateLock )
                {
                    ResetLiveSessionLocked();
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
            var gameIsRunning = isEliteRunning();
            lock ( stateLock )
            {
                missionTimerStates.Clear();
                liveSessionReady = gameIsRunning;
            }
        }

        private void ResetLiveSessionLocked ()
        {
            missionTimerStates.Clear();
            missionSnapshotReconciliations.Clear();
            liveSessionReady = false;
        }

        private bool IsLiveSessionReady ()
        {
            var gameIsRunning = isEliteRunning();
            lock ( stateLock )
            {
                return liveSessionReady && gameIsRunning;
            }
        }

        internal void ArmMissionTimersFromSnapshot ( MissionsEvent @event )
        {
            var gameIsRunning = isEliteRunning();
            var now = utcNow();
            lock ( stateLock )
            {
                ArmMissionTimersFromSnapshotLocked( @event, gameIsRunning, now );
            }
        }

        private void ArmMissionTimersFromSnapshotLocked ( MissionsEvent @event, bool gameIsRunning, DateTime now )
        {
            if ( !gameIsRunning )
            {
                ResetLiveSessionLocked();
                return;
            }
            if ( !liveSessionReady )
            {
                return;
            }

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
                    ArmMissionTimerLocked( mission, now );
                }
            }
        }

        internal void ArmMissionTimer ( Mission mission )
        {
            var gameIsRunning = isEliteRunning();
            var now = utcNow();
            lock ( stateLock )
            {
                if ( liveSessionReady && gameIsRunning )
                {
                    ArmMissionTimerLocked( mission, now );
                }
            }
        }

        private void ArmMissionTimerLocked ( Mission mission, DateTime now )
        {
            missionSnapshotReconciliations.Remove( mission.missionid );
            if ( mission.communal || mission.statusDef != MissionStatus.Active ||
                 mission.expiry is not DateTime deadline || deadline <= now )
            {
                missionTimerStates.Remove( mission.missionid );
                return;
            }

            if ( !missionTimerStates.TryGetValue( mission.missionid, out var timerState ) ||
                 timerState.deadline != deadline )
            {
                missionTimerStates[ mission.missionid ] = new MissionTimerState( deadline );
            }
        }

        private void CancelMissionSnapshotReconciliation ( ulong missionId )
        {
            lock ( stateLock )
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
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var currentBody = EDDI.Instance.GameState.CurrentStellarBody?.bodyname;
            var update = false;
            lock ( stateLock )
            {
                if ( @event.timestamp > updateDat )
                {
                    updateDat = @event.timestamp;
                    update = HandleCommodityCollectedLocked( @event, currentSystem, currentBody );
                }
            }
            if ( update ) { writeMissions(); }
        }

        private bool HandleCommodityCollectedLocked ( CommodityCollectedEvent @event, string currentSystem,
            string currentBody )
        {
            var mission = missions.FirstOrDefault( h => h.missionid == @event.missionid );
            if ( mission != null )
            {
                mission.sourcesystem = currentSystem;
                mission.sourcebody = currentBody;
            }
            return true;
        }

        private void handleCommodityPurchasedEvent ( CommodityPurchasedEvent @event )
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var currentStation = EDDI.Instance.GameState.CurrentStation?.name;
            var update = false;
            lock ( stateLock )
            {
                if ( @event.timestamp > updateDat )
                {
                    updateDat = @event.timestamp;
                    update = HandleCommodityPurchasedLocked( @event, currentSystem, currentStation );
                }
            }
            if ( update ) { writeMissions(); }
        }

        private bool HandleCommodityPurchasedLocked ( CommodityPurchasedEvent @event, string currentSystem,
            string currentStation )
        {
            var collectMissions = missions.Where( m => m.CommodityDefinition?.edname == @event.commodityDefinition.edname && m.tagsList.Contains( MissionType.Collect ) ).ToList();
            foreach ( var mission in collectMissions )
            {
                mission.sourcesystem = currentSystem;
                mission.sourcebody = currentStation;
            }
            return collectMissions.Count > 0;
        }

        private void handleCommodityRefinedEvent ( CommodityRefinedEvent @event )
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var currentStation = EDDI.Instance.GameState.CurrentStation?.name;
            var update = false;
            lock ( stateLock )
            {
                if ( @event.timestamp > updateDat )
                {
                    updateDat = @event.timestamp;
                    update = HandleCommodityRefinedLocked( @event, currentSystem, currentStation );
                }
            }
            if ( update ) { writeMissions(); }
        }

        private bool HandleCommodityRefinedLocked ( CommodityRefinedEvent @event, string currentSystem,
            string currentStation )
        {
            var miningMissions = missions.Where( m => m.CommodityDefinition?.edname == @event.commodityDefinition.edname && m.tagsList.Contains( MissionType.Mining ) ).ToList();
            foreach ( var mission in miningMissions )
            {
                mission.sourcesystem = currentSystem;
                mission.sourcebody = currentStation;
            }
            return miningMissions.Count > 0;
        }

        private void handleDataScannedEvent(DataScannedEvent @event)
        {
            var currentSystemAddress = EDDI.Instance.GameState.CurrentStarSystem?.systemAddress;
            var currentSystemName = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var now = utcNow();
            var update = false;
            MissionRedirectedEvent redirectedEvent = null;
            lock ( stateLock )
            {
                if (@event.timestamp >= updateDat)
                {
                    updateDat = @event.timestamp;
                    update = HandleDataScannedLocked( @event, currentSystemAddress, currentSystemName, now,
                        out redirectedEvent );
                }
            }
            if ( redirectedEvent != null ) { enqueueEvent( redirectedEvent ); }
            if ( update ) { writeMissions(); }
        }

        public bool _handleDataScannedEvent(DataScannedEvent @event)
        {
            var currentSystemAddress = EDDI.Instance.GameState.CurrentStarSystem?.systemAddress;
            var currentSystemName = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var now = utcNow();
            MissionRedirectedEvent redirectedEvent;
            bool update;
            lock ( stateLock )
            {
                update = HandleDataScannedLocked( @event, currentSystemAddress, currentSystemName, now,
                    out redirectedEvent );
            }
            if ( redirectedEvent != null ) { enqueueEvent( redirectedEvent ); }
            return update;
        }

        private bool HandleDataScannedLocked ( DataScannedEvent @event, ulong? currentSystemAddress,
            string currentSystemName, DateTime now, out MissionRedirectedEvent redirectedEvent )
        {
            redirectedEvent = null;
            if ( DataScan.FromName( @event.datalinktype ).edname == "TouristBeacon")
            {
                foreach (var mission in missions.ToList())
                {
                    // A `MissionRedirected` journal event isn't written for each waypoint in multi-destination passenger missions, so we handle those here.
                    if ( mission.tagsList.Contains(MissionType.SightSeeing) )
                    {
                        var system = mission.destinationsystems.FirstOrDefault(s => s.systemAddress == currentSystemAddress);
                        if ( system != null )
                        {
                            system.visited = true;
                            var waypointSystemName = mission.destinationsystems?
                                .FirstOrDefault(s => !s.visited)?.systemName;
                            if ( !string.IsNullOrEmpty( waypointSystemName ) )
                            {
                                // Set destination system to next in chain & trigger a 'Mission redirected' event
                                redirectedEvent = new MissionRedirectedEvent( now,
                                    mission.missionid, mission.name, null, null, waypointSystemName,
                                    currentSystemName );
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
            var gameIsRunning = isEliteRunning();
            var now = utcNow();
            lock ( stateLock )
            {
                if ( @event.timestamp >= updateDat )
                {
                    updateDat = @event.timestamp;
                    update = HandleMissionsLocked( @event );

                    // Apply snapshot state and cancel or arm its reconciliation while holding the same lock
                    // used by ProcessMissionTimers, so a positive expiry cannot race a due zero-expiry window.
                    ArmMissionTimersFromSnapshotLocked( @event, gameIsRunning, now );
                }
            }

            if ( update )
            {
                writeMissions();
            }
        }

        public bool _handleMissionsEvent(MissionsEvent @event)
        {
            lock ( stateLock )
            {
                return HandleMissionsLocked( @event );
            }
        }

        private bool HandleMissionsLocked ( MissionsEvent @event )
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
                        if ( !expiryIsZero && mission.expiry is not null )
                        {
                            update |= missionEntry.SetZeroExpiryClaimability( false );
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
            var now = utcNow();
            var update = false;
            lock ( stateLock )
            {
                if (@event.timestamp >= updateDat)
                {
                    updateDat = @event.timestamp;
                    HandlePassengersLocked( @event, now );
                    update = true;
                }
            }
            if ( update ) { writeMissions(); }
        }

        public void _handlePassengersEvent(PassengersEvent @event)
        {
            var now = utcNow();
            lock ( stateLock )
            {
                HandlePassengersLocked( @event, now );
            }
        }

        private void HandlePassengersLocked ( PassengersEvent @event, DateTime now )
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
                    mission = new Mission(passenger.missionid, "Mission_None", now.AddDays(1), status)
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

            var now = utcNow();
            List<Event> generatedEvents;
            lock ( stateLock )
            {
                if ( @event.timestamp > updateDat )
                {
                    updateDat = @event.timestamp;
                }
                generatedEvents = HandleCommunityGoalsLocked( @event, now );
            }
            foreach ( var generatedEvent in generatedEvents ) { enqueueEvent( generatedEvent ); }
            writeMissions();
        }

        public void _handleCommunityGoalsEvent(CommunityGoalsEvent @event)
        {
            var now = utcNow();
            List<Event> generatedEvents;
            lock ( stateLock )
            {
                generatedEvents = HandleCommunityGoalsLocked( @event, now );
            }
            foreach ( var generatedEvent in generatedEvents ) { enqueueEvent( generatedEvent ); }
        }

        private List<Event> HandleCommunityGoalsLocked ( CommunityGoalsEvent @event, DateTime now )
        {
            var generatedEvents = new List<Event>();
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
                        generatedEvents.Add( new CommunityGoalEvent( now, cgUpdates, goal ) );
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
                mission.statusDef = goal.iscomplete ? MissionStatus.Complete : MissionStatus.Active;
                mission.expiry = goal.expiryDateTime;
            }
            return generatedEvents;
        }

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var currentStation = EDDI.Instance.GameState.CurrentStation?.name;
            var now = utcNow();
            var update = false;
            CargoWingUpdateEvent wingUpdateEvent = null;
            lock ( stateLock )
            {
                if (@event.timestamp >= updateDat)
                {
                    wingUpdateEvent = HandleCargoDepotLocked( @event, currentSystem, currentStation, now );
                    updateDat = @event.timestamp;
                    update = true;
                }
            }
            if ( wingUpdateEvent != null ) { enqueueEvent( wingUpdateEvent ); }
            if ( update ) { writeMissions(); }
        }

        public void _handleCargoDepotEvent ( CargoDepotEvent @event )
        {
            var currentSystem = EDDI.Instance.GameState.CurrentStarSystem?.systemname;
            var currentStation = EDDI.Instance.GameState.CurrentStation?.name;
            var now = utcNow();
            CargoWingUpdateEvent wingUpdateEvent;
            lock ( stateLock )
            {
                wingUpdateEvent = HandleCargoDepotLocked( @event, currentSystem, currentStation, now );
            }
            if ( wingUpdateEvent != null ) { enqueueEvent( wingUpdateEvent ); }
        }

        private CargoWingUpdateEvent HandleCargoDepotLocked ( CargoDepotEvent @event, string currentSystem,
            string currentStation, DateTime now )
        {
            CargoWingUpdateEvent wingUpdateEvent = null;
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
                mission.sourcesystem = currentSystem;
                mission.sourcebody = currentStation;
                mission.originsystem = currentSystem;
                mission.originstation = currentStation;
            }

            if ( @event.updatetype == "Deliver" )
            {
                mission.originsystem = currentSystem;
                mission.originstation = currentStation;
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
                    wingUpdateEvent = new CargoWingUpdateEvent( now, mission.missionid,
                        updatetype, mission.CommodityDefinition, wingAmount, @event.collected, @event.delivered,
                        @event.totaltodeliver );
                }
            }

            mission.collected = @event.collected;
            mission.delivered = @event.delivered;

            return wingUpdateEvent;
        }

        public void handleMissionAbandonedEvent ( MissionAbandonedEvent @event )
        {
            lock ( stateLock )
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
            lock ( stateLock )
            {
                var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                if ( mission != null )
                {
                    RemoveMissionWithMissionId( @event.missionid );
                    return true;
                }
            }

            return false;
        }

        private bool handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            var update = false;
            var handled = false;
            lock ( stateLock )
            {
                if (@event.timestamp >= updateDat)
                {
                    updateDat = @event.timestamp;
                    update = HandleMissionAcceptedLocked(@event);
                    handled = true;
                }
            }
            if ( update ) { writeMissions(); }
            return handled;
        }

        public bool _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            lock ( stateLock )
            {
                return HandleMissionAcceptedLocked( @event );
            }
        }

        private bool HandleMissionAcceptedLocked ( MissionAcceptedEvent @event )
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

        private bool IsCommunityGoalMission ( ulong missionId )
        {
            lock ( stateLock )
            {
                return missions.Any( mission => mission.missionid == missionId && mission.communal );
            }
        }

        public void handleMissionCompletedEvent ( MissionCompletedEvent @event )
        {
            lock ( stateLock )
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
                lock ( stateLock )
                {
                    var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                    if ( mission != null )
                    {
                        RemoveMissionWithMissionId( @event.missionid );
                        update = true;
                    }
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
            lock ( stateLock )
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
            lock ( stateLock )
            {
                var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
                return mission?.statusDef == MissionStatus.Failed;
            }
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            var update = false;
            lock ( stateLock )
            {
                if (@event.timestamp >= updateDat)
                {
                    updateDat = @event.timestamp;
                    update = HandleMissionRedirectedLocked(@event);
                }
            }
            if ( update ) { writeMissions(); }
        }

        public bool _handleMissionRedirectedEvent ( MissionRedirectedEvent @event )
        {
            lock ( stateLock )
            {
                return HandleMissionRedirectedLocked( @event );
            }
        }

        private bool HandleMissionRedirectedLocked ( MissionRedirectedEvent @event )
        {
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            if ( mission != null )
            {
                var updated = !string.Equals( mission.destinationsystem, @event.newdestinationsystem,
                                  StringComparison.Ordinal ) ||
                              !string.Equals( mission.destinationstation, @event.newdestinationstation,
                                  StringComparison.Ordinal );
                mission.destinationsystem = @event.newdestinationsystem;
                mission.destinationstation = @event.newdestinationstation;
                // Delivery readiness comes from delivery counters. Other cargo-depot-tagged missions
                // (including salvage) can report objective completion by redirecting to their origin.
                var isVerifiedOriginReturn = !mission.tagsList.Contains( MissionType.Delivery ) && mission.originreturn &&
                                             !string.IsNullOrWhiteSpace( mission.originsystem ) &&
                                             !string.IsNullOrWhiteSpace( mission.originstation ) &&
                                             string.Equals( mission.originsystem, @event.newdestinationsystem,
                                                 StringComparison.OrdinalIgnoreCase ) &&
                                             string.Equals( mission.originstation, @event.newdestinationstation,
                                                 StringComparison.OrdinalIgnoreCase );
                return mission.SetRedirectClaimability( isVerifiedOriginReturn ) || updated;
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
            lock (stateLock)
            {
                var missionSnapshots = CloneMissions( missions );
                return
                [
                    GoalsCountVariable.WithValue( missions.Count( m => m.communal ) ),
                    MissionsVariable.WithValue( missionSnapshots ),
                    MissionsCountVariable.WithValue( missions.Count( IsCountedAsActive ) ),
                    MissionWarningVariable.WithValue( _missionWarning )
                ];
            }
        }

        public void writeMissions()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            List<Mission> missionSnapshots;
            int goalsCount;
            int missionsCount;
            int? warning;
            DateTime updatedAt;
            lock (stateLock)
            {
                missionSnapshots = CloneMissions( missions );
                goalsCount = missions.Count( m => m.communal );
                missionsCount = missions.Count( IsCountedAsActive );
                warning = _missionWarning;
                updatedAt = updateDat;
            }

            missionsConfig.missions = missionSnapshots;
            missionsConfig.goalsCount = goalsCount;
            missionsConfig.missionsCount = missionsCount;
            missionsConfig.missionWarning = warning;
            missionsConfig.updatedat = updatedAt;
            ConfigService.Instance.missionMonitorConfiguration = missionsConfig;

            // Make sure the UI is up to date
            RaiseOnUIThread(MissionUpdatedEvent, missions);
        }

        private void readMissions(MissionMonitorConfiguration configuration = null)
        {
            var missionsConfig = configuration ?? ConfigService.Instance.missionMonitorConfiguration;
            var newMissions = CloneMissions( missionsConfig.missions )
                .OrderBy( m => m.missionid ).ToList();
            var migrated = newMissions.Any( mission => mission.statusEDName == "Claim" );
            if ( migrated )
            {
                foreach ( var mission in newMissions.Where( mission => mission.statusEDName == "Claim" ) )
                {
                    // `communal` was not serialized before this migration. The parsed CommunityGoal tag is the
                    // only durable discriminator available in an actual legacy configuration.
                    if ( mission.communal || mission.tagsList.Contains( MissionType.CommunityGoal ) )
                    {
                        mission.communal = true;
                        mission.statusDef = MissionStatus.Complete;
                        mission.SetLegacyClaimability( true );
                    }
                    else
                    {
                        mission.statusDef = MissionStatus.Active;
                        if ( !mission.HasReliableCargoDepotProgress() )
                        {
                            mission.SetLegacyClaimability( true );
                        }
                    }
                }
            }

            lock (stateLock)
            {
                _missionWarning = missionsConfig.missionWarning;
                updateDat = missionsConfig.updatedat;

                // Update the missions log 
                missions.Clear();
                foreach (var mission in newMissions)
                {
                    AddMission(mission);
                }
            }

            if ( migrated )
            {
                missionsConfig.missions = CloneMissions( newMissions );
                if ( configuration is null )
                {
                    ConfigService.Instance.missionMonitorConfiguration = missionsConfig;
                }
            }
        }

        private static bool IsCountedAsActive ( Mission mission ) =>
            !mission.communal && mission.statusDef == MissionStatus.Active;

        private void AddMission(Mission mission)
        {
            if ( mission == null )
            {
                return;
            }

            lock (stateLock)
            {
                if ( missions.All( m => m.missionid != mission.missionid ) )
                {
                    missions.Add(mission);
                }
            }
        }

        private void RemoveMission(Mission mission)
        {
            RemoveMissionWithMissionId(mission.missionid);
        }

        private void RemoveMissionWithMissionId(ulong missionid)
        {
            lock (stateLock)
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

        private static List<Mission> CloneMissions ( IEnumerable<Mission> source )
        {
            var sourceList = source?.ToList() ?? [ ];
            var json = JsonConvert.SerializeObject( sourceList );
            var snapshots = JsonConvert.DeserializeObject<List<Mission>>( json ) ?? [ ];

            // These monitor-owned values are intentionally excluded from persisted JSON but must remain
            // available to detached runtime snapshots and to in-memory configuration snapshots.
            for ( var i = 0; i < Math.Min( sourceList.Count, snapshots.Count ); i++ )
            {
                snapshots[ i ].communal = sourceList[ i ].communal;
                snapshots[ i ].communalPercentileBand = sourceList[ i ].communalPercentileBand;
                snapshots[ i ].communalTier = sourceList[ i ].communalTier;
                snapshots[ i ].communalContribution = sourceList[ i ].communalContribution;
                snapshots[ i ].CommodityDefinition = sourceList[ i ].CommodityDefinition;
                snapshots[ i ].MicroResourceDefinition = sourceList[ i ].MicroResourceDefinition;
            }

            return snapshots;
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
