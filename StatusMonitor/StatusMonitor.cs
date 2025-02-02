using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusService;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiStatusMonitor
{
    [UsedImplicitly]
    public class StatusMonitor : IEddiMonitor
    {
        // Miscellaneous tracking
        private decimal preScoopFuelInTanks;
        private bool jumping;
        private string lastDestinationPOI;
        private string lastMusicTrack;

        internal Status currentStatus
        {
            get => _currentStatus ?? new Status(); 
            set => _currentStatus = value;
        }
        private Status _currentStatus;
        private Status lastStatus;

        private static readonly object statusLock = new object();

        [ExcludeFromCodeCoverage]
        public StatusMonitor ()
        {
            Logging.Info($"Initialized {MonitorName()}");
        }

        [ExcludeFromCodeCoverage]
        public string MonitorName()
        {
            return "Status monitor";
        }

        [ExcludeFromCodeCoverage]
        public string LocalizedMonitorName()
        {
            return "Status monitor";
        }

        [ExcludeFromCodeCoverage]
        public string MonitorDescription()
        {
            return "Monitor Elite: Dangerous' Status.json for current status.  This should not be disabled unless you are sure you know what you are doing, as it will result in many functions inside EDDI no longer working";
        }

        public bool IsRequired()
        {
            return true;
        }

        public bool NeedsStart()
        {
            return true;
        }

        [ExcludeFromCodeCoverage]
        public void Start()
        {
            StatusService.Instance.Start();
        }

        public void HandleStatus ( Status status )
        {
            _handleStatus( status, out var events );
            foreach ( var @event in events )
            {
                EDDI.Instance.enqueueEvent( @event );
            }
        }

        internal void _handleStatus ( Status status, out List<Event> events )
        {
            events = new List<Event>();
            if ( status is null ) { return; }

            lock ( statusLock )
            {
                lastStatus = currentStatus;
                currentStatus = status;
            }

            // Update vehicle information
            if ( !string.IsNullOrEmpty( status.vehicle ) && status.vehicle != lastStatus?.vehicle )
            {
                if ( EDDI.Instance.Vehicle != status.vehicle )
                {
                    var statusSummary = new Dictionary<string, Status> { { "isStatus", status }, { "wasStatus", lastStatus } };
                    Logging.Debug( $"Status changed vehicle from {lastStatus?.vehicle ?? "<NULL>"} to {status.vehicle}", statusSummary );
                    EDDI.Instance.Vehicle = status.vehicle;
                }
            }
            if ( status.vehicle == Constants.VEHICLE_SHIP && EDDI.Instance.CurrentShip != null )
            {
                EDDI.Instance.CurrentShip.cargoCarried = status.cargo_carried ?? 0;
                EDDI.Instance.CurrentShip.fuelInTanks = status.fuelInTanks ?? 0;
                EDDI.Instance.CurrentShip.fuelInReservoir = status.fuelInReservoir ?? 0;
            }

            if ( lastStatus is null ) { return; }

            // Trigger events for changed status, as applicable
            if ( status.shields_up != lastStatus.shields_up && status.vehicle == lastStatus.vehicle )
            {
                // React to changes in shield state.
                // We check the vehicle to make sure that events aren't generated when we switch vehicles, start the game, or stop the game.
                if ( status.shields_up )
                {
                    events.Add( new ShieldsUpEvent( status.timestamp ) );
                }
                else
                {
                    events.Add( new ShieldsDownEvent( status.timestamp ) );
                }
            }
            if ( status.srv_turret_deployed != lastStatus.srv_turret_deployed )
            {
                events.Add( new SRVTurretEvent( status.timestamp, status.srv_turret_deployed ) );
            }
            if ( status.silent_running != lastStatus.silent_running )
            {
                events.Add( new SilentRunningEvent( status.timestamp, status.silent_running ) );
            }
            if ( status.srv_under_ship != lastStatus.srv_under_ship && lastStatus.vehicle == Constants.VEHICLE_SRV )
            {
                // If the turret is deployable then we are not under our ship. And vice versa. 
                var deployable = !status.srv_under_ship;
                events.Add( new SRVTurretDeployableEvent( status.timestamp, deployable ) );
            }
            if ( status.fsd_status != lastStatus.fsd_status
                 && status.vehicle == Constants.VEHICLE_SHIP
                 && !status.docked )
            {
                if ( status.fsd_status == "ready" )
                {
                    switch ( lastStatus.fsd_status )
                    {
                        case "charging":
                            if ( !jumping && status.supercruise == lastStatus.supercruise )
                            {
                                events.Add( new ShipFsdEvent( status.timestamp, "charging cancelled" ) );
                            }
                            jumping = false;
                            break;
                        case "cooldown":
                            events.Add( new ShipFsdEvent( status.timestamp, "cooldown complete" ) );
                            break;
                        case "masslock":
                            events.Add( new ShipFsdEvent( status.timestamp, "masslock cleared" ) );
                            break;
                    }
                }
                else
                {
                    events.Add( new ShipFsdEvent( status.timestamp, status.fsd_status, status.fsd_hyperdrive_charging ) );
                }
            }
            if ( status.vehicle == lastStatus.vehicle ) // 'low fuel' is 25% or less
            {
                // Trigger `Low fuel` events for each 5% fuel increment at 25% fuel or less (where our vehicle remains constant)
                if ( ( status.low_fuel && !lastStatus.low_fuel ) || // 25%
                     ( status.fuel_percentile != null && // less than 20%, 15%, 10%, or 5%
                       lastStatus.fuel_percentile != null &&
                       status.fuel_percentile <= 4 &&
                       status.fuel_percentile < lastStatus.fuel_percentile ) )
                {
                    events.Add( new LowFuelEvent( status.timestamp ) );
                }
            }
            if ( status.scooping_fuel && !lastStatus.scooping_fuel )
            {
                events.Add( new ShipFuelScoopEvent( status.timestamp, true ) );
                preScoopFuelInTanks = status.fuelInTanks ?? 0;
            }
            if ( preScoopFuelInTanks > 0 && ( ( !status.scooping_fuel && lastStatus.scooping_fuel ) ||
                                              ( status.scooping_fuel && lastStatus.scooping_fuel &&
                                                StatusService.Instance.CurrentShip?.fueltanktotalcapacity == status.fuelInTanks &&
                                                StatusService.Instance.CurrentShip?.fueltanktotalcapacity > lastStatus.fuelInTanks ) ) )
            {
                events.Add( new ShipRefuelledEvent( status.timestamp, "Scoop", 0,
                    ( status.fuelInTanks ?? 0 ) - preScoopFuelInTanks,
                    status.fuelInTanks )
                {
                    full = StatusService.Instance.CurrentShip?.fueltanktotalcapacity == status.fuelInTanks
                } );
                preScoopFuelInTanks = 0;
                events.Add( new ShipFuelScoopEvent( status.timestamp, false ) );
            }
            if ( status.landing_gear_down != lastStatus.landing_gear_down
                 && status.vehicle == Constants.VEHICLE_SHIP && lastStatus.vehicle == Constants.VEHICLE_SHIP )
            {
                events.Add( new ShipLandingGearEvent( status.timestamp, status.landing_gear_down ) );
            }
            if ( status.cargo_scoop_deployed != lastStatus.cargo_scoop_deployed )
            {
                events.Add( new ShipCargoScoopEvent( status.timestamp, status.cargo_scoop_deployed ) );
            }
            if ( status.lights_on != lastStatus.lights_on )
            {
                events.Add( new ShipLightsEvent( status.timestamp, status.lights_on ) );
            }
            if ( status.hardpoints_deployed != lastStatus.hardpoints_deployed )
            {
                events.Add( new ShipHardpointsEvent( status.timestamp, status.hardpoints_deployed ) );
            }
            if ( status.flight_assist_off != lastStatus.flight_assist_off )
            {
                events.Add( new FlightAssistEvent( status.timestamp, status.flight_assist_off ) );
            }
            if ( !string.IsNullOrEmpty( status.destination_name ) && status.destination_name != lastStatus.destination_name
                                                                         && status.vehicle == lastStatus.vehicle )
            {
                if ( EDDI.Instance.CurrentStarSystem != null && EDDI.Instance.CurrentStarSystem.systemAddress ==
                    status.destinationSystemAddress && status.destination_name != lastDestinationPOI )
                {
                    var body = EDDI.Instance.CurrentStarSystem.bodies.FirstOrDefault(b =>
                        b.bodyId == status.destinationBodyId
                        && b.bodyname == status.destination_name);
                    var station = EDDI.Instance.CurrentStarSystem.stations.FirstOrDefault(s =>
                        s.name == status.destination_name);

                    // There is an FDev bug where both Encoded Emissions and High Grade Emissions use the `USS_HighGradeEmissions` edName.
                    // When this occurs, we need to fall back to our generic signal source name.
                    // It's also possible for both the standard name and localized name to be symbolic values. If so, prefer and try to match the value in the localized field. 
                    var signalSource = status.destination_name == "$USS_HighGradeEmissions;"
                        ? SignalSource.GenericSignalSource
                        : EDDI.Instance.CurrentStarSystem.signalSources.FirstOrDefault( s =>
                            s.edname == status.destination_name ) ?? SignalSource.FromEDName(
                            ( status.destination_localized_name?.StartsWith( "$" ) ?? false )
                                ? status.destination_localized_name
                                : status.destination_name );

                    // Might be a body (including the primary star of a different system if selecting a star system)
                    if ( body != null && status.destination_name == body.bodyname )
                    {
                        events.Add( new NextDestinationEvent(
                            status.timestamp,
                            status.destinationSystemAddress,
                            status.destinationBodyId,
                            status.destination_name,
                            status.destination_localized_name,
                            body ) );
                    }
                    // Might be a station (including megaship or fleet carrier)
                    else if ( station != null )
                    {
                        events.Add( new NextDestinationEvent(
                            status.timestamp,
                            status.destinationSystemAddress,
                            status.destinationBodyId,
                            status.destination_name,
                            status.destination_localized_name,
                            body,
                            station ) );
                    }
                    // Might be a non-station signal source
                    else if ( signalSource != null )
                    {
                        if ( !status.destination_localized_name?.StartsWith( "$" ) ?? false )
                        {
                            signalSource.fallbackLocalizedName = status.destination_localized_name;
                        }
                        events.Add( new NextDestinationEvent(
                            status.timestamp,
                            status.destinationSystemAddress,
                            status.destinationBodyId,
                            signalSource.invariantName,
                            signalSource.localizedName,
                            null,
                            null,
                            signalSource ) );
                    }
                    else if ( status.destination_name != lastDestinationPOI )
                    {
                        events.Add( new NextDestinationEvent(
                            status.timestamp,
                            status.destinationSystemAddress,
                            status.destinationBodyId,
                            status.destination_name,
                            status.destination_localized_name ?? status.destination_name,
                            body ) );
                    }
                    lastDestinationPOI = status.destination_name;
                }
            }
            if ( !status.gliding && lastStatus.gliding )
            {
                events.Add( new GlideEvent( status.timestamp, status.gliding, EDDI.Instance.CurrentStellarBody?.systemname, EDDI.Instance.CurrentStellarBody?.systemAddress, EDDI.Instance.CurrentStellarBody?.bodyname, EDDI.Instance.CurrentStellarBody?.bodyType ) );
            }
            else if ( status.gliding && !lastStatus.gliding && StatusService.Instance.lastEnteredNormalSpaceEvent != null )
            {
                var theEvent = StatusService.Instance.lastEnteredNormalSpaceEvent;
                events.Add( new GlideEvent( DateTime.UtcNow, status.gliding, theEvent.systemname, theEvent.systemAddress, theEvent.bodyname, theEvent.bodyType ) { fromLoad = theEvent.fromLoad } );
            }
            // Reset our fuel log if we change vehicles or refuel
            if ( status.vehicle != lastStatus.vehicle || status.fuel > lastStatus.fuel )
            {
                StatusService.Instance.fuelLog.Clear();
            }
            // Detect whether we're in combat
            if ( lastStatus.in_danger && !status.in_danger )
            {
                events.Add( new SafeEvent( DateTime.UtcNow ) { fromLoad = false } );
            }
        }

        [ExcludeFromCodeCoverage]
        public void Stop()
        {
            StatusService.Instance.Stop();
        }

        [ExcludeFromCodeCoverage]
        public void Reload()
        { }

        [ExcludeFromCodeCoverage]
        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public void PreHandle(Event @event)
        {
            // Some events can be derived from our status during a given event
            if ( @event is EnteredNormalSpaceEvent enteredNormalSpaceEvent )
            {
                handleEnteredNormalSpaceEvent( enteredNormalSpaceEvent );
            }
            else if ( @event is FSDEngagedEvent fsdEngagedEvent )
            {
                handleFSDEngagedEvent( fsdEngagedEvent );
            }
            else if ( @event is MusicEvent musicEvent )
            {
                handleMusicEvent( musicEvent );
            }
            else if ( @event is SettlementApproachedEvent settlementApproachedEvent )
            {
                handleSettlementApproachedEvent( settlementApproachedEvent );
            }
        }

        internal void handleSettlementApproachedEvent ( SettlementApproachedEvent @event )
        {
            // Synthesize a `Destination arrived` event when approaching a settlement / location we've been tracking,
            // if the journal hasn't already generated a `SupercruiseDestinationDrop` event
            if ( !@event.fromLoad &&
                 currentStatus?.destinationSystemAddress != null &&
                 currentStatus.destinationSystemAddress == @event.systemAddress &&
                 currentStatus.destinationBodyId == @event.bodyId &&
                 ( currentStatus.destination_name == @event.name ||
                   currentStatus.destination_localized_name == @event.name ) )
            {
                // Retrieve the last `SupercruiseDestinationDrop` event and verify that, if it exists, it does not match the settlement we may be approaching.
                if ( !EDDI.Instance.lastEventOfType.TryGetValue( "SupercruiseDestinationDrop",
                         out var supercruiseDestinationDrop ) ||
                     !( supercruiseDestinationDrop is DestinationArrivedEvent destinationArrivedEvent ) ||
                     destinationArrivedEvent.name != @event.name )
                {
                    destinationArrivedEvent = new DestinationArrivedEvent( currentStatus.timestamp, @event.name );
                    EDDI.Instance.enqueueEvent( destinationArrivedEvent );
                }
            }
        }

        internal void handleEnteredNormalSpaceEvent( EnteredNormalSpaceEvent @event )
        {
            // We can derive a "Glide" event from the context in our status
            StatusService.Instance.lastEnteredNormalSpaceEvent = @event;
        }

        internal void handleFSDEngagedEvent( FSDEngagedEvent @event )
        {
            if (@event.target == "Hyperspace")
            {
                jumping = true;
            }
            EDDI.Instance.enqueueEvent(new ShipFsdEvent( @event.timestamp, "charging complete" ) { fromLoad = @event.fromLoad });
        }

        internal void handleMusicEvent ( MusicEvent @event )
        {
            // Derive a "Station mailslot" event from changes to music tracks
            Status status = null;
            LockManager.GetLock(nameof(currentStatus), () => { status = currentStatus; } );

            if ( status?.vehicle == Constants.VEHICLE_SHIP )
            {
                if ( @event.musictrack == "Starport" && 
                     ( lastMusicTrack == "NoTrack" || lastMusicTrack == "Exploration" ) &&
                     !status.docked )
                {
                    EDDI.Instance.enqueueEvent( new StationMailslotEvent( @event.timestamp ) { fromLoad = @event.fromLoad } );
                }
            }

            lastMusicTrack = @event.musictrack;
        }

        [ExcludeFromCodeCoverage]
        public void PostHandle(Event @event)
        { }

        [ExcludeFromCodeCoverage]
        public void HandleProfile(JObject profile)
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( statusLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    { "status", new Tuple<Type, object>(typeof(Status), currentStatus ) },
                    { "lastStatus", new Tuple < Type, object >(typeof(Status), lastStatus ) }
                };
            }
        }
    }
}
