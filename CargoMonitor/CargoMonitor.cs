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
            initializeCargoMonitor();
        }

        public void initializeCargoMonitor(CargoMonitorConfiguration configuration = null)
        {
            readInventory(configuration);
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
            _handleCargoInventoryEvent(@event);
            writeInventory();
        }

        public void _handleCargoInventoryEvent(CargoInventoryEvent @event)
        {
            // CargoInventoryEvent does not contain missionid or cost information so fill in gaps here
            foreach (Cargo cargo in @event.inventory)
            {
                Cargo inventoryCargo = inventory.FirstOrDefault(c => c.edname == cargo.edname);
                if (inventoryCargo != null)
                {
                    // Found match of commodity
                    inventoryCargo.total = cargo.total;
                    inventoryCargo.stolen = cargo.stolen;
                    if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                    {
                        inventoryCargo.haulage = 0;
                        cargo.need = 0;
                    }
                    else
                    {
                        cargo.need = CalculateCargoNeed(inventoryCargo);
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
                    Cargo cargo = @event.inventory.FirstOrDefault(c => c.edname == inventoryCargo.edname);
                    if (cargo == null)
                    {
                        // Strip out the stray from the manifest
                        _RemoveCargoWithEDName(inventoryCargo.edname);
                    }
                }
            }
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            _handleCommodityCollectedEvent(@event);
            writeInventory();
        }

        public void _handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
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
                        string type = haulageAmount.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
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
                cargo.need = CalculateCargoNeed(cargo);
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, 1);
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
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            _handleCommodityEjectedEvent(@event);
            writeInventory();
        }

        public void _handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                bool handled = false;

                // Check for related missions
                if (cargo.haulageamounts.Any())
                {
                    cargo.ejected += @event.amount;
                    foreach (HaulageAmount haulageAmount in cargo.haulageamounts)
                    {
                        string type = haulageAmount.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
                        int total = cargo.haulageamounts.Where(ha => ha.name.ToLowerInvariant().Contains(type)).Sum(ha => ha.amount);
                        switch (type)
                        {
                            case "altruism":
                            case "collect":
                            case "mining":
                            case "piracy":
                                {
                                    if (cargo.owned >= @event.amount)
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

                RemoveCargo(cargo);
            }
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            _handleCommodityPurchasedEvent(@event);
            writeInventory();
        }

        public void _handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
                cargo.need = CalculateCargoNeed(cargo);
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, @event.amount, @event.price);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            _handleCommodityRefinedEvent(@event);
            writeInventory();
        }

        public void _handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned++;
                cargo.need = CalculateCargoNeed(cargo);
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, 1);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = 1;
                AddCargo(newCargo);
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            _handleCommoditySoldEvent(@event);
            writeInventory();
        }

        public void _handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                if (@event.stolen)
                {
                    // Cargo is stolen
                    cargo.stolen -= Math.Min(cargo.stolen, @event.amount);
                }
                else if (@event.blackmarket)
                {
                    // Cargo is mission-related
                    int amount = Math.Min(cargo.haulage, @event.amount);
                    cargo.haulage -= amount;
                    cargo.ejected += amount;
                }
                else
                {
                    // Cargo is owned by the commander
                    cargo.owned -= Math.Min(cargo.owned, @event.amount);
                }
                RemoveCargo(cargo);
            }
        }

        private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            _handlePowerCommodityObtainedEvent(@event);
            writeInventory();
        }

        public void _handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
                cargo.need = CalculateCargoNeed(cargo);
            }
            else
            {
                Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, @event.amount);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
        }

        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            _handlePowerCommodityDeliveredEvent(@event);
            writeInventory();
        }

        public void _handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned -= @event.amount;
                RemoveCargo(cargo);
            }
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            _handleLimpetPurchasedEvent(@event);
            writeInventory();
        }

        public void _handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned += @event.amount;
            }
            else
            {
                Cargo newCargo = new Cargo("Drones", @event.amount, @event.price);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = @event.amount;
                AddCargo(newCargo);
            }
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            _handleLimpetSoldEvent(@event);
            writeInventory();
        }

        public void _handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            Cargo cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned -= @event.amount;
                RemoveCargo(cargo);
            }
        }

        private void handleLimpetLaunchedEvent(LimpetLaunchedEvent @event)
        {
            _handleLimpetLaunchedEvent();
            writeInventory();
        }

        public void _handleLimpetLaunchedEvent()
        {
            Cargo cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned--;
                RemoveCargo(cargo);
            }
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            _handleMissionAbandonedEvent(@event);
            writeInventory();
        }

        public void _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
                    switch (type)
                    {
                        case "delivery":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                // Calculate the amount of mission-related cargo still in inventory
                                int obtained = haulageAmount.amount - inventoryCargo.ejected;
                                obtained = Math.Min(inventoryCargo.haulage, obtained);

                                // Convert that amount of cargo from `haulage` to `stolen`
                                inventoryCargo.haulage -= obtained;
                                inventoryCargo.stolen += obtained;

                                // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                                inventoryCargo.need -= (haulageAmount.amount - obtained);

                                // We didn't fail for ejecting cargo so we set this counter to zero
                                inventoryCargo.ejected = 0;
                            }
                            break;
                    }
                    inventoryCargo.haulageamounts.Remove(haulageAmount);

                    RemoveCargo(inventoryCargo);
                    break;
                }
            }
        }

        private void handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            if (@event.commodityDefinition != null)
            {
                _handleMissionAcceptedEvent(@event);
                writeInventory();
            }
        }

        public void _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            string type = @event.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
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
                        Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
                        if (cargo != null)
                        {
                            cargo.haulage += amount;
                            cargo.haulageamounts.Add(haulageAmount);
                            cargo.need = CalculateCargoNeed(cargo);
                        }
                        else
                        {
                            Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, amount);
                            newCargo.haulage = amount;
                            newCargo.stolen = 0;
                            newCargo.owned = 0;
                            newCargo.haulageamounts.Add(haulageAmount);
                            newCargo.need = CalculateCargoNeed(newCargo);
                            AddCargo(newCargo);
                        }
                    }
                    break;
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.commodityDefinition != null || @event.commodityrewards != null)
            {
                _handleMissionCompletedEvent(@event);
                writeInventory();
            }
        }

        public void _handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                HaulageAmount haulageAmount = cargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAtOrDefault(1)
                        .ToLowerInvariant();
                    string subtype = @event.name.Split('_').ElementAtOrDefault(2)
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
                                if (subtype != null)
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
                                else
                                {
                                    cargo.haulage -= @event.amount ?? 0;
                                }
                            }
                            break;
                    }
                    cargo.haulageamounts.Remove(haulageAmount);
                }
                else if (cargo.haulage >= @event.amount)
                {
                    cargo.haulage -= @event.amount ?? 0;
                }
                else if (cargo.owned >= @event.amount)
                {
                    cargo.owned -= @event.amount ?? 0;
                }

                RemoveCargo(cargo);
            }

            foreach (CommodityAmount commodityReward in @event.commodityrewards)
            {
                cargo = GetCargoWithEDName(commodityReward.edname);
                if (cargo != null)
                {
                    cargo.owned += commodityReward.amount;
                    cargo.need = CalculateCargoNeed(cargo);
                }
                else
                {
                    Cargo newCargo = new Cargo(commodityReward.edname, commodityReward.amount);
                    newCargo.haulage = 0;
                    newCargo.stolen = 0;
                    newCargo.owned = commodityReward.amount;
                    AddCargo(newCargo);
                }
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            _handleMissionFailedEvent(@event);
            writeInventory();
        }

        public void _handleMissionFailedEvent(MissionFailedEvent @event)
        {
            foreach (Cargo inventoryCargo in inventory.ToList())
            {
                HaulageAmount haulageAmount = inventoryCargo.haulageamounts.FirstOrDefault(ha => ha.id == @event.missionid);
                if (haulageAmount != null)
                {
                    string type = @event.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
                    switch (type)
                    {
                        case "delivery":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                // Calculate the amount of mission-related cargo still in inventory
                                int obtained = haulageAmount.amount;
                                // If not expired, then failure may have been due to jettisoning cargo
                                if (haulageAmount.expiry < DateTime.Now)
                                {
                                    obtained -= inventoryCargo.ejected;
                                    inventoryCargo.ejected = 0;
                                }
                                obtained = Math.Min(inventoryCargo.haulage, obtained);

                                // Convert that amount of cargo from `haulage` to `stolen`
                                inventoryCargo.haulage -= obtained;
                                inventoryCargo.stolen += obtained;

                                // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                                inventoryCargo.need -= (haulageAmount.amount - obtained);
                            }
                            break;
                    }
                    inventoryCargo.haulageamounts.Remove(haulageAmount);
                    RemoveCargo(inventoryCargo);
                    break;
                }
            }
        }

        private void handleSearchAndRescueEvent(SearchAndRescueEvent @event)
        {
            _handleSearchAndRescueEvent(@event);
            writeInventory();
        }

        public void _handleSearchAndRescueEvent(SearchAndRescueEvent @event)
        {
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned -= Math.Min(cargo.owned, @event.amount ?? 0);
                RemoveCargo(cargo);
            }
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.synthesis.Contains("Limpet")) // At present, only a basic recipe exists. Basic recipe name: "Limpet Basic"
            {
                _handleSynthesisedEvent();
                writeInventory();
            }
        }

        public void _handleSynthesisedEvent()
        {
            Cargo cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned += 4;
            }
            else
            {
                Cargo newCargo = new Cargo("Drones", 4);
                newCargo.haulage = 0;
                newCargo.stolen = 0;
                newCargo.owned = 4;
                AddCargo(newCargo);
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            _handleTechnologyBrokerEvent(@event);
            writeInventory();
        }

        public void _handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            foreach (CommodityAmount commodityAmount in @event.commodities)
            {
                Cargo cargo = GetCargoWithEDName(commodityAmount.edname);
                if (cargo != null)
                {
                    cargo.owned -= Math.Min(cargo.owned, commodityAmount.amount);
                    RemoveCargo(cargo);
                }
            }
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
                EDDI.Instance.eventHandler(new CargoUpdatedEvent(DateTime.Now, cargocarried));
                configuration.cargo = inventory;
                configuration.cargocarried = cargocarried;
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(InventoryUpdatedEvent, inventory);
        }

        private void readInventory(CargoMonitorConfiguration configuration = null)
        {
            lock (inventoryLock)
            {
                // Obtain current cargo inventory from configuration
                configuration = configuration ?? CargoMonitorConfiguration.FromFile();
                cargocarried = configuration.cargocarried;

                // Build a new inventory
                List<Cargo> newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (Cargo cargo in configuration.cargo)
                {
                    if (cargo.commodityDef == null)
                    {
                        cargo.commodityDef = CommodityDefinition.FromEDName(cargo.edname);
                    }
                    cargo.need = CalculateCargoNeed(cargo);
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

        private void RemoveCargo(Cargo cargo)
        {
            // Check if missions are pending
            if (cargo.haulageamounts == null || !cargo.haulageamounts.Any())
            {
                if (cargo.total < 1)
                {
                    // All of the commodity was either expended, ejected, or sold
                    _RemoveCargoWithEDName(cargo.edname);
                }
            }
            else
            {
                cargo.need = CalculateCargoNeed(cargo);
            }
        }

        private void _RemoveCargoWithEDName(string edname)
        {
            lock (inventoryLock)
            {
                if (edname != null)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        if (inventory[i].edname == edname)
                        {
                            inventory.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            writeInventory();
        }

        private Cargo GetCargoWithEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }
            return inventory.FirstOrDefault(c => c.edname == edname);
        }

        private int CalculateCargoNeed(Cargo cargo)
        {
            int need = 0;
            if (cargo != null && cargo.haulageamounts != null && cargo.haulageamounts.Any())
            {
                int haulageNeeded = 0;
                int ownedNeeded = 0;
                int stolenNeeded = 0;
                foreach (HaulageAmount haulageAmount in cargo.haulageamounts)
                {
                    string type = haulageAmount.name.Split('_').ElementAtOrDefault(1)
                        .ToLowerInvariant();
                    string subtype = haulageAmount.name.Split('_').ElementAtOrDefault(2);
                    if (subtype != null)
                    {
                        subtype = subtype.ToLowerInvariant().Replace("$", "");
                    }
                    switch (type)
                    {
                        case "altruism":
                        case "collect":
                        case "mining":
                        case "piracy":
                            {
                                ownedNeeded += haulageAmount.amount;
                            }
                            break;
                        case "delivery":
                        case "rescue":
                        case "smuggle":
                            {
                                haulageNeeded += haulageAmount.amount;
                            }
                            break;
                        case "salvage":
                            {
                                if (subtype != null)
                                {
                                    if (subtype.Contains("illegal"))
                                    {
                                        stolenNeeded += haulageAmount.amount;
                                    }
                                    else
                                    {
                                        haulageNeeded += haulageAmount.amount;
                                    }
                                }
                                else
                                {
                                    haulageNeeded += haulageAmount.amount;
                                }
                            }
                            break;
                    }
                }
                need = (haulageNeeded > cargo.haulage) ? haulageNeeded - cargo.haulage : 0;
                need += (ownedNeeded > cargo.owned) ? ownedNeeded - cargo.owned : 0;
                need += (stolenNeeded > cargo.stolen) ? stolenNeeded - cargo.stolen : 0;
            }
            return need;
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
