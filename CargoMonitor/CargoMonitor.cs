using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiCargoMonitor
{
    /**
     * Monitor cargo for the current ship
     * Missing: there is no event for when a drone is fired, so we cannot keep track of this individually.  Instead we have to rely
     * on the inventory events to give us information on the number of drones in-ship.
     */
    public class CargoMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Cargo> inventory = new ObservableCollection<Cargo>();

        // Total cargo carried
        public int cargocarried;
        bool cargoinitialized = false;

        private static readonly object inventoryLock = new object();

        public string MonitorName()
        {
            return "Cargo monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Track information on your cargo.";
        }

        public bool IsRequired()
        {
            return true;
        }

        public CargoMonitor()
        {
            readCargo();
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
            cargoinitialized = true;
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
            readCargo();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());

        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is CargoInventoryEvent)
            {
                handleCargoInventoryEvent((CargoInventoryEvent)@event);
            }
            else if (@event is CommodityCollectedEvent)
            {
                handleCommodityCollectedEvent((CommodityCollectedEvent)@event);
            }
            else if (@event is CommodityEjectedEvent)
            {
                handleCommodityEjectedEvent((CommodityEjectedEvent)@event);
            }
            else if (@event is CommodityPurchasedEvent)
            {
                handleCommodityPurchasedEvent((CommodityPurchasedEvent)@event);
            }
            else if (@event is CommodityRefinedEvent)
            {
                handleCommodityRefinedEvent((CommodityRefinedEvent)@event);
            }
            else if (@event is CommoditySoldEvent)
            {
                handleCommoditySoldEvent((CommoditySoldEvent)@event);
            }
            else if (@event is PowerCommodityObtainedEvent)
            {
                handlePowerCommodityObtainedEvent((PowerCommodityObtainedEvent)@event);
            }
            else if (@event is PowerCommodityDeliveredEvent)
            {
                handlePowerCommodityDeliveredEvent((PowerCommodityDeliveredEvent)@event);
            }
            else if (@event is LimpetPurchasedEvent)
            {
                handleLimpetPurchasedEvent((LimpetPurchasedEvent)@event);
            }
            else if (@event is LimpetSoldEvent)
            {
                handleLimpetSoldEvent((LimpetSoldEvent)@event);
            }
            else if (@event is MissionAbandonedEvent)
            {
                // If we abandon a mission with cargo it becomes stolen
                handleMissionAbandonedEvent((MissionAbandonedEvent)@event);
            }
            else if (@event is MissionAcceptedEvent)
            {
                // Check to see if this is a cargo mission and update our inventory accordingly
                handleMissionAcceptedEvent((MissionAcceptedEvent)@event);
            }
            else if (@event is MissionCompletedEvent)
            {
                // Check to see if this is a cargo mission and update our inventory accordingly
                handleMissionCompletedEvent((MissionCompletedEvent)@event);
            }
            else if (@event is MissionFailedEvent)
            {
                // If we fail a mission with cargo it becomes stolen
                handleMissionFailedEvent((MissionFailedEvent)@event);
            }
            // TODO Powerplay events
        }

        private void handleCargoInventoryEvent(CargoInventoryEvent @event)
        {
            if (!cargoinitialized)
            {
                readCargo();
                cargoinitialized = true;
            }

            lock (inventoryLock)
            cargocarried = 0;

            // CargoInventoryEvent does not contain stolen, missionid, or cost information so fill in gaps here
            foreach (Cargo cargo in @event.inventory)
            {
                cargocarried += cargo.amount;

                Cargo inventoryCargo = inventory.FirstOrDefault(c => c.name == cargo.name);
                if (inventoryCargo != null)
                {
                    // Match of commodity
                    inventoryCargo.amount = cargo.amount;
                }
                else
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    Commodity commodity = CommodityDefinitions.FromName(cargo.name);
                    AddCargo(cargo);
                }
            }

            foreach (Cargo inventoryCargo in inventory)
            {
                Cargo cargo = @event.inventory.FirstOrDefault(c => c.name == inventoryCargo.name);
                if (cargo == null)
                {
                    RemoveCargo(inventoryCargo.name);
                }
            }
            writeCargo();
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount++;

                if (@event.stolen)
                {
                    cargo.stolen++;
                }
            }
            else
            {
                Cargo newCargo = new Cargo();
                newCargo.name = @event.commodity.name;
                newCargo.category = @event.commodity.category;
                newCargo.price = (long)(@event.commodity.avgprice ?? 0);
                newCargo.commodity = @event.commodity;
                newCargo.amount = 1;
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                if (@event.stolen)
                {
                    newCargo.stolen = 1;
                }
                AddCargo(newCargo);
            }
            cargocarried++;
            writeCargo();
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount -= @event.amount;
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            cargocarried--;
            writeCargo();
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo();
                newCargo.name = @event.commodity.name;
                newCargo.category = @event.commodity.category;
                newCargo.price = (long)(@event.commodity.avgprice ?? 0);
                newCargo.commodity = @event.commodity;
                newCargo.amount = @event.amount;
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                AddCargo(newCargo);
            }
            cargocarried += @event.amount;
            writeCargo();
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount ++;
            }
            else
            {
                Cargo newCargo = new Cargo();
                newCargo.name = @event.commodity.name;
                newCargo.category = @event.commodity.category;
                newCargo.price = (long)(@event.commodity.avgprice ?? 0);
                newCargo.commodity = @event.commodity;
                newCargo.amount = 1;
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                AddCargo(newCargo);
            }
            cargocarried++;
            writeCargo();
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount -= @event.amount;
                if (@event.stolen)
                {
                    cargo.stolen -= @event.amount;
                }
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            cargocarried -= @event.amount;
            writeCargo();
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo();
                newCargo.name = @event.commodity.name;
                newCargo.category = @event.commodity.category;
                newCargo.commodity = @event.commodity;
                newCargo.amount = @event.amount;
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                AddCargo(newCargo);
            }
            cargocarried += @event.amount;
            writeCargo();
        }


        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount -= @event.amount;
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            cargocarried -= @event.amount;
            writeCargo();
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.amount += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo();
                newCargo.name = "Limpet";
                newCargo.commodity = CommodityDefinitions.FromName("Limpet");
                newCargo.category = newCargo.commodity.category;
                newCargo.price = @event.price;
                newCargo.amount = @event.amount;
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                AddCargo(newCargo);
            }
            cargocarried += @event.amount;
            writeCargo();
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.amount -= @event.amount;
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            cargocarried -= @event.amount;
            writeCargo();
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulageAmount != null)
                {
                    inventoryCargo.stolen += haulageAmount.amount;
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    break;
                }
            }
            writeCargo();
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.commodity.name != null)
            {
                HaulageAmount haulageAmount = new HaulageAmount();
                haulageAmount.missionid = @event.missionid ?? 0;
                haulageAmount.amount = @event.amount ?? 0;

                Cargo cargo = GetCargo(@event.commodity.name);
                if (cargo != null)
                {
                    cargo.amount += @event.amount ?? 0;
                    cargo.haulage += @event.amount ?? 0;
                    cargo.haulageamounts.Add(haulageAmount);
                }
                else
                {
                    Cargo newCargo = new Cargo();
                    newCargo.name = @event.commodity.name;
                    newCargo.category = @event.commodity.category;
                    newCargo.commodity = @event.commodity;
                    newCargo.amount = @event.amount ?? 0;
                    newCargo.haulage = @event.amount ?? 0;
                    newCargo.stolen = 0;
                    newCargo.haulageamounts.Add(haulageAmount);
                    AddCargo(newCargo);
                }
            }
            cargocarried += @event.amount ?? 0;
            writeCargo();
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity.name);
            if (cargo != null)
            {
                cargo.amount -= @event.amount ?? 0;
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
                else
                {
                    HaulageAmount haulageAmount = cargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                    if (haulageAmount != null)
                    {
                        cargo.haulage -= haulageAmount.amount;
                        cargo.haulageamounts.Remove(haulageAmount);
                    }
                }
            }

            foreach (CommodityAmount commodityReward in @event.commodityrewards)
            {
                Cargo cargoReward = GetCargo(commodityReward.commodity);
                if (cargoReward != null)
                {
                    cargoReward.amount += commodityReward.amount;
                }
                else
                {
                    Cargo newCargo = new Cargo();
                    newCargo.commodity = CommodityDefinitions.FromName(commodityReward.commodity);
                    newCargo.name = newCargo.commodity.name;
                    newCargo.category = newCargo.commodity.category;
                    newCargo.price = (long)(newCargo.commodity.avgprice ?? 0);
                    newCargo.amount = commodityReward.amount;
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    AddCargo(newCargo);
                }
                cargocarried += commodityReward.amount;
            }
            cargocarried -= @event.amount ?? 0;
            writeCargo();
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulageAmount != null)
                {
                    inventoryCargo.stolen += haulageAmount.amount;
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    break;
                }
            }
            writeCargo();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["inventory"] = new List<Cargo>(inventory)
            };
            return variables;
        }

        public void writeCargo()
        {
            lock (inventoryLock)
            {
                // Write material configuration with current inventory
                CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
                configuration.cargo = inventory;
                configuration.cargocarried = cargocarried;
                configuration.ToFile();
            }
        }

        private void readCargo()
        {
            lock (inventoryLock)
            {
                // Obtain current inventory from  configuration
                CargoMonitorConfiguration configuration = CargoMonitorConfiguration.FromFile();
                cargocarried = configuration.cargocarried;

                // Build a new inventory
                List<Cargo> newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (Cargo cargo in configuration.cargo)
                {
                    newInventory.Add(cargo);
                }

                // Now order the list by name
                newInventory = newInventory.OrderBy(c => c.name).ToList();

                // Update the inventory 
                inventory.Clear();
                foreach (Cargo cargo in newInventory)
                {
                    inventory.Add(cargo);
                }
            }
        }

        public void AddCargo(Cargo cargo)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _AddCargo(cargo);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _AddCargo(cargo);
                }));
            }
        }

        private void _AddCargo(Cargo cargo)
        {
            lock (inventoryLock)
            {
                if (cargo == null)
                {
                    return;
                }
                // Remove the ship first (just in case we are trying to add a ship that already exists)
                RemoveCargo(cargo.name);
                inventory.Add(cargo);

                // Build a new inventory
                List<Cargo> newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (Cargo inventoryCargo in inventory)
                {
                    newInventory.Add(inventoryCargo);
                }

                // Now order the list by name
                newInventory = newInventory.OrderBy(c => c.name).ToList();

                // Update the inventory 
                inventory.Clear();
                foreach (Cargo newCargo in newInventory)
                {
                    inventory.Add(newCargo);
                }
                writeCargo();
            }
        }

        public void RemoveCargo(string commodityName)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                _RemoveCargo(commodityName);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    _RemoveCargo(commodityName);
                }));
            }
        }

        private void _RemoveCargo(string commodityName)
        {
            lock (inventoryLock)
            {
                if (commodityName != null)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        if (inventory[i].name == commodityName)
                        {
                            inventory.RemoveAt(i);
                            break;
                        }
                    }
                    writeCargo();
                }
            }
        }

        public Cargo GetCargo(string commodityName)
        {
            if (commodityName == null)
            {
                return null;
            }
            return inventory.FirstOrDefault(c => c.name == commodityName);
        }

        public int GetCargoCarried()
        {
            return cargocarried;
        }

        public void LimpetUsed()
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.amount--;
                if (cargo.amount < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            cargocarried--;
            writeCargo();
        }
    }
}
