using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Ship> shipyard = new ObservableCollection<Ship>();
        // The ID of the current ship; can be null
        private int? currentShipId;

        public string MonitorName()
        {
            return "Ship monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Track information on your ships.";
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
            else if (@event is ShipRepairedEvent)
            {
                handleShipRepairedEvent((ShipRepairedEvent)@event);
            }
            else if (@event is ShipRepurchasedEvent)
            {
                handleShipRepurchasedEvent((ShipRepurchasedEvent)@event);
            }
            else if (@event is ShipRestockedEvent)
            {
                handleShipRestockedEvent((ShipRestockedEvent)@event);
            }
            // TODO ModulePurchasedEvent
            // TODO ModuleSoldEvent
            // TODO ModuleStoredEvent
            // TODO ModuleRetrievedEvent
            // TODO ModulesSwappedEvent
            // TODO ModulesStoredEvent
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            if (!inFighterOrBuggy(@event.ship))
            {
                SetCurrentShip(@event.shipid, @event.ship);
                Ship ship = GetCurrentShip();
                ship.name = @event.shipname;
                ship.ident = @event.shipident;
                if (@event.fuelcapacity.HasValue)
                {
                    ship.fueltanktotalcapacity = (decimal)@event.fuelcapacity;
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
                    storedShip.starsystem = EDDI.Instance.CurrentStarSystem.name;
                    storedShip.station = EDDI.Instance.CurrentStation.name;
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
                    storedShip.starsystem = EDDI.Instance.CurrentStarSystem.name;
                    storedShip.station = EDDI.Instance.CurrentStation.name;
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
                if (@event.name != null)
                {
                    ship.name = @event.name == "" ? null : @event.name;
                }
                if (@event.ident != null)
                {
                    ship.ident = @event.ident == "" ? null : @event.ident;
                }
            }
            writeShips();
        }

        private void handleShipSoldEvent(ShipSoldEvent @event)
        {
            RemoveShip(@event.shipid);

            writeShips();
        }

        private void handleShipLoadoutEvent(ShipLoadoutEvent @event)
        {
            // Obtain the ship to which this loadout refers
            Ship ship = GetShip(@event.shipid);
            if (ship == null)
            {
                // The ship is unknown - create it
                ship = ShipDefinitions.FromEDModel(@event.ship);
                ship.LocalId = (int)@event.shipid;
                ship.role = Role.MultiPurpose;
                AddShip(ship);
            }

            // Update name and ident if required
            if (@event.shipname != null && @event.shipname != "")
            {
                ship.name = @event.shipname;
            }
            if (@event.shipident != null && @event.shipident != "")
            {
                ship.ident = @event.shipident;
            }

            // Augment the ship info if required
            if (ship.model == null)
            {
                ship.model = @event.ship;
                ship.Augment();
            }

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
            ship.fueltankcapacity = (decimal)Math.Pow(2, ship.fueltank.@class);

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

        private void handleShipRepairedEvent(ShipRepairedEvent @event)
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

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
            // Obtain the shipyard from the profile
            List<Ship> profileShipyard = FrontierApi.ShipyardFromJson(profile);

            Ship profileCurrentShip = FrontierApi.ShipFromJson((JObject)profile["ship"]);

            // Information from the Frontier API can be out-of-date so we only use it to set our ship if we don't know what it already is
            if (currentShipId == null)
            {
                // This means that we don't have any info so far; set our active ship
                if (profileCurrentShip != null)
                {
                    SetCurrentShip(profileCurrentShip.LocalId, profileCurrentShip.model);
                }
            }

            // Add the raw JSON for each known ship provided in the profile
            // TODO Rationalise companion API data - munge the JSON according to the compartment information, removing anything that is out-of-sync
            if (profileCurrentShip != null)
            {
                Ship ship = GetShip(profileCurrentShip.LocalId);
                if (ship.model == null)
                {
                    // We don't know this ship's model but can fill it from the info we have
                    ship.model = profileCurrentShip.model;
                    ship.Augment();
                }
                // Obtain items that we can't obtain from the journal
                ship.value = profileCurrentShip.value;
                if (ship.cargohatch != null)
                {
                    // Engineering info for each module isn't in the journal, but we only use this to pass on to Coriolis so don't
                    // need to splice it in to our model.  We do, however, have cargo hatch information from the journal that we
                    // want to make avaialable to Coriolis so need to parse the raw data and add cargo hatch info as appropriate
                    JObject cargoHatchModule = new JObject();
                    cargoHatchModule.Add("on", ship.cargohatch.enabled);
                    cargoHatchModule.Add("priority", ship.cargohatch.priority);
                    cargoHatchModule.Add("value", ship.cargohatch.price);
                    cargoHatchModule.Add("health", ship.cargohatch.health);
                    cargoHatchModule.Add("name", "ModularCargoBayDoor");
                    JObject cargoHatchSlot = new JObject();
                    cargoHatchSlot.Add("module", cargoHatchModule);
                    JObject parsedRaw = JObject.Parse(profileCurrentShip.raw);
                    parsedRaw["modules"]["CargoHatch"] = cargoHatchSlot;
                    ship.raw = parsedRaw.ToString(Formatting.None);
                }
            }

            foreach (Ship profileShip in profileShipyard)
            {
                Ship ship = GetShip(profileShip.LocalId);
                if (ship != null)
                {
                    ship.raw = profileShip.raw;
                    if (ship.model == null)
                    {
                        // We don't know this ship's model but can fill it from the info we have
                        ship.model = profileShip.model;
                        ship.Augment();
                    }
                    // Obtain items that we can't obtain from the journal
                    ship.value = profileShip.value;
                }
            }

            writeShips();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables["ship"] = GetCurrentShip();
            variables["shipyard"] = shipyard;

            return variables;
        }

        public void writeShips()
        {
            // Write ship configuration with current inventory
            ShipMonitorConfiguration configuration = new ShipMonitorConfiguration();
            configuration.currentshipid = currentShipId;
            configuration.shipyard = shipyard;
            configuration.ToFile();
        }

        private void readShips()
        {
            // Obtain current inventory from  configuration
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

        private void AddShip(Ship ship)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                shipyard.Add(ship);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    shipyard.Add(ship);
                }));
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

        /// <summary>
        /// Obtain the current ship
        /// </summary>
        public Ship GetCurrentShip()
        {
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
                    Logging.Debug("Created ship ID " + localId);
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
            }
        }

        /// <summary>
        /// See if we're in a fighter or a buggy
        /// </summary>
        private bool inFighterOrBuggy(string model)
        {
            return (model == "Empire_Fighter" || model == "Federation_Fighter" || model == "Independent_Fighter" || model == "TestBuggy");
        }
    }
}
