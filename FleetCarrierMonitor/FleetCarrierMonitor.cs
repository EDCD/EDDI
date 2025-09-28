using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            set => EDDI.Instance.FleetCarrier = value;
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

        private readonly object carrierLock = new object();

        public string MonitorName () => "Fleet Carrier Monitor";

        public string LocalizedMonitorName () => Properties.Resources.monitorName;

        public string MonitorDescription () => Properties.Resources.monitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start ()
        { }

        public void Stop () => WriteConfiguration();

        public void Reload ()
        {
            Logging.Info( $"Reloaded {MonitorName()}" );
        }

        public UserControl ConfigurationTabItem () => null;

        public void PreHandle ( Event @event )
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
        }

        private bool CarrierIsDecommissioned ( DateTime timestamp, FleetCarrier carrier )
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

        private bool CarrierTimestampIsCurrent( DateTime timestamp, FleetCarrier carrier )
        {
            // We only want to update the carrier objects with new events
            return timestamp >= carrier?.timestamp;
        }

        private FleetCarrier GetOrCreateCarrier ( long carrierId, StationModel carrierType )
        {
            var carrier = carrierType == StationModel.SquadronCarrier 
                ? SquadronCarrier 
                : carrierType == StationModel.FleetCarrier 
                    ? FleetCarrier 
                    : null;
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
                else
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
            var carrier = GetOrCreateCarrier( @event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }

            if ( carrier != null )
            {
                carrier.fuel = @event.total;
                carrier.timestamp = @event.timestamp;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpCancelledEvent ( CarrierJumpCancelledEvent @event )
        {
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
            var carrier = GetOrCreateCarrier( (long)@event.carrierID, @event.carrierType );
            if ( !CarrierTimestampIsCurrent( @event.timestamp, carrier ) || 
                 CarrierIsDecommissioned( @event.timestamp, carrier ) )
            {
                return;
            }
            
            // This can trigger for a carrier where we're a passenger and not the owner
            if ( carrier != null && carrier.carrierID == @event.carrierID )
            {
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

        private void handleFileHeaderEvent ()
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

        public void PostHandle ( Event @event )
        {
            if ( @event.fromLoad ) { return; }
            
            if ( @event is CarrierJumpRequestEvent cjr )
            {
                if ( cjr.carrierType == StationModel.FleetCarrier )
                {
                    _ = RefreshFleetCarrierFromFrontierAPIAsync();
                }
                else if ( cjr.carrierType == StationModel.SquadronCarrier )
                {
                    _ = RefreshSquadronCarrierFromFrontierAPIAsync();
                }
            }
            else if ( @event is CarrierJumpEngagedEvent cje )
            {
                if ( cje.carrierType == StationModel.FleetCarrier )
                {
                    _ = RefreshFleetCarrierFromFrontierAPIAsync();
                }
                else if ( cje.carrierType == StationModel.SquadronCarrier )
                {
                    _ = RefreshSquadronCarrierFromFrontierAPIAsync();
                }
            }
            else if ( @event is CarrierJumpedEvent cj )
            {
                if ( cj.carrierType == StationModel.FleetCarrier )
                {
                    _ = RefreshFleetCarrierFromFrontierAPIAsync();
                }
                else if ( cj.carrierType == StationModel.SquadronCarrier )
                {
                    _ = RefreshSquadronCarrierFromFrontierAPIAsync();
                }
            }
            else if ( @event is CarrierPurchasedEvent cp )
            {
                if ( cp.carrierType == StationModel.FleetCarrier )
                {
                    _ = RefreshFleetCarrierFromFrontierAPIAsync();
                }
                else if ( cp.carrierType == StationModel.SquadronCarrier )
                {
                    _ = RefreshSquadronCarrierFromFrontierAPIAsync();
                }
            }
            else if ( @event is CarrierStatsEvent cs )
            {
                if ( cs.carrierType == StationModel.FleetCarrier )
                {
                    _ = RefreshFleetCarrierFromFrontierAPIAsync();
                }
                else if ( cs.carrierType == StationModel.SquadronCarrier )
                {
                    _ = RefreshSquadronCarrierFromFrontierAPIAsync();
                }
            }
            else if ( @event is CommanderContinuedEvent )
            {
                _ = RefreshFleetCarrierFromFrontierAPIAsync();
                _ = RefreshSquadronCarrierFromFrontierAPIAsync();
            }
        }

        public void HandleProfile ( JObject profile )
        {
            // This currently contains data from the Frontier API 'profile' and (optionally) 'market' and 'shipyard' endpoints.
        }

        public void HandleStatus ( Status status )
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            lock ( carrierLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    [ "carrier" ] = new Tuple<Type, object>( typeof( FleetCarrier ), FleetCarrier ),
                    [ "squadronCarrier" ] = new Tuple<Type, object>( typeof( FleetCarrier ), SquadronCarrier ),
                };
            }
        }

        private void OnCompanionAppServiceStateChanged ( CompanionAppService.State oldstate, CompanionAppService.State newstate )
        {
            // Obtain fleet carrier data once the Frontier API connects
            if ( oldstate != CompanionAppService.State.Authorized &&
                 newstate is CompanionAppService.State.Authorized )
            {
                _ = RefreshFleetCarrierFromFrontierAPIAsync();
                _ = RefreshSquadronCarrierFromFrontierAPIAsync();
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
                var configuration = ConfigService.Instance.fleetCarrierConfiguration;
                if ( configuration.fleetCarrier?.timestamp < FleetCarrier?.timestamp )
                {
                    EDDI.Instance.OnPropertyChanged( nameof( EDDI.Instance.FleetCarrier ) );
                }
                if ( configuration.squadronCarrier?.timestamp < SquadronCarrier?.timestamp )
                {
                    EDDI.Instance.OnPropertyChanged( nameof( EDDI.Instance.SquadronCarrier ) );
                }

                ConfigService.Instance.fleetCarrierConfiguration = new FleetCarrierConfiguration
                {
                    fleetCarrier = FleetCarrier.Copy(), squadronCarrier = SquadronCarrier.Copy()
                };
            }
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
