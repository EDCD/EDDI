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
        private FleetCarrier FleetCarrier => EDDI.Instance.FleetCarrier;

        public string MonitorName () => "Fleet Carrier Monitor";

        public string LocalizedMonitorName () => Properties.Resources.monitorName;

        public string MonitorDescription () => Properties.Resources.monitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start ()
        {
            CompanionAppService.Instance.StateChanged += OnCompanionAppServiceStateChanged;
            Task.Run( async () => await RefreshFleetCarrierFromFrontierAPIAsync( true ) ).ConfigureAwait( true );
        }

        public void Stop ()
        {
            CompanionAppService.Instance.StateChanged -= OnCompanionAppServiceStateChanged;
        }

        public void Reload ()
        { }

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
            else if ( @event is CarrierNameChangeEvent carrierNameChangeEvent )
            {
                handleCarrierNameChangeEvent( carrierNameChangeEvent );
            }
            else if ( @event is CarrierStatsEvent carrierStatsEvent )
            {
                handleCarrierStatsEvent( carrierStatsEvent );
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( FleetCarrier != null )
            {
                FleetCarrier.currentStarSystem = @event.systemname;
                FleetCarrier.nextStarSystem = null;
                UpdateFleetCarrierConfig();
            }
        }

        private void handleCarrierJumpEngagedEvent ( CarrierJumpEngagedEvent @event )
        {
            if ( FleetCarrier != null )
            {
                FleetCarrier.currentStarSystem = @event.systemname;
                FleetCarrier.nextStarSystem = null;
                UpdateFleetCarrierConfig();
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
                FleetCarrier.nextStarSystem = @event.systemname;
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
                UpdateFleetCarrierConfig();
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
            UpdateFleetCarrierConfig();
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
            if ( CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized )
            {
                var frontierApiCarrierJson = await CompanionAppService.Instance.FleetCarrierEndpoint.GetFleetCarrierAsync(forceRefresh);
                if ( frontierApiCarrierJson != null )
                {
                    var timestamp = frontierApiCarrierJson["timestamp"]?.ToObject<DateTime>() ?? DateTime.MinValue;

                    // Update our Fleet Carrier object
                    LockManager.GetLock( nameof( FleetCarrier ), () =>
                    {
                        FleetCarrier?.UpdateFrom( frontierApiCarrierJson, timestamp );
                    } );
                }
            }
        }

        private void UpdateFleetCarrierConfig ()
        {
            var configuration = ConfigService.Instance.eddiConfiguration;
            if ( configuration.fleetCarrier != FleetCarrier )
            {
                configuration.fleetCarrier = FleetCarrier;
                ConfigService.Instance.eddiConfiguration = configuration;
            }
        }
    }
}
