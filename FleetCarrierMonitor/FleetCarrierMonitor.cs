using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiFleetCarrierMonitor
{
    [ UsedImplicitly ]
    public class FleetCarrierMonitor : IEddiMonitor, INotifyPropertyChanged
    {
        public FleetCarrierMonitor ()
        {
            CompanionAppService.Instance.StateChanged += OnCompanionAppServiceStateChanged;
        }

        private FleetCarrier FleetCarrier
        {
            get => EDDI.Instance.FleetCarrier;
            set
            {
                EDDI.Instance.FleetCarrier = value;
                OnPropertyChanged();
            }
        }

        private FleetCarrier SquadronCarrier
        {
            get => _squadronCarrier;
            set
            {
                _squadronCarrier = value;
                OnPropertyChanged();
            }
        }
        private FleetCarrier _squadronCarrier;

        private readonly object carrierLock = new();

        private static readonly ConcurrentDictionary<long, CancellationTokenSource> _carrierJumpCts  = new();

        public string MonitorName () => "Fleet Carrier Monitor";

        public string LocalizedMonitorName () => Properties.Resources.monitorName;

        public string MonitorDescription () => Properties.Resources.monitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start ()
        { }

        public void Stop ()
        {
            foreach ( var carrierJumpCancellationTS in _carrierJumpCts.Values )
            {
                carrierJumpCancellationTS.Cancel();
            }
            WriteConfiguration();
        }

        public void Reload ()
        {
            Logging.Info( $"Reloaded {MonitorName()}" );
        }

        public UserControl ConfigurationTabItem () => null;

        public Task PreHandleAsync ( Event @event )
        {
            if ( @event is CarrierBankTransferEvent carrierBankTransferEvent )
            {
                handleCarrierBankTransferEvent( carrierBankTransferEvent );
            }
            else if ( @event is CarrierDecommissionCancelledEvent carrierDecommissionCancelledEvent )
            {
                handleCarrierDecomissionCancelledEvent( carrierDecommissionCancelledEvent );
            }
            else if ( @event is CarrierDecommissionScheduledEvent carrierDecommissionScheduledEvent )
            {
                handleCarrierDecomissionScheduledEvent( carrierDecommissionScheduledEvent );
            }
            else if ( @event is CarrierDockingPermissionEvent carrierDockingPermissionEvent )
            {
                handleCarrierDockingPermissionEvent( carrierDockingPermissionEvent );
            }
            else if ( @event is CarrierFinanceEvent carrierFinanceEvent )
            {
                handleCarrierFinanceEvent( carrierFinanceEvent );
            }
            else if ( @event is CarrierFuelDepositEvent carrierFuelDepositEvent )
            {
                handleCarrierFuelDepositEvent( carrierFuelDepositEvent );
            }
            else if ( @event is CarrierJumpCancelledEvent carrierJumpCancelled )
            {
                handleCarrierJumpCancelledEvent( carrierJumpCancelled );
            }
            else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is CarrierJumpEngagedEvent carrierJumpEngagedEvent )
            {
                handleCarrierJumpEngagedEvent( carrierJumpEngagedEvent );
            }
            else if ( @event is CarrierJumpRequestEvent carrierJumpRequestEvent )
            {
                handleCarrierJumpRequestEvent( carrierJumpRequestEvent );
            }
            else if ( @event is CarrierLocationEvent carrierLocationEvent )
            {
                handleCarrierLocationEvent( carrierLocationEvent );
            }
            else if ( @event is CarrierNameChangeEvent carrierNameChangeEvent )
            {
                handleCarrierNameChangeEvent( carrierNameChangeEvent );
            }
            else if ( @event is CarrierStatsEvent carrierStatsEvent )
            {
                handleCarrierStatsEvent( carrierStatsEvent );
            }
            else if ( @event is CommodityPurchasedEvent commodityPurchasedEvent )
            {
                handleCommodityPurchasedEvent( commodityPurchasedEvent );
            }
            else if ( @event is CommoditySoldEvent commoditySoldEvent )
            {
                handleCommoditySoldEvent( commoditySoldEvent );
            }
            else if ( @event is FileHeaderEvent )
            {
                handleFileHeaderEvent();
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }

            return Task.CompletedTask;
        }

        private static bool CarrierIsDecommissioned ( DateTime timestamp, FleetCarrier carrier )
        {
            if ( timestamp > carrier?.DecomissionDateTime )
            {
                // The carrier has been decommisioned. We need to remove its configuration.
                if ( carrier.carrierType == StationModel.SquadronCarrier )
                {
                    ConfigService.Instance.fleetCarrierConfiguration.squadronCarrier = null;
                }
                else
                {
                    ConfigService.Instance.fleetCarrierConfiguration.fleetCarrier = null;
                }
                return true;
            }

            return false;
        }

        private static bool CarrierTimestampIsCurrent( DateTime timestamp, FleetCarrier carrier )
        {
            // We only want to update the carrier objects with new events
            return timestamp >= carrier?.timestamp;
        }

        private FleetCarrier GetOrCreateCarrier ( long carrierId, StationModel carrierType )
        {
            var carrier = GetCarrier( carrierId );
            if ( carrier is null || carrier.carrierID != carrierId )
            {
                carrier = new FleetCarrier( carrierId, carrierType );
                if ( carrierType == StationModel.SquadronCarrier )
                {
                    SquadronCarrier = carrier;
                }
                else if ( carrierType == StationModel.FleetCarrier )
                {
                    FleetCarrier = carrier;
                }
                else if ( carrierType != null )
                {
                    throw new ArgumentException( $"Unknown 'carrierType' {carrierType.edname}" );
                }
            }

            return carrier;
        }

        private FleetCarrier GetCarrier ( long carrierId )
        {
            if ( FleetCarrier?.carrierID == carrierId )
            {
                return FleetCarrier;
            }

            if ( SquadronCarrier?.carrierID == carrierId )
            {
                return SquadronCarrier;
            }
            
            return null;
        }

        private void handleCarrierBankTransferEvent ( CarrierBankTransferEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.bankBalance = @event.bankBalance;
                WriteConfiguration();
            }
        }

        private void handleCarrierDecomissionCancelledEvent ( CarrierDecommissionCancelledEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.state = "normalOperation";
                carrier.DecomissionDateTime = null;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierDecomissionScheduledEvent ( CarrierDecommissionScheduledEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.state = "pendingDecommission";
                carrier.DecomissionDateTime = @event.timestamp + @event.decommissionTimespan;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierDockingPermissionEvent ( CarrierDockingPermissionEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.dockingAccess = @event.dockingAccess;
                carrier.notoriousAccess = @event.allowNotorious;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierFinanceEvent ( CarrierFinanceEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.bankBalance = @event.bankBalance;
                carrier.bankReservedBalance = @event.bankReservedBalance;
                carrier.bankPurchaseAllocationsBalance = @event.bankBalance
                                                         - @event.bankReservedBalance
                                                         - @event.bankAvailableBalance;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierFuelDepositEvent ( CarrierFuelDepositEvent @event )
        {
            // May be written when interacting with other commander's carriers (in which case the event should be ignored)
            var carrier = GetCarrier( @event.carrierID );
            if ( carrier != null )
            {
                if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) ||
                     CarrierIsDecommissioned( @event.timestamp, carrier ) )
                {
                    return;
                }

                carrier.fuel = @event.total;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpCancelledEvent ( CarrierJumpCancelledEvent @event )
        {
            // Cancel any pending carrier jump related events
            if ( _carrierJumpCts.TryGetValue( @event.carrierID, out var carrierJumpCancellationTS ) )
            {
                carrierJumpCancellationTS.Cancel();
            }
            
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.SetNextLocation( null, null, null );
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( @event.carrierID is null ) { return; }
            var carrier = GetCarrier( (long)@event.carrierID );
            
            // This can trigger for a carrier where we're a passenger and not the owner
            if ( carrier != null && carrier.carrierID == @event.carrierID )
            {
                if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) ||
                     CarrierIsDecommissioned( @event.timestamp, carrier ) )
                {
                    return;
                }

                carrier.name = @event.carriername;
                carrier.Market.name = @event.carriername;
                carrier.Market.marketId = @event.carrierID;
                carrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                carrier.SetNextLocation( null, null, null );
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpEngagedEvent ( CarrierJumpEngagedEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                carrier.SetNextLocation( null, null, null );
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpRequestEvent ( CarrierJumpRequestEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.SetNextLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierLocationEvent ( CarrierLocationEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            carrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyID );
            carrier.timestamp = @event.timestamp;
            WriteConfiguration();
        }

        private void handleCarrierNameChangeEvent ( CarrierNameChangeEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.name = @event.name;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierStatsEvent ( CarrierStatsEvent @event )
        {
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.name = @event.name;
                carrier.callsign = @event.callsign;
                carrier.dockingAccess = @event.dockingAccess;
                carrier.notoriousAccess = @event.notoriousAccess;
                carrier.state = @event.pendingDecommission ? "pendingDecommission" : carrier.state;
                carrier.fuel = @event.fuel;
                carrier.jumpRangeMax = @event.jumpRangeMax;
                carrier.usedCapacity = @event.usedCapacity;
                carrier.freeCapacity = @event.freeCapacity;
                carrier.bankBalance = @event.bankBalance;
                carrier.bankReservedBalance = @event.bankReservedBalance;
                carrier.bankPurchaseAllocationsBalance = @event.bankBalance -
                                                         @event.bankReservedBalance -
                                                         @event.bankAvailableBalance;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCommodityPurchasedEvent ( CommodityPurchasedEvent @event )
        {
            var carrier = GetCarrier( @event.marketid );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( @event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium" )
            {
                carrier.fuelInCargo -= @event.amount;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCommoditySoldEvent ( CommoditySoldEvent @event )
        {
            var carrier = GetCarrier( @event.marketid );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( @event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium" )
            {
                carrier.fuelInCargo += @event.amount;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private static void handleFileHeaderEvent ()
        {
            EDDI.Instance.FleetCarrier = ConfigService.Instance.fleetCarrierConfiguration.fleetCarrier;
            EDDI.Instance.SquadronCarrier = ConfigService.Instance.fleetCarrierConfiguration.squadronCarrier;
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            if ( @event.marketId is null ||
                 ( @event.stationModel != StationModel.FleetCarrier &&
                   @event.stationModel != StationModel.SquadronCarrier ) )
            {
                return;
            }
            
            var carrier = GetCarrier( (long)@event.marketId );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            // If we are at a carrier we own, make sure that it is up to date.
            if ( carrier != null )
            {
                FleetCarrier.SetCurrentLocation(@event.systemAddress, @event.systemname, @event.bodyId);
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        public Task PostHandleAsync ( Event @event )
        {
            if ( @event.fromLoad ) { return Task.CompletedTask; }

            switch ( @event )
            {
                case CarrierJumpCancelledEvent cjc:
                    HandleCarrierJumpCancelledAsync( cjc ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    break;

                case CarrierJumpRequestEvent cjr:
                    HandleCarrierJumpRequestAsync( cjr ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    RefreshCarrierFromFrontierAPI( cjr.carrierType );
                    break;

                case CarrierJumpEngagedEvent cje:
                    RefreshCarrierFromFrontierAPI( cje.carrierType );
                    break;

                case CarrierJumpedEvent cj:
                    HandleCarrierJumpedAsync( cj ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    RefreshCarrierFromFrontierAPI( cj.carrierType );
                    break;

                case CarrierPurchasedEvent cp:
                    RefreshCarrierFromFrontierAPI( cp.carrierType );
                    break;

                case CarrierStatsEvent cs:
                    RefreshCarrierFromFrontierAPI( cs.carrierType );
                    break;

                case CommanderContinuedEvent _:
                    RefreshCarrierFromFrontierAPI( StationModel.FleetCarrier );
                    RefreshCarrierFromFrontierAPI( StationModel.SquadronCarrier );
                    break;
            }

            return Task.CompletedTask;
        }

        private static async Task HandleCarrierJumpCancelledAsync ( CarrierJumpCancelledEvent cjc )
        {
            var cts = ResetCarrierSchedule(cjc.carrierID);
            var token = cts.Token;

            // Cooldown timer starts when the carrier jump is cancelled and lasts for one minute
            var cooldownDelay = TimeSpan.FromMinutes(1);

            try
            {
                await DelayThenAsync( cooldownDelay, () =>
                {
                    var carrier = StationModel.SquadronCarrier.edname.Equals( cjc.carrierType.edname )
                        ? EDDI.Instance.SquadronCarrier
                        : EDDI.Instance.FleetCarrier;

                    EDDI.Instance.enqueueEvent(
                        new CarrierCooldownEvent(
                            cjc.timestamp.Add( cooldownDelay ),
                            cjc.carrierID,
                            carrier?.callsign,
                            cjc.carrierType,
                            carrier?.currentStarSystem,
                            carrier?.currentStarSystemAddress,
                            null,
                            carrier?.currentBodyID,
                            null ) );
                }, token ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                // Expected
            }
            finally
            {
                CleanupCarrierSchedule( cjc.carrierID, cts );
            }
        }

        private static async Task HandleCarrierJumpRequestAsync ( CarrierJumpRequestEvent cjr )
        {
            var cts = ResetCarrierSchedule(cjr.carrierID);
            var token = cts.Token;

            var departureDelay = cjr.departureTime - cjr.timestamp;
            if ( departureDelay < TimeSpan.Zero ) { departureDelay = TimeSpan.Zero; }

            var padLockDelay = departureDelay - TimeSpan.FromSeconds(Constants.carrierLandingPadLockdownSeconds);
            if ( padLockDelay < TimeSpan.Zero ) { padLockDelay = TimeSpan.Zero; }

            // Cooldown timer starts when engaged
            var cooldownDelay = departureDelay + TimeSpan.FromSeconds(Constants.carrierPostJumpSeconds);

            try
            {
                var carrierPadsLockedEvent = DelayThenAsync(padLockDelay, () =>
                {
                    EDDI.Instance.enqueueEvent( new CarrierPadsLockedEvent(cjr.timestamp.Add(padLockDelay), cjr.carrierID, cjr.carrierType));
                }, token );

                var carrierJumpEngagedEvent = DelayThenAsync( departureDelay, () =>
                {
                    if ( EDDI.Instance.CurrentStarSystem != null )
                    {
                        var originStarSystem = EDDI.Instance.CurrentStarSystem.systemname;
                        var originSystemAddress = EDDI.Instance.CurrentStarSystem.systemAddress;

                        EDDI.Instance.enqueueEvent(
                            new CarrierJumpEngagedEvent(
                                cjr.timestamp.Add( departureDelay ),
                                cjr.systemname, cjr.systemAddress,
                                originStarSystem, originSystemAddress,
                                cjr.bodyname, cjr.bodyId,
                                cjr.carrierID, cjr.carrierType ) );
                    }
                }, token );

                var carrierCooldownEvent = DelayThenAsync( cooldownDelay, () =>
                {
                    EDDI.Instance.enqueueEvent(
                        new CarrierCooldownEvent(
                            cjr.timestamp.Add( cooldownDelay ),
                            cjr.carrierID,
                            null,
                            cjr.carrierType,
                            cjr.systemname,
                            cjr.systemAddress,
                            cjr.bodyname,
                            cjr.bodyId,
                            null ) );
                }, token );

                await Task.WhenAll( carrierPadsLockedEvent, carrierJumpEngagedEvent, carrierCooldownEvent ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                // Expected: replaced/canceled by a newer schedule
            }
            finally
            {
                CleanupCarrierSchedule( cjr.carrierID, cts );
            }
        }

        private static async Task HandleCarrierJumpedAsync ( CarrierJumpedEvent cj )
        {
            if ( cj.carrierID == null ) { return; }

            var carrierId = (long)cj.carrierID;

            // Replace any existing schedule
            var cts = ResetCarrierSchedule(carrierId);
            var token = cts.Token;

            // The cooldown timer starts when the jump is engaged, not when it ends
            var cooldownDelay = TimeSpan.FromSeconds(Constants.carrierPostJumpSeconds - Constants.carrierJumpSeconds);
            if ( cooldownDelay < TimeSpan.Zero )
            {
                cooldownDelay = TimeSpan.Zero;
            }

            try
            {
                await DelayThenAsync( cooldownDelay, () =>
                {
                    EDDI.Instance.enqueueEvent(
                        new CarrierCooldownEvent(
                            cj.timestamp.Add( cooldownDelay ),
                            carrierId,
                            cj.carriername,
                            cj.carrierType,
                            cj.systemname,
                            cj.systemAddress,
                            cj.bodyname,
                            cj.bodyId,
                            cj.bodyType ) );
                }, token ).ConfigureAwait( false );
            }
            catch ( OperationCanceledException )
            {
                // Expected
            }
            finally
            {
                CleanupCarrierSchedule( carrierId, cts );
            }
        }

        public Task HandleProfileAsync ( JObject profile )
        {
            // This currently contains data from the Frontier API 'profile' and (optionally) 'market' and 'shipyard' endpoints.
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            return Task.CompletedTask;
        }
        
        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            lock ( carrierLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    [ "carrier" ] = new( typeof( FleetCarrier ), FleetCarrier ),
                    [ "squadronCarrier" ] = new( typeof( FleetCarrier ), SquadronCarrier ),
                };
            }
        }

        private void OnCompanionAppServiceStateChanged ( CompanionAppService.State oldstate, CompanionAppService.State newstate )
        {
            // Obtain fleet carrier data once the Frontier API connects
            if ( oldstate != CompanionAppService.State.Authorized &&
                 newstate is CompanionAppService.State.Authorized )
            {
                RefreshCarrierFromFrontierAPI( StationModel.FleetCarrier );
                RefreshCarrierFromFrontierAPI( StationModel.SquadronCarrier );
            }
        }

        /// <summary>Obtain fleet carrier information from the companion API and use it to refresh our own data</summary>
        private async Task RefreshFleetCarrierFromFrontierAPIAsync ( bool forceRefresh = false )
        {
            try
            {
                if ( CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized )
                {
                    var frontierApiCarrierJson = await CompanionAppService.Instance.FleetCarrierEndpoint.GetFleetCarrierAsync(forceRefresh).ConfigureAwait(false);
                    if ( frontierApiCarrierJson != null )
                    {
                        var timestamp = frontierApiCarrierJson["timestamp"]?.ToObject<DateTime>() ?? DateTime.MinValue;
                        var carrierID = frontierApiCarrierJson[ "market" ]?[ "id" ]?.ToObject<long>() ?? throw new ArgumentException("Invalid 'carrierID'");
                        FleetCarrier = GetOrCreateCarrier( carrierID, StationModel.FleetCarrier );

                        // Update our Fleet Carrier object
                        lock ( carrierLock )
                        {
                            FleetCarrier.UpdateFrom( frontierApiCarrierJson, timestamp );
                        }

                        // Get location data if it's not already defined
                        if ( FleetCarrier.currentStarSystemAddress is null )
                        {
                            var wp = await EDDI.Instance.DataProvider
                                .GetOrFetchSystemWaypointAsync(
                                    frontierApiCarrierJson[ "currentStarSystem" ]?.ToString() ).ConfigureAwait(false);
                            if ( wp != null )
                            {
                                lock ( carrierLock )
                                {
                                    FleetCarrier.SetCurrentLocation( wp.systemAddress, wp.systemName, null );
                                }
                            }
                        }
                        WriteConfiguration();
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                // Nothing to do here, the task was cancelled.
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to handle Frontier API Fleet Carrier Data", ex );
            }
        }

        /// <summary>Obtain squadron carrier information from the companion API and use it to refresh our own data</summary>
        private async Task RefreshSquadronCarrierFromFrontierAPIAsync ( bool forceRefresh = false )
        {
            try
            {
                if ( CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized )
                {
                    var frontierApiSquadronJson = await CompanionAppService.Instance.SquadronEndpoint
                        .GetSquadronAsync( forceRefresh ).ConfigureAwait( false );
                    if ( frontierApiSquadronJson != null )
                    {
                        var timestamp = frontierApiSquadronJson[ "timestamp" ]?.ToObject<DateTime>() ??
                                        DateTime.MinValue;

                        // Update our Squadron Carrier object
                        if ( SquadronCarrier != null && frontierApiSquadronJson[ "squadronCarrier" ] is JToken squadronCarrier )
                        {
                            lock ( carrierLock )
                            {
                                SquadronCarrier.UpdateFrom( squadronCarrier, timestamp );
                            }
                            
                            // Get location data if it's not already defined
                            if ( SquadronCarrier.currentStarSystemAddress is null )
                            {
                                var wp = await EDDI.Instance.DataProvider
                                    .GetOrFetchSystemWaypointAsync(
                                        squadronCarrier[ "currentStarSystem" ]?.ToString() ).ConfigureAwait(false);
                                if ( wp != null )
                                {
                                    lock ( carrierLock )
                                    {
                                        SquadronCarrier.SetCurrentLocation( wp.systemAddress, wp.systemName, null );
                                    }
                                }
                            }

                            WriteConfiguration();
                        }
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                // Nothing to do here, the task was cancelled.
            }
            catch ( Exception ex )
            {
                Logging.Error("Failed to handle Frontier API Squadron Data", ex);
            }
        }

        private void WriteConfiguration ()
        {
            lock ( carrierLock )
            {
                ConfigService.Instance.fleetCarrierConfiguration = new FleetCarrierConfiguration
                {
                    fleetCarrier = FleetCarrier.Copy(), squadronCarrier = SquadronCarrier.Copy()
                };

                var configuration = ConfigService.Instance.fleetCarrierConfiguration;
                if ( configuration.fleetCarrier?.timestamp < FleetCarrier?.timestamp )
                {
                    EDDI.Instance.OnPropertyChanged( nameof( EDDI.Instance.FleetCarrier ) );
                }
                if ( configuration.squadronCarrier?.timestamp < SquadronCarrier?.timestamp )
                {
                    EDDI.Instance.OnPropertyChanged( nameof( EDDI.Instance.SquadronCarrier ) );
                }
            }
        }
        
        private static void CleanupCarrierSchedule ( long carrierId, CancellationTokenSource cts )
        {
            // Remove only if the dictionary still points to THIS cts (prevents removing a newer one).
            if ( _carrierJumpCts.TryGetValue( carrierId, out var current ) && ReferenceEquals( current, cts ) )
            {
                _carrierJumpCts.TryRemove( carrierId, out _ );
            }

            cts.Dispose();
        }

        private static async Task DelayThenAsync ( TimeSpan delay, Action action, CancellationToken token = default )
        {
            if ( delay <= TimeSpan.Zero )
            {
                action();
                return;
            }

            await Task.Delay( delay, token ).ConfigureAwait( false );
            action();
        }

        private void RefreshCarrierFromFrontierAPI ( StationModel carrierType )
        {
            if ( carrierType == StationModel.FleetCarrier )
            {
                RefreshFleetCarrierFromFrontierAPIAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }
            else if ( carrierType == StationModel.SquadronCarrier )
            {
                RefreshSquadronCarrierFromFrontierAPIAsync().SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
            }
        }

        private static CancellationTokenSource ResetCarrierSchedule ( long carrierId )
        {
            var newCts = new CancellationTokenSource();

            _carrierJumpCts.AddOrUpdate( carrierId, newCts, ( _, existing ) =>
            {
                try
                {
                    existing.Cancel();
                }
                catch ( ObjectDisposedException ) { }
                return newCts; // replace with the new CTS
            } );

            return newCts;
        }

        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
        
        #endregion
    }
}
