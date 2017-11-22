﻿using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipMonitor : EDDIMonitor
    {
        private static List<string> HARDPOINT_SIZES = new List<string>() { "Huge", "Large", "Medium", "Small", "Tiny" };

        // Observable collection for us to handle changes
        public ObservableCollection<Ship> shipyard = new ObservableCollection<Ship>();
        // The ID of the current ship; can be null
        private int? currentShipId;
        private int? currentProfileId;
        private static readonly object shipyardLock = new object();

        public string MonitorName()
        {
            return "Ship monitor";
        }

        public string MonitorLocalName()
        {
            return I18N.GetString("ship_monitor_name");
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return I18N.GetString("ship_monitor_desc");
        }

        public bool IsRequired()
        {
            return true;
        }

        public ShipMonitor()
        {
            readShips();
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
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
            else if (@event is CargoInventoryEvent)
            {
                handleCargoInventoryEvent((CargoInventoryEvent)@event);
            }
            else if (@event is LimpetPurchasedEvent)
            {
                handleLimpetPurchasedEvent((LimpetPurchasedEvent)@event);
            }
            else if (@event is LimpetSoldEvent)
            {
                handleLimpetSoldEvent((LimpetSoldEvent)@event);
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

            // TODO ModulesSwappedEvent

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
            if (!inFighterOrBuggy(@event.ship))
            {
                SetCurrentShip(@event.shipid, @event.ship);
                Ship ship = GetCurrentShip();
                if (ship == null)
                {
                    // We don't know of this ship so need to create it
                    ship = ShipDefinitions.FromEDModel(@event.ship);
                    ship.LocalId = (int)@event.shipid;
                    ship.role = Role.MultiPurpose;
                    AddShip(ship);
                }
                setShipName(ship, @event.shipname);
                setShipIdent(ship, @event.shipident);
                if (@event.fuelcapacity.HasValue)
                {
                    ship.fueltanktotalcapacity = (decimal?)@event.fuelcapacity;
                }
                writeShips();
            }
        }

        private void handleShipPurchasedEvent(ShipPurchasedEvent @event)
        {
            // We don't have a ship ID for the new ship at this point so just handle what we did with our old ship
            if (@event.storedshipid != null)
            {
                // We stored a ship - set its location to the current location
                Ship storedShip = GetShip(@event.storedshipid);
                if (storedShip != null)
                {
                    // Set location of stored ship to the current sstem
                    storedShip.starsystem = EDDI.Instance?.CurrentStarSystem?.name;
                    storedShip.station = EDDI.Instance?.CurrentStation?.name;
                }
            }
            else if (@event.soldshipid != null)
            {
                // We sold a ship - remove it
                RemoveShip(@event.soldshipid);
            }
        }

        private void handleShipDeliveredEvent(ShipDeliveredEvent @event)
        {
            // Set this is our current ship
            SetCurrentShip(@event.shipid, @event.ship);
        }

        private void handleShipSwappedEvent(ShipSwappedEvent @event)
        {
            // Update our current ship
            SetCurrentShip(@event.shipid, @event.ship);

            if (@event.storedshipid != null)
            {
                // We stored a ship - set its location to the current location
                Ship storedShip = GetShip(@event.storedshipid);
                if (storedShip != null)
                {
                    // Set location of stored ship to the current sstem
                    storedShip.starsystem = EDDI.Instance?.CurrentStarSystem?.name;
                    storedShip.station = EDDI.Instance?.CurrentStation?.name;
                }
            }
            else if (@event.soldshipid != null)
            {
                // We sold a ship - remove it
                RemoveShip(@event.soldshipid);
            }

            writeShips();
        }

        private void handleShipRenamedEvent(ShipRenamedEvent @event)
        {
            Ship ship = GetShip(@event.shipid);
            if (ship != null)
            {
                setShipName(ship, @event.name);
                setShipIdent(ship, @event.ident);
            }
            writeShips();
        }

        private void handleShipSoldEvent(ShipSoldEvent @event)
        {
            RemoveShip(@event.shipid);

            writeShips();
        }

        private void handleShipSoldOnRebuyEvent(ShipSoldOnRebuyEvent @event)
        {
            RemoveShip(@event.shipid);

            writeShips();
        }

        private void handleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            // Obtain the ship to which this loadout refers
            Logging.Debug("Current Ship Id is: " + currentShipId + ", Loadout Ship Id is " + @event.shipid);
            Ship ship = GetShip(@event.shipid);

            if (ship == null)
            {
                // The ship is unknown - create it
                Logging.Debug("Unknown ship ID " + @event.shipid);
                ship = ShipDefinitions.FromEDModel(@event.ship);
                ship.LocalId = (int)@event.shipid;
                ship.role = Role.MultiPurpose;
            }

            // Update name, ident & paintjob if required
            setShipName(ship, @event.shipname);
            setShipIdent(ship, @event.shipident);
            ship.paintjob = @event.paintjob;

            // Set the standard modules
            Compartment compartment = @event.compartments.FirstOrDefault(c => c.name == "Armour");
            if (compartment != null)
            {
                ship.bulkheads = compartment.module;
                // We take ship overall health from here
                ship.health = compartment.module.health;
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
            ship.compartments = @event.compartments.Where(c => c.name.StartsWith("Slot") || c.name.StartsWith("Military")).ToList();

            // Hardpoints
            ship.hardpoints = @event.hardpoints;

            // total fuel tank capacity
            ship.fueltanktotalcapacity = ship.fueltankcapacity + (int)ship.compartments.Where(c => c.module != null && c.module.name.EndsWith("Fuel Tank")).Sum(c => Math.Pow(2, c.module.@class));

            // Cargo capacity
            ship.cargocapacity = (int)ship.compartments.Where(c => c.module != null && c.module.name.EndsWith("Cargo Rack")).Sum(c => Math.Pow(2, c.module.@class));

            // Update the global variable
            EDDI.Instance.CurrentShip = ship;

            AddShip(ship);
            writeShips();
        }

        private void handleShipRebootedEvent(ShipRebootedEvent @event)
        {
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
            AddModule((int)@event.shipid, @event.slot, @event.buymodule);
            writeShips();
        }

        private void handleModuleRetrievedEvent(ModuleRetrievedEvent @event)
        {
            AddModule((int)@event.shipid, @event.slot, @event.module);
            writeShips();
        }

        private void handleModuleSoldEvent(ModuleSoldEvent @event)
        {
            RemoveModule((int)@event.shipid, @event.slot);
            writeShips();
        }

        private void handleModuleSoldFromStorageEvent(ModuleSoldFromStorageEvent @event)
        {
            // We don't do anything here as the ship object is unaffected
        }

        private void handleModuleStoredEvent(ModuleStoredEvent @event)
        {
            RemoveModule((int)@event.shipid, @event.slot, @event.replacementmodule);
            writeShips();
        }

        private void handleModulesStoredEvent(ModulesStoredEvent @event)
        {
            foreach (string slot in @event.slots)
                RemoveModule((int)@event.shipid, slot);
            writeShips();
        }

        private void handleModuleSwappedEvent(ModuleSwappedEvent @event)
        {
            Ship ship = GetShip(@event.shipid);

            string fromSlot = @event.fromslot;
            string toSlot = @event.toslot;

            // Module is a hardpoint
            if (fromSlot.Contains("Hardpoint"))
            {
                // Build new dictionary of ship hardpoints, excepting the swapped hardpoints
                // Save ship hardpoints which match the 'From' and 'To' slots
                Dictionary<string, Hardpoint> hardpoints = new Dictionary<string, Hardpoint>();

                foreach (Hardpoint hpt in ship.hardpoints)
                {
                    if (hpt.name == fromSlot)
                        hpt.name = toSlot;
                    if (hpt.name == toSlot)
                        hpt.name = fromSlot;

                    hardpoints.Add(hpt.name, hpt);
                }

                // Clear ship hardpoints and repopulate in correct order
                ship.hardpoints.Clear();
                foreach (string size in HARDPOINT_SIZES)
                {
                    for (int i = 1; i < 12; i++)
                    {
                        Hardpoint hpt;
                        hardpoints.TryGetValue(size + "Hardpoint" + i, out hpt);
                        if (hpt != null)
                            ship.hardpoints.Add(hpt);
                    }
                }
            }

            //Module is a compartment
            else
            {
                // Build new dictionary of ship compartments, excepting the swapped compartments
                // Save ship compartments which match the 'From' and 'To' slots
                Dictionary<string, Compartment> compartments = new Dictionary<string, Compartment>();

                foreach (Compartment cpt in ship.compartments)
                {
                    if (cpt.name == fromSlot)
                        cpt.name = toSlot;
                    if (cpt.name == toSlot)
                        cpt.name = fromSlot;

                    compartments.Add(cpt.name, cpt);
                }

                // Clear ship compartments and repopulate in correct order
                ship.compartments.Clear();
                for (int i = 1; i <= 12; i++)
                    for (int j = 1; j <= 8; j++)
                    {
                        Compartment cpt;
                        compartments.TryGetValue("Slot" + i.ToString("00") + "_Size" + j, out cpt);
                        if (cpt != null)
                            ship.compartments.Add(cpt);
                    }

                for (int i = 1; i <= 3; i++)
                {
                    Compartment cpt;
                    compartments.TryGetValue("Military" + i.ToString("00"), out cpt);
                    if (cpt != null)
                        ship.compartments.Add(cpt);
                }
            }
            writeShips();
        }

        private void handleModuleTransferEvent(ModuleTransferEvent @event)
        {
            // We don't do anything here as the ship object is unaffected
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
            /// The ship may have engineering data, request a profile refresh from the Frontier API a minute after switching
            refreshProfileDelayed(@event.shipid, currentProfileId);
        }

        private void handleCargoInventoryEvent(CargoInventoryEvent @event)
        {
            Ship ship = GetCurrentShip();
            ship.cargo = @event.inventory;
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo limpets = GetCurrentShip().cargo.Find(c => c.commodity.name == "Limpet");
            if (limpets == null)
            {
                // No limpets so create an entry
                limpets = new Cargo();
                limpets.commodity = CommodityDefinitions.FromName("Limpet");
                limpets.price = @event.price;
                limpets.amount = 0;
                GetCurrentShip().cargo.Add(limpets);
            }
            limpets.amount += @event.amount;
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Cargo limpets = GetCurrentShip().cargo.Find(c => c.commodity.name == "Limpet");
            if (limpets != null)
            {
                limpets.amount -= @event.amount;
                if (limpets.amount < 0) limpets.amount = 0;
            }
        }

        public void HandleProfile(JObject profile)
        {
            // Obtain the current ship from the profile
            Ship profileCurrentShip = FrontierApi.ShipFromJson((JObject)profile["ship"]);

            // Obtain the shipyard from the profile
            List<Ship> profileShipyard = FrontierApi.ShipyardFromJson(profileCurrentShip, profile);

            if (profileCurrentShip != null)
            {
                currentProfileId = profileCurrentShip.LocalId;
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
                    else
                    {
                        ship.raw = profileCurrentShip.raw;
                        ship.launchbays = profileCurrentShip.launchbays;
                        ship.paintjob = profileCurrentShip.paintjob;

                        if (ship.cargohatch != null)
                        {
                            // Engineering info for each module isn't in the journal, but we only use this to pass on to Coriolis so don't
                            // need to splice it in to our model.  We do, however, have cargo hatch information from the journal that we
                            // want to make avaialable to Coriolis so need to parse the raw data and add cargo hatch info as appropriate
                            JObject cargoHatchModule = new JObject
                        {
                            { "on", ship.cargohatch.enabled },
                            { "priority", ship.cargohatch.priority },
                            { "value", ship.cargohatch.price },
                            { "health", ship.cargohatch.health },
                            { "name", "ModularCargoBayDoor" }
                        };
                            JObject cargoHatchSlot = new JObject
                        {
                            { "module", cargoHatchModule }
                        };
                            JObject parsedRaw = JObject.Parse(profileCurrentShip.raw);
                            parsedRaw["modules"]["CargoHatch"] = cargoHatchSlot;
                            ship.raw = parsedRaw.ToString(Formatting.None);
                        }
                    }
                    Logging.Debug("Ship is: " + JsonConvert.SerializeObject(ship));
                }
            }

            // Prune ships from the Shipyard that are not found in the Profile Shipyard 
            foreach (Ship ship in shipyard)
            {
                Ship profileShip = profileShipyard.FirstOrDefault(s => s.LocalId == ship.LocalId);
                if (profileShip == null)
                    RemoveShip(ship.LocalId);
            }

            // Add ships from the Profile Shipyard that are not found in the Shipyard 
            // Update name, ident and value of ships in the Shipyard 
            foreach (Ship profileShip in profileShipyard)
            {
                Ship ship = GetShip(profileShip.LocalId);
                if (ship == null)
                {
                    // This is a new ship, add it to the shipyard
                    ship = profileShip;
                    AddShip(ship);
                }
            }
            writeShips();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["ship"] = GetCurrentShip(),
                ["shipyard"] = new List<Ship>(shipyard)
            };
            return variables;
        }

        public void writeShips()
        {
            lock (shipyardLock)
            {
                // Write ship configuration with current inventory
                ShipMonitorConfiguration configuration = new ShipMonitorConfiguration()
                {
                    currentshipid = currentShipId,
                    shipyard = shipyard
                };
                configuration.ToFile();
            }
        }

        private void readShips()
        {
            lock (shipyardLock)
            {
                // Obtain current inventory from configuration
                ShipMonitorConfiguration configuration = ShipMonitorConfiguration.FromFile();

                // Build a new shipyard
                List<Ship> newShipyard = new List<Ship>();
                foreach (Ship ship in configuration.shipyard)
                {
                    newShipyard.Add(ship);
                }

                // Now order the list by model
                newShipyard = newShipyard.OrderBy(s => s.model).ToList();

                // Update the shipyard
                shipyard.Clear();
                foreach (Ship ship in newShipyard)
                {
                    AddShip(ship);
                }

                currentShipId = configuration.currentshipid;
            }
        }

        private void AddShip(Ship ship)
        {
            if (ship == null)
            {
                return;
            }

            // Ensure that we have a role for this ship

            if (ship.role == null)
            {
                ship.role = Role.MultiPurpose;
            }

            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _AddShip(ship);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _AddShip(ship);
                }));
            }
        }

        private void _AddShip(Ship ship)
        {
            lock (shipyardLock)
            {
                if (ship == null)
                {
                    return;
                }
                // Remove the ship first (just in case we are trying to add a ship that already exists)
                RemoveShip(ship.LocalId);
                shipyard.Add(ship);
            }
        }

        /// <summary>
        /// Remove a ship from the shipyard
        /// </summary>
        private void RemoveShip(int? localid)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _RemoveShip(localid);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _RemoveShip(localid);
                }));
            }
        }

        /// <summary>
        /// Remove a ship from the shipyard
        /// </summary>
        private void _RemoveShip(int? localid)
        {
            lock (shipyardLock)
            {
                if (localid.HasValue)
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
        }

        /// <summary>
        /// Obtain the current ship
        /// </summary>
        public Ship GetCurrentShip()
        {
            EDDI.Instance.CurrentShip = GetShip(currentShipId);
            return GetShip(currentShipId);
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
            return shipyard.FirstOrDefault(s => s.LocalId == localId);
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
                        ship.role = Role.MultiPurpose;
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

        private void AddModule(int shipid, string slot, Module module)
        {
            Ship ship = GetShip(shipid);

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
                ship.paintjob = module.EDName;
            else if (slot.Contains("Hardpoint"))
            {
                // This is a hardpoint
                Hardpoint hardpoint = new Hardpoint() { name = slot };
                hardpoint.module = module;

                if (hardpoint.name.StartsWith("Tiny"))
                    hardpoint.size = 0;
                else if (hardpoint.name.StartsWith("Small"))
                    hardpoint.size = 1;
                else if (hardpoint.name.StartsWith("Medium"))
                    hardpoint.size = 2;
                else if (hardpoint.name.StartsWith("Large"))
                    hardpoint.size = 3;
                else if (hardpoint.name.StartsWith("Huge"))
                    hardpoint.size = 4;

                // Build new dictionary of ship hardpoints, excepting sold/stored hardpoint
                Dictionary<string, Hardpoint> hardpoints = new Dictionary<string, Hardpoint>();
                foreach (Hardpoint hp in ship.hardpoints)
                {
                    if (hp.name != slot)
                        hardpoints.Add(hp.name, hp);
                }
                hardpoints.Add(hardpoint.name, hardpoint);

                // Clear ship hardpoints and repopulate in correct order
                ship.hardpoints.Clear();
                foreach (string size in HARDPOINT_SIZES)
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        Hardpoint hp;
                        hardpoints.TryGetValue(size + "Hardpoint" + i, out hp);
                        if (hp != null)
                            ship.hardpoints.Add(hp);
                    }
                }
            }
            else if (slot.Contains("Slot") || slot.Contains("Military"))
            {
                // This is a compartment
                Compartment compartment = new Compartment() { name = slot };
                compartment.module = module;

                // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"
                if (slot.Contains("Slot"))
                {
                    Match matches = Regex.Match(compartment.name, @"Size([0-9]+)");
                    if (matches.Success)
                        compartment.size = Int32.Parse(matches.Groups[1].Value);
                }
                else if(slot.Contains("Military"))
                    compartment.size = (int)ship.militarysize;

                // Build new dictionary of ship compartments, excepting sold/stored compartment
                Dictionary<string, Compartment> compartments = new Dictionary<string, Compartment>();
                foreach (Compartment cpt in ship.compartments)
                {
                    if (cpt.name != slot)
                        compartments.Add(cpt.name, cpt);
                }
                compartments.Add(compartment.name, compartment);

                // Clear ship compartments and repopulate in correct order
                ship.compartments.Clear();
                for (int i = 1; i <= 12; i++)
                    for (int j = 1; j <= 8; j++)
                    {
                        Compartment cpt;
                        compartments.TryGetValue("Slot" + i.ToString("00") + "_Size" + j, out cpt);
                        if (cpt != null)
                            ship.compartments.Add(cpt);
                    }

                for (int i = 1; i <= 3; i++)
                {
                    Compartment cpt;
                    compartments.TryGetValue("Military" + i.ToString("00"), out cpt);
                    if (cpt != null)
                        ship.compartments.Add(cpt);
                }
            }
        }

        private void RemoveModule(int shipid, string slot, Module replacement = null)
        {
            Ship ship = GetShip(shipid);

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
                    ship.paintjob = null;
                else if (slot.Contains("Hardpoint"))
                {
                    // Build new list of ship hardpoints, excepting sold/stored hardpoint
                    List<Hardpoint> hardpoints = new List<Hardpoint>();
                    foreach (Hardpoint hpt in ship.hardpoints)
                    {
                        if (hpt.name != slot)
                            hardpoints.Add(hpt);
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
                            compartments.Add(cpt);
                    }
                    ship.compartments = compartments;
                }
            }
        }

        /// <summary>
        /// See if we're in a fighter or a buggy
        /// </summary>
        private bool inFighterOrBuggy(string model)
        {
            return (model == "Empire_Fighter" || model == "Federation_Fighter" || model == "Independent_Fighter" || model == "TestBuggy");
        }

        static async void refreshProfileDelayed(int? shipId, int? profileId)
        {
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(20));
                EDDI.Instance.refreshProfile();
            } while (shipId != profileId);
            Logging.Debug("Current Ship Id is: " + shipId + ", Profile Ship Id is " + profileId);
        }
    }
}

