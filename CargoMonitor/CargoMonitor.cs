using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiShipMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiCargoMonitor
{
    /**
     * Monitor cargo for the current ship
     */
    public class CargoMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Cargo> inventory { get; private set; }

        public int cargocarried;
        private static readonly object inventoryLock = new object();
        public event EventHandler InventoryUpdatedEvent;

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
            inventory = new ObservableCollection<Cargo>();
            BindingOperations.CollectionRegistering += Inventory_CollectionRegistering;

            readInventory();
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        private void Inventory_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(inventory, inventoryLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(inventory, inventoryLock); });
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
            readInventory();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());

        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(inventory, inventoryLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(inventory); });
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
            else if (@event is LimpetLaunchedEvent)
            {
                handleLimpetLaunchedEvent((LimpetLaunchedEvent)@event);
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
            else if (@event is SynthesisedEvent)
            {
                handleSynthesisedEvent((SynthesisedEvent)@event);
            }
            else if (@event is TechnologyBrokerEvent)
            {
                handleTechnologyBrokerEvent((TechnologyBrokerEvent)@event);
            }
        }

        private void handleCargoInventoryEvent(CargoInventoryEvent @event)
        {
            // CargoInventoryEvent does not contain missionid or cost information so fill in gaps here
            foreach (Cargo cargo in @event.inventory)
            {
                Cargo inventoryCargo = inventory.FirstOrDefault(c => c.name == cargo.name);
                if (inventoryCargo != null)
                {
                    // Found match of commodity
                    inventoryCargo.total = cargo.total;
                    inventoryCargo.stolen = cargo.stolen;
                    inventoryCargo.other = cargo.total - cargo.stolen - inventoryCargo.haulage;
                }
                else
                {
                    // We haven't heard of this cargo so add it to the inventory directly
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
            writeInventory();
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.total++;
                if (@event.stolen)
                {
                    cargo.stolen++;
                }
                else
                {
                    cargo.other++;
                }
            }

            else
            {
                Cargo newCargo = new Cargo(@event.commodity, 1);
                newCargo.haulage = 0;
                if (@event.stolen)
                {
                    newCargo.stolen = 1;
                }
                else
                {
                    newCargo.other = 1;
                }
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                if (cargo.haulageamounts.Any(ha => ha.amount >= @event.amount))
                {
                    cargo.ejected += @event.amount;
                    cargo.haulage -= @event.amount;
                }
                else if (cargo.stolen > 0)
                {
                    cargo.stolen -= @event.amount;
                }
                else
                {
                    cargo.other -= @event.amount;
                }

                if (cargo.total < 1)
                {
                    // All of the commodity was ejected
                    RemoveCargo(cargo.name);
                }
            }
            writeInventory();
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.other += @event.amount;
                cargo.total += @event.amount; ;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, @event.price, @event.amount);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.other = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.other++;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, 1);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.other = 1;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                if (@event.stolen)
                {
                    // Cargo is stolen
                    cargo.stolen -= @event.amount;
                }
                else if (@event.blackmarket && cargo.haulageamounts.Any(ha => ha.amount >= @event.amount))
                {
                    // Cargo is mission-related
                    cargo.ejected = @event.amount;
                    cargo.haulage -= @event.amount;
                }
                else
                {
                    // Cargo is owned by the commander
                    cargo.other -= @event.amount;
                }
                if (cargo.total < 1)
                {
                    // All of the commodity was sold
                    RemoveCargo(cargo.name);
                }
            }
            writeInventory();
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.other += @event.amount;
                cargo.total = cargo.haulage + cargo.stolen + cargo.other;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, @event.amount);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.other = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }


        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.other -= @event.amount;
                cargo.total = cargo.haulage + cargo.stolen + cargo.other;

                if (cargo.total < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            writeInventory();
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.other += @event.amount;
                cargo.total = cargo.haulage + cargo.stolen + cargo.other;
            }
            else
            {
                Cargo newCargo = new Cargo("Limpet", @event.amount, @event.price);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.other = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.other -= @event.amount;
                cargo.total = cargo.haulage + cargo.stolen + cargo.other;

                if (cargo.total < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            writeInventory();
        }

        private void handleLimpetLaunchedEvent(LimpetLaunchedEvent @event)
        {
            Cargo cargo = GetCargo("Limpet");
            if (cargo != null)
            {
                cargo.other --;
                cargo.total = cargo.haulage + cargo.stolen + cargo.other;

                if (cargo.total < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }
            writeInventory();
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulageAmount != null)
                {
                    int remaining = haulageAmount.amount - inventoryCargo.ejected;
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    inventoryCargo.haulage -= remaining;
                    inventoryCargo.stolen += remaining;
                    inventoryCargo.ejected = 0;
                    break;
                }
            }
            writeInventory();
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.commodity != null)
            {
                HaulageAmount haulageAmount = new HaulageAmount(@event.missionid ?? 0, @event.amount ?? 0);

                Cargo cargo = GetCargo(@event.commodity);
                if (cargo != null)
                {
                    cargo.haulage += @event.amount ?? 0;
                    cargo.total += @event.amount ?? 0;
                    cargo.haulageamounts.Add(haulageAmount);
                }
                else
                {
                    Cargo newCargo = new Cargo(@event.commodity, @event.amount ?? 0);
                    newCargo.haulage = @event.amount ?? 0;
                    newCargo.stolen = 0;
                    newCargo.other = 0;
                    newCargo.haulageamounts.Add(haulageAmount);
                    AddCargo(newCargo);
                }
            }
            writeInventory();
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            Cargo cargo = GetCargo(@event.commodity);
            if (cargo != null)
            {
                cargo.haulage -= @event.amount ?? 0;
                HaulageAmount haulageAmount = cargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulageAmount != null)
                {
                    cargo.haulageamounts.Remove(haulageAmount);
                }

                cargo.total -= @event.amount ?? 0;
                if (cargo.total < 1)
                {
                    RemoveCargo(cargo.name);
                }
            }

            foreach (CommodityAmount commodityReward in @event.commodityrewards)
            {
                Cargo cargoReward = GetCargo(commodityReward.commodity);
                if (cargoReward != null)
                {
                    cargoReward.other += commodityReward.amount;
                    cargo.total += commodityReward.amount;
                }
                else
                {
                    Cargo newCargo = new Cargo(commodityReward.commodity, commodityReward.amount);
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    newCargo.other = commodityReward.amount;
                    AddCargo(newCargo);
                }
            }
            writeInventory();
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulageAmount != null)
                {
                    int remaining = haulageAmount.amount - inventoryCargo.ejected;
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    inventoryCargo.haulage -= remaining;
                    inventoryCargo.stolen += remaining;
                    inventoryCargo.ejected = 0;
                    break;
                }
            }
            writeInventory();
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.synthesis.Contains("Limpet"))
            {
                Cargo cargo = GetCargo("Limpet");
                if (cargo != null)
                {
                    cargo.other += 4;
                    cargo.total += 4;
                }
                else
                {
                    Cargo newCargo = new Cargo("Limpet", 4);
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    newCargo.other = 4;
                    AddCargo(newCargo);
                    cargo.total = cargo.haulage + cargo.stolen + cargo.other;
                }
                writeInventory();
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            foreach (CommodityAmount commodityAmount in @event.commodities)
            {
                Cargo cargo = GetCargo(commodityAmount.commodity);
                if (cargo != null)
                {
                    cargo.other -= commodityAmount.amount;
                    cargo.total -= commodityAmount.amount;
                }
            }
            writeInventory();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["inventory"] = new List<Cargo>(inventory)
            };
            return variables;
        }

        public void writeInventory()
        {
            lock (inventoryLock)
            {
                // Write cargo configuration with current inventory
                CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
                cargocarried = 0;
                foreach (Cargo cargo in inventory)
                {
                    cargocarried += cargo.total;
                }

                Ship ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
                if (ship != null)
                {
                    ship.cargocarried = cargocarried;
                }

                configuration.cargo = inventory;
                configuration.cargocarried = cargocarried;
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(InventoryUpdatedEvent, inventory);
        }

        private void readInventory()
        {
            lock (inventoryLock)
            {
                // Obtain current cargo inventory from configuration
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

        private void AddCargo(Cargo cargo)
        {
            if (cargo == null)
            {
                return;
            }

            lock (inventoryLock)
            {
                inventory.Add(cargo);
            }
            writeInventory();
        }

        private void RemoveCargo(string commodityName)
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
                }
            }
            writeInventory();
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
    }
}
