using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCargoMonitor
{
    /// Monitor cargo for the current ship
    public class CargoMonitor : IEddiMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Cargo> inventory { get; private set; } = new ObservableCollection<Cargo>();
        public int cargoCarried => inventory.Sum(c => c.total);
        private DateTime updateDat;

        private static readonly object inventoryLock = new object();
        [UsedImplicitly] public event EventHandler InventoryUpdatedEvent;

        public string MonitorName()
        {
            return "Cargo monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.CargoMonitor.cargo_monitor_name;
        }

        public string MonitorDescription()
        {
            return Properties.CargoMonitor.cargo_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        /// <summary>
        /// Create a new CargoMonitor, reading the configuration from the default location on the file system.
        /// This is required for the DLL to load
        /// </summary>
        [UsedImplicitly]
        public CargoMonitor() : this(null)
        { }

        /// <summary>
        /// Create a new CargoMonitor, optionally passing in a non-default configuration
        /// </summary>
        /// <param name="configuration">The configuration to use. If null, it will be read from the file system</param>
        public CargoMonitor(CargoMonitorConfiguration configuration = null)
        {
            BindingOperations.CollectionRegistering += Inventory_CollectionRegistering;
            readInventory( configuration );
            ConfigService.Instance.PropertyChanged += ConfigChanged;
            Logging.Info( $"Initialized {MonitorName()}" );
        }

        private void ConfigChanged ( object sender, PropertyChangedEventArgs e )
        {
            if ( e.PropertyName.Equals( nameof(ConfigService.Instance.missionMonitorConfiguration) ) )
            {
                CalculateCargoNeeds();
            }
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
            return false;
        }

        public void Start ()
        { }

        public void Stop ()
        { }

        public void Reload()
        {
            readInventory();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void HandleProfile(JObject profile)
        { }

        public void HandleStatus ( Status status )
        { }

        public void PreHandle(Event @event)
        {
            // Handle the events that we care about
            if (@event is CargoEvent cargoEvent)
            {
                handleCargoEvent(cargoEvent);
            }
            else if (@event is CommodityCollectedEvent commodityCollectedEvent)
            {
                handleCommodityCollectedEvent(commodityCollectedEvent);
            }
            else if (@event is CommodityEjectedEvent commodityEjectedEvent)
            {
                handleCommodityEjectedEvent(commodityEjectedEvent);
            }
            else if (@event is CommodityPurchasedEvent commodityPurchasedEvent)
            {
                handleCommodityPurchasedEvent(commodityPurchasedEvent);
            }
            else if (@event is CommodityRefinedEvent commodityRefinedEvent)
            {
                handleCommodityRefinedEvent(commodityRefinedEvent);
            }
            else if (@event is CommoditySoldEvent commoditySoldEvent)
            {
                handleCommoditySoldEvent(commoditySoldEvent);
            }
            else if (@event is CargoDepotEvent cargoDepotEvent)
            {
                handleCargoDepotEvent(cargoDepotEvent);
            }
            else if (@event is DiedEvent)
            {
                handleDiedEvent();
            }
            else if (@event is EngineerContributedEvent engineerContributedEvent)
            {
                handleEngineerContributedEvent(engineerContributedEvent);
            }
            else if (@event is LimpetPurchasedEvent limpetPurchasedEvent)
            {
                handleLimpetPurchasedEvent(limpetPurchasedEvent);
            }
            else if (@event is MissionAbandonedEvent missionAbandonedEvent)
            {
                handleMissionAbandonedEvent(missionAbandonedEvent);
            }
            else if (@event is MissionCompletedEvent missionCompletedEvent)
            {
                handleMissionCompletedEvent(missionCompletedEvent);
            }
            else if (@event is SynthesisedEvent synthesisedEvent)
            {
                handleSynthesisedEvent(synthesisedEvent);
            }
        }

        public void PostHandle ( Event @event )
        {
            // Calculate cargo needs using the post handler (so that mission configuration information is already updated)
            if ( @event.type.Contains( "Cargo" ) || 
                 @event.type.Contains( "Contribution" ) ||
                 @event.type.Contains( "Market" ) || 
                 @event.type.Contains( "Mining" ) ||
                 @event.type.Contains( "Mission" ) )
            {
                if ( @event.timestamp >= updateDat )
                {
                    updateDat = @event.timestamp;
                    CalculateCargoNeeds();
                }
            }
        }

        // Keeps inventory levels synced to the game
        private void handleCargoEvent(CargoEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCargoEvent(@event);
                writeInventory();
            }
        }

        internal void _handleCargoEvent(CargoEvent @event)
        {
            if (@event.vessel == Constants.VEHICLE_SHIP)
            {
                if (@event.inventory != null)
                {
                    var infoList = @event.inventory.ToList();

                    // Remove strays from the manifest
                    foreach (var inventoryCargo in inventory.ToList())
                    {
                        var name = inventoryCargo.edname;
                        var infoItem = @event.inventory.FirstOrDefault(i => i.name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (infoItem == null)
                        {
                            // Strip out the stray from the manifest
                            _RemoveCargoWithEDName( inventoryCargo.edname );
                        }
                    }

                    // Update existing cargo in the manifest
                    while (infoList.Any())
                    {
                        var name = infoList.ToList().First().name;
                        var cargoInfo = infoList.Where(i => i.name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
                        var cargo = inventory.FirstOrDefault(c => c.edname.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (cargo != null)
                        {
                            var total = cargoInfo.Sum(i => i.count);
                            var stolen = cargoInfo.Where(i => i.missionid == null).Sum(i => i.stolen);
                            var missionCount = cargoInfo.Count(i => i.missionid != null);
                            if (total != cargo.total || stolen != cargo.stolen || missionCount != cargo.missionCargo.Values.Sum())
                            {
                                UpdateCargoFromInfo(cargo, cargoInfo);
                            }
                        }
                        else
                        {
                            // Add cargo entries for those missing
                            cargo = new Cargo(name);
                            UpdateCargoFromInfo(cargo, cargoInfo);
                        }
                        AddOrUpdateCargo(cargo);
                        infoList.RemoveAll(i => i.name == name);
                    }
                }
            }
        }

        private void handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommodityCollectedEvent( @event );
                writeInventory();
            }
        }

        private void _handleCommodityCollectedEvent(CommodityCollectedEvent @event)
        {
            var cargo = GetCargoWithEDName( @event.commodityDefinition?.edname ) ??
                        new Cargo( @event.commodityDefinition?.edname );
            if ( @event.missionid != null )
            {
                cargo.AddDetailedQty( (long)@event.missionid, 1 );
            }
            else if ( @event.stolen )
            {
                cargo.AddDetailedQty( CargoType.stolen, 1, 0 );
            }
            else
            {
                cargo.AddDetailedQty( CargoType.legal, 1, 0 );
            }
            AddOrUpdateCargo( cargo );
        }

        private void handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommodityEjectedEvent( @event );
                writeInventory();
            }
        }

        internal void _handleCommodityEjectedEvent(CommodityEjectedEvent @event)
        {
            var cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            if ( @event.missionid != null )
            {
                cargo?.RemoveDetailedQty( (long)@event.missionid, @event.amount );
            }
            else
            {
                cargo?.RemoveDetailedQty( CargoType.legal, @event.amount );
            }
            TryRemoveCargo( cargo );
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommodityPurchasedEvent( @event );
                writeInventory();
            }
        }

        internal void _handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            var cargo = GetCargoWithEDName( @event.commodityDefinition?.edname ) ??
                        new Cargo( @event.commodityDefinition?.edname );
            cargo.AddDetailedQty( CargoType.legal, @event.amount, @event.price );
            AddOrUpdateCargo( cargo );
        }

        private void handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommodityRefinedEvent( @event );
                writeInventory();
            }
        }

        private void _handleCommodityRefinedEvent(CommodityRefinedEvent @event)
        {
            var cargo = GetCargoWithEDName( @event.commodityDefinition?.edname ) ??
                        new Cargo( @event.commodityDefinition?.edname );
            cargo.AddDetailedQty( CargoType.legal, 1, 0 );
            AddOrUpdateCargo( cargo );
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCommoditySoldEvent(@event);
                writeInventory();
            }
        }

        internal void _handleCommoditySoldEvent ( CommoditySoldEvent @event )
        {
            var cargo = GetCargoWithEDName(@event.commodityDefinition?.edname);
            cargo?.RemoveDetailedQty( @event.stolen ? CargoType.stolen : CargoType.legal, @event.amount );
            TryRemoveCargo( cargo );
        }

        // If cargo is collected or delivered via a cargo depot
        private void handleCargoDepotEvent(CargoDepotEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleCargoDepotEvent(@event);
                writeInventory();
            }
        }

        internal void _handleCargoDepotEvent ( CargoDepotEvent @event )
        {
            var cargo = GetCargoWithMissionId( @event.missionid, out _ ) ??
                        new Cargo( @event.commodityDefinition.edname );
            switch ( @event.updatetype )
            {
                case "Collect":
                    {
                        cargo.AddDetailedQty( @event.missionid, @event.amount ?? 0 );
                        AddOrUpdateCargo( cargo );
                    }
                    break;
                case "Deliver":
                    {
                        cargo.RemoveDetailedQty( @event.missionid, @event.amount ?? 0 );
                        TryRemoveCargo( cargo );
                    }
                    break;
            }
        }

        private void handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleLimpetPurchasedEvent( @event );
                writeInventory();
            }
        }

        internal void _handleLimpetPurchasedEvent(LimpetPurchasedEvent @event)
        {
            var cargo = GetCargoWithEDName("Drones") ?? new Cargo("Drones");
            cargo.AddDetailedQty(CargoType.legal, @event.amount, @event.price);
            AddOrUpdateCargo(cargo);
        }

        // If we abandon a mission with cargo it becomes stolen
        private void handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleMissionAbandonedEvent( @event );
                writeInventory();
            }
        }

        internal void _handleMissionAbandonedEvent(MissionAbandonedEvent @event)
        {
            var cargo = GetCargoWithMissionId(@event.missionid, out var amount);
            if ( cargo != null )
            {
                cargo.RemoveDetailedQty( @event.missionid, amount );
                cargo.AddDetailedQty( CargoType.stolen, amount, cargo.price );
            }
        }

        // Check to see if this is a cargo mission and update our inventory accordingly
        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.commodityDefinition != null || @event.commodityrewards != null)
            {
                if (@event.timestamp > updateDat)
                {
                    updateDat = @event.timestamp;
                    if (_handleMissionCompletedEvent(@event))
                    {
                        writeInventory();
                    }
                }
            }
        }

        private bool _handleMissionCompletedEvent ( MissionCompletedEvent @event )
        {
            var cargo = GetCargoWithMissionId( @event.missionid, out var amount );
            if ( cargo != null )
            {
                cargo.RemoveDetailedQty( @event.missionid, Math.Max( @event.amount ?? 0, amount ) );
                TryRemoveCargo( cargo );
                return true;
            }

            return false;
        }

        private void handleDiedEvent()
        {
            inventory.Clear();
            writeInventory();
        }

        private void handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleEngineerContributedEvent(@event))
                {
                    writeInventory();
                }
            }
        }

        private bool _handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.commodityAmount != null)
            {
                var cargo = GetCargoWithEDName(@event.commodityAmount.edname);
                if (cargo != null)
                {
                    cargo.RemoveDetailedQty(CargoType.legal, Math.Min(cargo.owned, @event.commodityAmount.amount));
                    TryRemoveCargo( cargo );
                    return true;
                }
            }
            return false;
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleSynthesisedEvent(@event))
                {
                    writeInventory();
                }
            }
        }

        internal bool _handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.synthesis.Contains("Limpet"))
            {
                var cargo = GetCargoWithEDName("Drones") ?? new Cargo("Drones");
                cargo.AddDetailedQty(CargoType.legal, 4, 0);
                AddOrUpdateCargo(cargo);
                return true;
            }
            return false;
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            lock ( inventoryLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    ["inventory"] = new Tuple<Type, object>(typeof(List<Cargo>), inventory.ToList() ),
                    ["cargoCarried"] = new Tuple<Type, object>(typeof(int), cargoCarried)
                };                
            }
        }

        public void writeInventory()
        {
            lock (inventoryLock)
            {
                // Write cargo configuration with current inventory
                var configuration = new CargoMonitorConfiguration()
                {
                    updatedat = updateDat,
                    cargo = inventory,
                    cargocarried = cargoCarried
                };
                ConfigService.Instance.cargoMonitorConfiguration = configuration;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(InventoryUpdatedEvent, inventory);
        }

        private void readInventory(CargoMonitorConfiguration configuration = null)
        {
            lock (inventoryLock)
            {
                // Obtain current cargo inventory from configuration
                configuration = configuration ?? ConfigService.Instance.cargoMonitorConfiguration;
                updateDat = configuration.updatedat;

                // Build a new inventory
                var newInventory = new List<Cargo>();

                // Start with the materials we have in the log
                foreach (var cargo in configuration.cargo)
                {
                    if (cargo.commodityDef == null)
                    {
                        cargo.commodityDef = CommodityDefinition.FromEDName(cargo.edname);
                    }
                    newInventory.Add(cargo);
                }

                // Now order the list by name
                newInventory = newInventory.OrderBy(c => c.invariantName).ToList();

                // Update the inventory 
                inventory.Clear();
                foreach (var cargo in newInventory)
                {
                    inventory.Add(cargo);
                }
            }
        }

        private void AddOrUpdateCargo(Cargo cargo)
        {
            if (cargo == null) { return; }
            lock (inventoryLock)
            {
                var found = false;
                for (var i = 0; i < inventory.Count; i++)
                {
                    if (string.Equals(inventory[i].edname, cargo.edname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        found = true;
                        inventory[i] = cargo;
                        break;
                    }
                }
                if (!found)
                {
                    inventory.Add(cargo);
                }
            }
        }

        private bool TryRemoveCargo ( Cargo cargo )
        {
            if ( cargo != null && ( cargo.total + cargo.need ) < 1 )
            {
                // All of the commodity was either expended, ejected, or sold
                _RemoveCargoWithEDName( cargo.edname );
                return true;
            }

            return false;
        }

        private void _RemoveCargoWithEDName(string edname)
        {
            lock (inventoryLock)
            {
                if (edname != null)
                {
                    edname = edname.ToLowerInvariant();
                    for (var i = 0; i < inventory.Count; i++)
                    {
                        if (inventory[i].edname.ToLowerInvariant() == edname)
                        {
                            inventory.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        [CanBeNull]
        public Cargo GetCargoWithEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }
            edname = edname.ToLowerInvariant();
            return inventory.FirstOrDefault(c => c.edname.ToLowerInvariant() == edname);
        }

        [CanBeNull]
        public Cargo GetCargoWithMissionId ( long missionid, out int amount )
        {
            amount = 0;
            foreach ( var cargo in inventory.ToList() )
            {
                var missionCargo = cargo.missionCargo.Where( c => c.Key == missionid ).ToList();
                if ( missionCargo.Any() )
                {
                    amount = missionCargo.Sum( c => c.Value );
                    return cargo;
                }
            }
            return null;
        }

        private void UpdateCargoFromInfo ( Cargo cargo, List<CargoInfoItem> infoList )
        {
            cargo.missionCargo = infoList.Where( i => i.missionid != null ).ToDictionary( i => (long)i.missionid, i => i.count );
            cargo.stolen = infoList.Where( i => i.missionid == null ).Sum( i => i.stolen );
            cargo.owned = infoList.Sum( i => i.count ) - cargo.haulage - cargo.stolen;
        }

        private void CalculateCargoNeeds ()
        {
            try
            {
                var missionsConfig = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
                var missions = missionsConfig
                    .Where( m =>
                        m.CommodityDefinition != null &&
                        m.amount != null &&
                        m.delivered < m.amount &&
                        !m.communal )
                    .ToList();

                List<Cargo> currentCargo;
                lock ( inventoryLock )
                {
                    currentCargo = inventory.ToList();
                }

                // Add any mission cargo types we need and which are not already present in our inventory
                foreach ( var mission in missions.Where( m => !currentCargo.Any( c =>
                             c.edname.Equals( m.CommodityDefinition.edname,
                                 StringComparison.InvariantCultureIgnoreCase ) ) ) )
                {
                    var cargo = new Cargo( mission.CommodityDefinition.edname );
                    AddOrUpdateCargo( cargo );
                }

                // Update need for each cargo type
                foreach ( var cargo in currentCargo )
                {
                    var missionsData = missions
                        .Where( m => m.CommodityDefinition.edname == cargo.commodityDef.edname )
                        .ToList();
                    var missionNeeds = missionsData.Sum( m => m.amount - m.delivered ) ?? 0;
                    var wingCargo = missionsData.Sum( m => m.wingCollected );

                    cargo.need = missionNeeds - cargo.haulage - wingCargo;
                    TryRemoveCargo( cargo );
                }

                writeInventory();
            }
            catch ( Exception e )
            {
                Logging.Error( "Failed to update cargo needs", e );
            }
        }

        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                var uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
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
