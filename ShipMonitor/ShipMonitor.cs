using Eddi;
using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipMonitor : EDDIMonitor
    {
        // Observable collection for us to handle
        public ObservableCollection<Ship> shipyard = new ObservableCollection<Ship>();

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
            return null;
//            return new ConfigurationWindow();
        }

        public void Handle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is ShipSwappedEvent)
            {
                handleShipSwappedEvent((ShipSwappedEvent)@event);
            }
            else if (@event is ShipSoldEvent)
            {
                handleShipSoldEvent((ShipSoldEvent)@event);
            }
            else if (@event is ShipPurchasedEvent)
            {
                handleShipPurchasedEvent((ShipPurchasedEvent)@event);
            }
            else if (@event is ShipDeliveredEvent)
            {
                handleShipDeliveredEvent((ShipDeliveredEvent)@event);
            }
            else if (@event is ShipRenamedEvent)
            {
                handleShipRenamedEvent((ShipRenamedEvent)@event);
            }
            // TODO loadout
        }

        private void handleShipSwappedEvent(ShipSwappedEvent @event)
        {
        }

        private void handleShipSoldEvent(ShipSoldEvent @event)
        {
            for (int i = 0; i < EDDI.Instance.Shipyard.Count; i++)
            {
                if (EDDI.Instance.Shipyard[i].LocalId == @event.shipid)
                {
                    EDDI.Instance.Shipyard.RemoveAt(i);
                    break;
                }
            }
        }

        private void handleShipPurchasedEvent(ShipPurchasedEvent @event)
        {
        }

        private void handleShipDeliveredEvent(ShipDeliveredEvent @event)
        {
        }

        private void handleShipRenamedEvent(ShipRenamedEvent @event)
        {
            if (EDDI.Instance.Ship.LocalId == @event.Ship.LocalId)
            {
                // This ship
                @event.Ship.name = @event.name;
                if (@event.ident != null)
                {
                    @event.Ship.ident = @event.ident;
                }
            }
            writeShips();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables["shipyard"] = EDDI.Instance.Shipyard;

            return variables;
        }

        public void writeShips()
        {
            // Write ship configuration with current inventory
            ShipsConfiguration configuration = new ShipsConfiguration();
            configuration.Ships = new ObservableCollection<Ship>();
            configuration.Ships.Add(EDDI.Instance.Ship);
            foreach (Ship storedShip in EDDI.Instance.Shipyard)
            {
                configuration.Ships.Add(storedShip);
            }
            //configuration.Ships = shipyard;
            configuration.ToFile();
        }

        private void readShips()
        {
            // Obtain current inventory from  configuration
            ShipsConfiguration configuration = ShipsConfiguration.FromFile();

            // Build a new shipyard
            List<Ship> newShipyard = new List<Ship>();
            foreach (Ship ship in configuration.Ships)
            {
                newShipyard.Add(ship);
            }

            // Now order the list by model
            newShipyard = newShipyard.OrderBy(s => s.model).ToList();

            // Update the shipyard
            shipyard.Clear();
            foreach (Ship ship in newShipyard)
            {
                shipyard.Add(ship);
            }
        }
    }
}
