using EddiCompanionAppService;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiFleetCarrierMonitor
{
    [ UsedImplicitly ]
    public class FleetCarrierMonitor : IEddiMonitor
    {
        public FleetCarrierMonitor ()
        {
            Task.Run( async () =>
            {
                EDDI.Instance.FleetCarrier = ConfigService.Instance.fleetCarrierConfiguration.fleetCarrier;
                await RefreshFleetCarrierFromFrontierAPIAsync( true );
            } ).ConfigureAwait( false );
            CompanionAppService.Instance.StateChanged += OnCompanionAppServiceStateChanged;
        }

        private FleetCarrier FleetCarrier
        {
            get => EDDI.Instance.FleetCarrier;
            set => EDDI.Instance.FleetCarrier = value;
        }

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
            if ( @event.timestamp < FleetCarrier?.timestamp )
            {
                // We only want to update the FleetCarrier object with new events
                return;
            }

            if ( @event.timestamp > FleetCarrier?.DecomissionDateTime )
            {
                // The FleetCarrier has been decommisioned. We need to remove its configuration.
                ConfigService.Instance.fleetCarrierConfiguration = null;
                return;
            }

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
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.timestamp = @event.timestamp;
            }
        }

        private void handleCarrierBankTransferEvent ( CarrierBankTransferEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.bankBalance = @event.bankBalance;
                WriteConfiguration();
            }
        }

        private void handleCarrierDecomissionCancelledEvent ( CarrierDecommissionCancelledEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.state = "normalOperation";
                FleetCarrier.DecomissionDateTime = null;
                WriteConfiguration();
            }
        }

        private void handleCarrierDecomissionScheduledEvent ( CarrierDecommissionScheduledEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.state = "pendingDecommission";
                FleetCarrier.DecomissionDateTime = @event.timestamp + @event.decommissionTimespan;
                WriteConfiguration();
            }
        }

        private void handleCarrierDockingPermissionEvent ( CarrierDockingPermissionEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.dockingAccess = @event.dockingAccess;
                FleetCarrier.notoriousAccess = @event.allowNotorious;
                WriteConfiguration();
            }
        }

        private void handleCarrierFinanceEvent ( CarrierFinanceEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.bankBalance = @event.bankBalance;
                FleetCarrier.bankReservedBalance = @event.bankReservedBalance;
                FleetCarrier.bankPurchaseAllocationsBalance = @event.bankBalance
                                                              - @event.bankReservedBalance
                                                              - @event.bankAvailableBalance;
                WriteConfiguration();
            }
        }

        private void handleCarrierFuelDepositEvent ( CarrierFuelDepositEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.fuel = @event.total;
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            // This can trigger for a carrier where we're a passenger and not the owner
            if ( FleetCarrier != null && FleetCarrier.carrierID == @event.carrierID )
            {
                FleetCarrier.name = @event.carriername;
                FleetCarrier.Market.name = @event.carriername;
                FleetCarrier.Market.marketId = @event.carrierID;
                FleetCarrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                FleetCarrier.SetNextLocation( null, null, null );
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpEngagedEvent ( CarrierJumpEngagedEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                FleetCarrier.SetNextLocation( null, null, null );
                WriteConfiguration();
            }
        }

        private void handleCarrierJumpRequestEvent ( CarrierJumpRequestEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.SetNextLocation( @event.systemAddress, @event.systemname, @event.bodyId );
                WriteConfiguration();
            }
        }

        private void handleCarrierLocationEvent ( CarrierLocationEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.SetCurrentLocation( @event.systemAddress, @event.systemname, @event.bodyID );
                FleetCarrier.SetNextLocation( null, null, null );
                WriteConfiguration();
            }
        }

        private void handleCarrierNameChangeEvent ( CarrierNameChangeEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.name = @event.name;
                WriteConfiguration();
            }
        }

        private void handleCarrierStatsEvent ( CarrierStatsEvent @event )
        {
            if ( FleetCarrier is null || FleetCarrier.carrierID != @event.carrierID )
            {
                EDDI.Instance.FleetCarrier = new FleetCarrier( @event.carrierID );
            }

            if ( FleetCarrier != null )
            {
                FleetCarrier.name = @event.name;
                FleetCarrier.callsign = @event.callsign;
                FleetCarrier.dockingAccess = @event.dockingAccess;
                FleetCarrier.notoriousAccess = @event.notoriousAccess;
                FleetCarrier.fuel = @event.fuel;
                FleetCarrier.usedCapacity = @event.usedCapacity;
                FleetCarrier.freeCapacity = @event.freeCapacity;
                FleetCarrier.bankBalance = @event.bankBalance;
                FleetCarrier.bankReservedBalance = @event.bankReservedBalance;
                FleetCarrier.bankPurchaseAllocationsBalance = @event.bankBalance -
                                                              @event.bankReservedBalance -
                                                              @event.bankAvailableBalance;
                WriteConfiguration();
            }
        }

        private void handleCommodityPurchasedEvent ( CommodityPurchasedEvent @event )
        {
            if ( FleetCarrier != null && @event.marketid == FleetCarrier?.carrierID )
            {
                if ( @event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium" )
                {
                    FleetCarrier.fuelInCargo -= @event.amount;
                    WriteConfiguration();
                }
            }
        }

        private void handleCommoditySoldEvent ( CommoditySoldEvent @event )
        {
            if ( FleetCarrier != null && @event.marketid == FleetCarrier?.carrierID )
            {
                if ( @event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium" )
                {
                    FleetCarrier.fuelInCargo += @event.amount;
                    WriteConfiguration();
                }
            }
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            // If we are at our fleet carrier, make sure that the carrier location is up to date.
            if ( @event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID )
            {
                FleetCarrier.SetCurrentLocation(@event.systemAddress, @event.systemname, @event.bodyId);
                WriteConfiguration();
            }
        }

        public void PostHandle ( Event @event )
        {
            if ( @event is CarrierJumpRequestEvent
                 || @event is CarrierJumpEngagedEvent
                 || @event is CarrierJumpedEvent
                 || @event is CarrierPurchasedEvent
                 || @event is CarrierStatsEvent
                 || @event is CommanderContinuedEvent )
            {
                if ( !@event.fromLoad )
                {
                    Task.Run( async () => await RefreshFleetCarrierFromFrontierAPIAsync() ).ConfigureAwait( false );
                }
            }
        }

        public void HandleProfile ( JObject profile )
        {
            // By the time the profile gets here the FleetCarrier onject is already updated and we just need to save it.
            WriteConfiguration();
        }

        public void HandleStatus ( Status status )
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return null;
        }

        private void OnCompanionAppServiceStateChanged ( CompanionAppService.State oldstate, CompanionAppService.State newstate )
        {
            // Obtain fleet carrier data once the Frontier API connects
            if ( oldstate != CompanionAppService.State.Authorized &&
                 newstate is CompanionAppService.State.Authorized )
            {
                Task.Run( async () => await RefreshFleetCarrierFromFrontierAPIAsync( true ) ).ConfigureAwait( false );
            }
        }

        /// <summary>Obtain fleet carrier information from the companion API and use it to refresh our own data</summary>
        private async Task RefreshFleetCarrierFromFrontierAPIAsync ( bool forceRefresh = false )
        {
            try
            {
                if ( CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized )
                {
                    var frontierApiCarrierJson = await CompanionAppService.Instance.FleetCarrierEndpoint.GetFleetCarrierAsync(forceRefresh);
                    if ( frontierApiCarrierJson != null )
                    {
                        var timestamp = frontierApiCarrierJson["timestamp"]?.ToObject<DateTime>() ?? DateTime.MinValue;
                        var carrierID = frontierApiCarrierJson[ "market" ]?[ "id" ]?.ToObject<long?>();
                        if ( FleetCarrier is null ) { FleetCarrier = new FleetCarrier( carrierID ); }

                        // Update our Fleet Carrier object
                        LockManager.GetLock( nameof( FleetCarrier ), () =>
                        {
                            FleetCarrier.UpdateFrom( frontierApiCarrierJson, timestamp );

                            // Get location data
                            var wp = EDDI.Instance.DataProvider
                                .GetOrFetchSystemWaypoint( frontierApiCarrierJson[ "currentStarSystem" ]?.ToString() );
                            FleetCarrier.currentStarSystemAddress = wp?.systemAddress;
                            FleetCarrier.currentStarSystem = wp?.systemName ?? frontierApiCarrierJson[ "currentStarSystem" ]?.ToString();
                        } );
                        WriteConfiguration();
                    }
                }
            }
            catch ( OperationCanceledException )
            {
                // Nothing to do here, the task was cancelled.
            }
        }

        private void WriteConfiguration ()
        {
            LockManager.GetLock( nameof( FleetCarrier ), () =>
            {
                var configuration = ConfigService.Instance.fleetCarrierConfiguration;
                if ( configuration.fleetCarrier?.timestamp < FleetCarrier?.timestamp )
                {
                    configuration.fleetCarrier = FleetCarrier;
                    ConfigService.Instance.fleetCarrierConfiguration = configuration;
                }

                EDDI.Instance.OnPropertyChanged( nameof( EDDI.Instance.FleetCarrier ) );
            } );
        }
    }
}
