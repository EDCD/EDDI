using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
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

        public void Start()
        {
            // We don't actively do anything, just listen to events, so nothing to do here
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
            // TODO loadout
            // TODO 
        }

        private void handleCommanderContinuedEvent(CommanderContinuedEvent @event)
        {
            SetCurrentShip(@event.shipid, @event.ship);
            Ship ship = GetCurrentShip();
            ship.name = @event.shipname;
            ship.ident = @event.shipident;
            writeShips();
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
                    ship.name = @event.name;
                }
                if (@event.ident != null)
                {
                    ship.ident = @event.ident;
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
            // TODO
        }

        private void handleShipRebootedEvent(ShipRebootedEvent @event)
        {
            // TODO
        }

        private void handleShipRepairedEvent(ShipRepairedEvent @event)
        {
            // TODO
        }

        private void handleShipRefuelledEvent(ShipRefuelledEvent @event)
        {
            // TODO
        }

        private void handleShipRestockedEvent(ShipRestockedEvent @event)
        {
            // TODO
        }

        public void PostHandle(Event @event)
        {
        }

        public void Handle(Profile profile)
        {
            // Information from the Frontier API can be out-of-date so we only use it to set our ship if we don't know what it already is
            if (currentShipId == null)
            {
                // This means that we don't have any info so far; set our active ship
                if (profile.Ship != null)
                {
                    SetCurrentShip(profile.Ship.LocalId, profile.Ship.model);
                }
            }

            // Add the raw JSON for each known ship provided in the profile
            // TODO Rationalise companion API data - munge the JSON according to the compartment information, removing anything tht is out-of-sync
            if (profile.Ship != null)
            {
                GetShip(profile.Ship.LocalId).raw = profile.Ship.raw;
            }

            foreach (Ship profileShip in profile.Shipyard)
            {
                Ship ship = GetShip(profileShip.LocalId);
                if (ship != null)
                {
                    ship.raw = profileShip.raw;
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
            if (Application.Current.Dispatcher.CheckAccess())
            {
                shipyard.Add(ship);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
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
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
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
    }
}
