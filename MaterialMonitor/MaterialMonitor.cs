using System.Collections.Generic;
using Eddi;
using EddiDataDefinitions;
using System.Windows.Controls;
using System;
using EddiEvents;
using Utilities;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
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

        public string LocalizedMonitorName()
        {
            return EddiMaterialMonitor.Properties.MaterialMonitor.name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
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
            else if (@event is MaterialTradedEvent)
            {
                handleMaterialTradedEvent((MaterialTradedEvent)@event);
            }
            else if (@event is SynthesisedEvent)
            {
                handleSynthesisedEvent((SynthesisedEvent)@event);
            }
            else if (@event is ModificationCraftedEvent)
            {
                handleModificationCraftedEvent((ModificationCraftedEvent)@event);
            }
            else if (@event is TechnologyBrokerEvent)
            {
                handleTechnologyBrokerEvent((TechnologyBrokerEvent)@event);
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
                setMaterial(materialAmount.edname, materialAmount.amount);
                knownNames.Add(materialAmount.edname);
            }

            // Update configuration information
            writeMaterials();
        }

        private void handleMaterialCollectedEvent(MaterialCollectedEvent @event)
        {
            incMaterial(@event.edname, @event.amount);
        }

        private void handleMaterialDiscardedEvent(MaterialDiscardedEvent @event)
        {
            decMaterial(@event.edname, @event.amount);
        }

        private void handleMaterialDonatedEvent(MaterialDonatedEvent @event)
        {
            decMaterial(@event.edname, @event.amount);
        }

        private void handleMaterialTradedEvent(MaterialTradedEvent @event)
        {
            decMaterial(@event.paid_edname, @event.paid_quantity);
            incMaterial(@event.received_edname, @event.received_quantity);
        }

        private void handleSynthesisedEvent(SynthesisedEvent @event)
        {
            foreach (MaterialAmount component in @event.materials)
            {
                decMaterial(component.edname, component.amount);
            }
        }

        private void handleModificationCraftedEvent(ModificationCraftedEvent @event)
        {
            foreach (MaterialAmount component in @event.materials)
            {
                decMaterial(component.edname, component.amount);
            }
        }

        private void handleTechnologyBrokerEvent(TechnologyBrokerEvent @event)
        {
            foreach (MaterialAmount material in @event.materials)
            {
                decMaterial(material.edname, material.amount);
            }
        }

        public void HandleProfile(JObject profile)
        {
        }

        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string edname, int amount)
        {
            lock(inventoryLock)
            {
                Material material = Material.FromEDName(edname);
                MaterialAmount ma = inventory.Where(inv => inv.edname == material.edname).FirstOrDefault();
                if (ma == null)
                {
                    // No information for the current material - create one and set it to 0
                    ma = new MaterialAmount(material, 0);
                    inventory.Add(ma);
                }

                int previous = ma.amount;
                ma.amount += amount;
                Logging.Debug(ma.edname + ": " + previous + "->" + ma.amount);

                if (ma.maximum.HasValue)
                {
                    if (previous <= ma.maximum && ma.amount > ma.maximum)
                    {
                        // We have crossed the high water threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromEDName(edname), "Maximum", (int)ma.maximum, ma.amount, "Increase"));
                    }
                }
                if (ma.desired.HasValue)
                {
                    if (previous < ma.desired && ma.amount >= ma.desired)
                    {
                        // We have crossed the desired threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromEDName(edname), "Desired", (int)ma.desired, ma.amount, "Increase"));
                    }
                }

                writeMaterials();
            }
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string edname, int amount)
        {
            lock(inventoryLock)
            {
                Material material = Material.FromEDName(edname);
                MaterialAmount ma = inventory.Where(inv => inv.edname == material.edname).FirstOrDefault();
                if (ma == null)
                {
                    // No information for the current material - create one and set it to amount
                    ma = new MaterialAmount(material, amount);
                    inventory.Add(ma);
                }

                int previous = ma.amount;
                ma.amount -= amount;
                Logging.Debug(ma.edname + ": " + previous + "->" + ma.amount);

                // We have limits for this material; carry out relevant checks
                if (ma.minimum.HasValue)
                {
                    if (previous >= ma.minimum && ma.amount < ma.minimum)
                    {
                        // We have crossed the low water threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromEDName(edname), "Minimum", (int)ma.minimum, ma.amount, "Decrease"));
                    }
                }
                if (ma.desired.HasValue)
                {
                    if (previous >= ma.desired && ma.amount < ma.desired)
                    {
                        // We have crossed the desired threshold for this material
                        pendingEvents.Enqueue(new MaterialThresholdEvent(DateTime.Now, Material.FromEDName(edname), "Desired", (int)ma.desired, ma.amount, "Decrease"));
                    }
                }

                writeMaterials();
            }
        }

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string edname, int amount)
        {
            lock(inventoryLock)
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
                    // Remove incorrect edname introduced in an earlier version of EDDI
                    if (ma.edname == "tg_shipsystemdata") // Should be "shipsystemsdata"
                    {
                        continue;
                    }

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
                foreach (Material material in Material.AllOfThem)
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

        private void populateMaterialBlueprints()
        {
            string data = Net.DownloadString(Constants.EDDI_SERVER_URL + "materialuses.json");
            if (data != null)
            {
                Dictionary<string, List<Blueprint>> blueprints = JsonConvert.DeserializeObject<Dictionary<string, List<Blueprint>>>(data);
                foreach (KeyValuePair<string, List<Blueprint>> kv in blueprints)
                {
                    Material material = Material.AllOfThem.FirstOrDefault(m => m.invariantName == kv.Key);
                    if (material != null)
                    {
                        material.blueprints = kv.Value;
                    }
                }
            }
        }

        private void populateMaterialLocations()
        {            
            string data = Net.DownloadString(Constants.EDDI_SERVER_URL + "materiallocations.json");
            if (data != null)
            {
                Dictionary<string, Dictionary<string, object>> locations= JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(data);
                foreach (KeyValuePair<string, Dictionary<string, object>> kv in locations)
                {
                    Material material = Material.AllOfThem.FirstOrDefault(m => m.invariantName == kv.Key);
                    if (material != null)
                    {
                        material.location = (string)kv.Value["location"];
                    }
                }
            }
        }
    }
}
