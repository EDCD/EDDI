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
            // CargoInventoryEvent does not contain stolen, missionid, or cost information so fill in gaps here
            foreach (Cargo cargo in @event.inventory)
            {
                bool found = false;
                int totalAmount = 0;
                foreach (Cargo inventoryCargo in inventory)
                {
                    if (inventoryCargo.commodity == cargo.commodity)
                    {
                        // Match of commodity
                        found = true;
                        totalAmount += cargo.amount;
                    }
                }
                if (!found)
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    AddCargo(cargo);
                }
                else if (totalAmount < cargo.amount)
                {
                    // Inventory has not enough of this comodity
                    cargo.amount = cargo.amount - totalAmount;
                    AddCargo(cargo);
                }
                else if (totalAmount > cargo.amount)
                {
                    // Inventory has too much of this comodity
                    cargo.amount = totalAmount - cargo.amount;
                    RemoveCargo(cargo);
                }
            }

            foreach (Cargo inventoryCargo in inventory)
            {
                bool found = false;
                foreach (Cargo cargo in @event.inventory)
                {
                    if (cargo.commodity == inventoryCargo.commodity)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    
                }
            }

            writeCargo();
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.catagory = commodity.category;
                cargo.amount = 1;
                cargo.price = 0;
                if (commodity.avgprice != null)
                {
                    cargo.price = (long)commodity.avgprice;
                }
                cargo.stolen = @event.stolen;
                AddCargo(cargo);

                writeCargo();
            }
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                RemoveCargo(cargo);

                writeCargo();
            }
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.catagory = commodity.category;
                cargo.amount = @event.amount;
                cargo.price = @event.price;
                cargo.stolen = false;
                AddCargo(cargo);

                writeCargo();
            }
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.catagory = commodity.category;
                cargo.amount = 1;
                cargo.price = 0;
                if (commodity.avgprice != null)
                {
                    cargo.price = (long)commodity.avgprice;
                }
                cargo.stolen = false;
                AddCargo(cargo);

                writeCargo();
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                cargo.stolen = @event.stolen;
                RemoveCargo(cargo);

                writeCargo();
            }
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.catagory = commodity.category;
                cargo.amount = @event.amount;
                cargo.stolen = false;
                AddCargo(cargo);

                writeCargo();
            }
        }


        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                cargo.stolen = false;
                RemoveCargo(cargo);

                writeCargo();
            }
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Commodity limpet = CommodityDefinitions.FromName("Limpet");
            Cargo cargo = new Cargo();
            cargo.commodity = limpet.name;
            cargo.catagory = limpet.category;
            cargo.amount = @event.amount;
            cargo.price = @event.price;
            cargo.stolen = false;
            AddCargo(cargo);

            writeCargo();
        }


        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Commodity limpet = CommodityDefinitions.FromName("Limpet");
            Cargo cargo = new Cargo();
            cargo.commodity = limpet.name;
            cargo.amount = @event.amount;
            cargo.price = @event.price;
            cargo.stolen = false;
            RemoveCargo(cargo);

            writeCargo();
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.missionid == @event.missionid)
                {
                    Cargo cargo = new Cargo();
                    cargo.commodity = inventoryCargo.commodity;
                    cargo.missionid = inventoryCargo.missionid;
                    cargo.amount = inventoryCargo.amount;
                    RemoveCargo(cargo);

                    cargo.missionid = null;
                    cargo.price = 0;
                    cargo.stolen = true;
                    AddCargo(cargo);

                    writeCargo();
                }
            }
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            Commodity commodity = CommodityDefinitions.FromName(@event.commodity);
            if (commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.catagory = commodity.category;
                cargo.missionid = @event.missionid;
                cargo.amount = (int)@event.amount;
                cargo.price = 0;
                if (commodity.avgprice != null)
                {
                    cargo.price = (long)commodity.avgprice;
                }
                cargo.stolen = false;
                AddCargo(cargo);

                writeCargo();
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.missionid = @event.missionid;
                cargo.amount = (int)@event.amount;
                RemoveCargo(cargo);

                writeCargo();
            }
            if (@event.commodityrewards != null)
            {
                foreach (CommodityAmount reward in @event.commodityrewards)
                {
                    Commodity rewardCommodity = CommodityDefinitions.FromName(reward.commodity);
                    Cargo cargo = new Cargo();
                    cargo.commodity = reward.commodity;
                    cargo.catagory = rewardCommodity.category;
                    cargo.amount = reward.amount;
                    cargo.price = 0;
                    cargo.stolen = false;
                    AddCargo(cargo);

                    writeCargo();
                }
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.missionid == @event.missionid)
                {
                    Cargo cargo = new Cargo();
                    cargo.commodity = inventoryCargo.commodity;
                    cargo.missionid = inventoryCargo.missionid;
                    cargo.amount = inventoryCargo.amount;
                    RemoveCargo(cargo);

                    cargo.missionid = null;
                    cargo.price = 0;
                    cargo.stolen = true;
                    AddCargo(cargo);

                    writeCargo();
                }
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["cargo"] = new List<Cargo>(inventory)
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
                configuration.ToFile();
            }
        }

        private void readCargo()
        {
            lock (inventoryLock)
            {
                // Obtain current inventory from  configuration
                CargoMonitorConfiguration configuration = CargoMonitorConfiguration.FromFile();

                // Build a new inventory
                List<Cargo> newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (Cargo cargo in configuration.cargo)
                {
                    newInventory.Add(cargo);
                }

                // Now order the list by name
                newInventory = newInventory.OrderBy(c => c.commodity).ToList();

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
                addCargo(cargo);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    addCargo(cargo);
                }));
            }
        }

        public void RemoveCargo(Cargo cargo)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                removeCargo(cargo);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    removeCargo(cargo);
                }));
            }
        }

        private void addCargo(Cargo cargo)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.commodity == cargo.commodity)
                {
                    // Matching commodity; see if the details match
                    if (inventoryCargo.missionid != null)
                    {
                        // Mission-specific cargo
                        if (inventoryCargo.missionid == cargo.missionid)
                        {
                            // Both for the same mission - add to this
                            inventoryCargo.amount += cargo.amount;
                            return;
                        }
                        // Different mission; skip
                        continue;
                    }

                    if (inventoryCargo.stolen == cargo.stolen)
                    {
                        // Both of the same legality
                        if (inventoryCargo.price == cargo.price)
                        {
                            // Same cost basis - add to this
                            inventoryCargo.amount += cargo.amount;
                            return;
                        }
                    }
                }
            }
            // No matching cargo - add entry
            inventory.Add(cargo);
        }

        private void removeCargo(Cargo cargo)
        {
            while (cargo.amount > 0)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    Cargo inventoryCargo = inventory[i];
                    if (inventoryCargo.commodity == cargo.commodity)
                    {
                        // Matching commodity; see if the details match
                        if (inventoryCargo.missionid != null)
                        {
                            // Mission-specific cargo
                            if (inventoryCargo.missionid == cargo.missionid)
                            {
                                // Both for the same mission - remove from this
                                if (inventoryCargo.amount == cargo.amount)
                                {
                                    inventory.RemoveAt(i);
                                    return;
                                }
                                else if (inventoryCargo.amount < cargo.amount)
                                {
                                    inventoryCargo.amount = 0;
                                    cargo.amount -= inventoryCargo.amount;
                                }
                                else
                                {
                                    inventoryCargo.amount -= cargo.amount;
                                    return;
                                }
                            }
                            // Different mission; skip
                            continue;
                        }

                        if (inventoryCargo.stolen == cargo.stolen)
                        {
                           // Both of the same legality - remove from this
                            if (inventoryCargo.amount == cargo.amount)
                            {
                                inventory.RemoveAt(i);
                                return;
                            }
                            else if (inventoryCargo.amount < cargo.amount)
                            {
                                inventoryCargo.amount = 0;
                                cargo.amount -= inventoryCargo.amount;
                            }
                            else
                            {
                                inventoryCargo.amount -= cargo.amount;
                                return;
                            }
                            // Different legality; skip
                            continue;
                        }
                    }
                }
                if (cargo.missionid != null || cargo.stolen)
                {
                    cargo.missionid = null;
                    cargo.stolen = false;
                }
            }
            // No matching cargo - ignore
            Logging.Debug("Did not find match for cargo " + JsonConvert.SerializeObject(cargo));
        }

        public void LimpetUsed()
        {
            Commodity limpet = CommodityDefinitions.FromName("Limpet");
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.commodity == "Limpet" && inventoryCargo.amount > 0)
                {
                    inventoryCargo.amount--;
                }
                inventoryCargo.price -= (long)limpet.sellprice;
            }
        }
    }
}
