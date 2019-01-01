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
        private bool checkHaulage = false;

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
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is CargoEvent)
            {
                handleCargoEvent((CargoEvent)@event);
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
            else if (@event is DiedEvent)
            {
                handleDiedEvent((DiedEvent)@event);
            }
            else if (@event is EngineerContributedEvent)
            {
                handleEngineerContributedEvent((EngineerContributedEvent)@event);
            }
        }

        private void handleCargoEvent(CargoEvent @event)
        {
            if(_handleCargoEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleCargoEvent(CargoEvent @event)
        {
            bool cargoUpdated = false;
            if (@event.vessel == Constants.VEHICLE_SHIP)
            {
                cargoCarried = @event.cargocarried;
                if (@event.inventory != null)
                {
                    List<CargoInfo> infoList = @event.inventory.ToList();

                    // Remove strays from the manifest
                    foreach (Cargo inventoryCargo in inventory.ToList())
                    {
                        CargoInfo info = @event.inventory.FirstOrDefault(i => i.name == inventoryCargo.edname.ToLowerInvariant());
                        if (info == null)
                        {
                            if (inventoryCargo.haulageData == null || !inventoryCargo.haulageData.Any())
                            {
                                // Strip out the stray from the manifest
                                _RemoveCargoWithEDName(inventoryCargo.edname);
                                if (@event.update) { return true; }
                            }
                            else
                            {
                                // Keep cargo entry in manifest with zeroed amounts, if missions are pending
                                inventoryCargo.total = 0;
                                inventoryCargo.haulage = 0;
                                inventoryCargo.owned = 0;
                                inventoryCargo.stolen = 0;
                                inventoryCargo.CalculateNeed();
                            }
                            cargoUpdated = true;
                        }
                    }

                    while (infoList.Count() > 0)
                    {
                        string name = infoList.ToList().First().name;
                        List<CargoInfo> cargoInfo = infoList.Where(i => i.name == name).ToList();
                        Cargo cargo = inventory.FirstOrDefault(c => c.edname.ToLowerInvariant() == name);
                        if (cargo != null)
                        {
                            int total = cargoInfo.Sum(i => i.count);
                            int stolen = infoList.Where(i => i.missionid == null).Sum(i => i.stolen);
                            int missionCount = infoList.Where(i => i.missionid != null).Count();
                            if (total != cargo.total || stolen != cargo.stolen || missionCount != cargo.haulageData.Count())
                            {
                                UpdateCargoFromInfo(cargo, cargoInfo);
                                if (@event.update) { return true; }
                                cargoUpdated = true;
                            }
                        }
                        else
                        {
                            // Add cargo entries for those missing
                            cargo = new Cargo(name, 0);
                            UpdateCargoFromInfo(cargo, cargoInfo);
                            AddCargo(cargo);
                            cargoUpdated = true;
                        }

                        infoList.RemoveAll(i => i.name == name);
                    }
                }
            }
            return cargoUpdated;
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            if (_handleCommodityCollectedEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            bool update = false;
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                if (EDDI.Instance?.Vehicle != Constants.VEHICLE_SHIP)
                {
                    if (@event.missionid != null)
                    {
                        cargo.haulage++;
                    }
                    else if (@event.stolen)
                    {
                        cargo.stolen++;
                    }
                    else
                    {
                        cargo.owned++;
                    }
                    cargo.CalculateNeed();
                    update = true;
                }

                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == @event.missionid);
                if (haulage != null)
                {
                    switch (haulage.typeEDName)
                    {
                        case "mining":
                        case "piracy":
                        case "rescue":
                        case "salvage":
                            {
                                haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                                haulage.sourcebody = EDDI.Instance?.CurrentStellarBody?.name;
                                update = true;
                            }
                            break;
                    }
                }
            }
            return update;
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            if(_handleCommodityEjectedEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            bool update = false;
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                if (EDDI.Instance?.Vehicle != Constants.VEHICLE_SHIP)
                {
                    if (@event.missionid != null)
                    {
                        cargo.haulage -= @event.amount;
                    }
                    else
                    {
                        cargo.owned -= @event.amount;
                    }
                    cargo.CalculateNeed();
                    update = true;
                }

                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == @event.missionid);
                if (haulage != null)
                {
                    switch (haulage.typeEDName)
                    {
                        case "delivery":
                        case "deliverywing":
                        case "smuggle":
                            {
                                haulage.status = "Failed";
                                Mission mission = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))
                                    .GetMissionWithMissionId(@event.missionid ?? 0);
                                if (mission != null)
                                {
                                    mission.statusDef = MissionStatus.FromEDName("Failed");
                                }
                                update = true;
                            }
                            break;
                    }
                }
            }
            return update;
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            if(_handleCommodityPurchasedEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            bool update = false;
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.typeEDName
                    .ToLowerInvariant()
                    .Contains("collect"));
                if (haulage != null)
                {
                    haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                    haulage.sourcebody = EDDI.Instance?.CurrentStation?.name;
                    update = true;
                }
            }
            return update;
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            _handleCommodityRefinedEvent(@event);
            writeInventory();
        }

        private bool _handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            bool update = false;
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.typeEDName
                    .ToLowerInvariant()
                    .Contains("mining"));
                if (haulage != null)
                {
                    haulage.sourcesystem = EDDI.Instance?.CurrentStarSystem?.name;
                    haulage.sourcebody = EDDI.Instance?.CurrentStation?.name;
                    update = true;
                }
            }
            return update;
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            if(_handleCommoditySoldEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            bool update = false;
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                // Flag event to check for a failed mission in following 'Cargo' event
                checkHaulage = true;
            }
            return update;
        }

        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            _handleCargoDepotEvent(@event);
            writeInventory();
        }

        private void _handleCargoDepotEvent(CargoDepotEvent @event)
        {
            Mission mission = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))
                .GetMissionWithMissionId(@event.missionid ?? 0);
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

                            // Update commodity definition if instantiated other than 'Mission accepted'
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
                            string name = mission?.name ?? "MISSION_DeliveryWing";
                            haulage = new Haulage(@event.missionid ?? 0, name, originSystem, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                        }
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

                                //Update commodity definition
                                if (cargo.commodityDef.edname == "Unknown")
                                {
                                    haulage.amount = @event.totaltodeliver;
                                    cargo.commodityDef = @event.commodityDefinition;
                                    haulage.originsystem = (@event.startmarketid == 0) ? EDDI.Instance?.CurrentStarSystem?.name : null;
                                }
                            }
                            else
                            {
                                string originSystem = (@event.startmarketid == 0) ? EDDI.Instance?.CurrentStarSystem?.name : null;
                                string name = mission?.name ?? (@event.startmarketid == 0 ? "MISSION_CollectWing" : "MISSION_DeliveryWing");
                                haulage = new Haulage(@event.missionid ?? 0, name, originSystem, amountRemaining, null);
                                cargo.haulageData.Add(haulage);
                            }
                            cargo.CalculateNeed();
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
                            string name = mission?.name ?? (@event.startmarketid == 0 ? "MISSION_CollectWing" : "MISSION_DeliveryWing");
                            haulage = new Haulage(@event.missionid ?? 0, name, originSystem, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                            cargo.CalculateNeed();
                        }
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
                            string name = mission?.name ?? (@event.startmarketid == 0 ? "MISSION_CollectWing" : "MISSION_DeliveryWing");
                            haulage = new Haulage(@event.missionid ?? 0, name, null, amountRemaining, null, true);
                            cargo.haulageData.Add(haulage);
                        }
                        cargo.CalculateNeed();

                        int amount = Math.Max(@event.collected - haulage.collected, @event.delivered - haulage.delivered);
                        if (amount > 0)
                        {
                            string updatetype = @event.collected > haulage.collected ? "Collect" : "Deliver";
                            EDDI.Instance.eventHandler(new CargoWingUpdateEvent(DateTime.Now, haulage.missionid, updatetype, cargo.commodityDef, amount, @event.collected, @event.delivered, @event.totaltodeliver));
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
            if (_handleMissionsEvent(@event))
            {
                writeInventory();
            }
        }

        private bool _handleMissionsEvent(MissionsEvent @event)
        {
            bool update = false;
            foreach (Cargo cargo in inventory.ToList())
            {
                foreach (Haulage haulage in cargo.haulageData.ToList())
                {
                    Mission mission = @event.missions.FirstOrDefault(m => m.missionid == haulage.missionid);
                    if (mission == null)
                    {
                        cargo.haulageData.Remove(haulage);
                        update = true;
                    }
                }
            }
            return update;
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
                int onboard = haulage.remaining - haulage.need;
                cargo.haulage -= onboard;
                cargo.stolen += onboard;
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
                        if (cargo == null)
                        {
                            cargo = new Cargo(@event.commodityDefinition?.edname, 0);
                            AddCargo(cargo);
                        }
                        cargo.haulageData.Add(haulage);
                        cargo.CalculateNeed();
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
            Cargo cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if (cargo != null)
            {
                Haulage haulage = cargo.haulageData.FirstOrDefault(ha => ha.missionid == @event.missionid);
                if (haulage != null)
                {
                    cargo.haulageData.Remove(haulage);
                }
                RemoveCargo(cargo);
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
                int onboard = haulage.remaining - haulage.need;
                cargo.haulage -= onboard;
                cargo.stolen += onboard;
                cargo.haulageData.Remove(haulage);
                RemoveCargo(cargo);
            }
        }

        private void handleDiedEvent(DiedEvent @event)
        {
            inventory.Clear();
            writeInventory();
        }

        private void handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.commodityAmount != null)
            {
                Cargo cargo = GetCargoWithEDName(@event.commodityAmount.edname);
                if (cargo != null)
                {
                    cargo.owned -= Math.Min(cargo.owned, @event.commodityAmount.amount);
                    RemoveCargo(cargo);
                }
            }
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
                if (cargo.haulageData.FirstOrDefault(h => h.missionid == missionid) != null)
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
                Haulage haulage = cargo.haulageData.FirstOrDefault(h => h.missionid == missionid);
                if (haulage != null)
                {
                    return haulage;
                }
            }
            return null;
        }

        private void UpdateCargoFromInfo(Cargo cargo, List<CargoInfo> infoList)
        {
            cargo.total = infoList.Sum(i => i.count);
            cargo.haulage = infoList.Where(i => i.missionid != null).Sum(i => i.count);
            cargo.stolen = infoList.Where(i => i.missionid == null).Sum(i => i.stolen);
            cargo.owned = cargo.total - cargo.haulage - cargo.stolen;

            foreach (CargoInfo info in infoList.Where(i => i.missionid != null).ToList())
            {
                Mission mission = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))
                    .GetMissionWithMissionId(info.missionid ?? 0);

                Haulage cargoHaulage = cargo.haulageData.FirstOrDefault(h => h.missionid == info.missionid);
                if (cargoHaulage != null)
                {
                    int need = cargoHaulage.remaining - info.count;

                    // Check for sold haulage
                    if (need != cargoHaulage.need)
                    {
                        if (checkHaulage && need > cargoHaulage.need)
                        {
                            // We lost haulage
                            switch (cargoHaulage.typeEDName)
                            {
                                case "delivery":
                                case "deliverywing":
                                case "smuggle":
                                    {
                                        cargoHaulage.status = "Failed";
                                        if (mission != null)
                                        {
                                            mission.statusDef = MissionStatus.FromEDName("Failed");
                                        }
                                    }
                                    break;
                            }
                        }
                        cargoHaulage.need = need;
                    }
                }
                else
                {
                    string name = mission?.name ?? "Mission_None";
                    int amount = mission?.amount ?? info.count;
                    DateTime? expiry = mission?.expiry;

                    cargoHaulage = new Haulage(info.missionid ?? 0, name, mission?.originsystem, amount, expiry);
                    cargo.haulageData.Add(cargoHaulage);
                }
            }
            cargo.CalculateNeed();
            checkHaulage = false;
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
