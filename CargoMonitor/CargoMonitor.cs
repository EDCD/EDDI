using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiMissionMonitor;
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
        public int cargoCarried;

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
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle mission related events after the mission object has been created
            if (@event is MissionExpiredEvent)
            {
                // Check to see if this is a cargo mission and update our inventory accordingly
                handleMissionExpiredEvent((MissionExpiredEvent)@event);
            }
            else if (@event is MissionRedirectedEvent)
            {
                handleMissionRedirectedEvent((MissionRedirectedEvent)@event);
            }
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
            else if (@event is CargoDepotEvent)
            {
                // If cargo is collected or delivered in a wing mission
                handleCargoDepotEvent((CargoDepotEvent)@event);
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
            else if (@event is DiedEvent)
            {
                handleDiedEvent((DiedEvent)@event);
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
                Cargo inventoryCargo = inventory.FirstOrDefault(c => c.edname.ToLowerInvariant() == cargo.edname.ToLowerInvariant());
                if (inventoryCargo != null)
                {
                    // Found match of commodity
                    inventoryCargo.total = cargo.total;
                    inventoryCargo.stolen = cargo.stolen;
                    if (inventoryCargo.haulageData == null || !inventoryCargo.haulageData.Any())
                    {
                        inventoryCargo.haulage = 0;
                        inventoryCargo.need = 0;
                    }
                    else
                    {
                        CalculateCargoNeed(inventoryCargo);
                    }
                    inventoryCargo.owned = cargo.total - cargo.stolen - inventoryCargo.haulage;
                    inventoryCargo.ejected = 0;
                }
                else
                {
                    AddCargo(cargo);
                }
            }
            // Remove strays from the manifest
            foreach (Cargo inventoryCargo in inventory.ToList())
            {
                // Keep cargo in manifest if missions are pending
                if (inventoryCargo.haulageData == null || !inventoryCargo.haulageData.Any())
                {
                    // Check for pending missions
                    if (inventoryCargo.haulageamounts == null || !inventoryCargo.haulageamounts.Any())
                    {
                        // Strip out the stray from the manifest
                        _RemoveCargoWithEDName(inventoryCargo.edname);
                    }
                    else
                    {
                        // Keep cargo entry in manifest with zeroed amounts, if missions are pending
                        inventoryCargo.haulage = 0;
                        inventoryCargo.owned = 0;
                        inventoryCargo.stolen = 0;
                        inventoryCargo.ejected = 0;
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
                else if (cargo.haulageData.Any())
                {
                    foreach (Haulage haulage in cargo.haulageData)
                    {
                        int total = cargo.haulageData.Where(ha => ha.name.ToLowerInvariant().Contains(haulage.type)).Sum(ha => ha.amount);
                        switch (haulage.type)
                        {
                            case "altruism":
                            case "collect":
                            case "collectwing":
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
                CalculateCargoNeed(cargo);
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
                if (cargo.haulageData.Any())
                {
                    cargo.ejected += @event.amount;
                    foreach (Haulage haulage in cargo.haulageData)
                    {
                        switch (haulage.type)
                        {
                            case "altruism":
                            case "collect":
                            case "collectwing":
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
                            case "deliverywing":
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
                CalculateCargoNeed(cargo);
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
                CalculateCargoNeed(cargo);
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
                else if (@event.blackmarket && !@event.illegal)
                {
                    // Assume cargo is mission-related
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
                CalculateCargoNeed(cargo);
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

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            _handleCargoDepotEvent(@event);
            writeInventory();
        }

        private void _handleCargoDepotEvent(CargoDepotEvent @event)
        {
            int amountRemaining = @event.totaltodeliver - @event.delivered;
            foreach (Cargo cargo in inventory)
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    switch (@event.updatetype)
                    {
                        case "Collect":
                            {
                                if (haulage.type == "DeliveryWing")
                                {
                                    cargo.haulage += @event.amount ?? 0;
                                }
                                CalculateCargoNeed(cargo);
                            }
                            break;
                        case "Deliver":
                            {
                                if (haulage.type == "CollectWing")
                                {
                                    cargo.owned -= @event.amount ?? 0;
                                }
                                else if (haulage.type == "DeliveryWing")
                                {
                                    cargo.haulage -= @event.amount ?? 0;
                                }
                                haulage.amount = amountRemaining;
                                CalculateCargoNeed(cargo);
                            }
                            break;
                        case "WingUpdate":
                            {
                                haulage.amount = amountRemaining;
                                CalculateCargoNeed(cargo);
                            }
                            break;
                    }
                    break;
                }
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
                Haulage haulage = inventoryCargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    switch (haulage.type)
                    {
                        case "delivery":
                        case "deliverywing":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                // Calculate the amount of mission-related cargo still in inventory
                                int obtained = haulage.amount - inventoryCargo.ejected;
                                obtained = Math.Min(inventoryCargo.haulage, obtained);

                                // Convert that amount of cargo from `haulage` to `stolen`
                                inventoryCargo.haulage -= obtained;
                                inventoryCargo.stolen += obtained;

                                // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                                inventoryCargo.need -= (haulage.amount - obtained);

                                // We didn't fail for ejecting cargo so we set this counter to zero
                                inventoryCargo.ejected = 0;
                            }
                            break;
                    }
                    inventoryCargo.haulageData.Remove(haulage);

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
            bool naval = @event.name.ToLowerInvariant().Contains("rank");
            switch (type)
            {
                case "altruism":
                case "collect":
                case "collectwing":
                case "delivery":
                case "deliverywing":
                case "mining":
                case "piracy":
                case "rescue":
                case "salvage":
                case "smuggle":
                    {
<<<<<<< HEAD
                        int amount = (type == "delivery" && naval || type == "smuggle") ? @event.amount ?? 0 : 0;
                        HaulageAmount haulageAmount = new HaulageAmount(@event.missionid ?? 0, @event.name, @event.amount ?? 0, (DateTime)@event.expiry);
=======
                        string originSystem = EDDI.Instance?.CurrentStarSystem?.name;
                        int amount = (type == "delivery" || type == "smuggle") ? @event.amount ?? 0 : 0;
                        Haulage haulage = new Haulage(@event.missionid ?? 0, @event.name, originSystem, @event.amount ?? 0, (DateTime)@event.expiry);
>>>>>>> Tweaks. Added localised mission name.
                        Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
                        if (cargo != null)
                        {
                            cargo.haulage += amount;
                            cargo.haulageData.Add(haulage);
                            CalculateCargoNeed(cargo);
                        }
                        else
                        {
                            Cargo newCargo = new Cargo(@event.commodityDefinition?.edname, amount);
                            newCargo.haulage = amount;
                            newCargo.stolen = 0;
                            newCargo.owned = 0;
                            newCargo.haulageData.Add(haulage);
                            CalculateCargoNeed(newCargo);
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
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    int amount = Math.Min(haulageAmount.amount, @event.amount ?? 0);
                    string type = @event.name.Split('_').ElementAtOrDefault(1)
                        .ToLowerInvariant();
                    bool legal = @event.name.ToLowerInvariant().Contains("illegal") ? false : true;
                    switch (type)
                    {
                        case "altruism":
                        case "collect":
                        case "mining":
                            {
                                cargo.owned -= amount;
                            }
                            break;
                        case "delivery":
                        case "rescue":
                        case "smuggle":
                            {
                                cargo.haulage -= amount;
                            }
                            break;
                        case "piracy":
                            {
                                if (cargo.stolen < amount)
                                {
                                    cargo.owned -= amount;
                                }
                                else
                                {
                                    cargo.stolen -= amount;
                                }
                            }
                            break;
                        case "salvage":
                            {
                                if (legal)
                                {
                                    cargo.haulage -= @event.amount ?? 0;
                                }
                                else
                                {
                                    cargo.stolen -= @event.amount ?? 0;
                                }
                            }
                            break;
                    }
                    cargo.haulageData.Remove(haulage);
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
                    CalculateCargoNeed(cargo);
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

        private void handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            _handleMissionExpiredEvent(@event);
            writeInventory();
        }

        public void _handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            foreach (Cargo Cargo in inventory.ToList())
            {
                Haulage haulage = Cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    haulage.status = "Failed";
                    break;
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
                Haulage haulage = inventoryCargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    string type = @event.name.Split('_').ElementAtOrDefault(1).ToLowerInvariant();
                    switch (type)
                    {
                        case "delivery":
                        case "deliverywing":
                        case "rescue":
                        case "salvage":
                        case "smuggle":
                            {
                                // Calculate the amount of mission-related cargo still in inventory
                                int obtained = haulage.amount;
                                // If not expired, then failure may have been due to jettisoning cargo
<<<<<<< HEAD
                                if (haulageAmount.expiry < DateTime.UtcNow)
=======
                                if (haulage.expiry < DateTime.Now)
>>>>>>> Tweaks. Added localised mission name.
                                {
                                    obtained -= inventoryCargo.ejected;
                                    inventoryCargo.ejected = 0;
                                }
                                obtained = Math.Min(inventoryCargo.haulage, obtained);

                                // Convert that amount of cargo from `haulage` to `stolen`
                                inventoryCargo.haulage -= obtained;
                                inventoryCargo.stolen += obtained;

                                // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                                inventoryCargo.need -= (haulage.amount - obtained);
                            }
                            break;
                    }
                    inventoryCargo.haulageData.Remove(haulage);
                    RemoveCargo(inventoryCargo);
                    break;
                }
            }
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            _handleMissionRedirectedEvent(@event);
            writeInventory();
        }

        public void _handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            // Adjust cargo haulage & stolen amounts to account for completed missions
            foreach (Cargo cargo in inventory.ToList())
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    if (@event.newdestinationsystem == haulage.originsystem)
                    {
                        haulage.status = "Complete";
                        int haulageAmount = cargo.haulageData
                            .Where(ha => ha.status == "Complete" && ha.legal)
                            .Sum(ha => ha.amount);
                        int stolenAmount = cargo.haulageData
                            .Where(ha => ha.status == "Complete" && !ha.legal)
                            .Sum(ha => ha.amount);

                        int total = cargo.total;
                        cargo.haulage = Math.Max(cargo.haulage, haulageAmount);
                        cargo.stolen = Math.Max(cargo.stolen, stolenAmount);
                        cargo.owned = total - cargo.haulage - cargo.stolen;
                    }
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
            Cargo cargo = GetCargoWithEDName(@event.commodity?.edname);
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

        private void handleDiedEvent(DiedEvent @event)
        {
            inventory.Clear();
            writeInventory();
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["inventory"] = new List<Cargo>(inventory),
                ["cargoCarried"] = cargoCarried
    };
            return variables;
        }

        public void writeInventory()
        {
            lock (inventoryLock)
            {
                // Write cargo configuration with current inventory
                CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
                cargoCarried = 0;
                foreach (Cargo cargo in inventory)
                {
                    cargoCarried += cargo.total;
                }
                EDDI.Instance.eventHandler(new CargoUpdatedEvent(DateTime.UtcNow, cargoCarried));
                configuration.cargo = inventory;
                configuration.cargocarried = cargoCarried;
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
                cargoCarried = configuration.cargocarried;

                // Build a new inventory
                List<Cargo> newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (Cargo cargo in configuration.cargo)
                {
                    if (cargo.commodityDef == null)
                    {
                        cargo.commodityDef = CommodityDefinition.FromEDName(cargo.edname);
                    }
                    CalculateCargoNeed(cargo);
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
            if (cargo.haulageData == null || !cargo.haulageData.Any())
            {
                if (cargo.total < 1)
                {
                    // All of the commodity was either expended, ejected, or sold
                    _RemoveCargoWithEDName(cargo.edname);
                }
            }
            else
            {
                CalculateCargoNeed(cargo);
            }
        }

        private void _RemoveCargoWithEDName(string edname)
        {
            lock (inventoryLock)
            {
                if (edname != null)
                {
                    edname = edname.ToLowerInvariant();
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        if (inventory[i].edname.ToLowerInvariant() == edname)
                        {
                            inventory.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            writeInventory();
        }

        public Cargo GetCargoWithEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }
            edname = edname.ToLowerInvariant();
            return inventory.FirstOrDefault(c => c.edname.ToLowerInvariant() == edname);
        }

        private void CalculateCargoNeed(Cargo cargo)
        {
            if (cargo != null && cargo.haulageData != null && cargo.haulageData.Any())
            {
<<<<<<< HEAD
                if (cargo.haulageamounts.FirstOrDefault(ha => ha.id == missionid) != null)
=======
                int haulageNeeded = 0;
                int ownedNeeded = 0;
                int stolenNeeded = 0;
                foreach (Haulage haulage in cargo.haulageData)
>>>>>>> Tweaks. Added localised mission name.
                {
                    switch (haulage.type)
                    {
                        case "altruism":
                        case "collect":
                        case "collectwing":
                        case "mining":
                        case "piracy":
                            {
                                ownedNeeded += haulage.amount;
                            }
                            break;
                        case "delivery":
                        case "deliverywing":
                        case "rescue":
                        case "smuggle":
                            {
                                haulageNeeded += haulage.amount;
                            }
                            break;
                        case "salvage":
                            if (haulage.legal)
                            {
                                haulageNeeded += haulage.amount;
                            }
                            else
                            {
                                stolenNeeded += haulage.amount;
                            }
                            break;
                    }
                }
                cargo.need = (haulageNeeded > cargo.haulage) ? haulageNeeded - cargo.haulage : 0;
                cargo.need += (ownedNeeded > cargo.owned) ? ownedNeeded - cargo.owned : 0;
                cargo.need += (stolenNeeded > cargo.stolen) ? stolenNeeded - cargo.stolen : 0;
            }
        }

        public HaulageAmount GetHaulageWithMissionId(long missionid)
        {
            foreach (Cargo cargo in inventory.ToList())
            {
                HaulageAmount haulageamount = cargo.haulageamounts.FirstOrDefault(ha => ha.id == missionid);
                if (haulageamount != null)
                {
                    return haulageamount;
                }
            }
            return null;
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
