using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Utilities;

namespace EddiStatusMonitor
{
    public class StatusMonitor : EDDIMonitor
    {
        // Miscellaneous tracking
        private bool jumping;
        private string lastDestinationPOI;

        public StatusMonitor()
        {
            StatusService.StatusUpdatedEvent += HandleStatus;
            Logging.Info($"Initialized {MonitorName()}");
        }

        public string MonitorName()
        {
            return "Status monitor";
        }

        public string LocalizedMonitorName()
        {
            return "Status monitor";
        }

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

        public void Start()
        {
            StatusService.Instance.Start();
        }

        private void HandleStatus(object sender, EventArgs e)
        {
            try
            {
                if (StatusService.Instance.CurrentStatus != null)
                {
                    var thisStatus = StatusService.Instance.CurrentStatus;
                    var lastStatus = StatusService.Instance.LastStatus;

                    // Update the commander's credit balance
                    if (thisStatus.credit_balance != null && EDDI.Instance.Cmdr != null)
                    {
                        EDDI.Instance.Cmdr.credits = Convert.ToUInt64(thisStatus.credit_balance);
                    }

                    // Update vehicle information
                    if (!string.IsNullOrEmpty(thisStatus.vehicle) && thisStatus.vehicle != lastStatus.vehicle && lastStatus.vehicle == EDDI.Instance.Vehicle)
                    {
                        var statusSummary = new Dictionary<string, Status> { { "isStatus", thisStatus }, { "wasStatus", lastStatus } };
                        Logging.Debug($"Status changed vehicle from {lastStatus.vehicle} to {thisStatus.vehicle}", statusSummary);

                        EDDI.Instance.Vehicle = thisStatus.vehicle;
                    }
                    if (thisStatus.vehicle == Constants.VEHICLE_SHIP && EDDI.Instance.CurrentShip != null)
                    {
                        EDDI.Instance.CurrentShip.cargoCarried = thisStatus.cargo_carried ?? 0;
                        EDDI.Instance.CurrentShip.fuelInTanks = thisStatus.fuelInTanks;
                    }

                    // Trigger events for changed status, as applicable
                    if (thisStatus.shields_up != lastStatus.shields_up && thisStatus.vehicle == lastStatus.vehicle)
                    {
                        // React to changes in shield state.
                        // We check the vehicle to make sure that events aren't generated when we switch vehicles, start the game, or stop the game.
                        if (thisStatus.shields_up)
                        {
                            EDDI.Instance.enqueueEvent(new ShieldsUpEvent(thisStatus.timestamp));
                        }
                        else
                        {
                            EDDI.Instance.enqueueEvent(new ShieldsDownEvent(thisStatus.timestamp));
                        }
                    }
                    if (thisStatus.srv_turret_deployed != lastStatus.srv_turret_deployed)
                    {
                        EDDI.Instance.enqueueEvent(new SRVTurretEvent(thisStatus.timestamp, thisStatus.srv_turret_deployed));
                    }
                    if (thisStatus.silent_running != lastStatus.silent_running)
                    {
                        EDDI.Instance.enqueueEvent(new SilentRunningEvent(thisStatus.timestamp, thisStatus.silent_running));
                    }
                    if (thisStatus.srv_under_ship != lastStatus.srv_under_ship && lastStatus.vehicle == Constants.VEHICLE_SRV)
                    {
                        // If the turret is deployable then we are not under our ship. And vice versa. 
                        bool deployable = !thisStatus.srv_under_ship;
                        EDDI.Instance.enqueueEvent(new SRVTurretDeployableEvent(thisStatus.timestamp, deployable));
                    }
                    if (thisStatus.fsd_status != lastStatus.fsd_status
                        && thisStatus.vehicle == Constants.VEHICLE_SHIP
                        && !thisStatus.docked)
                    {
                        if (thisStatus.fsd_status == "ready")
                        {
                            switch (lastStatus.fsd_status)
                            {
                                case "charging":
                                    if (!jumping && thisStatus.supercruise == lastStatus.supercruise)
                                    {
                                        EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "charging cancelled"));
                                    }
                                    jumping = false;
                                    break;
                                case "cooldown":
                                    EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "cooldown complete"));
                                    break;
                                case "masslock":
                                    EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "masslock cleared"));
                                    break;
                            }
                        }
                        else
                        {
                            EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, thisStatus.fsd_status, thisStatus.fsd_hyperdrive_charging));
                        }
                    }
                    if (thisStatus.low_fuel != lastStatus.low_fuel)
                    {
                        // Don't trigger 'low fuel' event when fuel exceeds 25% or when we're not in our ship
                        if (thisStatus.low_fuel
                            && thisStatus.vehicle == Constants.VEHICLE_SHIP)
                        {
                            EDDI.Instance.enqueueEvent(new ShipLowFuelEvent(thisStatus.timestamp));
                        }
                    }
                    if (thisStatus.landing_gear_down != lastStatus.landing_gear_down
                        && thisStatus.vehicle == Constants.VEHICLE_SHIP && lastStatus.vehicle == Constants.VEHICLE_SHIP)
                    {
                        EDDI.Instance.enqueueEvent(new ShipLandingGearEvent(thisStatus.timestamp, thisStatus.landing_gear_down));
                    }
                    if (thisStatus.cargo_scoop_deployed != lastStatus.cargo_scoop_deployed)
                    {
                        EDDI.Instance.enqueueEvent(new ShipCargoScoopEvent(thisStatus.timestamp, thisStatus.cargo_scoop_deployed));
                    }
                    if (thisStatus.lights_on != lastStatus.lights_on)
                    {
                        EDDI.Instance.enqueueEvent(new ShipLightsEvent(thisStatus.timestamp, thisStatus.lights_on));
                    }
                    if (thisStatus.hardpoints_deployed != lastStatus.hardpoints_deployed)
                    {
                        EDDI.Instance.enqueueEvent(new ShipHardpointsEvent(thisStatus.timestamp, thisStatus.hardpoints_deployed));
                    }
                    if (thisStatus.flight_assist_off != lastStatus.flight_assist_off)
                    {
                        EDDI.Instance.enqueueEvent(new FlightAssistEvent(thisStatus.timestamp, thisStatus.flight_assist_off));
                    }
                    if (!string.IsNullOrEmpty(thisStatus.destination_name) && thisStatus.destination_name != lastStatus.destination_name
                        && thisStatus.vehicle == lastStatus.vehicle)
                    {
                        if (EDDI.Instance.CurrentStarSystem != null && EDDI.Instance.CurrentStarSystem.systemAddress ==
                            thisStatus.destinationSystemAddress && thisStatus.destination_name != lastDestinationPOI)
                        {
                            var body = EDDI.Instance.CurrentStarSystem.bodies.FirstOrDefault(b =>
                                b.bodyId == thisStatus.destinationBodyId
                                && b.bodyname == thisStatus.destination_name);
                            var station = EDDI.Instance.CurrentStarSystem.stations.FirstOrDefault(s =>
                                s.name == thisStatus.destination_name);

                            // There is an FDev bug where both Encoded Emissions and High Grade Emissions use the `USS_HighGradeEmissions` edName.
                            // When this occurs, we need to fall back to our generic signal source name.
                            var signalSource = thisStatus.destination_name == "$USS_HighGradeEmissions;"
                                ? SignalSource.GenericSignalSource
                                : EDDI.Instance.CurrentStarSystem.signalSources.FirstOrDefault(s =>
                                      s.edname == thisStatus.destination_name) ?? SignalSource.FromEDName(thisStatus.destination_name);

                            // Might be a body (including the primary star of a different system if selecting a star system)
                            if (body != null && thisStatus.destination_name == body.bodyname)
                            {
                                EDDI.Instance.enqueueEvent(new NextDestinationEvent(
                                    thisStatus.timestamp,
                                    thisStatus.destinationSystemAddress,
                                    thisStatus.destinationBodyId,
                                    thisStatus.destination_name,
                                    thisStatus.destination_localized_name,
                                    body));
                            }
                            // Might be a station (including megaship or fleet carrier)
                            else if (station != null)
                            {
                                EDDI.Instance.enqueueEvent(new NextDestinationEvent(
                                    thisStatus.timestamp,
                                    thisStatus.destinationSystemAddress,
                                    thisStatus.destinationBodyId,
                                    thisStatus.destination_name,
                                    thisStatus.destination_localized_name,
                                    body,
                                    station));
                            }
                            // Might be a non-station signal source
                            else if (signalSource != null)
                            {
                                if (!thisStatus.destination_localized_name?.StartsWith("$") ?? false)
                                {
                                    signalSource.fallbackLocalizedName = thisStatus.destination_localized_name;
                                }
                                EDDI.Instance.enqueueEvent(new NextDestinationEvent(
                                    thisStatus.timestamp,
                                    thisStatus.destinationSystemAddress,
                                    thisStatus.destinationBodyId,
                                    signalSource.invariantName,
                                    signalSource.localizedName,
                                    null,
                                    null,
                                    signalSource));
                            }
                            else if (thisStatus.destination_name != lastDestinationPOI)
                            {
                                EDDI.Instance.enqueueEvent(new NextDestinationEvent(
                                    thisStatus.timestamp,
                                    thisStatus.destinationSystemAddress,
                                    thisStatus.destinationBodyId,
                                    thisStatus.destination_name,
                                    thisStatus.destination_localized_name ?? thisStatus.destination_name,
                                    body));
                            }
                            lastDestinationPOI = thisStatus.destination_name;
                        }
                    }
                    if (!thisStatus.gliding && lastStatus.gliding)
                    {
                        EDDI.Instance.enqueueEvent(new GlideEvent(thisStatus.timestamp, thisStatus.gliding, EDDI.Instance.CurrentStellarBody?.systemname, EDDI.Instance.CurrentStellarBody?.systemAddress, EDDI.Instance.CurrentStellarBody?.bodyname, EDDI.Instance.CurrentStellarBody?.bodyType));
                    }
                    else if (thisStatus.gliding && !lastStatus.gliding && StatusService.Instance.lastEnteredNormalSpaceEvent != null)
                    {
                        var theEvent = StatusService.Instance.lastEnteredNormalSpaceEvent;
                        EDDI.Instance.enqueueEvent(new GlideEvent(DateTime.UtcNow, thisStatus.gliding, theEvent.systemname, theEvent.systemAddress, theEvent.bodyname, theEvent.bodyType) { fromLoad = theEvent.fromLoad });
                    }
                    // Reset our fuel log if we change vehicles or refuel
                    if (thisStatus.vehicle != lastStatus.vehicle || thisStatus.fuel > lastStatus.fuel)
                    {
                        StatusService.Instance.fuelLog = null;
                    }
                    // Detect whether we're in combat
                    if (lastStatus.in_danger && !thisStatus.in_danger)
                    {
                        EDDI.Instance.enqueueEvent(new SafeEvent(DateTime.UtcNow) { fromLoad = false });
                    }
                }
            }
            catch (Exception exception)
            {
                Logging.Debug("Failing to handle status", StatusService.Instance?.CurrentStatus);
                Logging.Error("Failed to handle status", exception);
            }
        }

        public void Stop()
        {
            StatusService.Instance.Stop();
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public void PreHandle(Event @event)
        {
            // Some events can be derived from our status during a given event
            if (@event is EnteredNormalSpaceEvent)
            {
                handleEnteredNormalSpaceEvent(@event);
            }
            else if (@event is FSDEngagedEvent)
            {
                handleFSDEngagedEvent(@event);
            }
        }

        private void handleEnteredNormalSpaceEvent(Event @event)
        {
            // We can derive a "Glide" event from the context in our status
            StatusService.Instance.lastEnteredNormalSpaceEvent = (EnteredNormalSpaceEvent)@event;
        }

        private void handleFSDEngagedEvent(Event @event)
        {
            if (((FSDEngagedEvent)@event).target == "Hyperspace")
            {
                jumping = true;
            }
            EDDI.Instance.enqueueEvent(new ShipFsdEvent(DateTime.UtcNow, "charging complete") { fromLoad = @event.fromLoad });
        }

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        public IDictionary<string, object> GetVariables()
        {
            Dictionary<string, object> variables = new Dictionary<string, object>
            {
                { "status", StatusService.Instance.CurrentStatus },
                { "lastStatus", StatusService.Instance.LastStatus }
            };
            return variables;
        }
    }
}
