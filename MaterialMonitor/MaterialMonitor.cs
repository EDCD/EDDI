using Eddi;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiMissionMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : EDDIMonitor
    {
        // Observable collection for us to handle
        public ObservableCollection<MaterialAmount> inventory { get; private set; } = new ObservableCollection<MaterialAmount>();
        public int? maxStationDistanceFromStarLs;

        private static readonly object inventoryLock = new object();
        public event EventHandler InventoryUpdatedEvent;

        // The material monitor both consumes and emits events, but only one for a given event.  We hold any pending events here so
        // they are fired at the correct time
        private ConcurrentQueue<Event> pendingEvents = new ConcurrentQueue<Event>();

        public string MonitorName()
        {
            return "Material monitor";
        }

        public string LocalizedMonitorName()
        {
            return EddiMaterialMonitor.Properties.MaterialMonitor.name;
        }

        public string MonitorDescription()
        {
            return EddiMaterialMonitor.Properties.MaterialMonitor.name;
        }

        public bool IsRequired()
        {
            return true;
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

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(inventory, inventoryLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(inventory); });
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

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

        }

        // Flush any pending events
        public void PostHandle(Event @event)
        {
            // Spin out event in to a different thread to stop blocking
            Thread thread = new Thread(() =>
            {
                try
                {
                    while (pendingEvents.TryDequeue(out Event pendingEvent))
                    {
                        EDDI.Instance.enqueueEvent(pendingEvent);
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void handleMaterialInventoryEvent(MaterialInventoryEvent @event)
        {
            // Set all listed material quantities to match the event
            List<string> knownNames = new List<string>();
            foreach (MaterialAmount materialAmount in @event.inventory)
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
        }

        private void handleMaterialDiscardedEvent(MaterialDiscardedEvent @event)
        {
            decMaterial(@event.edname, @event.amount, @event.fromLoad);
        }

        private void handleMaterialDonatedEvent(MaterialDonatedEvent @event)
        {
            decMaterial(@event.edname, @event.amount, @event.fromLoad);
        }

        private void handleMaterialTradedEvent(MaterialTradedEvent @event)
        {
            decMaterial(@event.paid_edname, @event.paid_quantity, @event.fromLoad);
            incMaterial(@event.received_edname, @event.received_quantity, @event.fromLoad);
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount component in @event.materials)
                {
                    decMaterial(component.edname, component.amount, @event.fromLoad);
                }
            }
        }

        private void handleModificationCraftedEvent(ModificationCraftedEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount component in @event.materials)
                {
                    decMaterial(component.edname, component.amount, @event.fromLoad);
                }
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            if (@event.materials?.Count > 0)
            {
                foreach (MaterialAmount material in @event.materials)
                {
                    decMaterial(material.edname, material.amount, @event.fromLoad);
                }
            }
        }

        private void handleMissionCompletedEvent(MissionCompletedEvent @event)
        {
            if (@event.materialsrewards?.Count > 0)
            {
                foreach (MaterialAmount material in @event.materialsrewards)
                {
                    incMaterial(material.edname, material.amount, @event.fromLoad);
                }
            }
        }

        private void handleEngineerContributedEvent(EngineerContributedEvent @event)
        {
            if (@event.materialAmount != null)
            {
                decMaterial(@event.materialAmount.edname, @event.materialAmount.amount, @event.fromLoad);
            }
        }

        public void HandleProfile(JObject profile)
        {
        }

        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string edname, int amount, bool fromLogLoad = false)
        {
            lock (inventoryLock)
            {
                var material = Material.FromEDName(edname);
                var ma = inventory.FirstOrDefault(inv => inv.edname == material.edname);
                if (ma == null)
                {
                    // No information for the current material - create one and set it to 0
                    ma = new MaterialAmount(material, 0);
                    inventory.Add(ma);
                }

                var previous = ma.amount;
                ma.amount += amount;
                Logging.Debug(ma.edname + ": " + previous + "->" + ma.amount);

                if (ma.maximum != null && incMaterialThreshold(previous, ma.amount, ma.maximum))
                {
                    // We have crossed the high water threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Maximum", (int)ma.maximum, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }
                if (ma.desired != null && incMaterialThreshold(previous, ma.amount, ma.desired))
                {
                    // We have crossed the desired threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Desired", (int)ma.desired, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }
                if (ma.minimum != null && incMaterialThreshold(previous, ma.amount, ma.minimum))
                {
                    // We have crossed the minimum threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, material, "Minimum", (int)ma.minimum, ma.amount, "Increase") { fromLoad = fromLogLoad });
                }

                writeMaterials();
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
                var ma = inventory.FirstOrDefault(inv => inv.edname == material.edname);
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
                if (ma.minimum != null && decMaterialThreshold(previous, ma.amount, ma.minimum))
                {
                    // We have crossed the minimum threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Minimum", (int)ma.minimum, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }
                if (ma.desired != null && decMaterialThreshold(previous, ma.amount, ma.desired))
                {
                    // We have crossed the desired threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Desired", (int)ma.desired, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }
                if (ma.maximum != null && decMaterialThreshold(previous, ma.amount, ma.maximum))
                {
                    // We have crossed the maximum threshold for this material
                    pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.UtcNow, Material.FromEDName(edname), "Maximum", (int)ma.maximum, ma.amount, "Decrease") { fromLoad = fromLogLoad });
                }

                writeMaterials();
            }
        }

        private bool incMaterialThreshold(int previous, int amount, int? target)
        {
            // For the comparison operators <, >, <=, and >=, if one or both operands are null, the result is false
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types
            return previous < target && target <= amount;
        }

        private bool decMaterialThreshold(int previous, int amount, int? target)
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
                Material material = Material.FromEDName(edname);
                MaterialAmount ma = inventory.Where(inv => inv.edname == material.edname).FirstOrDefault();
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

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["materials"] = new List<MaterialAmount>(inventory)
            };
            return variables;
        }

        public void writeMaterials()
        {
            lock (inventoryLock)
            {
                // Write material configuration with current inventory
                MaterialMonitorConfiguration configuration = new MaterialMonitorConfiguration
                {
                    materials = inventory,
                    maxStationDistanceFromStarLs = maxStationDistanceFromStarLs
                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(InventoryUpdatedEvent, inventory);
        }

        private void readMaterials()
        {
            lock (inventoryLock)
            {
                // Obtain current inventory from  configuration
                MaterialMonitorConfiguration configuration = MaterialMonitorConfiguration.FromFile();
                maxStationDistanceFromStarLs = configuration.maxStationDistanceFromStarLs ?? Constants.maxStationDistanceDefault;

                // Build a new inventory
                List<MaterialAmount> newInventory = new List<MaterialAmount>();

                // Start with the materials we have in the log
                foreach (MaterialAmount ma in configuration.materials)
                {
                    MaterialAmount ma2 = new MaterialAmount(ma.edname, ma.amount, ma.minimum, ma.desired, ma.maximum);
                    // Make sure the edname is unique before adding the material to the new inventory 
                    if (newInventory.Where(inv => inv.edname == ma2.edname).Count() == 0)
                    {
                        // Set material maximums if they aren't already defined
                        if (ma2.maximum == null || !ma2.maximum.HasValue)
                        {
                            int rarityLevel = Material.FromEDName(ma2.edname).rarity.level;
                            if (rarityLevel > 0)
                            {
                                ma2.maximum = -50 * (rarityLevel) + 350;
                            }
                        }
                        newInventory.Add(ma2);
                    }
                }

                // Add in any new materials
                foreach (Material material in Material.AllOfThem.ToList())
                {
                    MaterialAmount ma = newInventory.Where(inv => inv.edname == material.edname).FirstOrDefault();
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
                foreach (MaterialAmount ma in newInventory)
                {
                    inventory.Add(ma);
                }
            }
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
