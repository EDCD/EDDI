using Eddi;
using EddiDataDefinitions;
using EddiEvents;
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

        public string LocalizedMonitorName()
        {
            return Properties.CargoMonitor.cargo_monitor_name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return Properties.CargoMonitor.cargo_monitor_desc;
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
            else if (@event is SearchAndRescueEvent)
            {
                handleSearchAndRescueEvent((SearchAndRescueEvent)@event);
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
                Cargo inventoryCargo = inventory.FirstOrDefault(c => c.invariantName == cargo.invariantName);
                if (inventoryCargo != null)
                {
                    // Found match of commodity
                    inventoryCargo.total = cargo.total;
                    inventoryCargo.stolen = cargo.stolen;
                    if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                    {
                        inventoryCargo.haulage = 0;
                    }
                    inventoryCargo.owned = cargo.total - cargo.stolen - inventoryCargo.haulage;
                    inventoryCargo.ejected = 0;
                }
                else
                {
                    // We haven't heard of this cargo so add it to the inventory directly
                    AddCargo(cargo);
                }
            }
            // Remove strays from the manifest
            foreach (Cargo inventoryCargo in inventory.ToList())
            {
                // Keep cargo in manifest if missions are pending
                if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                {
                    Cargo cargo = @event.inventory.FirstOrDefault(c => c.invariantName == inventoryCargo.invariantName);
                    if (cargo == null)
                    {
                        // Strip out the stray from the manifest
                        RemoveCargoWithInvariantName(inventoryCargo.invariantName);
                    }
                }
            }
            writeInventory();
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                bool handled = false;
                if (@event.stolen)
                {
                    cargo.stolen++;
                }
                else if (cargo.haulageamounts.Any())
                {
                    foreach (HaulageAmount haulageAmount in cargo.haulageamounts)
                    {
                        string type = haulageAmount.name.Split('_').ElementAt(1).ToLowerInvariant();
                        int total = cargo.haulageamounts.Where(ha => ha.name.ToLowerInvariant().Contains(type)).Sum(ha => ha.amount);
                        switch (type)
                        {
                            case "altruism":
                            case "collect":
                            case "mining":
                            case "piracy":
                                {
                                    if (cargo.owned < total)
                                    {
                                        cargo.owned++;
                                        handled = true;
                                    }
                                }
                                break;
                            case "rescue":
                            case "salvage":
                                {
                                    if (cargo.haulage < total)
                                    {
                                        cargo.haulage++;
                                        handled = true;
                                    }
                                }
                                break;
                        }
                        if (handled)
                        {
                            break;
                        }
                    }
                }
                else if (!handled)
                {
                    cargo.owned++;
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
                    newCargo.owned = 1;
                }
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                bool handled = false;

                // Check for related missions
                if (cargo.haulageamounts.Any())
                {
                    cargo.ejected += @event.amount;
                    foreach (HaulageAmount haulageAmount in cargo.haulageamounts)
                    {
                        string type = haulageAmount.name.Split('_').ElementAt(1).ToLowerInvariant();
                        int total = cargo.haulageamounts.Where(ha => ha.name.ToLowerInvariant().Contains(type)).Sum(ha => ha.amount);
                        switch (type)
                        {
                            case "altruism":
                            case "collect":
                            case "mining":
                            case "piracy":
                                {
                                    if(cargo.owned >= @event.amount)
                                    {
                                        cargo.owned -= @event.amount;
                                        handled = true;
                                    }
                                }
                                break;
                            case "delivery":
                            case "rescue":
                            case "salvage":
                            case "smuggle":
                                {
                                    if (cargo.haulage >= @event.amount)
                                    {
                                        cargo.haulage -= @event.amount;
                                        handled = true;
                                    }
                                }
                                break;
                        }
                        if (handled)
                        {
                            break;
                        }
                    }
                }

                // Otherwise, order of preference is owned -> stolen
                if (!handled)
                {
                    if (cargo.owned >= @event.amount)
                    {
                        cargo.owned -= @event.amount;
                    }
                    else if (cargo.stolen >= @event.amount)
                    {
                        cargo.stolen -= @event.amount;
                    }
                }

                // If not mission related and all was ejected
                if (cargo.total < 1)
                {
                    // Check if other missions are pending
                    if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                    {
                        // All of the commodity was ejected
                        RemoveCargoWithInvariantName(cargo.invariantName);
                    }
                }
                writeInventory();
            }
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, @event.amount, @event.price);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                cargo.owned++;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, 1);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = 1;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                if (@event.stolen)
                {
                    // Cargo is stolen
                    cargo.stolen -= (cargo.stolen > @event.amount) ? @event.amount : cargo.stolen;
                }
                else if (@event.blackmarket)
                {
                    // Cargo is mission-related
                    int amount = (cargo.owned > @event.amount) ? @event.amount : cargo.owned;
                    cargo.haulage -= amount;
                    cargo.ejected += amount;
                }
                else
                {
                    // Cargo is owned by the commander
                    cargo.owned -= (cargo.owned > @event.amount) ? @event.amount : cargo.owned;
                }
                if (cargo.total < 1)
                {
                    if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                    {
                        // All of the commodity was sold
                        RemoveCargoWithInvariantName(cargo.invariantName);
                    }
                }
                writeInventory();
            }
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodity, @event.amount);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                cargo.owned -= @event.amount;
                if (cargo.total < 1)
                {
                    if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                    {
                        // All of the commodity was sold
                        RemoveCargoWithInvariantName(cargo.invariantName);
                    }
                }
                writeInventory();
            }
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName("Limpet");
            if (cargo != null)
            {
                cargo.owned += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo("Limpet", @event.amount, @event.price);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
            writeInventory();
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName("Limpet");
            if (cargo != null)
            {
                cargo.owned -= @event.amount;
                if (cargo.total < 1)
                {
                    RemoveCargoWithInvariantName(cargo.invariantName);
                }
                writeInventory();
            }
        }

        private void handleLimpetLaunchedEvent(LimpetLaunchedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName("Limpet");
            if (cargo != null)
            {
                cargo.owned--;
                if (cargo.total < 1)
                {
                    RemoveCargoWithInvariantName(cargo.invariantName);
                }
                writeInventory();
            }
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAt(1).ToLowerInvariant();
                    switch (type)
                    {
                        case "delivery":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                int remaining = haulageAmount.amount - inventoryCargo.ejected;
                                remaining = inventoryCargo.haulage < remaining ? inventoryCargo.haulage : remaining;
                                inventoryCargo.haulage -= remaining;
                                inventoryCargo.stolen += remaining;
                                inventoryCargo.ejected = 0;
                            }
                            break;
                    }
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    if (inventoryCargo.total < 1)
                    {
                        // Check if other missions are pending
                        if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                        {
                            // All of the commodity was turned in
                            RemoveCargoWithInvariantName(inventoryCargo.invariantName);
                        }
                    }
                    writeInventory();
                    break;
                }
            }
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.commodity != null)
            {
                string type = @event.name.Split('_').ElementAt(1).ToLowerInvariant();
                switch (type)
                {
                    case "altruism":
                    case "collect":
                    case "delivery":
                    case "mining":
                    case "piracy":
                    case "rescue":
                    case "salvage":
                    case "smuggle":
                        {
                            int amount = (type == "delivery" || type == "smuggle") ? @event.amount ?? 0 : 0;
                            HaulageAmount haulageAmount = new HaulageAmount(@event.missionid ?? 0, @event.name, @event.amount ?? 0, (DateTime)@event.expiry);
                            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
                            if (cargo != null)
                            {
                                cargo.haulage += amount;
                                cargo.haulageamounts.Add(haulageAmount);
                            }
                            else
                            {
                                Cargo newCargo = new Cargo(@event.commodity, amount);
                                newCargo.haulage = amount;
                                newCargo.stolen = 0;
                                newCargo.owned = 0;
                                newCargo.haulageamounts.Add(haulageAmount);
                                AddCargo(newCargo);
                            }
                            writeInventory();
                        }
                        break;
                }
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodity);
            if (cargo != null)
            {
                HaulageAmount haulageAmount = cargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAt(1)
                        .ToLowerInvariant();
                    string subtype = @event.name.Split('_').ElementAt(2)
                        .ToLowerInvariant()
                        .Replace("$", "");
                    switch (type)
                    {
                        case "altruism":
                        case "collect":
                        case "mining":
                            {
                                cargo.owned -= @event.amount ?? 0;
                            }
                            break;
                        case "delivery":
                        case "rescue":
                        case "smuggle":
                            {
                                cargo.haulage -= @event.amount ?? 0;
                            }
                            break;
                        case "piracy":
                            {
                                if (cargo.stolen < @event.amount)
                                {
                                    cargo.owned -= @event.amount ?? 0;
                                }
                                else
                                {
                                    cargo.stolen -= @event.amount ?? 0;
                                }
                            }
                            break;
                        case "salvage":
                            {
                                if (subtype.Contains("illegal"))
                                {
                                    cargo.stolen -= @event.amount ?? 0;
                                }
                                else
                                {
                                    cargo.haulage -= @event.amount ?? 0;
                                }
                            }
                            break;
                    }
                    cargo.haulageamounts.Remove(haulageAmount);
                }
                if (cargo.total < 1)
                {
                    // Check if other missions are pending
                    if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                    {
                        // All of the commodity was turned in
                        RemoveCargoWithInvariantName(cargo.invariantName);
                    }
                }
            }

            foreach (CommodityAmount commodityReward in @event.commodityrewards)
            {
                cargo = GetCargoWithInvariantName(commodityReward.commodity);
                if (cargo != null)
                {
                    cargo.owned += commodityReward.amount;
                }
                else
                {
                    Cargo newCargo = new Cargo(commodityReward.commodity, commodityReward.amount);
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    newCargo.owned = commodityReward.amount;
                    AddCargo(newCargo);
                }
            }
            writeInventory();
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory.ToList())
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAt(1).ToLowerInvariant();
                    switch (type)
                    {
                        case "delivery":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                int remaining = haulageAmount.amount;

                                // If not expired, then failure due to jettison
                                if (haulageAmount.expiry < DateTime.Now)
                                {
                                    remaining -= inventoryCargo.ejected;
                                    inventoryCargo.ejected = 0;
                                }
                                // Transfer remaining haulage to stolen
                                remaining = inventoryCargo.haulage < remaining ? inventoryCargo.haulage : remaining;
                                inventoryCargo.haulage -= remaining;
                                inventoryCargo.stolen += remaining;
                            }
                            break;
                    }
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    if (inventoryCargo.total < 1)
                    {
                        // Check if other missions are pending
                        if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                        {
                            // All of the commodity was turned in
                            RemoveCargoWithInvariantName(inventoryCargo.invariantName);
                        }
                    }
                    writeInventory();
                    break;
                }
            }
        }

        private void handleSearchAndRescueEvent(SearchAndRescueEvent @event)
        {
            Cargo cargo = GetCargoWithInvariantName(@event.commodityname);
            if (cargo != null)
            {
                cargo.owned -= (cargo.owned > @event.amount) ? @event.amount ?? 0 : cargo.owned;
                if (cargo.total < 1)
                {
                    // Check if other missions are pending
                    if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                    {
                        // All of the commodity was turned in
                        RemoveCargoWithInvariantName(cargo.invariantName);
                    }
                }
                writeInventory();
            }
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.synthesis.Contains("Limpet"))
            {
                Cargo cargo = GetCargoWithInvariantName("Limpet");
                if (cargo != null)
                {
                    cargo.owned += 4;
                }
                else
                {
                    Cargo newCargo = new Cargo("Limpet", 4);
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    newCargo.owned = 4;
                    AddCargo(newCargo);
                }
                writeInventory();
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            foreach (CommodityAmount commodityAmount in @event.commodities)
            {
                Cargo cargo = GetCargoWithInvariantName(commodityAmount.commodity);
                if (cargo != null)
                {
                    cargo.owned -= (cargo.owned > commodityAmount.amount) ? commodityAmount.amount : cargo.owned;
                    if (cargo.total < 1)
                    {
                        // Check if other missions are pending
                        if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
                        {
                            // All of the commodity was turned in
                            RemoveCargoWithInvariantName(cargo.invariantName);
                        }
                    }
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
                newInventory = newInventory.OrderBy(c => c.invariantName).ToList();

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

        private void RemoveCargoWithInvariantName(string invariantCommodityName)
        {
            lock (inventoryLock)
            {
                if (invariantCommodityName != null)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        if (inventory[i].invariantName == invariantCommodityName)
                        {
                            inventory.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            writeInventory();
        }

        public Cargo GetCargoWithInvariantName(string invariantCommodityName)
        {
            if (invariantCommodityName == null)
            {
                return null;
            }
            return inventory.FirstOrDefault(c => c.invariantName == invariantCommodityName);
        }

        public int GetCargoWithInvariantNameCarried()
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
