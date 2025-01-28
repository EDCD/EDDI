using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiShipMonitor
{
    public class ShipMonitor : IEddiMonitor
    {
        private static readonly List<string> HARDPOINT_SIZES = new List<string>() { "Huge", "Large", "Medium", "Small", "Tiny" };

        // Observable collection for us to handle changes
        public ObservableCollection<Ship> shipyard { get; internal set; }

        // List of stored modules from 'Stored modules' event
        public List<StoredModule> storedmodules { get; private set; }

        // The ID of the current ship; can be null
        internal int? currentShipId;

        private const int profileRefreshDelaySeconds = 20;

        private static readonly object shipyardLock = new object();
        public event EventHandler ShipyardUpdatedEvent;
        internal DateTime updatedAt;

        public string MonitorName()
        {
            return Properties.ShipMonitor.ResourceManager.GetString("name", CultureInfo.InvariantCulture);
        }

        public string LocalizedMonitorName()
        {
            return Properties.ShipMonitor.name;
        }

        public string MonitorDescription()
        {
            return Properties.ShipMonitor.desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public ShipMonitor()
        {
            shipyard = new ObservableCollection<Ship>();
            storedmodules = new List<StoredModule>();

            BindingOperations.CollectionRegistering += Shipyard_CollectionRegistering;

            readShips();
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void Shipyard_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(shipyard, shipyardLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(shipyard, shipyardLock); });
            }
        }

        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
            readShips();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Save()
        {
            writeShips();
        }

        /// <summary>
        /// We pre-handle the events to ensure that the data is up-to-date when it hits the responders
        /// </summary>
        public void PreHandle(Event @event)
        {
            // Handle the events that we care about
            if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if (@event is CommanderContinuedEvent commanderContinuedEvent)
            {
                handleCommanderContinuedEvent(commanderContinuedEvent);
            }
            else if (@event is LocationEvent locationEvent)
            {
                handleLocationEvent(locationEvent);
            }
            else if (@event is JumpedEvent jumpedEvent)
            {
                handleJumpedEvent(jumpedEvent);
            }
            else if (@event is ShipPurchasedEvent shipPurchasedEvent)
            {
                handleShipPurchasedEvent(shipPurchasedEvent);
            }
            else if (@event is ShipDeliveredEvent shipDeliveredEvent)
            {
                handleShipDeliveredEvent(shipDeliveredEvent);
            }
            else if (@event is ShipSwappedEvent shipSwappedEvent)
            {
                handleShipSwappedEvent(shipSwappedEvent);
            }
            else if (@event is ShipRenamedEvent shipRenamedEvent)
            {
                handleShipRenamedEvent(shipRenamedEvent);
            }
            else if (@event is ShipSoldEvent shipSoldEvent)
            {
                handleShipSoldEvent(shipSoldEvent);
            }
            else if (@event is ShipSoldOnRebuyEvent shipSoldOnRebuyEvent)
            {
                handleShipSoldOnRebuyEvent(shipSoldOnRebuyEvent);
            }
            else if (@event is ShipLoadoutEvent shipLoadoutEvent)
            {
                handleShipLoadoutEvent(shipLoadoutEvent);
            }
            else if (@event is StoredShipsEvent storedShipsEvent)
            {
                handleStoredShipsEvent(storedShipsEvent);
            }
            else if (@event is ShipRebootedEvent shipRebootedEvent)
            {
                handleShipRebootedEvent(shipRebootedEvent);
            }
            else if (@event is ShipRefuelledEvent shipRefuelledEvent)
            {
                handleShipRefuelledEvent(shipRefuelledEvent);
            }
            else if (@event is ShipAfmuRepairedEvent)
            {
                handleShipAFMURepairedEvent();
            }
            else if (@event is ShipRepairedEvent shipRepairedEvent)
            {
                handleShipRepairedEvent(shipRepairedEvent);
            }
            else if (@event is ShipRepairDroneEvent)
            {
                handleShipRepairDroneEvent();
            }
            else if (@event is ShipRestockedEvent)
            {
                handleShipRestockedEvent();
            }
            else if (@event is ModulePurchasedEvent modulePurchasedEvent)
            {
                handleModulePurchasedEvent(modulePurchasedEvent);
            }
            else if (@event is ModuleRetrievedEvent moduleRetrievedEvent)
            {
                handleModuleRetrievedEvent(moduleRetrievedEvent);
            }
            else if (@event is ModuleSoldEvent moduleSoldEvent)
            {
                handleModuleSoldEvent(moduleSoldEvent);
            }
            else if (@event is ModuleSoldFromStorageEvent)
            {
                handleModuleSoldFromStorageEvent();
            }
            else if (@event is ModuleStoredEvent moduleStoredEvent)
            {
                handleModuleStoredEvent(moduleStoredEvent);
            }
            else if (@event is ModulesStoredEvent modulesStoredEvent)
            {
                handleModulesStoredEvent(modulesStoredEvent);
            }
            else if (@event is ModuleSwappedEvent moduleSwappedEvent)
            {
                handleModuleSwappedEvent(moduleSwappedEvent);
            }
            else if (@event is ModuleTransferEvent)
            {
                handleModuleTransferEvent();
            }
            else if (@event is ModuleInfoEvent moduleInfoEvent)
            {
                handleModuleInfoEvent(moduleInfoEvent);
            }
            else if (@event is StoredModulesEvent storedModulesEvent)
            {
                handleStoredModulesEvent(storedModulesEvent);
            }
            else if (@event is BountyIncurredEvent bountyIncurredEvent)
            {
                handleBountyIncurredEvent(bountyIncurredEvent);
            }
            else if (@event is BountyPaidEvent bountyPaidEvent)
            {
                handleBountyPaidEvent(bountyPaidEvent);
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( @event.timestamp > updatedAt )
            {
                lock ( shipyardLock )
                {
                    foreach ( var ship in shipyard )
                    {
                        if ( ship.StoredLocation.marketId == @event.carrierId )
                        {
                            ship.StoredLocation = new Ship.Location( @event.systemname, @event.systemAddress, @event.x,
                                @event.y, @event.z, @event.carriername, @event.carrierId );
                            ship.distance = ship.DistanceLY( EDDI.Instance.CurrentStarSystem );
                        }
                    }
                    writeShips();
                }
            }
        }

        // Set the ship name conditionally, avoiding filtered names
        private void setShipName(Ship ship, string name)
        {
            if (ship is null) { return; }
            if (string.IsNullOrEmpty(name))
            {
                ship.name = null;
            }
            else if (!name.Contains("***"))
            {
                ship.name = name;
            }
        }

        // Set the ship ident conditionally, avoiding filtered idents
        private void setShipIdent(Ship ship, string ident)
        {
            if (ship is null) { return; }
            if (string.IsNullOrEmpty(ident))
            {
                ship.ident = null;
            }
            else if (ident != null && !ident.Contains("***"))
            {
                ship.ident = ident;
            }
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (!inFighter(@event.shipEDModel) && !inBuggy(@event.shipEDModel) && !onFoot(@event.shipEDModel) && !inTaxi(@event.shipEDModel))
                {
                    SetCurrentShip((int?)@event.shipid, @event.shipEDModel);
                    var ship = GetCurrentShip();
                    if (ship is null && !string.IsNullOrEmpty(@event.shipEDModel) && @event.shipid != null)
                    {
                        // We don't know of this ship so need to create it
                        ship = ShipDefinitions.FromEDModel(@event.shipEDModel);
                        ship.LocalId = (int)@event.shipid;
                        ship.Role = Role.MultiPurpose;
                        AddShip(ship);
                    }

                    if (ship is null) { return; }
                    setShipName(ship, @event.shipname);
                    setShipIdent(ship, @event.shipident);
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                foreach (var shipInYard in shipyard)
                {
                    // Ignore current ship, since (obviously) it's not stored
                    if (shipInYard.LocalId == currentShipId) { continue; }
                    // Otherwise, update the distance to that ship
                    shipInYard.distance = shipInYard.DistanceLY(@event.x, @event.y, @event.z);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                foreach (var shipInYard in shipyard)
                {
                    // Ignore current ship, since (obviously) it's not stored
                    if (shipInYard.LocalId == currentShipId) { continue; }
                    // Otherwise, update the distance to that ship
                    shipInYard.distance = shipInYard.DistanceLY(@event.x, @event.y, @event.z);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipPurchasedEvent(ShipPurchasedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                // We don't have a ship ID for the new ship at this point so just handle what we did with our old ship
                if (@event.storedshipid != null)
                {
                    // We stored a ship - set its location to the current location
                    var storedShip = GetShip(@event.storedshipid);
                    if (storedShip != null && EDDI.Instance.CurrentStarSystem != null)
                    {
                        // Set location of stored ship to the current system
                        storedShip.StoredLocation = new Ship.Location( EDDI.Instance.CurrentStarSystem, EDDI.Instance?.CurrentStation?.name, EDDI.Instance?.CurrentStation?.marketId );
                        storedShip.distance = 0;
                    }
                }
                else if (@event.soldshipid != null)
                {
                    // We sold a ship - remove it
                    RemoveShip(@event.soldshipid);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipDeliveredEvent(ShipDeliveredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                // Set this as our current ship
                SetCurrentShip(@event.shipid, @event.edModel);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipSwappedEvent(ShipSwappedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;

                // Set ship hull and module health with a profile refresh before we write the stored ship.
                EDDI.Instance?.refreshProfile();

                // Update our current ship
                SetCurrentShip(@event.shipid, @event.edModel);

                if (@event.storedshipid != null)
                {
                    // We stored a ship - set its location to the current location
                    var storedShip = GetShip(@event.storedshipid);
                    if (storedShip != null)
                    {
                        // Set location of stored ship to the current system
                        if ( EDDI.Instance?.CurrentStarSystem != null )
                        {
                            storedShip.StoredLocation = new Ship.Location( EDDI.Instance.CurrentStarSystem,
                                EDDI.Instance?.CurrentStation?.name, EDDI.Instance?.CurrentStation?.marketId );
                            storedShip.distance = 0;
                        }
                    }
                }
                else if (@event.soldshipid != null)
                {
                    // We sold a ship - remove it
                    RemoveShip(@event.soldshipid);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipRenamedEvent(ShipRenamedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid);
                if (ship != null)
                {
                    setShipName(ship, @event.name);
                    setShipIdent(ship, @event.ident);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipSoldEvent(ShipSoldEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                RemoveShip(@event.shipid);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipSoldOnRebuyEvent(ShipSoldOnRebuyEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                RemoveShip(@event.shipid);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        internal void handleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                // If we're in the SRV when we start the game, we'll still get a Loadout event for our parent ship
                updatedAt = @event.timestamp;
                if (!inFighter(@event.edModel))
                {
                    var ship = ParseShipLoadoutEvent(@event);

                    // Update the local and global variables
                    EDDI.Instance.CurrentShip = ship;
                    SetCurrentShip( ship.LocalId, ship.EDName );

                    AddShip( ship);
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        internal Ship ParseShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            lock (shipyardLock)
            {
                // Obtain the ship to which this loadout refers
                Logging.Debug("Current Ship Id is: " + currentShipId + ", Loadout Ship Id is " + @event.shipid);
                var ship = GetShip(@event.shipid);

                if (ship == null)
                {
                    // The ship is unknown - create it
                    Logging.Debug("Unknown ship ID " + @event.shipid);
                    ship = @event.shipDefinition;
                    ship.LocalId = @event.shipid;
                }

                // Save a copy of the raw event so that we can send it to other 3rd party apps
                ship.raw = @event.raw;

                // Update model (in case it was solely from the edname), name, ident, and paintjob if required
                ship.model = @event.shipDefinition.model ?? @event.ship ?? @event.edModel;
                setShipName(ship, @event.shipname);
                setShipIdent(ship, @event.shipident);
                ship.paintjob = @event.paintjob;
                ship.hot = @event.hot;

                // Augment with template values
                ship.Augment();

                // Write ship value, if given by the loadout event
                if (@event.value != null)
                {
                    ship.value = (long)@event.value;
                }
                ship.hullvalue = @event.hullvalue;
                ship.modulesvalue = @event.modulesvalue;
                ship.rebuy = @event.rebuy;
                ship.unladenmass = @event.unladenmass;
                ship.maxjumprange = @event.maxjumprange;
                ship.health = @event.hullhealth;

                // Calculate and update our commander's insurance rate
                if ( @event.value > 0 && EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.insurance = Math.Round((decimal)@event.rebuy / (@event.value ?? 0), 2);
                }

                // Set the standard modules
                var compartment = @event.compartments.FirstOrDefault(c => c.name == "Armour");
                if (compartment != null)
                {
                    ship.bulkheads = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "ShipCockpit");
                if (compartment != null)
                {
                    ship.canopy = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "PowerPlant");
                if (compartment != null)
                {
                    ship.powerplant = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "MainEngines");
                if (compartment != null)
                {
                    ship.thrusters = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "PowerDistributor");
                if (compartment != null)
                {
                    ship.powerdistributor = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "FrameShiftDrive");
                if (compartment != null)
                {
                    ship.frameshiftdrive = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "LifeSupport");
                if (compartment != null)
                {
                    ship.lifesupport = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "Radar");
                if (compartment != null)
                {
                    ship.sensors = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "FuelTank");
                if (compartment != null)
                {
                    ship.fueltank = compartment.module;
                }

                compartment = @event.compartments.FirstOrDefault(c => c.name == "CargoHatch");
                if (compartment != null)
                {
                    ship.cargohatch = compartment.module;
                }

                // Internal + restricted modules
                var compartments = new List<Compartment>();
                foreach (var cpt in @event.compartments
                             .Where(c => c.name.StartsWith("Slot") || c.name.StartsWith("Military")).ToList())
                {
                    compartments.Add(cpt);
                }

                ship.compartments = compartments;

                // Hardpoints
                var hardpoints = new List<Hardpoint>();
                foreach (var hpt in @event.hardpoints)
                {
                    hardpoints.Add(hpt);
                }

                ship.hardpoints = hardpoints;

                return ship;
            }
        }

        private void handleStoredShipsEvent(StoredShipsEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (@event.shipyard != null)
                {
                    //Update ship location data in the event
                    var quickSystems =
                        EDDI.Instance?.DataProvider.GetOrFetchQuickStarSystems(
                            @event.shipyard.Select( sh => sh.starsystem ).Distinct().ToArray(), false ) ??
                        new List<StarSystem>();
                    foreach ( var ship in @event.shipyard )
                    {
                        if ( !string.IsNullOrEmpty( ship.starsystem ) )
                        {
                            var systemData = quickSystems.FirstOrDefault( sys => sys.systemname == @event.system);
                            var stationData = systemData?.stations?.FirstOrDefault( s => s.marketId == ship.marketid );
                            ship.StoredLocation = systemData is null || stationData is null
                                ? null
                                : new Ship.Location( systemData, stationData?.name, stationData?.marketId );
                            ship.distance = ship.DistanceLY( EDDI.Instance?.CurrentStarSystem );
                        }
                        else
                        {
                            ship.StoredLocation =
                                EDDI.Instance?.CurrentStarSystem is null || EDDI.Instance.CurrentStation is null
                                    ? null
                                    : new Ship.Location( 
                                        EDDI.Instance.CurrentStarSystem,
                                        EDDI.Instance.CurrentStation.name, 
                                        EDDI.Instance.CurrentStation.marketId );
                            ship.distance = 0;
                        }
                    }

                    //Check for ships missing from the shipyard
                    foreach (var shipInEvent in @event.shipyard)
                    {
                        var shipInYard = GetShip(shipInEvent.LocalId);

                        // Add ship from the event if not in shipyard
                        if (shipInYard == null)
                        {
                            shipInEvent.Role = Role.MultiPurpose;
                            AddShip(shipInEvent);
                        }

                        // Update ship in the shipyard to latest data
                        else
                        {
                            if (!string.IsNullOrEmpty(shipInEvent.name))
                            {
                                shipInYard.name = shipInEvent.name;
                            }
                            shipInYard.value = shipInEvent.value;
                            shipInYard.hot = shipInEvent.hot;
                            shipInYard.intransit = shipInEvent.intransit;
                            shipInYard.StoredLocation = shipInEvent.StoredLocation;
                            shipInYard.distance = shipInEvent.distance;
                            shipInYard.transferprice = shipInEvent.transferprice;
                            shipInYard.transfertime = shipInEvent.transfertime;
                        }
                    }

                    // Prune ships no longer in the shipyard
                    var idsToRemove = new List<int>(shipyard.Count);
                    foreach (var shipInYard in shipyard)
                    {
                        // Ignore current ship, since (obviously) it's not stored
                        if (shipInYard.LocalId == currentShipId) { continue; }

                        var shipInEvent = @event.shipyard.FirstOrDefault(s => s.LocalId == shipInYard.LocalId);
                        if (shipInEvent == null)
                        {
                            idsToRemove.Add(shipInYard.LocalId);
                        }
                    }
                    _RemoveShips(idsToRemove);

                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        internal void handleStoredModulesEvent(StoredModulesEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (@event.storedmodules != null)
                {
                    var quickSystems =
                        EDDI.Instance?.DataProvider.GetOrFetchQuickStarSystems(
                            @event.storedmodules.Select( m => m.system ).Distinct().ToArray(), false ) ?? new List<StarSystem>();
                    foreach ( var module in @event.storedmodules )
                    {
                        var moduleSystem = quickSystems.FirstOrDefault( system => system.systemname == module.system );
                        module.station = moduleSystem?.stations
                            ?.FirstOrDefault( station => station.marketId == module.marketid )?.name;
                    }

                    storedmodules = @event.storedmodules;
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        private void handleShipRebootedEvent(ShipRebootedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetCurrentShip();
                if (ship == null) { return; }
                foreach (var compartmentName in @event.compartments)
                {
                    // Find the matching module and set health to 1%
                    // Update the event with a list of the repaired modules.
                    if (compartmentName == "ShipCockpit" && ship.canopy != null)
                    {
                        ship.canopy.health = 1;
                        @event.Modules.Add( ship.canopy );
                    }
                    else if (compartmentName == "PowerPlant" && ship.powerplant != null)
                    {
                        ship.powerplant.health = 1;
                        @event.Modules.Add( ship.powerplant );
                    }
                    else if (compartmentName == "MainEngines" && ship.thrusters != null)
                    {
                        ship.thrusters.health = 1;
                        @event.Modules.Add( ship.thrusters );
                    }
                    else if (compartmentName == "PowerDistributor" && ship.powerdistributor != null)
                    {
                        ship.powerdistributor.health = 1;
                        @event.Modules.Add( ship.powerdistributor );
                    }
                    else if (compartmentName == "FrameShiftDrive" && ship.frameshiftdrive != null)
                    {
                        ship.frameshiftdrive.health = 1;
                        @event.Modules.Add( ship.frameshiftdrive );
                    }
                    else if (compartmentName == "LifeSupport" && ship.lifesupport != null)
                    {
                        ship.lifesupport.health = 1;
                        @event.Modules.Add( ship.lifesupport );
                    }
                    else if (compartmentName == "Radar" && ship.sensors != null)
                    {
                        ship.sensors.health = 1;
                        @event.Modules.Add( ship.sensors );
                    }
                    else if (compartmentName == "CargoHatch" && ship.cargohatch != null)
                    {
                        ship.cargohatch.health = 1;
                        @event.Modules.Add( ship.cargohatch );
                    }
                    else if (compartmentName.Contains("Hardpoint"))
                    {
                        foreach (var hardpoint in ship.hardpoints)
                        {
                            if (hardpoint.name == compartmentName && hardpoint.module != null)
                            {
                                hardpoint.module.health = 1;
                                @event.Modules.Add( hardpoint.module );
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var compartment in ship.compartments)
                        {
                            if (compartment.name == compartmentName && compartment.module != null)
                            {
                                compartment.module.health = 1;
                                @event.Modules.Add( compartment.module );
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void handleShipAFMURepairedEvent()
        {
            // This doesn't give us enough information at present to do anything useful
        }

        private void handleShipRepairedEvent(ShipRepairedEvent @event)
        {
            if (@event.itemEDNames.Contains("All") || @event.itemEDNames.Contains("Wear"))
            {
                var currentShip = GetCurrentShip();
                if ( currentShip != null )
                {
                    currentShip.health = 100M;
                }
            }
            if (!@event.fromLoad) { writeShips(); }
        }

        private void handleShipRepairDroneEvent()
        {
            // This event does not report the percentage of hull repaired.
            // It reports the integrity repaired (which we can't use since we do not calculate integrity).
            // Set ship hull and module health with a profile refresh.
            EDDI.Instance?.refreshProfile();
        }

        private void handleShipRefuelledEvent(ShipRefuelledEvent @event)
        {
            // Determine if this refuel takes the ship to full tanks (if not already determined)
            if (@event.full is null)
            {
                @event.full = GetShip(currentShipId)?.fueltanktotalcapacity == @event.total;
            }

            // We use status to track current fuel level so we won't update the ship fuel level here
        }

        private void handleShipRestockedEvent()
        {
            // TODO
        }

        private void handleModulePurchasedEvent(ModulePurchasedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? @event.shipid : ship.LocalId;
                AddModule(ship, @event.slot, @event.buymodule);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleRetrievedEvent(ModuleRetrievedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? @event.shipid : ship.LocalId;
                AddModule(ship, @event.slot, @event.module);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleSoldEvent(ModuleSoldEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? @event.shipid : ship.LocalId;
                RemoveModule(ship, @event.slot);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleSoldFromStorageEvent()
        {
            // We don't do anything here as the ship object is unaffected
        }

        private void handleModuleStoredEvent(ModuleStoredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? @event.shipid : ship.LocalId;
                RemoveModule(ship, @event.slot, @event.replacementmodule);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModulesStoredEvent(ModulesStoredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                var ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? @event.shipid : ship.LocalId;
                foreach (var slot in @event.slots)
                {
                    RemoveModule(ship, slot);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        internal void handleModuleSwappedEvent(ModuleSwappedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                lock ( shipyardLock )
                {
                    var ship = GetShip(@event.shipid);
                    var fromSlot = @event.fromslot;
                    var toSlot = @event.toslot;

                    if ( fromSlot.Contains( "Hardpoint" ) ) // Module is a hardpoint
                    {
                        // Find our hardpoints. Add them if them are missing
                        var fromHardpoint = ship.hardpoints.FirstOrDefault( h => h.name == fromSlot );
                        if ( fromHardpoint is null )
                        {
                            fromHardpoint = new Hardpoint { name = fromSlot, size = getHardpointSize( fromSlot ) };
                            ship.hardpoints.Add( fromHardpoint );
                        }

                        var toHardpoint = ship.hardpoints.FirstOrDefault( h => h.name == toSlot );
                        if ( toHardpoint is null )
                        {
                            toHardpoint = new Hardpoint { name = toSlot, size = getHardpointSize( toSlot ) };
                            ship.hardpoints.Add( toHardpoint );
                        }

                        sortHardpoints( ship );

                        // Swap the modules
                        var fromModule = fromHardpoint.module;
                        var toModule = toHardpoint.module;
                        fromHardpoint.module = toModule;
                        toHardpoint.module = fromModule;
                    }
                    else //Module is a compartment
                    {
                        // Find our compartments. Add them if them are missing
                        var fromCompartment = ship.compartments.FirstOrDefault( c => c.name == fromSlot );
                        if ( fromCompartment is null )
                        {
                            fromCompartment = new Compartment()
                            {
                                name = fromSlot, size = getCompartmentSize( fromSlot, ship.militarysize )
                            };
                            ship.compartments.Add( fromCompartment );
                        }

                        var toCompartment = ship.compartments.FirstOrDefault( c => c.name == toSlot );
                        if ( toCompartment is null )
                        {
                            toCompartment = new Compartment()
                            {
                                name = toSlot, size = getCompartmentSize( toSlot, ship.militarysize )
                            };
                            ship.compartments.Add( toCompartment );
                        }

                        sortCompartments( ship );

                        // Swap the modules
                        var fromModule = fromCompartment?.module;
                        var toModule = toCompartment?.module;
                        fromCompartment.module = toModule;
                        toCompartment.module = fromModule;
                    }

                    updatedAt = @event.timestamp;
                }

                if ( !@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleTransferEvent()
        {
            // We don't do anything here as the ship object is unaffected
        }

        private void handleModuleInfoEvent(ModuleInfoEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                lock ( shipyardLock )
                {
                    var ship = GetCurrentShip();
                    if ( ship != null && @event.Modules != null )
                    {
                        for ( var i = 0; i < @event.Modules.Count(); i++ )
                        {
                            var position = i + 1;
                            var priority = @event.Modules[ i ].priority + 1;
                            var power = @event.Modules[ i ].power;

                            var slot = @event.Modules[ i ].slot;
                            if ( !string.IsNullOrEmpty( slot ) )
                            {
                                switch ( slot )
                                {
                                    case "CargoHatch":
                                        {
                                            ship.cargohatch = ship.cargohatch
                                                              ?? Module.FromEDName( @event.Modules[ i ].item )
                                                              ?? new Module();
                                            ship.cargohatch.position = position;
                                            ship.cargohatch.priority = priority;
                                            ship.cargohatch.power = power;
                                            break;
                                        }
                                    case "FrameShiftDrive":
                                        {
                                            ship.frameshiftdrive = ship.frameshiftdrive
                                                                   ?? Module.FromEDName( @event.Modules[ i ].item )
                                                                   ?? new Module();
                                            ship.frameshiftdrive.position = position;
                                            ship.frameshiftdrive.priority = priority;
                                            ship.frameshiftdrive.power = power;
                                            break;
                                        }
                                    case "LifeSupport":
                                        {
                                            ship.lifesupport = ship.lifesupport
                                                               ?? Module.FromEDName( @event.Modules[ i ].item )
                                                               ?? new Module();
                                            ship.lifesupport.position = position;
                                            ship.lifesupport.priority = priority;
                                            ship.lifesupport.power = power;
                                            break;
                                        }
                                    case "MainEngines":
                                        {
                                            ship.thrusters = ship.thrusters
                                                             ?? Module.FromEDName( @event.Modules[ i ].item )
                                                             ?? new Module();
                                            ship.thrusters.position = position;
                                            ship.thrusters.priority = priority;
                                            ship.thrusters.power = power;
                                        }
                                        break;
                                    case "PowerDistributor":
                                        {
                                            ship.powerdistributor = ship.powerdistributor
                                                                    ?? Module.FromEDName( @event.Modules[ i ].item )
                                                                    ?? new Module();
                                            ship.powerdistributor.position = position;
                                            ship.powerdistributor.priority = priority;
                                            ship.powerdistributor.power = power;
                                        }
                                        break;
                                    case "PowerPlant":
                                        {
                                            ship.powerplant = ship.powerplant
                                                              ?? Module.FromEDName( @event.Modules[ i ].item )
                                                              ?? new Module();
                                            ship.powerplant.position = position;
                                            ship.powerplant.priority = priority;
                                            ship.powerplant.power = power;
                                        }
                                        break;
                                    case "Radar":
                                        {
                                            ship.sensors = ship.sensors
                                                           ?? Module.FromEDName( @event.Modules[ i ].item )
                                                           ?? new Module();
                                            ship.sensors.position = position;
                                            ship.sensors.priority = priority;
                                            ship.sensors.power = power;
                                        }
                                        break;
                                    case "ShipCockpit":
                                        {
                                            ship.canopy = ship.canopy
                                                          ?? Module.FromEDName( @event.Modules[ i ].item )
                                                          ?? new Module();
                                            ship.canopy.position = position;
                                            ship.canopy.priority = priority;
                                            ship.canopy.power = power;
                                        }
                                        break;
                                }

                                if ( slot.Contains( "Slot" ) )
                                {
                                    var compartment = ship.compartments.FirstOrDefault( c => c.name == slot );
                                    if ( compartment != null )
                                    {
                                        compartment.module = compartment.module
                                                             ?? Module.FromEDName( @event.Modules[ i ].item )
                                                             ?? new Module();
                                        compartment.module.position = position;
                                        compartment.module.priority = priority;
                                        compartment.module.power = power;
                                    }
                                }
                                else if ( slot.Contains( "Hardpoint" ) )
                                {
                                    var hardpoint = ship.hardpoints.FirstOrDefault( h => h.name == slot );
                                    if ( hardpoint != null )
                                    {
                                        hardpoint.module = hardpoint.module
                                                           ?? Module.FromEDName( @event.Modules[ i ].item )
                                                           ?? new Module();
                                        hardpoint.module.position = position;
                                        hardpoint.module.priority = priority;
                                        hardpoint.module.power = power;
                                    }
                                }
                            }
                        }
                    }
                }
                if ( !@event.fromLoad )
                {
                    writeShips();
                }
            }
        }

        private void handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                // Committing a crime while in multicrew will apply a fine or bounty to your most valuable ship.
                lock ( shipyardLock )
                {
                    var ship = EDDI.Instance.Vehicle == Constants.VEHICLE_MULTICREW
                        ? shipyard.ToList().OrderByDescending(s => s.value).FirstOrDefault()
                        : GetCurrentShip();
                    if ( ship != null )
                    {
                        ship.hot = true;
                        if ( !@event.fromLoad )
                        { writeShips(); }
                    }
                }
            }
        }

        private void handleBountyPaidEvent(BountyPaidEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                lock ( shipyardLock )
                {
                    var ship = GetShip(@event.shipid);
                    if ( ship != null )
                    {
                        ship.hot = false;
                        if ( !@event.fromLoad )
                        { writeShips(); }
                    }
                }
            }
        }

        public void PostHandle(Event @event)
        {
            if (@event is ShipLoadoutEvent shipLoadoutEvent)
            {
                posthandleShipLoadoutEvent(shipLoadoutEvent);
            }
        }

        private void posthandleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            if (!@event.fromLoad)
            {
                /// The ship may have Frontier API specific data, request a profile refresh from the Frontier API shortly after switching
                refreshProfileDelayed();
            }
        }

        // Note: At a minimum, the API Profile data is required to update the current ship's launchbay status
        public void HandleProfile(JObject profile)
        {
            // Obtain the current ship from the profile
            var profileCurrentShip = FrontierApi.ShipFromJson((JObject)profile["ship"]);

            // Obtain the shipyard from the profile
            var profileShipyard = FrontierApi.ShipyardFromJson(profileCurrentShip, profile);

            if (profileCurrentShip != null)
            {
                if (currentShipId == null)
                {
                    // This means that we don't have any info so far; set our active ship
                    currentShipId = profileCurrentShip.LocalId;
                }
                Logging.Debug("Current Ship Id is: " + currentShipId + ", Profile Ship Id is: " + profileCurrentShip.LocalId);

                if (currentShipId == profileCurrentShip.LocalId)
                {
                    var ship = GetShip(currentShipId);
                    if (ship == null)
                    {
                        // Information from the Frontier API can be out-of-date, use it to set our ship if we don't know what it already is
                        ship = profileCurrentShip;
                        AddShip(ship);
                    }
                    else
                    {
                        lock ( shipyardLock )
                        {
                            // Update launch bays from profile
                            if ( profileCurrentShip.launchbays.Any() )
                            {
                                ship.launchbays = profileCurrentShip.launchbays;
                            }
                            else
                            {
                                ship.launchbays.Clear();
                            }

                            // Update ship hull health from profile
                            ship.health = profileCurrentShip.health;

                            // Update ship modules from the profile
                            ship.bulkheads = ship.bulkheads ?? new Module();
                            ship.bulkheads.UpdateFromFrontierAPIModule( profileCurrentShip.bulkheads );

                            ship.powerplant = ship.powerplant ?? new Module();
                            ship.powerplant.UpdateFromFrontierAPIModule( profileCurrentShip.powerplant );

                            ship.thrusters = ship.thrusters ?? new Module();
                            ship.thrusters.UpdateFromFrontierAPIModule( profileCurrentShip.thrusters );

                            ship.powerdistributor = ship.powerdistributor ?? new Module();
                            ship.powerdistributor.UpdateFromFrontierAPIModule( profileCurrentShip.powerdistributor );

                            ship.frameshiftdrive = ship.frameshiftdrive ?? new Module();
                            ship.frameshiftdrive.UpdateFromFrontierAPIModule( profileCurrentShip.frameshiftdrive );

                            ship.lifesupport = ship.lifesupport ?? new Module();
                            ship.lifesupport.UpdateFromFrontierAPIModule( profileCurrentShip.lifesupport );

                            ship.sensors = ship.sensors ?? new Module();
                            ship.sensors.UpdateFromFrontierAPIModule( profileCurrentShip.sensors );

                            ship.fueltank = ship.fueltank ?? new Module();
                            ship.fueltank.UpdateFromFrontierAPIModule( profileCurrentShip.fueltank );

                            ship.cargohatch = ship.cargohatch ?? new Module();
                            ship.cargohatch.UpdateFromFrontierAPIModule( profileCurrentShip.cargohatch );

                            foreach ( var profileHardpoint in profileCurrentShip.hardpoints )
                            {
                                foreach ( var shipHardpoint in ship.hardpoints )
                                {
                                    if ( profileHardpoint.module != null && profileHardpoint.module.invariantName ==
                                        shipHardpoint.module?.invariantName )
                                    {
                                        shipHardpoint.module = shipHardpoint.module ?? new Module();
                                        shipHardpoint.module.UpdateFromFrontierAPIModule( profileHardpoint.module );
                                    }
                                }
                            }

                            foreach ( var profileCompartment in profileCurrentShip.compartments )
                            {
                                foreach ( var shipCompartment in ship.compartments )
                                {
                                    if ( profileCompartment.module != null && profileCompartment.module.invariantName ==
                                        shipCompartment.module.invariantName )
                                    {
                                        shipCompartment.module = shipCompartment.module ?? new Module();
                                        shipCompartment.module.UpdateFromFrontierAPIModule( profileCompartment.module );
                                    }
                                }
                            }
                        }
                    }

                    Logging.Debug("Ship is: ", ship);
                }
                else
                {
                    refreshProfileDelayed();
                }
            }

            // Prune ships from the Shipyard that are not found in the Profile Shipyard 
            var idsToRemove = new List<int>(shipyard.Count);
            lock ( shipyardLock )
            {
                foreach ( var ship in shipyard )
                {
                    var profileShip = profileShipyard.FirstOrDefault(s => s.LocalId == ship.LocalId);
                    if ( profileShip == null )
                    {
                        idsToRemove.Add( ship.LocalId );
                    }
                }
            }
            _RemoveShips(idsToRemove);

            // Add ships from the Profile Shipyard that are not found in the Shipyard 
            // Update name, ident and value of ships in the Shipyard 
            foreach (var profileShip in profileShipyard)
            {
                var ship = GetShip(profileShip.LocalId);
                if (ship == null)
                {
                    // This is a new ship, add it to the shipyard
                    AddShip(profileShip);
                }
                else
                {
                    ship.StoredLocation = profileShip.StoredLocation;
                    ship.distance = ship.DistanceLY( EDDI.Instance.CurrentStarSystem );
                }
            }

            writeShips();
        }

        public void HandleStatus ( Status status )
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( shipyardLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    ["ship"] = new Tuple<Type, object>(typeof(Ship), GetCurrentShip() ),
                    ["storedmodules"] = new Tuple<Type, object>(typeof(List<StoredModule>), storedmodules.ToList() ),
                    ["shipyard"] = new Tuple<Type, object>( typeof( List<Ship> ), shipyard.ToList() )
                };
            }
        }

        private void writeShips()
        {
            lock (shipyardLock)
            {
                // Write ship configuration with current inventory
                var configuration = new ShipMonitorConfiguration()
                {
                    currentshipid = currentShipId,
                    shipyard = shipyard,
                    storedmodules = storedmodules,
                    updatedat = updatedAt
                };
                ConfigService.Instance.shipMonitorConfiguration = configuration;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(ShipyardUpdatedEvent, shipyard);
        }

        private void readShips()
        {
            lock (shipyardLock)
            {
                // Obtain current inventory from configuration
                var configuration = ConfigService.Instance.shipMonitorConfiguration;
                updatedAt = configuration.updatedat;

                // Build a new shipyard
                var newShiplist = configuration.shipyard.OrderBy(s => s.model).ToList();
                var newModuleList = configuration.storedmodules.OrderBy(s => s.slot).ToList();

                // There was a bug (ref. #1894) that added the SRV as a ship. Clean that up here.
                newShiplist = newShiplist.Where(s => s.EDName != "SRV").ToList();

                // Update the shipyard
                shipyard = new ObservableCollection<Ship>(newShiplist);
                currentShipId = configuration.currentshipid;
                storedmodules = new List<StoredModule>(newModuleList);
            }
        }

        internal void AddShip(Ship ship)
        {
            if (ship == null)
            {
                return;
            }

            // Ensure that we have a role for this ship
            if (ship.Role == null)
            {
                ship.Role = Role.MultiPurpose;
            }
            _ReplaceOrAddShip(ship);
        }

        private void _ReplaceOrAddShip(Ship ship)
        {
            if (ship == null)
            {
                return;
            }
            lock (shipyardLock)
            {
                for (var i = 0; i < shipyard.Count; i++)
                {
                    if (shipyard[i].LocalId == ship.LocalId)
                    {
                        shipyard[i] = ship; // this is much more efficient than removing and adding
                        return;
                    }
                }
                // not found, so add
                shipyard.Add(ship);
            }
        }

        /// <summary>
        /// Remove a ship from the shipyard
        /// </summary>
        internal void RemoveShip(int? localid)
        {
            if (localid == null)
            {
                return;
            }
            _RemoveShip(localid);
        }

        /// <summary>
        /// Remove a ship from the shipyard
        /// </summary>
        private void _RemoveShip(int? localid)
        {
            if (localid == null)
            {
                return;
            }
            lock (shipyardLock)
            {
                for (var i = 0; i < shipyard.Count; i++)
                {
                    if (shipyard[i].LocalId == localid)
                    {
                        shipyard.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Remove a list of ships from the shipyard
        /// </summary>
        private void _RemoveShips(List<int> idsToRemove)
        {
            idsToRemove.Sort();
            lock (shipyardLock)
            {
                for (var i = 0; i < shipyard.Count; i++)
                {
                    if (idsToRemove.BinarySearch(shipyard[i].LocalId) >= 0)
                    {
                        shipyard.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Obtain the current ship
        /// </summary>
        public Ship GetCurrentShip()
        {
            var currentShip = GetShip(currentShipId);
            EDDI.Instance.CurrentShip = currentShip ?? EDDI.Instance.CurrentShip;
            return currentShip;
        }

        /// <summary>
        /// Obtain a specific ship as identified by its local ID
        /// </summary>
        public Ship GetShip(int? localId)
        {
            if (localId == null)
            {
                return null;
            }
            Ship ship;
            lock (shipyardLock)
            {
                ship = shipyard.FirstOrDefault(s => s.LocalId == localId);
            }
            return ship;
        }

        public Ship GetShip(int? localId, string model)
        {
            Ship ship;
            if (localId == null)
            {
                // No local ID so take the current ship
                ship = GetCurrentShip();
            }
            else
            {
                // Find the ship with the given local ID
                ship = GetShip(localId);
            }
            if (ship == null)
            {
                // Provide a basic ship based on the model template if no ship is found using the local ID
                ship = ShipDefinitions.FromModel(model);
                if (ship == null)
                {
                    ship = ShipDefinitions.FromEDModel(model);
                }
            }
            return ship;
        }

        public void SetCurrentShip(int? localId, string EDName = null)
        {
            lock (shipyardLock)
            {
                // Ensure that this ID is present
                var ship = GetShip(localId);
                if (ship == null)
                {
                    // We don't know about this ship yet
                    Logging.Debug("Unknown ship ID " + localId);
                    if (localId.HasValue && EDName != null)
                    {
                        // We can make one though
                        ship = ShipDefinitions.FromEDModel(EDName);
                        ship.LocalId = (int)localId;
                        ship.Role = Role.MultiPurpose;
                        AddShip(ship);
                        currentShipId = ship.LocalId;
                        Logging.Debug("Created ship ID " + localId + ";  ", ship);
                    }
                    else
                    {
                        Logging.Warn("Cannot set ship ID " + localId + "; unsetting current ship");
                        currentShipId = null;
                    }
                }
                else
                {
                    Logging.Debug("Current ship ID is " + localId);
                    currentShipId = ship.LocalId;
                    // Location for the current ship is always null, as it's with us
                    ship.StoredLocation = null;
                    ship.distance = null;
                }
                EDDI.Instance.CurrentShip = ship;
            }
        }

        internal void AddModule(Ship ship, string slot, Module module)
        {
            if (ship != null && slot != null && module != null)
            {
                try
                {
                    Logging.Debug($"Adding module {module?.edname} to ship {ship?.LocalId} in slot {slot}", module);
                    lock ( shipyardLock )
                    {
                        switch ( slot )
                        {
                            case "Armour":
                                ship.bulkheads = module;
                                break;
                            case "PowerPlant":
                                ship.powerplant = module;
                                break;
                            case "MainEngines":
                                ship.thrusters = module;
                                break;
                            case "PowerDistributor":
                                ship.powerdistributor = module;
                                break;
                            case "FrameShiftDrive":
                                ship.frameshiftdrive = module;
                                break;
                            case "LifeSupport":
                                ship.lifesupport = module;
                                break;
                            case "Radar":
                                ship.sensors = module;
                                break;
                            case "FuelTank":
                                {
                                    ship.fueltank = module;
                                }
                                break;
                            case "CargoHatch":
                                ship.cargohatch = module;
                                break;
                        }

                        if ( slot.Contains( "PaintJob" ) )
                        {
                            ship.paintjob = module.edname;
                        }
                        else if ( slot.Contains( "Hardpoint" ) )
                        {
                            // This is a hardpoint
                            var hardpoint = ship.hardpoints.FirstOrDefault( h => h.name == slot );
                            if ( hardpoint is null )
                            {
                                hardpoint = new Hardpoint { name = slot, size = getHardpointSize( slot ) };
                                ship.hardpoints.Add( hardpoint );
                            }

                            hardpoint.module = module;
                            sortHardpoints( ship );
                        }
                        else if ( slot.Contains( "Slot" ) || slot.Contains( "Military" ) )
                        {
                            // This is a compartment
                            var compartment = ship.compartments.FirstOrDefault( c => c.name == slot );
                            if ( compartment is null )
                            {
                                compartment = new Compartment
                                {
                                    name = slot, size = getCompartmentSize( slot, ship.militarysize )
                                };
                                ship.compartments.Add( compartment );
                            }

                            compartment.module = module;
                            sortCompartments( ship );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to add module {module?.edname} to ship {ship?.LocalId} in slot {slot}.", ex);
                    throw;
                }
            }
            else
            {
                Logging.Warn("Cannot add the module. Ship ID " + ship?.LocalId + " or ship slot " + slot + " was not found.");
            }
        }

        private static void sortCompartments(Ship ship)
        {
            try
            {
                Logging.Debug($"Sorting ship {ship.LocalId} compartments", ship.compartments);

                // Build new dictionary of ship compartments, excepting sold/stored compartment
                var compartments = new Dictionary<string, Compartment>();
                foreach (var cpt in ship.compartments)
                {
                    compartments.Add(cpt.name, cpt);
                }

                // Clear ship compartments and repopulate in correct order
                ship.compartments.Clear();
                for (var i = 1; i <= 12; i++)
                {
                    for (var j = 1; j <= 8; j++)
                    {
                        if (compartments.TryGetValue("Slot" + i.ToString("00") + "_Size" + j, out var cpt))
                        {
                            ship.compartments.Add(cpt);
                        }
                    }
                }

                for (var i = 1; i <= 3; i++)
                {
                    if (compartments.TryGetValue("Military" + i.ToString("00"), out var cpt))
                    {
                        ship.compartments.Add(cpt);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Logging.Error($"Failed to sort ship {ship?.LocalId} compartments", ex);
            }
        }

        private static int getCompartmentSize(string slot, int? militarySlotSize)
        {
            // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"
            if (!string.IsNullOrEmpty(slot))
            {
                if (slot.Contains("Slot"))
                {
                    var matches = Regex.Match(slot, @"Size([0-9]+)");
                    if (matches.Success)
                    {
                        return int.Parse(matches.Groups[1].Value);
                    }
                }
                else if (slot.Contains("Military") && militarySlotSize != null)
                {
                    return (int)militarySlotSize;
                }
            }
            // Compartment size could not be determined
            Logging.Error("Ship compartment slot size could not be determined for " + slot);
            return -1;
        }

        private static void sortHardpoints(Ship ship)
        {
            try
            {
                Logging.Debug($"Sorting ship {ship.LocalId} hardpoints", ship.hardpoints);

                // Build new dictionary of ship hardpoints, excepting sold/stored hardpoint
                var hardpoints = new Dictionary<string, Hardpoint>();
                foreach (var hp in ship.hardpoints)
                {
                    hardpoints.Add(hp.name, hp);
                }

                // Clear ship hardpoints and repopulate in correct order
                ship.hardpoints.Clear();
                foreach (var size in HARDPOINT_SIZES)
                {
                    for (var i = 1; i <= 12; i++)
                    {
                        if (hardpoints.TryGetValue(size + "Hardpoint" + i, out var hp))
                        {
                            ship.hardpoints.Add(hp);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Logging.Error($"Failed to sort ship {ship?.LocalId} hardpoints", ex);
            }
        }

        private static int getHardpointSize(string slot)
        {
            if (!string.IsNullOrEmpty(slot))
            {
                if (slot.StartsWith("Tiny"))
                {
                    return 0;
                }
                else if (slot.StartsWith("Small"))
                {
                    return 1;
                }
                else if (slot.StartsWith("Medium"))
                {
                    return 2;
                }
                else if (slot.StartsWith("Large"))
                {
                    return 3;
                }
                else if (slot.StartsWith("Huge"))
                {
                    return 4;
                }                
            }

            // Hardpoint size could not be determined
            Logging.Error("Ship hardpoint slot size could not be determined for " + slot);
            return -1;
        }

        internal void RemoveModule(Ship ship, string slot, Module replacement = null)
        {
            if ( ship != null && slot != null )
            {
                try
                {
                    Logging.Debug( $"Removing module from slot {slot} in ship {ship.LocalId} in slot {slot}. Replacement module is: " + replacement is null ? "<None>" : JsonConvert.SerializeObject( replacement ) );
                    lock ( shipyardLock )
                    {
                        if ( replacement != null )
                        {
                            switch ( slot )
                            {
                                case "Armour":
                                    ship.bulkheads = replacement;
                                    break;
                                case "PowerPlant":
                                    ship.powerplant = replacement;
                                    break;
                                case "MainEngines":
                                    ship.thrusters = replacement;
                                    break;
                                case "PowerDistributor":
                                    ship.powerdistributor = replacement;
                                    break;
                                case "FrameShiftDrive":
                                    ship.frameshiftdrive = replacement;
                                    break;
                                case "LifeSupport":
                                    ship.lifesupport = replacement;
                                    break;
                                case "Radar":
                                    ship.sensors = replacement;
                                    break;
                                case "FuelTank":
                                    ship.fueltank = replacement;
                                    break;
                                case "CargoHatch":
                                    ship.cargohatch = replacement;
                                    break;
                            }

                        }
                        else
                        {
                            if ( slot.Contains( "PaintJob" ) )
                            {
                                ship.paintjob = null;
                            }
                            else if ( slot.Contains( "Hardpoint" ) )
                            {
                                // Build new list of ship hardpoints, excepting sold/stored hardpoint
                                var hardpoints = new List<Hardpoint>();
                                foreach ( var hpt in ship.hardpoints )
                                {
                                    if ( hpt.name != slot )
                                    {
                                        hardpoints.Add( hpt );
                                    }
                                }

                                ship.hardpoints = hardpoints;
                            }
                            else if ( slot.Contains( "Slot" ) || slot.Contains( "Military" ) )
                            {
                                // Build new list of ship compartments, excepting sold/stored compartment
                                var compartments = new List<Compartment>();
                                foreach ( var cpt in ship.compartments )
                                {
                                    if ( cpt.name != slot )
                                    {
                                        compartments.Add( cpt );
                                    }
                                }

                                ship.compartments = compartments;
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Logging.Error( $"Failed to remove module from slot {slot} on ship {ship?.LocalId}.", ex );
                    throw;
                }
            }
            else
            {
                Logging.Warn("Cannot remove the module. Ship ID " + ship?.LocalId + " or ship slot " + slot + " was not found.");
            }
        }

        /// <summary> See if we're in a fighter </summary>
        private bool inFighter(string model)
        {
            return model.Contains("Fighter");
        }

        /// <summary> See if we're in a buggy / SRV </summary>
        private bool inBuggy(string edModel)
        {
            return edModel.Contains("Buggy") || edModel.Contains("SRV");
        }

        /// <summary> See if we're on foot </summary>
        private bool onFoot(string edModel)
        {
            return edModel.Contains("Suit");
        }

        private bool inTaxi(string edModel)
        {
            return edModel.Contains("_taxi");
        }

        private Task _refreshProfileDelayed;
        private void refreshProfileDelayed()
        {
            if (_refreshProfileDelayed == null || _refreshProfileDelayed.IsCompleted)
            {
                _refreshProfileDelayed = new Task(() =>
                {
                    Task.Delay(TimeSpan.FromSeconds(profileRefreshDelaySeconds));
                    EDDI.Instance.refreshProfile();
                });
                _refreshProfileDelayed.Start();
            }
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

        public class JumpDetail
        {
            [PublicAPI]
            public decimal distance { get; private set; }

            [PublicAPI]
            public int jumps { get; private set; }

            public JumpDetail() { }

            public JumpDetail(decimal distance, int jumps)
            {
                this.distance = distance;
                this.jumps = jumps;
            }
        }
    }
}

