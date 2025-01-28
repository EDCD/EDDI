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
        private readonly List<Mission> communityGoalHolder = new List<Mission>();

        private DateTime updateDat;
        public int? missionWarning;

        private static readonly object missionsLock = new object();
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
            return true;
        }

        public MissionMonitor()
        {
            missions = new ObservableCollection<Mission>();
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
                List<Mission> missionsList;
                lock ( missionsLock )
                {
                    missionsList = missions.ToList();
                }

                // Generate 'Expired' and 'Warning' events when conditions met
                try
                {
                    foreach ( var mission in missionsList )
                    {
                        if ( mission.expiry != null && mission.statusDef == MissionStatus.Active )
                        {
                            if ( mission.expiry < DateTime.UtcNow )
                            {
                                if ( mission.communal )
                                {
                                    if ( mission.reward is null )
                                    {
                                        RemoveMission( mission );
                                    }
                                    else
                                    {
                                        mission.statusDef = MissionStatus.Claim;
                                    }
                                }
                                else
                                {
                                    EDDI.Instance.enqueueEvent( new MissionExpiredEvent( DateTime.UtcNow,
                                        mission.missionid, mission.name ) );
                                }
                            }
                            else if ( missionWarning > 0 &&
                                      mission.expiry < DateTime.UtcNow.AddMinutes( (double)missionWarning ) )
                            {
                                if ( !mission.expiring && mission.timeRemaining != null )
                                {
                                    mission.expiring = true;
                                    EDDI.Instance.enqueueEvent( new MissionWarningEvent( DateTime.UtcNow,
                                        mission.missionid, mission.name,
                                        (int)( (TimeSpan)mission.timeRemaining ).TotalMinutes ) );
                                }
                            }
                            else if ( mission.expiring )
                            {
                                mission.expiring = false;
                            }
                        }

                        mission.UpdateTimeRemaining();
                    }

                    Thread.Sleep( 5000 );
                }
                catch ( Exception e )
                {
                    Logging.Error("Failed to respond appropriately for an expiring mission", e);
                }
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

        public void HandleProfile(JObject profile)
        { }

        public void HandleStatus ( Status status )
        { }

        public void PreHandle ( Event @event )
        {
            // Handle the events that we care about
            if ( @event is CommodityCollectedEvent commodityCollectedEvent )
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
                handleMissionAcceptedEvent( missionAcceptedEvent );
            }
            else if ( @event is MissionExpiredEvent missionExpiredEvent )
            {
                handleMissionExpiredEvent( missionExpiredEvent );
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
                handleMissionAbandonedEvent( missionAbandonedEvent );
            }
            else if ( @event is MissionCompletedEvent missionCompletedEvent )
            {
                handleMissionCompletedEvent( missionCompletedEvent );
            }
            else if ( @event is MissionFailedEvent missionFailedEvent )
            {
                handleMissionFailedEvent( missionFailedEvent );
            }
        }

        public void PostHandle ( Event @event )
        {
            // Use the post-handler to remove missions from the missions list only after we have reacted to them.
            if ( @event is MissionAbandonedEvent missionAbandonedEvent )
            {
                postHandleMissionAbandonedEvent( missionAbandonedEvent );
            }
            else if ( @event is MissionCompletedEvent missionCompletedEvent )
            {
                postHandleMissionCompletedEvent( missionCompletedEvent );
            }
            else if ( @event is MissionFailedEvent missionFailedEvent )
            {
                postHandleMissionFailedEvent( missionFailedEvent );
            }
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
                mission.sourcesystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance?.CurrentStellarBody?.bodyname;
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
                mission.sourcesystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance?.CurrentStation?.name;
            }
            return collectMissions.Any();
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
                mission.sourcesystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance?.CurrentStation?.name;
            }
            return miningMissions.Any();
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
                foreach (Mission mission in missions.ToList())
                {
                    // A `MissionRedirected` journal event isn't written for each waypoint in multi-destination passenger missions, so we handle those here.
                    if ( mission.tagsList.Contains(MissionType.SightSeeing) )
                    {
                        var system = mission.destinationsystems.FirstOrDefault(s => s.systemAddress == EDDI.Instance?.CurrentStarSystem?.systemAddress);
                        if ( system != null )
                        {
                            system.visited = true;
                            var waypointSystemName = mission.destinationsystems?
                                .FirstOrDefault(s => s.visited == false)?.systemName;
                            if ( !string.IsNullOrEmpty( waypointSystemName ) )
                            {
                                // Set destination system to next in chain & trigger a 'Mission redirected' event
                                EDDI.Instance?.enqueueEvent( new MissionRedirectedEvent( DateTime.UtcNow,
                                    mission.missionid, mission.name, null, null, waypointSystemName,
                                    EDDI.Instance?.CurrentStarSystem?.systemname ) );
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
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionsEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionsEvent(MissionsEvent @event)
        {
            bool update = false;
            foreach (var mission in @event.missions)
            {
                var missionEntry = missions.FirstOrDefault(m => m.missionid == mission.missionid);

                // If the mission exists in the log, update status
                if (missionEntry != null)
                {
                    switch (mission.statusEDName)
                    {
                        case "Active":
                            {
                                if (missionEntry.statusDef == MissionStatus.Failed)
                                {
                                    if (mission.expiry > missionEntry.expiry)
                                    {
                                        // Fix status if erroneously reported as failed
                                        missionEntry.expiry = mission.expiry;
                                        missionEntry.statusDef = MissionStatus.Active;
                                        update = true;
                                    }
                                }
                                else if (missionEntry.statusDef == MissionStatus.Claim)
                                {
                                    if (mission.expiry > missionEntry.expiry)
                                    {
                                        // Fix expiry if it has been extended by completion
                                        missionEntry.expiry = mission.expiry;
                                        update = true;
                                    }
                                }
                                else if (missionEntry.statusDef == MissionStatus.Active)
                                {
                                    missionEntry.expiry = mission.expiry;
                                    UpdateRedirectStatus(missionEntry);
                                    update = true;
                                }
                            }
                            break;
                        default:
                            {
                                if (missionEntry.statusDef != mission.statusDef)
                                {
                                    missionEntry.statusDef = mission.statusDef;
                                    update = true;
                                }
                            }
                            break;
                    }

                    //If placeholder from 'Passengers' event, add 'Missions' parameters
                    if (missionEntry.name.Contains("None"))
                    {
                        missionEntry.name = mission.name;
                        missionEntry.expiry = mission.expiry;
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
            foreach (Mission missionEntry in missions.ToList())
            {
                // Community goals aren't written by the `Missions` event. We'll keep them until they expire, then once they expire we'll
                // move them to a holder until we see another CommunityGoal event. If there is none, the entry is automatically removed.
                if (missionEntry.communal)
                {
                    if (missionEntry.expiry is null)
                    {
                        RemoveMissionWithMissionId( missionEntry.missionid );
                        communityGoalHolder.Add(missionEntry);
                    }
                    else
                    {
                        continue;
                    }
                }
                
                Mission mission = @event.missions.FirstOrDefault(m => m.missionid == missionEntry.missionid);
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
            foreach (Passenger passenger in @event.passengers)
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
                    MissionStatus status = MissionStatus.Active;
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
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommunityGoalsEvent(@event);
                writeMissions();
            }
        }

        public void _handleCommunityGoalsEvent(CommunityGoalsEvent @event)
        {
            // Prune community goals not reported from the CommunityGoalsEvent.
            foreach (var cgMissionID in missions.ToList().Where(m => m.communal).Select(m => m.missionid))
            {
                if (!@event.goals.Select(cg => (long)cg.cgid).Contains(cgMissionID))
                {
                    RemoveMissionWithMissionId(cgMissionID);
                }
            }

            // Update missions status
            foreach (var goal in @event.goals)
            {
                // Find or create our mission (excluding completed goals without contributions)
                var mission = communityGoalHolder.FirstOrDefault(m => m.missionid == goal.cgid);
                if (mission != null)
                {
                    communityGoalHolder.Remove(mission);
                    AddMission(mission);
                }
                else
                {
                    mission = missions.FirstOrDefault(m => m.missionid == goal.cgid);
                }
                if (mission == null && (!goal.iscomplete || (goal.iscomplete && goal.contribution > 0)))
                {
                    mission = new Mission(goal.cgid, "MISSION_CommunityGoal", goal.expiryDateTime, MissionStatus.Active);
                    AddMission(mission);
                }

                if (!@event.fromLoad && mission != null)
                {
                    // Raise events for the notable changes in community goal status.
                    var cgUpdates = new List<CGUpdate>();
                    if (mission.communalTier < goal.tier)
                    {
                        // Did the goal's current tier change?
                        cgUpdates.Add(new CGUpdate("Tier", "Increase"));
                    }
                    if (goal.contribution > 0)
                    {
                        // Smaller percentile bands are better, larger percentile bands are worse
                        if (mission.communalPercentileBand > goal.percentileband)
                        {
                            // Did the player's percentile band increase (reach a smaller value)?
                            cgUpdates.Add(new CGUpdate("Percentile", "Increase"));
                        }
                        if (mission.communalPercentileBand < goal.percentileband)
                        {
                            // Did the player's percentile band decrease (reach a larger value)?
                            cgUpdates.Add(new CGUpdate("Percentile", "Decrease"));
                        }
                    }
                    if (cgUpdates.Any())
                    {
                        EDDI.Instance.enqueueEvent(new CommunityGoalEvent(DateTime.UtcNow, cgUpdates, goal));
                    }

                    // Update our mission records
                    mission.localisedname = goal.name;
                    mission.originsystem = goal.system;
                    mission.originstation = goal.station;
                    mission.destinationsystem = goal.system;
                    mission.destinationstation = goal.station;
                    mission.reward = goal.contribution > 0 ? goal.tierreward : 0;
                    mission.communal = true;
                    mission.communalPercentileBand = goal.percentileband;
                    mission.communalTier = goal.tier;
                    mission.expiry = goal.expiryDateTime;
                    if (goal.iscomplete)
                    {
                        if (goal.contribution > 0)
                        {
                            mission.statusDef = MissionStatus.Claim;
                        }
                        else
                        {
                            RemoveMissionWithMissionId(mission.missionid);
                        }
                    }
                }
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
                mission.sourcesystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.sourcebody = EDDI.Instance?.CurrentStation?.name;
                mission.originsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.originstation = EDDI.Instance?.CurrentStation?.name;
            }

            if ( @event.updatetype == "Deliver" )
            {
                mission.originsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                mission.originstation = EDDI.Instance?.CurrentStation?.name;
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
                    EDDI.Instance?.enqueueEvent( new CargoWingUpdateEvent( DateTime.UtcNow, mission.missionid,
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

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionAcceptedEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            bool update = false;

            // Protect against duplicates and empty strings
            bool exists = missions?.Any(m => m.missionid == @event.missionid) ?? false;
            bool valid = !string.IsNullOrEmpty(@event.name) && !@event.name.Contains("StartZone");
            if (!exists && valid)
            {
                AddMission(@event.Mission);
                update = true;
            }
            return update;
        }

        public void handleMissionCompletedEvent ( MissionCompletedEvent @event )
        {
            if ( @event.timestamp >= updateDat )
            {
                updateDat = @event.timestamp;
                Mission mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
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
            bool update = false;

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

        private void handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            // 'Expired' is a non-journal event and not subject to 'LogLoad'
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleMissionExpiredEvent(@event))
                {
                    writeMissions();
                }
            }
        }

        public bool _handleMissionExpiredEvent ( MissionExpiredEvent @event )
        {
            var mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
            if ( mission != null )
            {
                if ( mission.communal && mission.communalPercentileBand != 100 )
                {
                    mission.statusDef = MissionStatus.Claim;
                    return true;
                }

                if ( mission.statusDef != MissionStatus.Claim )
                {
                    mission.statusDef = MissionStatus.Failed;
                    return true;
                }
            }

            return false;
        }

        public void handleMissionFailedEvent ( MissionFailedEvent @event )
        {
            if ( @event.timestamp >= updateDat )
            {
                updateDat = @event.timestamp;
                Mission mission = missions.FirstOrDefault( m => m.missionid == @event.missionid );
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
            if ( mission != null )
            {
                RemoveMissionWithMissionId( @event.missionid );
                return true;
            }

            return false;
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

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock (missionsLock)
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    ["goalsCount"] = new Tuple<Type, object>(typeof(int), missions.Count(m => m.communal)),
                    ["missions"] = new Tuple<Type, object>(typeof(List<Mission>), missions.ToList()),
                    ["missionsCount"] = new Tuple<Type, object>(typeof(int), missions.Count(m => !m.shared && !m.communal)),
                    ["missionWarning"] = new Tuple<Type, object>(typeof(int), missionWarning)
                };
            }
        }

        public void writeMissions()
        {
            lock (missionsLock)
            {
                // Write bookmarks configuration with current list
                var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
                missionsConfig.missions = missions;
                missionsConfig.goalsCount = missions.Count(m => m.communal);
                missionsConfig.missionsCount = missions.Count(m => !m.shared && !m.communal);
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
                List<Mission> newMissions = new List<Mission>();

                // Start with the missions we have in the log
                foreach (Mission mission in missionsConfig.missions)
                {
                    newMissions.Add(mission);
                }

                // Now order the list by mission id
                newMissions = newMissions.OrderBy(m => m.missionid).ToList();

                // Update the missions log 
                missions.Clear();
                foreach (Mission mission in newMissions)
                {
                    missions.Add(mission);
                }
            }
        }

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

        private void RemoveMissionWithMissionId(long missionid)
        {
            lock (missionsLock)
            {
                for (int i = 0; i < missions.Count; i++)
                {
                    if (missions[i].missionid == missionid)
                    {
                        missions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public bool UpdateRedirectStatus(Mission mission)
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
                SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
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
