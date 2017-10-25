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
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
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
            // CargoInventoryEvent does not contain stolen, missionid, or cost information so merge it here
            foreach (Cargo cargo in @event.inventory)
            {
                bool added = false;
                int totalAmount = 0;
                foreach (Cargo inventoryCargo in inventory)
                {
                    if (inventoryCargo.commodity.name == cargo.commodity.name)
                    {
                        // Match of commodity
                        added = true;
                        totalAmount += cargo.amount;
                    }
                }
                if (!added || totalAmount != cargo.amount)
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    AddCargo(cargo);
                }
            }

            foreach (Cargo inventoryCargo in inventory)
            {
                bool found = false;
                foreach (Cargo cargo in @event.inventory)
                {
                    if (cargo.commodity.name == inventoryCargo.commodity.name)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    
                }
            }
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = 1;
                cargo.price = 0;
                cargo.stolen = @event.stolen;
                AddCargo(cargo);
            }
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                RemoveCargo(cargo);
            }
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                cargo.price = @event.price;
                AddCargo(cargo);
            }
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = 1;
                cargo.price = 0;
                AddCargo(cargo);
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                if (@event.stolen)
                {
                    cargo.price = @event.profit;
                }
                else
                {
                    cargo.price = @event.price;
                }
                cargo.stolen = @event.stolen;
                RemoveCargo(cargo);
            }
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                AddCargo(cargo);
            }
        }


        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.amount = @event.amount;
                RemoveCargo(cargo);
            }
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Commodity limpet = CommodityDefinitions.FromName("Limpet");
            Cargo cargo = new Cargo();
            cargo.commodity = limpet;
            cargo.amount = @event.amount;
            cargo.price = @event.price;
            AddCargo(cargo);
        }


        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Commodity limpet = CommodityDefinitions.FromName("Limpet");
            Cargo cargo = new Cargo();
            cargo.commodity = limpet;
            cargo.amount = @event.amount;
            cargo.price = @event.price;
            RemoveCargo(cargo);
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.missionid == @event.missionid)
                {
                    inventoryCargo.stolen = true;
                }
            }
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.commodity != null)
            {
                Cargo cargo = new Cargo();
                cargo.commodity = @event.commodity;
                cargo.missionid = @event.missionid;
                cargo.amount = (int)@event.amount;
                cargo.price = 0;
                AddCargo(cargo);
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
            }
            if (@event.commodityrewards != null)
            {
                foreach (CommodityAmount reward in @event.commodityrewards)
                {
                    Cargo cargo = new Cargo();
                    cargo.commodity = reward.commodity;
                    cargo.amount = reward.amount;
                    AddCargo(cargo);
                }
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.missionid == @event.missionid)
                {
                    inventoryCargo.stolen = true;
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
                if (inventoryCargo.commodity.name == cargo.commodity.name)
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
            for (int i = 0; i < inventory.Count; i++)
            {
                Cargo inventoryCargo = inventory[i];
                if (inventoryCargo.commodity.name == cargo.commodity.name)
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
                            else
                            {
                                inventoryCargo.amount -= cargo.amount;
                            }
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
                            // Same cost basis - remove from this
                            if (inventoryCargo.amount == cargo.amount)
                            {
                                inventory.RemoveAt(i);
                                return;
                            }
                            else
                            {
                                inventoryCargo.amount -= cargo.amount;
                            }
                            return;
                        }
                    }
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
                if (inventoryCargo.commodity.name == "Limpet" && inventoryCargo.amount > 0)
                {
                    inventoryCargo.amount--;
                }
                inventoryCargo.price -= (long)limpet.sellprice;
            }
        }
    }
}
