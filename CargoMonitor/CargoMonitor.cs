using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
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

        private static Dictionary<string, string> CHAINED = new Dictionary<string, string>()
        {
            {"clearingthepath", "delivery"},
            {"helpfinishtheorder", "delivery"},
            {"rescuefromthetwins", "salvage"},
            {"rescuethewares", "salvage"}
        };

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
            else if (@event is MissionsEvent)
            {
                // Remove cargo haulage stragglers for completed missions
                handleMissionsEvent((MissionsEvent)@event);
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

        private void _handleCargoInventoryEvent(CargoInventoryEvent @event)
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
                        inventoryCargo.CalculateNeed();
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
                Cargo cargo = @event.inventory.FirstOrDefault(c => c.edname.ToLowerInvariant() == inventoryCargo.edname.ToLowerInvariant());
                if (cargo == null)
                {
                    if (inventoryCargo.haulageData == null || !inventoryCargo.haulageData.Any())
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

        private void _handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
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
                        int total = cargo.haulageData.Where(ha => ha.name.ToLowerInvariant().Contains(haulage.typeEDName)).Sum(ha => ha.amount);
                        switch (haulage.typeEDName)
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
                        haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                        haulage.sourcebody = EDDI.Instance?.CurrentStellarBody?.name;

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
                cargo.CalculateNeed();
            }
            else
            {
                cargo = new Cargo(@event.commodityDefinition?.edname, 1);
                if (@event.stolen)
                {
                    cargo.stolen = 1;
                }
                else
                {
                    cargo.owned = 1;
                }
                AddCargo(cargo);
            }
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            _handleCommodityEjectedEvent(@event);
            writeInventory();
        }

        private void _handleCommodityEjectedEvent(CommodityEjectedEvent @event)
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
                        switch (haulage.typeEDName)
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

        private void _handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
                cargo.CalculateNeed();
                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.typeEDName == "collect");
                if (haulage != null)
                {
                    haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                    haulage.sourcebody = EDDI.Instance?.CurrentStation?.name;
                }
            }
            else
            {
                cargo = new Cargo(@event.commodityDefinition?.edname, @event.amount, @event.price)
                {
                    stolen = 0,
                    owned = @event.amount
                };

                AddCargo(cargo);
            }
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            _handleCommodityRefinedEvent(@event);
            writeInventory();
        }

        private void _handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned++;
                cargo.CalculateNeed();
            }
            else
            {
                cargo = new Cargo(@event.commodityDefinition?.edname, 1)
                {
                    stolen = 0,
                    owned = 1
                };

                AddCargo(cargo);
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            _handleCommoditySoldEvent(@event);
            writeInventory();
        }

        private void _handleCommoditySoldEvent(CommoditySoldEvent @event)
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

        private void _handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                cargo.owned += @event.amount;
                cargo.CalculateNeed();
            }
            else
            {
                cargo = new Cargo(@event.commodityDefinition?.edname, @event.amount)
                {
                    stolen = 0,
                    owned = @event.amount
                };

                AddCargo(cargo);
            }
        }

        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            _handlePowerCommodityDeliveredEvent(@event);
            writeInventory();
        }

        private void _handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
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

        private void _handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned += @event.amount;
            }
            else
            {
                cargo = new Cargo("Drones", @event.amount, @event.price)
                {
                    stolen = 0,
                    owned = @event.amount
                };

                AddCargo(cargo);
            }
        }

        private void handleLimpetSoldEvent(LimpetSoldEvent @event)
        {
            _handleLimpetSoldEvent(@event);
            writeInventory();
        }

        private void _handleLimpetSoldEvent(LimpetSoldEvent @event)
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

        private void _handleLimpetLaunchedEvent()
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
            Cargo cargo = new Cargo();
            Haulage haulage = new Haulage();
            int amountRemaining = @event.totaltodeliver - @event.delivered;

            switch (@event.updatetype)
            {
                case "Collect":
                    {
                        cargo = GetCargoWithMissionId(@event.missionid ?? 0);
                        if (cargo != null)
                        {
                            // Cargo instantiated by either 'Mission accepted' event or previous 'WingUpdate' update
                            haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                            haulage.remaining = amountRemaining;

                            // Update commodity definition if instantiated by previous 'WingUpdate' update
                            if (cargo.commodityDef.edname == "Unknown")
                            {
                                cargo.commodityDef = @event.commodityDefinition;
                                haulage.originsystem = EDDI.Instance?.CurrentStarSystem?.name;
                            }
                        }
                        else
                        {
                            // First exposure to new cargo.
                            cargo = new Cargo(@event.commodityDefinition.edname, 0);
                            AddCargo(cargo);

                            string originSystem = EDDI.Instance?.CurrentStarSystem?.name;
                            haulage = new Haulage(@event.missionid ?? 0, "MISSION_DeliveryWing", originSystem, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                        }
                        cargo.haulage += @event.amount ?? 0;
                        cargo.CalculateNeed();
                        haulage.collected = @event.collected;
                        haulage.delivered = @event.delivered;
                        haulage.startmarketid = @event.startmarketid;
                        haulage.endmarketid = @event.endmarketid;
                    }
                    break;
                case "Deliver":
                    {
                        cargo = GetCargoWithMissionId(@event.missionid ?? 0);
                        if (cargo != null)
                        {
                            // Cargo instantiated by either 'Mission accepted' event, previous 'WingUpdate' or 'Collect' updates
                            haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                            if (haulage != null)
                            {
                                haulage.remaining = amountRemaining;

                                //Update commodity definition if intantiated by previous 'WingUpdate' update
                                if (cargo.commodityDef.edname == "Unknown")
                                {
                                    cargo.commodityDef = @event.commodityDefinition;
                                    haulage.originsystem = (@event.startmarketid == 0) ? EDDI.Instance?.CurrentStarSystem?.name : null;
                                }
                            }
                            else
                            {
                                string originSystem = (@event.startmarketid == 0) ? EDDI.Instance?.CurrentStarSystem?.name : null;
                                string type = (@event.startmarketid == 0) ? "MISSION_CollectWing" : "MISSION_DeliveryWing";
                                haulage = new Haulage(@event.missionid ?? 0, type, originSystem, amountRemaining, null);
                                cargo.haulageData.Add(haulage);
                            }
                        }
                        else
                        {
                            // Cargo instantiated by previous 'Market buy' event
                            cargo = GetCargoWithEDName(@event.commodityDefinition.edname);
                            if (cargo == null)
                            {
                                cargo = new Cargo(@event.commodityDefinition.edname, 0);
                                AddCargo(cargo);
                            }
                            string originSystem = (@event.startmarketid == 0) ? EDDI.Instance?.CurrentStarSystem?.name : null;
                            string type = (@event.startmarketid == 0) ? "MISSION_CollectWing" : "MISSION_DeliveryWing";
                            haulage = new Haulage(@event.missionid ?? 0, type, originSystem, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                        }

                        if (haulage.typeEDName.Contains("delivery"))
                        {
                            cargo.haulage -= Math.Min(@event.amount ?? 0, cargo.haulage);
                        }
                        else
                        {
                            cargo.owned -= Math.Min(@event.amount ?? 0, cargo.owned);
                        }
                        cargo.CalculateNeed();
                        haulage.collected = @event.collected;
                        haulage.delivered = @event.delivered;
                        haulage.endmarketid = (haulage.endmarketid == 0) ? @event.endmarketid : haulage.endmarketid;

                        // Check for mission completion
                        if (amountRemaining == 0)
                        {
                            if (haulage.shared)
                            {
                                cargo.haulageData.Remove(haulage);
                                RemoveCargo(cargo);
                            }
                            else
                            {
                                haulage.status = "Complete";
                            }
                        }
                    }
                    break;
                case "WingUpdate":
                    {
                        cargo = GetCargoWithMissionId(@event.missionid ?? 0);
                        if (cargo != null)
                        {
                            // Cargo instantiated by either 'Mission accepted' event, previous 'WingUpdate' or 'Collect' updates
                            haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                            haulage.remaining = amountRemaining;
                        }
                        else
                        {
                            // First exposure to new cargo, use 'Unknown' as placeholder
                            cargo = new Cargo("Unknown", 0);
                            AddCargo(cargo);
                            string type = (@event.startmarketid == 0) ? "MISSION_CollectWing" : "MISSION_DeliveryWing";
                            haulage = new Haulage(@event.missionid ?? 0, type, null, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                        }

                        int amount = Math.Max(@event.collected - haulage.collected, @event.delivered - haulage.delivered);
                        if (amount > 0)
                        {
                            string updatetype = @event.collected > haulage.collected ? "Collect" : "Deliver";
                            EDDI.Instance.eventHandler(new CargoWingUpdateEvent(DateTime.Now, haulage.missionid, updatetype, cargo.commodityDef, amount, @event.collected, @event.delivered, @event.totaltodeliver));
                            cargo.CalculateNeed();
                            haulage.collected = @event.collected;
                            haulage.delivered = @event.delivered;
                            if (updatetype == "Collect" && haulage.startmarketid == 0)
                            {
                                haulage.startmarketid = @event.startmarketid;
                                haulage.endmarketid = @event.endmarketid;
                            }
                            else if (updatetype == "Deliver" && haulage.endmarketid == 0)
                            {
                                haulage.endmarketid = @event.endmarketid;
                            }
                        }

                        // Check for mission completion
                        if (amountRemaining == 0)
                        {
                            if (haulage.shared)
                            {
                                cargo.haulageData.Remove(haulage);
                                RemoveCargo(cargo);
                            }
                            else
                            {
                                haulage.status = "Complete";
                            }
                        }
                    }
                    break;
            }
        }

        private void handleMissionsEvent(MissionsEvent @event)
        {
            _handleMissionsEvent(@event);
            writeInventory();
        }

        private void _handleMissionsEvent(MissionsEvent @event)
        {
            foreach (Cargo cargo in inventory.ToList())
            {
                foreach (Haulage haulage in cargo.haulageData.ToList())
                {
                    Mission mission = @event.missions.FirstOrDefault(m => m.missionid == haulage.missionid);
                    if (mission == null)
                    {
                        cargo.haulageData.Remove(haulage);
                    }
                }
            }
        }

        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            _handleMissionAbandonedEvent(@event);
            writeInventory();
        }

        private void _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            Haulage haulage = GetHaulageWithMissionId(@event.missionid ?? 0);
            if (haulage != null)
            {
                Cargo cargo = GetCargoWithMissionId(@event.missionid ?? 0);
                switch (haulage.typeEDName)
                {
                    case "delivery":
                    case "deliverywing":
                    case "rescue":
                    case "salvage":
                    case "smuggle":
                        {
                            // Calculate the amount of mission-related cargo still in inventory
                            int obtained = haulage.remaining - cargo.ejected;
                            obtained = Math.Min(cargo.haulage, obtained);

                            // Convert that amount of cargo from `haulage` to `stolen`
                            cargo.haulage -= obtained;
                            cargo.stolen += obtained;

                            // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                            cargo.need -= (haulage.remaining - obtained);

                            // We didn't fail for ejecting cargo so we set this counter to zero
                            cargo.ejected = 0;
                        }
                        break;
                }
                cargo.haulageData.Remove(haulage);
                RemoveCargo(cargo);
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

        private void _handleMissionAcceptedEvent(MissionAcceptedEvent @event)
        {
            Cargo cargo = new Cargo();

            string type = @event.name.Split('_').ElementAt(1)?.ToLowerInvariant();
            if (type != null && CHAINED.TryGetValue(type, out string value))
            {
                type = value;
            }
            else if (type == "ds" || type == "rs" || type == "welcome")
            {
                type = @event.name.Split('_').ElementAt(2)?.ToLowerInvariant();
            }

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
                        int amount = (type == "delivery" && naval || type == "smuggle") ? @event.amount ?? 0 : 0;
                        string originSystem = EDDI.Instance?.CurrentStarSystem?.name;
                        Haulage haulage = new Haulage(@event.missionid ?? 0, @event.name, originSystem, @event.amount ?? 0, @event.expiry)
                        {
                            startmarketid = (type.Contains("delivery") && !naval) ? EDDI.Instance?.CurrentStation?.marketId ?? 0 : 0,
                            endmarketid = (type.Contains("collect")) ? EDDI.Instance?.CurrentStation?.marketId ?? 0 : 0,
                        };
                        if (type.Contains("delivery") || type == "smuggle")
                        {
                            haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                            haulage.sourcebody = EDDI.Instance?.CurrentStation?.name;
                        }
                        else if (type == "rescue" || type == "salvage")
                        {
                            haulage.sourcesystem = @event.destinationsystem;
                        }

                        cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
                        if (cargo != null)
                        {
                            cargo.haulage += amount;
                            cargo.haulageData.Add(haulage);
                            cargo.CalculateNeed();
                        }
                        else
                        {
                            cargo = new Cargo(@event.commodityDefinition?.edname, 0)
                            {
                                haulage = amount
                            };
                            cargo.haulageData.Add(haulage);
                            cargo.CalculateNeed();
                            AddCargo(cargo);
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

        private void _handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    int amount = Math.Min(haulage.remaining, @event.amount ?? 0);
                    switch (haulage.typeEDName)
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
                                if (haulage.legal)
                                {
                                    cargo.haulage -= amount;
                                }
                                else
                                {
                                    cargo.stolen -= amount;
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
                    cargo.CalculateNeed();
                }
                else
                {
                    cargo = new Cargo(commodityReward.edname, commodityReward.amount)
                    {
                        stolen = 0,
                        owned = commodityReward.amount
                    };

                    AddCargo(cargo);
                }
            }
        }

        private void handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            _handleMissionExpiredEvent(@event);
            writeInventory();
        }

        private void _handleMissionExpiredEvent(MissionExpiredEvent @event)
        {
            Haulage haulage = GetHaulageWithMissionId(@event.missionid ?? 0);
            if (haulage != null)
            {
                haulage.status = "Failed";
            }
        }

        private void handleMissionFailedEvent(MissionFailedEvent @event)
        {
            _handleMissionFailedEvent(@event);
            writeInventory();
        }

        private void _handleMissionFailedEvent(MissionFailedEvent @event)
        {
            Haulage haulage = GetHaulageWithMissionId(@event.missionid ?? 0);
            if (haulage != null)
            {
                Cargo cargo = GetCargoWithMissionId(@event.missionid ?? 0);
                switch (haulage.typeEDName)
                {
                    case "delivery":
                    case "deliverywing":
                    case "rescue":
                    case "salvage":
                    case "smuggle":
                        {
                            // Calculate the amount of mission-related cargo still in inventory
                            int obtained = haulage.remaining;

                            // If not expired, then failure may have been due to jettisoning cargo
                            if (haulage.expiry < DateTime.Now)
                            {
                                obtained -= cargo.ejected;
                                cargo.ejected = 0;
                            }
                            obtained = Math.Min(cargo.haulage, obtained);

                            // Convert that amount of cargo from `haulage` to `stolen`
                            cargo.haulage -= obtained;
                            cargo.stolen += obtained;

                            // Reduce our `need` counter by the amount of mission related cargo which had not yet been obtained.
                            cargo.need -= (haulage.remaining - obtained);
                        }
                        break;
                }
                cargo.haulageData.Remove(haulage);
                RemoveCargo(cargo);
            }
        }

        private void handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            _handleMissionRedirectedEvent(@event);
            writeInventory();
        }

        private void _handleMissionRedirectedEvent(MissionRedirectedEvent @event)
        {
            // Adjust cargo haulage & stolen amounts to account for completed missions
            Haulage haulage = GetHaulageWithMissionId(@event.missionid ?? 0);
            if (haulage != null)
            {
                if (@event.newdestinationsystem == haulage.originsystem)
                {
                    Cargo cargo = GetCargoWithMissionId(@event.missionid ?? 0);
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

        private void handleSearchAndRescueEvent(SearchAndRescueEvent @event)
        {
            _handleSearchAndRescueEvent(@event);
            writeInventory();
        }

        private void _handleSearchAndRescueEvent(SearchAndRescueEvent @event)
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

        private void _handleSynthesisedEvent()
        {
            Cargo cargo = new Cargo();
            cargo = GetCargoWithEDName("Drones");
            if (cargo != null)
            {
                cargo.owned += 4;
            }
            else
            {
                cargo = new Cargo("Drones", 4)
                {
                    stolen = 0,
                    owned = 4
                };

                AddCargo(cargo);
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            _handleTechnologyBrokerEvent(@event);
            writeInventory();
        }

        private void _handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
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

                int sum = inventory.Sum(c => c.total);
                if (cargoCarried != sum)
                {
                    cargoCarried = sum;
                    EDDI.Instance.eventHandler(new CargoUpdatedEvent(DateTime.UtcNow, cargoCarried));
                }
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
                    cargo.CalculateNeed();
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
                cargo.CalculateNeed();
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

        public Cargo GetCargoWithMissionId(long missionid)
        {
            foreach (Cargo cargo in inventory.ToList())
            {
                if (cargo.haulageData.FirstOrDefault(ha => ha.missionid == missionid) != null)
                {
                    return cargo;
                }
            }
            return null;
        }

        public Haulage GetHaulageWithMissionId(long missionid)
        {
            foreach (Cargo cargo in inventory.ToList())
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == missionid);
                if (haulage != null)
                {
                    return haulage;
                }
            }
            return null;
        }

        public string GetSourceRoute(string system = null)
        {
            int missionsCount = inventory.Sum(c => c.haulageData.Count());

            // Missions Route Event variables
            decimal sourceDistance = 0;
            string sourceSystem = null;
            string sourceSystems = null;
            List<string> sourceList = new List<string>();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missionsCount > 0)
            {
                var sourceDict = new SortedDictionary<long, string>();
                string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
                bool fromHere = system == currentSystem;
                StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(currentSystem, true);
                StarSystem dest = new StarSystem();             // Destination star system

                foreach (Cargo cargo in inventory.Where(c => c.haulageData.Any()).ToList())
                {
                    foreach (Haulage haulage in cargo.haulageData.Where(h => h.status == "Active" && h.sourcesystem != null).ToList())
                    {
                        if (fromHere && haulage.originsystem != currentSystem)
                        {
                            break;
                        }

                        dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(haulage.sourcesystem, true);
                        long distance = (long)(CalculateDistance(curr, dest) * 100);
                        if (!sourceDict.TryGetValue(distance, out string val))
                        {
                            sourceDict.Add(distance, haulage.sourcesystem);
                        }
                        missionids.Add(haulage.missionid);
                    }
                }

                if (sourceDict != null)
                {
                    sourceList = sourceDict.Values.ToList();
                    sourceSystem = sourceList[0];
                    sourceDistance = (decimal)sourceDict.Keys.FirstOrDefault() / 100;
                    sourceSystems = string.Join("_", sourceList);
                }
            }
            EDDI.Instance.eventHandler(new MissionsRouteEvent(DateTime.Now, "source", sourceSystem, sourceSystems, sourceList.Count(), sourceDistance, 0, missionids));
            return sourceSystem;
        }

        private decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            return (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(curr.x - dest.x), 2)
                + Math.Pow((double)(curr.y - dest.y), 2)
                + Math.Pow((double)(curr.z - dest.z), 2)), 2);
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
