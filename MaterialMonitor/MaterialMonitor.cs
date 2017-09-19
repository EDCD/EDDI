using System.Collections.Generic;
using Eddi;
using EddiDataDefinitions;
using System.Windows.Controls;
using System;
using System.Text.RegularExpressions;
using System.IO;
using EddiEvents;
using Utilities;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using EddiCompanionAppService;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : EDDIMonitor
    {
        // Observable collection for us to handle
        public ObservableCollection<MaterialAmount> inventory = new ObservableCollection<MaterialAmount>();
        private static readonly object inventoryLock = new object();

        // The material monitor both consumes and emits events, but only one for a given event.  We hold any pending events here so
        // they are fired at the correct time
        private ConcurrentQueue<Event> pendingEvents = new ConcurrentQueue<Event>();

        public string MonitorName()
        {
            return "Material monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Track the amount of materials and generate events when limits are reached.";
        }

        public bool IsRequired()
        {
            return true;
        }

        public MaterialMonitor()
        {
            readMaterials();
            populateMaterialBlueprints();
            populateMaterialLocations();
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
            readMaterials();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is MaterialInventoryEvent)
            {
                handleMaterialInventoryEvent((MaterialInventoryEvent)@event);
            }
            else if (@event is MaterialCollectedEvent)
            {
                handleMaterialCollectedEvent((MaterialCollectedEvent)@event);
            }
            else if (@event is MaterialDiscardedEvent)
            {
                handleMaterialDiscardedEvent((MaterialDiscardedEvent)@event);
            }
            else if (@event is MaterialDonatedEvent)
            {
                handleMaterialDonatedEvent((MaterialDonatedEvent)@event);
            }
            else if (@event is SynthesisedEvent)
            {
                handleSynthesisedEvent((SynthesisedEvent)@event);
            }
            else if (@event is ModificationCraftedEvent)
            {
                handleModificationCraftedEvent((ModificationCraftedEvent)@event);
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
                        EDDI.Instance.eventHandler(pendingEvent);
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
            List<string> knownNames = new List<string>();
            foreach (MaterialAmount materialAmount in @event.inventory)
            {
                setMaterial(materialAmount.material, materialAmount.amount);
                knownNames.Add(materialAmount.material);
            }

            // Update configuration information
            writeMaterials();
        }

        private void handleMaterialCollectedEvent(MaterialCollectedEvent @event)
        {
            incMaterial(@event.name, @event.amount);
        }

        private void handleMaterialDiscardedEvent(MaterialDiscardedEvent @event)
        {
            decMaterial(@event.name, @event.amount);
        }

        private void handleMaterialDonatedEvent(MaterialDonatedEvent @event)
        {
            decMaterial(@event.name, @event.amount);
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            foreach (MaterialAmount component in @event.materials)
            {
                decMaterial(component.material, component.amount);
            }
        }

        private void handleModificationCraftedEvent(ModificationCraftedEvent @event)
        {
            foreach (MaterialAmount component in @event.materials)
            {
                decMaterial(component.material, component.amount);
            }
        }

        public void HandleProfile(JObject profile)
        {
        }

        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string name, int amount)
        {
            lock(inventoryLock)
            {
                Material material = Material.FromName(name);
                MaterialAmount ma = inventory.Where(inv => inv.material == material.name).FirstOrDefault();
                if (ma == null)
                {
                    // No information for the current material - create one and set it to 0
                    ma = new MaterialAmount(material, 0);
                    inventory.Add(ma);
                }

                int previous = ma.amount;
                ma.amount += amount;
                Logging.Debug(ma.material + ": " + previous + "->" + ma.amount);

                if (ma.maximum.HasValue)
                {
                    if (previous <= ma.maximum && ma.amount > ma.maximum)
                    {
                        // We have crossed the high water threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Maximum", (int)ma.maximum, ma.amount, "Increase"));
                    }
                }
                if (ma.desired.HasValue)
                {
                    if (previous < ma.desired && ma.amount >= ma.desired)
                    {
                        // We have crossed the desired threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)ma.desired, ma.amount, "Increase"));
                    }
                }

                writeMaterials();
            }
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string name, int amount)
        {
            lock(inventoryLock)
            {
                Material material = Material.FromName(name);
                MaterialAmount ma = inventory.Where(inv => inv.material == material.name).FirstOrDefault();
                if (ma == null)
                {
                    // No information for the current material - create one and set it to amount
                    ma = new MaterialAmount(material, amount);
                    inventory.Add(ma);
                }

                int previous = ma.amount;
                ma.amount -= amount;
                Logging.Debug(ma.material + ": " + previous + "->" + ma.amount);

                // We have limits for this material; carry out relevant checks
                if (ma.minimum.HasValue)
                {
                    if (previous >= ma.minimum && ma.amount < ma.minimum)
                    {
                        // We have crossed the low water threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Minimum", (int)ma.minimum, ma.amount, "Decrease"));
                    }
                }
                if (ma.desired.HasValue)
                {
                    if (previous >= ma.desired && ma.amount < ma.desired)
                    {
                        // We have crossed the desired threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)ma.desired, ma.amount, "Decrease"));
                    }
                }

                writeMaterials();
            }
        }

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string name, int amount)
        {
            lock(inventoryLock)
            {
                Material material = Material.FromName(name);
                MaterialAmount ma = inventory.Where(inv => inv.material == material.name).FirstOrDefault();
                if (ma == null)
                {
                    // No information for the current material - create one and set it to amount
                    ma = new MaterialAmount(material, amount);
                    Logging.Debug(ma.material + ": " + ma.amount);
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
            lock(inventoryLock)
            {
                // Write material configuration with current inventory
                MaterialMonitorConfiguration configuration = new MaterialMonitorConfiguration();
                configuration.materials = inventory;
                configuration.ToFile();
            }
        }

        private void readMaterials()
        {
            lock(inventoryLock)
            {
                // Obtain current inventory from  configuration
                MaterialMonitorConfiguration configuration = MaterialMonitorConfiguration.FromFile();

                // Build a new inventory
                List<MaterialAmount> newInventory = new List<MaterialAmount>();

                // Start with the materials we have in the log
                foreach (MaterialAmount ma in configuration.materials)
                {
                    newInventory.Add(ma);
                }

                // Add in any new materials
                foreach (Material material in Material.MATERIALS)
                {
                    MaterialAmount ma = newInventory.Where(inv => inv.material == material.name).FirstOrDefault();
                    if (ma == null)
                    {
                        // We don't have this one - add it
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

        private void populateMaterialBlueprints()
        {
            // TODO: Transfer this function to a new server
            string data = Net.DownloadString("http://api.eddp.co/_materialuses");
            if (data != null)
            {
                Dictionary<string, List<Blueprint>> blueprints = JsonConvert.DeserializeObject<Dictionary<string, List<Blueprint>>>(data);
                foreach (KeyValuePair<string, List<Blueprint>> kv in blueprints)
                {
                    Material material = Material.MATERIALS.FirstOrDefault(m => m.name == kv.Key);
                    if (material != null)
                    {
                        material.blueprints = kv.Value;
                    }
                }
            }
        }

        private void populateMaterialLocations()
        {
            // TODO: Transfer this function to a new server
            string data = Net.DownloadString("http://api.eddp.co/_materiallocations");
            if (data != null)
            {
                Dictionary<string, Dictionary<string, object>> locations= JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);
                foreach (KeyValuePair<string, Dictionary<string, object>> kv in locations)
                {
                    Material material = Material.MATERIALS.FirstOrDefault(m => m.name == kv.Key);
                    if (material != null)
                    {
                        material.location = (string)kv.Value["location"];
                    }
                }
            }
        }
    }
}
