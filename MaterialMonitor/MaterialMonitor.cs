using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : IEddiMonitor
    {
        // Observable collection for us to handle
        public ObservableCollection<MaterialAmount> inventory { get; private set; } = [ ];

        private static readonly object inventoryLock = new();
        public event EventHandler InventoryUpdatedEvent;

        // The material monitor both consumes and emits events, but only one for a given event.  We hold any pending events here so
        // they are fired at the correct time
        private readonly ConcurrentQueue<Event> generatedEvents = new();

        public string MonitorName()
        {
            return "Material monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.MaterialMonitor.name;
        }

        public string MonitorDescription()
        {
            return Properties.MaterialMonitor.name;
        }

        public bool IsRequired()
        {
            return false;
        }

        public MaterialMonitor()
        {
            BindingOperations.CollectionRegistering += Inventory_CollectionRegistering;
            readMaterials();
            Logging.Info($"Initialized {MonitorName()}");
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
            readMaterials();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public Task PreHandleAsync(Event @event)
        {
            // Handle the events that we care about
            if (@event is MaterialInventoryEvent materialInventoryEvent)
            {
                handleMaterialInventoryEvent(materialInventoryEvent);
            }
            else if (@event is MaterialCollectedEvent materialCollectedEvent)
            {
                handleMaterialCollectedEvent(materialCollectedEvent);
            }
            else if (@event is MaterialDiscardedEvent materialDiscardedEvent)
            {
                handleMaterialDiscardedEvent(materialDiscardedEvent);
            }
            else if (@event is MaterialDonatedEvent materialDonatedEvent)
            {
                handleMaterialDonatedEvent(materialDonatedEvent);
            }
            else if (@event is MaterialTradedEvent materialTradedEvent)
            {
                handleMaterialTradedEvent(materialTradedEvent);
            }
            else if (@event is SynthesisedEvent synthesisedEvent)
            {
                handleSynthesisedEvent(synthesisedEvent);
            }
            else if (@event is ModificationCraftedEvent modificationCraftedEvent)
            {
                handleModificationCraftedEvent(modificationCraftedEvent);
            }
            else if (@event is TechnologyBrokerEvent technologyBrokerEvent)
            {
                handleTechnologyBrokerEvent(technologyBrokerEvent);
            }
            else if (@event is MissionCompletedEvent missionCompletedEvent)
            {
                handleMissionCompletedEvent(missionCompletedEvent);
            }
            else if (@event is EngineerContributedEvent engineerContributedEvent)
            {
                handleEngineerContributedEvent(engineerContributedEvent);
            }

            return Task.CompletedTask;
        }

        // Flush any generated events asynchronously
        public Task PostHandleAsync ( Event @event )
        {
            try
            {
                Task.Run( () =>
                {
                    while ( generatedEvents.TryDequeue( out var pendingEvent ) )
                    {
                        EDDI.Instance.enqueueEvent( pendingEvent );
                    }
                } );
            }
            catch ( OperationCanceledException oce )
            {
                Logging.Debug( "Task cancelled.", oce );
            }

            return Task.CompletedTask;
        }

        private void handleMaterialInventoryEvent(MaterialInventoryEvent @event)
        {
            // Set all listed material quantities to match the event
            var knownNames = new List<string>();
            foreach (var materialAmount in @event.inventory)
            {
                setMaterial(materialAmount.edname, materialAmount.amount);
                knownNames.Add(materialAmount.edname);
            }

            // Set any unlisted materials with known definitions to zero
            var unlistedMaterials = Material.AllOfThem
                .Where(m => m.Category != MaterialCategory.Unknown)
                .Select(m => m.edname).Except(knownNames).ToList();
            foreach (var unlistedMaterial in unlistedMaterials)
            {
                setMaterial(unlistedMaterial, 0);
            }

            // Clean up any materials which are both unlisted and unknown
            lock (inventoryLock)
            {
                var unknownUnlistedMaterials = inventory
                    .Where(m => !knownNames.Contains(m.edname) && !unlistedMaterials.Contains(m.edname)).ToList();
                foreach (var unknownUnlistedMaterial in unknownUnlistedMaterials)
                {
                    inventory.Remove(unknownUnlistedMaterial);
                }
            }

            // Update configuration information
            writeMaterials();
        }

        private void handleMaterialCollectedEvent(MaterialCollectedEvent @event)
        {
            incMaterial(@event.edname, @event.amount, @event.fromLoad);
            @event.total = inventory
                .Where( m => m.edname.Equals( @event.edname, StringComparison.InvariantCultureIgnoreCase ) )
                .Sum( m => m.amount );

            writeMaterials();
        }

        private void handleMaterialDiscardedEvent(MaterialDiscardedEvent @event)
        {
            decMaterial(@event.edname, @event.amount, @event.fromLoad);
            @event.total = inventory
                .Where( m => m.edname.Equals( @event.edname, StringComparison.InvariantCultureIgnoreCase ) )
                .Sum( m => m.amount );

            writeMaterials();
        }

        private void handleMaterialDonatedEvent(MaterialDonatedEvent @event)
        {
            decMaterial(@event.edname, @event.amount, @event.fromLoad);
            @event.total = inventory
                .Where( m => m.edname.Equals( @event.edname, StringComparison.InvariantCultureIgnoreCase ) )
                .Sum( m => m.amount );

            writeMaterials();
        }

        private void handleMaterialTradedEvent(MaterialTradedEvent @event)
        {
            decMaterial(@event.paid_edname, @event.paid_quantity, @event.fromLoad);
            @event.lost_total = inventory
                .Where( m => m.edname.Equals( @event.paid_edname, StringComparison.InvariantCultureIgnoreCase ) )
                .Sum( m => m.amount );

            incMaterial( @event.received_edname, @event.received_quantity, @event.fromLoad );
            @event.received_total = inventory
                .Where( m => m.edname.Equals( @event.received_edname, StringComparison.InvariantCultureIgnoreCase ) )
                .Sum( m => m.amount );

            writeMaterials();
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (var component in @event.materials)
                {
                    decMaterial(component.edname, component.amount, @event.fromLoad);
                }

                writeMaterials();
            }
        }

        private void handleModificationCraftedEvent(ModificationCraftedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (var component in @event.materials)
                {
                    decMaterial(component.edname, component.amount, @event.fromLoad);
                }

                writeMaterials();
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (var material in @event.materials)
                {
                    decMaterial(material.edname, material.amount, @event.fromLoad);
                }

                writeMaterials();
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.materialsrewards?.Count > 0)
            {
                foreach (var material in @event.materialsrewards)
                {
                    incMaterial(material.edname, material.amount, @event.fromLoad);
                }

                writeMaterials();
            }
        }

        private void handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.materialAmount != null)
            {
                decMaterial(@event.materialAmount.edname, @event.materialAmount.amount, @event.fromLoad);
            }

            writeMaterials();
        }

        public Task HandleProfileAsync(JObject profile)
        {
            return Task.CompletedTask;
        }

        public Task HandleStatusAsync ( Status status )
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string edname, int amount, bool fromLogLoad = false)
        {
            lock (inventoryLock)
            {
                var material = Material.FromEDName(edname);
                var ma = inventory.FirstOrDefault(inv => inv.edname == material?.edname);
                if (ma == null)
                {
                    // No information for the current material - create one and set it to 0
                    ma = new MaterialAmount(material, 0);
                    inventory.Add(ma);
                }

                var previous = ma.amount;

                // Calculate a hard maximum storage limit for the material from its rarity level
                var max = 300;
                var rarityLevel = Material.FromEDName(ma.edname)?.Rarity.level ?? 0;
                if (rarityLevel > 0)
                {
                    max = (-50 * rarityLevel ) + 350;
                }

                // Add our amount, not exceeding our storage maximum
                ma.amount += Math.Min(amount, max - ma.amount);
                Logging.Debug(ma.edname + ": " + previous + "->" + ma.amount);

                if (ma.maximum != null && CheckIncreaseMaterialThreshold(previous, ma.amount, ma.maximum))
                {
                    // We have crossed the high water threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Maximum", (int)ma.maximum, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }
                if (ma.desired != null && CheckIncreaseMaterialThreshold(previous, ma.amount, ma.desired))
                {
                    // We have crossed the desired threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Desired", (int)ma.desired, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }
                if (ma.minimum != null && CheckIncreaseMaterialThreshold(previous, ma.amount, ma.minimum))
                {
                    // We have crossed the minimum threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Minimum", (int)ma.minimum, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }
            }
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string edname, int amount, bool fromLogLoad = false)
        {
            lock (inventoryLock)
            {
                var material = Material.FromEDName(edname);
                var ma = inventory.FirstOrDefault(inv => inv.edname == material?.edname);
                if (ma == null)
                {
                    // No information for the current material - create one and set it to amount
                    ma = new MaterialAmount(material, amount);
                    inventory.Add(ma);
                }

                var previous = ma.amount;
                ma.amount -= Math.Min(amount, previous); // Never subtract more than we started with
                Logging.Debug(ma.edname + ": " + previous + "->" + ma.amount);

                // We have limits for this material; carry out relevant checks
                if (ma.minimum != null && CheckDecreaseMaterialThreshold(previous, ma.amount, ma.minimum))
                {
                    // We have crossed the minimum threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Minimum", (int)ma.minimum, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }
                if (ma.desired != null && CheckDecreaseMaterialThreshold(previous, ma.amount, ma.desired))
                {
                    // We have crossed the desired threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Desired", (int)ma.desired, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }
                if (ma.maximum != null && CheckDecreaseMaterialThreshold(previous, ma.amount, ma.maximum))
                {
                    // We have crossed the maximum threshold for this material
                    generatedEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Maximum", (int)ma.maximum, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }
            }
        }

        internal static bool CheckIncreaseMaterialThreshold(int previous, int amount, int? target)
        {
            // For the comparison operators <, >, <=, and >=, if one or both operands are null, the result is false
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types
            return previous < target && target <= amount;
        }

        internal static bool CheckDecreaseMaterialThreshold(int previous, int amount, int? target)
        {
            // For the comparison operators <, >, <=, and >=, if one or both operands are null, the result is false
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types
            return previous >= target && target > amount;
        }

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string edname, int amount)
        {
            lock (inventoryLock)
            {
                var material = Material.FromEDName(edname);
                var ma = inventory.FirstOrDefault( inv => inv.edname == material?.edname );
                if (ma == null)
                {
                    // No information for the current material - create one and set it to amount
                    ma = new MaterialAmount(material, amount);
                    Logging.Debug(ma.edname + ": " + ma.amount);
                    inventory.Add(ma);
                }
                ma.amount = amount;
            }
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( inventoryLock )
            {
                return new Dictionary<string, Tuple<Type, object>>
                {
                    [ "materials" ] = new(typeof(List<MaterialAmount>), inventory.ToList() )
                };
            }
        }

        public void writeMaterials()
        {
            lock (inventoryLock)
            {
                // Write material configuration with current inventory
                var materials = inventory.Select(m => new MaterialAmount(m.edname, m.amount, m.minimum, m.desired, m.maximum)).ToList();
                var configuration = new MaterialMonitorConfiguration
                {
                    materials = materials,
                };
                ConfigService.Instance.materialMonitorConfiguration = configuration;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(InventoryUpdatedEvent, inventory);
        }

        private void readMaterials()
        {
            lock (inventoryLock)
            {
                // Obtain current inventory from  configuration
                var configuration = ConfigService.Instance.materialMonitorConfiguration;

                // Build a new inventory
                var newInventory = new List<MaterialAmount>();

                // Start with the materials we have in the log
                foreach (var ma in configuration.materials)
                {
                    var ma2 = new MaterialAmount(ma.edname, ma.amount, ma.minimum, ma.desired, ma.maximum);
                    // Make sure the edname is unique before adding the material to the new inventory 
                    if (newInventory.All(inv => inv.edname != ma2.edname))
                    {
                        // Set material maximums if they aren't already defined
                        if (ma2.maximum == null)
                        {
                            var rarityLevel = Material.FromEDName(ma2.edname)?.Rarity.level ?? 0;
                            if (rarityLevel > 0)
                            {
                                ma2.maximum = (-50 * rarityLevel) + 350;
                            }
                        }
                        newInventory.Add(ma2);
                    }
                }

                // Add in any new materials
                foreach (var material in Material.AllOfThem.ToList())
                {
                    var ma = newInventory.FirstOrDefault( inv => inv.edname == material.edname );
                    if (ma == null)
                    {
                        // We don't have this one - add it and set it to zero
                        Logging.Debug("Adding new material " + material.invariantName + " to the materials list");
                        ma = new MaterialAmount(material, 0);
                        newInventory.Add(ma);
                    }
                }

                // Now order the list by name
                newInventory = newInventory.OrderBy(m => m.material).ToList();

                // Update the inventory 
                inventory.Clear();
                foreach (var ma in newInventory)
                {
                    inventory.Add(ma);
                }
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
