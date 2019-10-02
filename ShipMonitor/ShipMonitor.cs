using Eddi;
using EddiCargoMonitor;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiShipMonitor
{
    // Microsoft does not recommend disposing of Task members: <https://devblogs.microsoft.com/pfxteam/do-i-need-to-dispose-of-tasks/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ShipMonitor : EDDIMonitor
    {
        private static readonly List<string> HARDPOINT_SIZES = new List<string>() { "Huge", "Large", "Medium", "Small", "Tiny" };

        // Observable collection for us to handle changes
        public ObservableCollection<Ship> shipyard { get; private set; }

        // List of stored modules from 'Stored modules' event
        public List<StoredModule> storedmodules { get; private set; }

        // The ID of the current ship; can be null
        private int? currentShipId;

        private const int profileRefreshDelaySeconds = 20;

        private static readonly object shipyardLock = new object();
        public event EventHandler ShipyardUpdatedEvent;
        private DateTime updatedAt;

        public string MonitorName()
        {
            return Properties.ShipMonitor.ResourceManager.GetString("name", CultureInfo.InvariantCulture);
        }

        public string LocalizedMonitorName()
        {
            return Properties.ShipMonitor.name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
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
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
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
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(shipyard, shipyardLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(shipyard); });
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
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is CommanderContinuedEvent)
            {
                handleCommanderContinuedEvent((CommanderContinuedEvent)@event);
            }
            else if (@event is ShipPurchasedEvent)
            {
                handleShipPurchasedEvent((ShipPurchasedEvent)@event);
            }
            else if (@event is ShipDeliveredEvent)
            {
                handleShipDeliveredEvent((ShipDeliveredEvent)@event);
            }
            else if (@event is ShipSwappedEvent)
            {
                handleShipSwappedEvent((ShipSwappedEvent)@event);
            }
            else if (@event is ShipRenamedEvent)
            {
                handleShipRenamedEvent((ShipRenamedEvent)@event);
            }
            else if (@event is ShipSoldEvent)
            {
                handleShipSoldEvent((ShipSoldEvent)@event);
            }
            else if (@event is ShipSoldOnRebuyEvent)
            {
                handleShipSoldOnRebuyEvent((ShipSoldOnRebuyEvent)@event);
            }
            else if (@event is ShipLoadoutEvent)
            {
                handleShipLoadoutEvent((ShipLoadoutEvent)@event);
            }
            else if (@event is StoredShipsEvent)
            {
                handleStoredShipsEvent((StoredShipsEvent)@event);
            }
            else if (@event is ShipRebootedEvent)
            {
                handleShipRebootedEvent((ShipRebootedEvent)@event);
            }
            else if (@event is ShipRefuelledEvent)
            {
                handleShipRefuelledEvent((ShipRefuelledEvent)@event);
            }
            else if (@event is ShipAfmuRepairedEvent)
            {
                handleShipAFMURepairedEvent((ShipAfmuRepairedEvent)@event);
            }
            else if (@event is ShipRepairedEvent)
            {
                handleShipRepairedEvent((ShipRepairedEvent)@event);
            }
            else if (@event is ShipRepairDroneEvent)
            {
                handleShipRepairDroneEvent((ShipRepairDroneEvent)@event);
            }
            else if (@event is ShipRepurchasedEvent)
            {
                handleShipRepurchasedEvent((ShipRepurchasedEvent)@event);
            }
            else if (@event is ShipRestockedEvent)
            {
                handleShipRestockedEvent((ShipRestockedEvent)@event);
            }
            else if (@event is ModulePurchasedEvent)
            {
                handleModulePurchasedEvent((ModulePurchasedEvent)@event);
            }
            else if (@event is ModuleRetrievedEvent)
            {
                handleModuleRetrievedEvent((ModuleRetrievedEvent)@event);
            }
            else if (@event is ModuleSoldEvent)
            {
                handleModuleSoldEvent((ModuleSoldEvent)@event);
            }
            else if (@event is ModuleSoldFromStorageEvent)
            {
                handleModuleSoldFromStorageEvent((ModuleSoldFromStorageEvent)@event);
            }
            else if (@event is ModuleStoredEvent)
            {
                handleModuleStoredEvent((ModuleStoredEvent)@event);
            }
            else if (@event is ModulesStoredEvent)
            {
                handleModulesStoredEvent((ModulesStoredEvent)@event);
            }
            else if (@event is ModuleSwappedEvent)
            {
                handleModuleSwappedEvent((ModuleSwappedEvent)@event);
            }
            else if (@event is ModuleTransferEvent)
            {
                handleModuleTransferEvent((ModuleTransferEvent)@event);
            }
            else if (@event is ModuleInfoEvent)
            {
                handleModuleInfoEvent((ModuleInfoEvent)@event);
            }
            else if (@event is StoredModulesEvent)
            {
                handleStoredModulesEvent((StoredModulesEvent)@event);
            }
            else if (@event is BountyIncurredEvent)
            {
                handleBountyIncurredEvent((BountyIncurredEvent)@event);
            }
            else if (@event is BountyPaidEvent)
            {
                handleBountyPaidEvent((BountyPaidEvent)@event);
            }
        }

        // Set the ship name conditionally, avoiding filtered names
        private void setShipName(Ship ship, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ship.name = null;
            }
            else if (name != null && !name.Contains("***"))
            {
                ship.name = name;
            }
        }

        // Set the ship ident conditionally, avoiding filtered idents
        private void setShipIdent(Ship ship, string ident)
        {
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
                if (!inFighter(@event.ship) && !inBuggy(@event.ship))
                {
                    SetCurrentShip(@event.shipid, @event.ship);
                    Ship ship = GetCurrentShip();
                    if (ship == null)
                    {
                        // We don't know of this ship so need to create it
                        ship = ShipDefinitions.FromEDModel(@event.ship);
                        ship.LocalId = (int)@event.shipid;
                        ship.Role = Role.MultiPurpose;
                        AddShip(ship);
                    }
                    setShipName(ship, @event.shipname);
                    setShipIdent(ship, @event.shipident);
                    if (@event.fuelcapacity.HasValue)
                    {
                        ship.fueltanktotalcapacity = @event.fuelcapacity;
                    }
                    if (!@event.fromLoad) { writeShips(); }
                }
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
                    Ship storedShip = GetShip(@event.storedshipid);
                    if (storedShip != null)
                    {
                        // Set location of stored ship to the current system
                        storedShip.starsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                        storedShip.station = EDDI.Instance?.CurrentStation?.name;
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
                SetCurrentShip(@event.shipid, @event.ship);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleShipSwappedEvent(ShipSwappedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;

                // Save swapped ship size for minor faction station update
                LandingPadSize swappedShipSize = GetCurrentShip()?.size;

                // Update our current ship
                SetCurrentShip(@event.shipid, @event.ship);

                if (@event.storedshipid != null)
                {
                    // We stored a ship - set its location to the current location
                    Ship storedShip = GetShip(@event.storedshipid);
                    if (storedShip != null)
                    {
                        // Set location of stored ship to the current sstem
                        storedShip.starsystem = EDDI.Instance?.CurrentStarSystem?.systemname;
                        storedShip.station = EDDI.Instance?.CurrentStation?.name;
                    }
                }
                else if (@event.soldshipid != null)
                {
                    // We sold a ship - remove it
                    RemoveShip(@event.soldshipid);
                }
                if (!@event.fromLoad) { writeShips(); }

                // Update stations in minor faction records
                if (swappedShipSize != LandingPadSize.Large && swappedShipSize != GetCurrentShip()?.size)
                {
                    ((CrimeMonitor)EDDI.Instance.ObtainMonitor("Crime monitor"))?.UpdateStations();
                }
            }
        }

        private void handleShipRenamedEvent(ShipRenamedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid);
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

        private void handleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (!inFighter(@event.ship) && !inBuggy(@event.ship))
                {
                    Ship ship = ParseShipLoadoutEvent(@event);

                    // Update the global variable
                    EDDI.Instance.CurrentShip = ship;

                    AddShip(ship);
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        private Ship ParseShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            // Obtain the ship to which this loadout refers
            Logging.Debug("Current Ship Id is: " + currentShipId + ", Loadout Ship Id is " + @event.shipid);
            Ship ship = GetShip(@event.shipid);

            if (ship == null)
            {
                // The ship is unknown - create it
                Logging.Debug("Unknown ship ID " + @event.shipid);
                ship = @event.shipDefinition;
                ship.LocalId = (int)@event.shipid;
                ship.Role = Role.MultiPurpose;
            }

            // Save a copy of the raw event so that we can send it to other 3rd party apps
            ship.raw = @event.raw;

            // Update model (in case it was solely from the edname), name, ident & paintjob if required
            ship.model = @event.ship;
            setShipName(ship, @event.shipname);
            setShipIdent(ship, @event.shipident);
            ship.paintjob = @event.paintjob;
            ship.hot = @event.hot;

            // Write ship value, if given by the loadout event
            if (@event.value != null)
            {
                ship.value = (long)@event.value;
            }
            ship.rebuy = @event.rebuy;
            ship.unladenmass = @event.unladenmass;
            ship.maxjumprange = @event.maxjumprange;

            // Set the standard modules
            Compartment compartment = @event.compartments.FirstOrDefault(c => c.name == "Armour");
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
                ship.optimalmass = @event.optimalmass;
                ship.maxfuelperjump = MaxFuelPerJump(ship);
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
            if (ship.fueltank != null)
            {
                ship.fueltankcapacity = (decimal)Math.Pow(2, ship.fueltank.@class);
            }

            compartment = @event.compartments.FirstOrDefault(c => c.name == "CargoHatch");
            if (compartment != null)
            {
                ship.cargohatch = compartment.module;
            }

            // Internal + restricted modules
            List<Compartment> compartments = new List<Compartment>();
            foreach (Compartment cpt in @event.compartments.Where(c => c.name.StartsWith("Slot") || c.name.StartsWith("Military")).ToList())
            {
                compartments.Add(cpt);
            }
            ship.compartments = compartments;

            // Hardpoints
            List<Hardpoint> hardpoints = new List<Hardpoint>();
            foreach (Hardpoint hpt in @event.hardpoints)
            {
                hardpoints.Add(hpt);
            }
            ship.hardpoints = hardpoints;

            // total fuel tank capacity
            ship.fueltanktotalcapacity = ship.fueltankcapacity + (int)ship.compartments.Where(c => c.module != null && c.module.basename.Equals("FuelTank")).Sum(c => Math.Pow(2, c.module.@class));

            // Cargo capacity
            ship.cargocapacity = (int)ship.compartments.Where(c => c.module != null && c.module.basename.Contains("CargoRack")).Sum(c => Math.Pow(2, c.module.@class));
            return ship;
        }

        private void handleStoredShipsEvent(StoredShipsEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (@event.shipyard != null)
                {
                    //Check for ships missing from the shipyard
                    foreach (Ship shipInEvent in @event.shipyard)
                    {
                        Ship shipInYard = GetShip(shipInEvent.LocalId);

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
                            shipInYard.starsystem = shipInEvent.starsystem;
                            shipInYard.marketid = shipInEvent.marketid;
                            shipInYard.station = shipInEvent.station;
                            shipInYard.transferprice = shipInEvent.transferprice;
                            shipInYard.transfertime = shipInEvent.transfertime;
                        }
                    }

                    // Prune ships no longer in the shipyard
                    List<int> idsToRemove = new List<int>(shipyard.Count);
                    foreach (Ship shipInYard in shipyard)
                    {
                        // Ignore current ship, since (obviously) it's not stored
                        if (shipInYard.LocalId == currentShipId) { continue; }

                        Ship shipInEvent = @event.shipyard.FirstOrDefault(s => s.LocalId == shipInYard.LocalId);
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

        private void handleStoredModulesEvent(StoredModulesEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                if (@event.storedmodules != null)
                {
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
                Ship ship = GetCurrentShip();
                if (ship == null)
                {
                    return;
                }
                foreach (string modulename in @event.modules)
                {
                    // Find the matching module and set health to 1%
                    if (modulename == "ShipCockpit" && ship.canopy != null)
                    {
                        ship.canopy.health = 1;
                    }
                    else if (modulename == "PowerPlant" && ship.powerplant != null)
                    {
                        ship.powerplant.health = 1;
                    }
                    else if (modulename == "MainEngines" && ship.thrusters != null)
                    {
                        ship.thrusters.health = 1;
                    }
                    else if (modulename == "PowerDistributor" && ship.powerdistributor != null)
                    {
                        ship.powerdistributor.health = 1;
                    }
                    else if (modulename == "FrameShiftDrive" && ship.frameshiftdrive != null)
                    {
                        ship.frameshiftdrive.health = 1;
                    }
                    else if (modulename == "LifeSupport" && ship.lifesupport != null)
                    {
                        ship.lifesupport.health = 1;
                    }
                    else if (modulename == "Radar" && ship.sensors != null)
                    {
                        ship.sensors.health = 1;
                    }
                    else if (modulename == "CargoHatch" && ship.cargohatch != null)
                    {
                        ship.cargohatch.health = 1;
                    }
                    else if (modulename == "DataLinkScanner" && ship.datalinkscanner != null)
                    {
                        ship.datalinkscanner.health = 1;
                    }
                    else if (modulename.Contains("Hardpoint"))
                    {
                        foreach (Hardpoint hardpoint in ship.hardpoints)
                        {
                            if (hardpoint.name == modulename && hardpoint.module != null)
                            {
                                hardpoint.module.health = 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (Compartment compartment in ship.compartments)
                        {
                            if (compartment.name == modulename && compartment.module != null)
                            {
                                compartment.module.health = 1;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void handleShipAFMURepairedEvent(ShipAfmuRepairedEvent @event)
        {
            // This doesn't give us enough information at present to do anything useful
        }

        private void handleShipRepairedEvent(ShipRepairedEvent @event)
        {
            // This doesn't give us enough information at present to do anything useful
        }

        private void handleShipRepairDroneEvent(ShipRepairDroneEvent @event)
        {
            // This doesn't give us enough information at present to do anything useful
        }

        private void handleShipRefuelledEvent(ShipRefuelledEvent @event)
        {
            // We do not keep track of current fuel level so nothing to do here
        }

        private void handleShipRestockedEvent(ShipRestockedEvent @event)
        {
            // TODO
        }

        private void handleShipRepurchasedEvent(ShipRepurchasedEvent @event)
        {
            // We don't do anything here as this is followed by a full ship loadout event
        }

        private void handleModulePurchasedEvent(ModulePurchasedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? (int)@event.shipid : ship.LocalId;
                AddModule(ship, @event.slot, @event.buymodule);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleRetrievedEvent(ModuleRetrievedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? (int)@event.shipid : ship.LocalId;
                AddModule(ship, @event.slot, @event.module);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleSoldEvent(ModuleSoldEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? (int)@event.shipid : ship.LocalId;
                RemoveModule(ship, @event.slot);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleSoldFromStorageEvent(ModuleSoldFromStorageEvent @event)
        {
            // We don't do anything here as the ship object is unaffected
        }

        private void handleModuleStoredEvent(ModuleStoredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? (int)@event.shipid : ship.LocalId;
                RemoveModule(ship, @event.slot, @event.replacementmodule);
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModulesStoredEvent(ModulesStoredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid) ?? @event.shipDefinition;
                ship.LocalId = ship.LocalId == 0 ? (int)@event.shipid : ship.LocalId;
                foreach (string slot in @event.slots)
                {
                    RemoveModule(ship, slot);
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleSwappedEvent(ModuleSwappedEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid);

                string fromSlot = @event.fromslot;
                string toSlot = @event.toslot;

                if (fromSlot.Contains("Hardpoint")) // Module is a hardpoint
                {
                    // Find our hardpoints. Add them if them are missing
                    Hardpoint fromHardpoint = ship.hardpoints.FirstOrDefault(h => h.name == fromSlot);
                    if (fromHardpoint is null)
                    {
                        fromHardpoint = new Hardpoint() { name = fromSlot };
                        fromHardpoint.size = getHardpointSize(fromSlot);
                        ship.hardpoints.Add(fromHardpoint);
                    }
                    Hardpoint toHardpoint = ship.hardpoints.FirstOrDefault(h => h.name == toSlot);
                    if (toHardpoint is null)
                    {
                        toHardpoint = new Hardpoint() { name = toSlot };
                        toHardpoint.size = getHardpointSize(toSlot);
                        ship.hardpoints.Add(toHardpoint);
                    }
                    sortHardpoints(ship);

                    // Swap the modules
                    Module fromModule = fromHardpoint?.module;
                    Module toModule = toHardpoint?.module;
                    fromHardpoint.module = toModule;
                    toHardpoint.module = fromModule;
                }
                else //Module is a compartment
                {
                    // Find our compartments. Add them if them are missing
                    Compartment fromCompartment = ship.compartments.FirstOrDefault(c => c.name == fromSlot);
                    if (fromCompartment is null)
                    {
                        fromCompartment = new Compartment() { name = fromSlot, size = getHardpointSize(fromSlot) };
                        ship.compartments.Add(fromCompartment);
                    }
                    Compartment toCompartment = ship.compartments.FirstOrDefault(c => c.name == toSlot);
                    if (toCompartment is null)
                    {
                        toCompartment = new Compartment() { name = toSlot, size = getHardpointSize(toSlot) };
                        ship.compartments.Add(toCompartment);
                    }
                    sortCompartments(ship);

                    // Swap the modules
                    Module fromModule = fromCompartment?.module;
                    Module toModule = toCompartment?.module;
                    fromCompartment.module = toModule;
                    toCompartment.module = fromModule;
                }
                if (!@event.fromLoad) { writeShips(); }
            }
        }

        private void handleModuleTransferEvent(ModuleTransferEvent @event)
        {
            // We don't do anything here as the ship object is unaffected
        }

        private void handleModuleInfoEvent(ModuleInfoEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetCurrentShip();
                if (ship != null)
                {
                    ModuleInfoReader info = ModuleInfoReader.FromFile();
                    if (info != null)
                    {
                        for (int i = 0; i < info.Modules.Count(); i++)
                        {
                            int position = i + 1;
                            int priority = info.Modules[i].priority + 1;
                            decimal power = info.Modules[i].power;

                            string slot = info.Modules[i].slot;
                            if (!String.IsNullOrEmpty(slot))
                            {
                                switch (slot)
                                {
                                    case "CargoHatch":
                                        {
                                            ship.cargohatch = ship.cargohatch ?? new Module();
                                            ship.cargohatch.position = position;
                                            ship.cargohatch.priority = priority;
                                            ship.cargohatch.power = power;
                                        }
                                        break;
                                    case "FrameShiftDrive":
                                        {
                                            ship.frameshiftdrive = ship.frameshiftdrive ?? new Module();
                                            ship.frameshiftdrive.position = position;
                                            ship.frameshiftdrive.priority = priority;
                                            ship.frameshiftdrive.power = power;
                                        }
                                        break;
                                    case "LifeSupport":
                                        {
                                            ship.lifesupport = ship.lifesupport ?? new Module();
                                            ship.lifesupport.position = position;
                                            ship.lifesupport.priority = priority;
                                            ship.lifesupport.power = power;
                                        }
                                        break;
                                    case "MainEngines":
                                        {
                                            ship.thrusters = ship.thrusters ?? new Module();
                                            ship.thrusters.position = position;
                                            ship.thrusters.priority = priority;
                                            ship.thrusters.power = power;
                                        }
                                        break;
                                    case "PowerDistributor":
                                        {
                                            ship.powerdistributor = ship.powerdistributor ?? new Module();
                                            ship.powerdistributor.position = position;
                                            ship.powerdistributor.priority = priority;
                                            ship.powerdistributor.power = power;
                                        }
                                        break;
                                    case "PowerPlant":
                                        {
                                            ship.powerplant = ship.powerplant ?? new Module();
                                            ship.powerplant.position = position;
                                            ship.powerplant.priority = priority;
                                            ship.powerplant.power = power;
                                        }
                                        break;
                                    case "Radar":
                                        {
                                            ship.sensors = ship.sensors ?? new Module();
                                            ship.sensors.position = position;
                                            ship.sensors.priority = priority;
                                            ship.sensors.power = power;
                                        }
                                        break;
                                    case "ShipCockpit":
                                        {
                                            ship.canopy = ship.canopy ?? new Module();
                                            ship.canopy.position = position;
                                            ship.canopy.priority = priority;
                                            ship.canopy.power = power;
                                        }
                                        break;
                                }

                                if (slot.Contains("Slot"))
                                {
                                    Compartment compartment = ship.compartments.FirstOrDefault(c => c.name == slot);
                                    if (compartment != null)
                                    {
                                        compartment.module = compartment.module ?? new Module();
                                        compartment.module.position = position;
                                        compartment.module.priority = priority;
                                        compartment.module.power = power;
                                    }
                                }
                                else if (slot.Contains("Hardpoint"))
                                {
                                    Hardpoint hardpoint = ship.hardpoints.FirstOrDefault(h => h.name == slot);
                                    if (hardpoint != null)
                                    {
                                        hardpoint.module = hardpoint.module ?? new Module();
                                        hardpoint.module.position = position;
                                        hardpoint.module.priority = priority;
                                        hardpoint.module.power = power;
                                    }
                                }
                            }
                        }
                    }
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        private void handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetCurrentShip();
                if (ship != null)
                {
                    ship.hot = true;
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        private void handleBountyPaidEvent(BountyPaidEvent @event)
        {
            if (@event.timestamp > updatedAt)
            {
                updatedAt = @event.timestamp;
                Ship ship = GetShip(@event.shipid);
                if (ship != null)
                {
                    ship.hot = false;
                    if (!@event.fromLoad) { writeShips(); }
                }
            }
        }

        public void PostHandle(Event @event)
        {
            if (@event is ShipLoadoutEvent)
            {
                posthandleShipLoadoutEvent((ShipLoadoutEvent)@event);
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
            Ship profileCurrentShip = FrontierApi.ShipFromJson((JObject)profile["ship"]);

            // Obtain the shipyard from the profile
            List<Ship> profileShipyard = FrontierApi.ShipyardFromJson(profileCurrentShip, profile);

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
                    Ship ship = GetShip(currentShipId);
                    if (ship == null)
                    {
                        // Information from the Frontier API can be out-of-date, use it to set our ship if we don't know what it already is
                        ship = profileCurrentShip;
                        AddShip(ship);
                    }
                    // Update launch bays and FSD optimum mass from profile
                    else
                    {
                        if (profileCurrentShip?.launchbays?.Any() ?? false)
                        {
                            ship.launchbays = profileCurrentShip.launchbays;
                        }
                        else
                        {
                            ship.launchbays.Clear();
                        }
                        ship.optimalmass = profileCurrentShip.optimalmass;
                    }
                    Logging.Debug("Ship is: " + JsonConvert.SerializeObject(ship));
                }
                else
                {
                    refreshProfileDelayed();
                }
            }

            // Prune ships from the Shipyard that are not found in the Profile Shipyard 
            List<int> idsToRemove = new List<int>(shipyard.Count);
            foreach (Ship ship in shipyard)
            {
                Ship profileShip = profileShipyard.FirstOrDefault(s => s.LocalId == ship.LocalId);
                if (profileShip == null)
                {
                    idsToRemove.Add(ship.LocalId);
                }
            }
            _RemoveShips(idsToRemove);

            // Add ships from the Profile Shipyard that are not found in the Shipyard 
            // Update name, ident and value of ships in the Shipyard 
            foreach (Ship profileShip in profileShipyard)
            {
                Ship ship = GetShip(profileShip.LocalId);
                if (ship == null)
                {
                    // This is a new ship, add it to the shipyard
                    AddShip(profileShip);
                }
            }

            writeShips();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["ship"] = GetCurrentShip(),
                ["storedmodules"] = new List<StoredModule>(storedmodules),
                ["shipyard"] = new List<Ship>(shipyard)
            };
            return variables;
        }

        private void writeShips()
        {
            lock (shipyardLock)
            {
                // Write ship configuration with current inventory
                ShipMonitorConfiguration configuration = new ShipMonitorConfiguration()
                {
                    currentshipid = currentShipId,
                    shipyard = shipyard,
                    storedmodules = storedmodules,
                    updatedat = updatedAt
                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(ShipyardUpdatedEvent, shipyard);
        }

        private void readShips()
        {
            lock (shipyardLock)
            {
                // Obtain current inventory from configuration
                ShipMonitorConfiguration configuration = ShipMonitorConfiguration.FromFile();
                updatedAt = configuration.updatedat;

                // Build a new shipyard
                List<Ship> newShiplist = configuration.shipyard.OrderBy(s => s.model).ToList();
                List<StoredModule> newModuleList = configuration.storedmodules.OrderBy(s => s.slot).ToList();

                // Update the shipyard
                shipyard = new ObservableCollection<Ship>(newShiplist);
                currentShipId = configuration.currentshipid;
                storedmodules = new List<StoredModule>(newModuleList);
            }
        }

        private void AddShip(Ship ship)
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
                for (int i = 0; i < shipyard.Count; i++)
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
        private void RemoveShip(int? localid)
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
                for (int i = 0; i < shipyard.Count; i++)
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
                for (int i = 0; i < shipyard.Count; i++)
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
            Ship currentShip = GetShip(currentShipId);
            EDDI.Instance.CurrentShip = currentShip;
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

        public void SetCurrentShip(int? localId, string model = null)
        {
            lock (shipyardLock)
            {
                // Ensure that this ID is present
                Ship ship = GetShip(localId);
                if (ship == null)
                {
                    // We don't know about this ship yet
                    Logging.Debug("Unknown ship ID " + localId);
                    if (localId.HasValue && model != null)
                    {
                        // We can make one though
                        ship = ShipDefinitions.FromEDModel(model);
                        ship.LocalId = (int)localId;
                        ship.Role = Role.MultiPurpose;
                        AddShip(ship);
                        currentShipId = ship.LocalId;
                        Logging.Debug("Created ship ID " + localId + ";  " + JsonConvert.SerializeObject(ship));
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
                    ship.starsystem = null;
                    ship.station = null;
                    EDDI.Instance.CurrentShip = ship;
                }
            }
        }

        private void AddModule(Ship ship, string slot, Module module)
        {
            if (ship != null && slot != null)
            {
                try
                {
                    switch (slot)
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
                                ship.fueltankcapacity = (decimal)Math.Pow(2, ship.fueltank.@class);
                            }
                            break;
                        case "CargoHatch":
                            ship.cargohatch = module;
                            break;
                    }

                    if (slot.Contains("PaintJob"))
                    {
                        ship.paintjob = module.EDName;
                    }
                    else if (slot.Contains("Hardpoint"))
                    {
                        // This is a hardpoint
                        Hardpoint hardpoint = ship.hardpoints.FirstOrDefault(h => h.name == slot);
                        if (hardpoint is null)
                        {
                            hardpoint = new Hardpoint() { name = slot };
                            hardpoint.size = getHardpointSize(slot);
                            ship.hardpoints.Add(hardpoint);
                        }
                        hardpoint.module = module;
                        sortHardpoints(ship);
                    }
                    else if (slot.Contains("Slot") || slot.Contains("Military"))
                    {
                        // This is a compartment
                        Compartment compartment = ship.compartments.FirstOrDefault(c => c.name == slot);
                        if (compartment is null)
                        {
                            compartment = new Compartment() { name = slot };
                            compartment.size = getCompartmentSize(slot, ship.militarysize);
                            ship.compartments.Add(compartment);
                        }
                        compartment.module = module;
                        sortCompartments(ship);
                    }
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "slot", slot },
                        { "module", module },
                        { "exception", ex },
                    };
                    Logging.Error("Failed to add module to ship.", data);
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
                // Build new dictionary of ship compartments, excepting sold/stored compartment
                Dictionary<string, Compartment> compartments = new Dictionary<string, Compartment>();
                foreach (Compartment cpt in ship.compartments)
                {
                    compartments.Add(cpt.name, cpt);
                }

                // Clear ship compartments and repopulate in correct order
                ship.compartments.Clear();
                for (int i = 1; i <= 12; i++)
                {
                    for (int j = 1; j <= 8; j++)
                    {
                        if (compartments.TryGetValue("Slot" + i.ToString("00") + "_Size" + j, out Compartment cpt))
                        {
                            ship.compartments.Add(cpt);
                        }
                    }
                }

                for (int i = 1; i <= 3; i++)
                {
                    if (compartments.TryGetValue("Military" + i.ToString("00"), out Compartment cpt))
                    {
                        ship.compartments.Add(cpt);
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "Exception", ex },
                    { "Ship compartments", ship.compartments}
                };
                Logging.Error("Failed to sort ship compartments", data);
            }
        }

        private static int getCompartmentSize(string slot, int? militarySlotSize)
        {
            // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"
            if ((bool)slot?.Contains("Slot"))
            {
                Match matches = Regex.Match(slot, @"Size([0-9]+)");
                if (matches.Success)
                {
                    return int.Parse(matches.Groups[1].Value);
                }
            }
            else if ((bool)slot?.Contains("Military") && militarySlotSize != null)
            {
                return (int)militarySlotSize;
            }
            // Compartment size could not be determined
            Logging.Error("Ship compartment slot size could not be determined for " + slot);
            return -1;
        }

        private static void sortHardpoints(Ship ship)
        {
            try
            {
                // Build new dictionary of ship hardpoints, excepting sold/stored hardpoint
                Dictionary<string, Hardpoint> hardpoints = new Dictionary<string, Hardpoint>();
                foreach (Hardpoint hp in ship.hardpoints)
                {
                    hardpoints.Add(hp.name, hp);
                }

                // Clear ship hardpoints and repopulate in correct order
                ship.hardpoints.Clear();
                foreach (string size in HARDPOINT_SIZES)
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        if (hardpoints.TryGetValue(size + "Hardpoint" + i, out Hardpoint hp))
                        {
                            ship.hardpoints.Add(hp);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "Exception", ex },
                    { "Ship", ship }
                };
                Logging.Error("Failed to sort ship hardpoints", data);
            }
        }

        private static int getHardpointSize(string slot)
        {
            if ((bool)slot?.StartsWith("Tiny"))
            {
                return 0;
            }
            else if ((bool)slot?.StartsWith("Small"))
            {
                return 1;
            }
            else if ((bool)slot?.StartsWith("Medium"))
            {
                return 2;
            }
            else if ((bool)slot?.StartsWith("Large"))
            {
                return 3;
            }
            else if ((bool)slot?.StartsWith("Huge"))
            {
                return 4;
            }
            // Hardpoint size could not be determined
            Logging.Error("Ship hardpoint slot size could not be determined for " + slot);
            return -1;
        }

        private void RemoveModule(Ship ship, string slot, Module replacement = null)
        {
            if (ship != null && slot != null)
            {
                try
                {
                    if (replacement != null)
                    {
                        switch (slot)
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
                                {
                                    ship.fueltank = replacement;
                                    ship.fueltankcapacity = (decimal)Math.Pow(2, ship.fueltank.@class);
                                }
                                break;
                            case "CargoHatch":
                                ship.cargohatch = replacement;
                                break;
                        }

                    }
                    else
                    {
                        if (slot.Contains("PaintJob"))
                        {
                            ship.paintjob = null;
                        }
                        else if (slot.Contains("Hardpoint"))
                        {
                            // Build new list of ship hardpoints, excepting sold/stored hardpoint
                            List<Hardpoint> hardpoints = new List<Hardpoint>();
                            foreach (Hardpoint hpt in ship.hardpoints)
                            {
                                if (hpt.name != slot)
                                {
                                    hardpoints.Add(hpt);
                                }
                            }
                            ship.hardpoints = hardpoints;
                        }
                        else if (slot.Contains("Slot") || slot.Contains("Military"))
                        {
                            // Build new list of ship compartments, excepting sold/stored compartment
                            List<Compartment> compartments = new List<Compartment>();
                            foreach (Compartment cpt in ship.compartments)
                            {
                                if (cpt.name != slot)
                                {
                                    compartments.Add(cpt);
                                }
                            }
                            ship.compartments = compartments;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>
                    {
                        { "slot", slot },
                        { "replacement", replacement },
                        { "exception", ex },
                    };
                    Logging.Error("Failed to remove module from ship.", data);
                    throw;
                }
            }
            else
            {
                Logging.Warn("Cannot remove the module. Ship ID " + ship?.LocalId + " or ship slot " + slot + " was not found.");
            }
        }

        public JumpDetail JumpDetails(string type)
        {
            Ship ship = GetCurrentShip();
            ship.maxfuelperjump = MaxFuelPerJump(ship);

            int cargoCarried = ((CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor")).cargoCarried;
            decimal? currentFuel = ((StatusMonitor)EDDI.Instance.ObtainMonitor("Status monitor")).currentStatus.fuelInTanks;
            decimal maxFuel = ship.fueltanktotalcapacity ?? 0;

            if (!string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "next":
                        {
                            decimal distance = JumpRange(ship, currentFuel ?? 0, cargoCarried);
                            return new JumpDetail(distance, 1);
                        }
                    case "max":
                        {
                            decimal distance = JumpRange(ship, ship.maxfuelperjump, cargoCarried);
                            return new JumpDetail(distance, 1);
                        }
                    case "total":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (currentFuel > 0)
                            {
                                total += JumpRange(ship, currentFuel ?? 0, cargoCarried);
                                jumps++;
                                currentFuel -= Math.Min(currentFuel ?? 0, ship.maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                    case "full":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (maxFuel > 0)
                            {
                                total += JumpRange(ship, maxFuel, cargoCarried);
                                jumps++;
                                maxFuel -= Math.Min(maxFuel, ship.maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                }
            }
            return null;
        }

        private decimal JumpRange(Ship ship, decimal currentFuel, int cargoCarried)
        {
            decimal boostConstant = 0;
            Module module = ship.compartments.FirstOrDefault(c => c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }

            Constants.ratingConstantFSD.TryGetValue(ship.frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(ship.frameshiftdrive.@class, out decimal powerConstant);
            decimal massRatio = ship.optimalmass / (ship.unladenmass + currentFuel + cargoCarried);
            decimal fuel = Math.Min(currentFuel, ship.maxfuelperjump);

            return ((decimal)Math.Pow((double)(1000 * fuel / ratingConstant), (double)(1 / powerConstant)) * massRatio) + boostConstant;
        }

        private decimal MaxFuelPerJump(Ship ship)
        {
            // Max fuel per jump calculated using unladen mass and max jump range w/ just enough fuel to complete max jump
            decimal boostConstant = 0;
            Module module = ship.compartments.FirstOrDefault(c => c?.module?.edname != null && c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }
            Constants.ratingConstantFSD.TryGetValue(ship.frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(ship.frameshiftdrive.@class, out decimal powerConstant);
            decimal maxJumpRange = ship.maxjumprange - boostConstant;
            decimal massRatio = (ship.unladenmass + ship.maxfuelperjump) / ship.optimalmass;

            return ratingConstant * (decimal)Math.Pow((double)(maxJumpRange * massRatio), (double)powerConstant) / 1000;
        }

        /// <summary> See if we're in a fighter </summary>
        private bool inFighter(string model)
        {
            return model.Contains("Fighter");
        }

        /// <summary> See if we're in a buggy / SRV </summary>
        private bool inBuggy(string model)
        {
            return model.Contains("Buggy");
        }

        private Task _refreshProfileDelayed;
        private void refreshProfileDelayed()
        {
            if (_refreshProfileDelayed == null || _refreshProfileDelayed.IsCompleted)
            {
                _refreshProfileDelayed = new Task(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(profileRefreshDelaySeconds));
                    EDDI.Instance.refreshProfile();
                });
                _refreshProfileDelayed.Start();
            }
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

        public class JumpDetail
        {
            public decimal distance { get; private set; }
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

