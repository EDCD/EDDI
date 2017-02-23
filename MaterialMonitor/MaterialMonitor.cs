using System.Collections.Generic;
using Eddi;
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
using EddiDataDefinitions;
=======
>>>>>>> Add material monitor.
using System.Windows.Controls;
using System;
using System.Text.RegularExpressions;
using System.IO;
using EddiEvents;
using Utilities;
using Newtonsoft.Json;
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
using System.Linq;
using System.Collections.ObjectModel;
using EddiCompanionAppService;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
=======
using EddiDataDefinitions;
>>>>>>> Add material monitor.

namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : EDDIMonitor
    {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        // Observable collection for us to handle
        public ObservableCollection<MaterialAmount> inventory = new ObservableCollection<MaterialAmount>();

        // The material monitor both consumes and emits events, but only one for a given event.  We hold any pending events here so
        // they are fired at the correct time
        private ConcurrentQueue<Event> pendingEvents = new ConcurrentQueue<Event>();
=======
        // The file to log speech
        public static readonly string LogFile = Constants.DATA_DIR + @"\materialsresponder.out";

        // Configuration of the responder
        private MaterialMonitorConfiguration configuration;

        // Our inventory of materials
        private IDictionary<string, int> inventory = new Dictionary<string, int>();
>>>>>>> Add material monitor.

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

<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        public bool IsRequired()
        {
            return true;
        }

        public MaterialMonitor()
        {
            readMaterials();
            populateMaterialBlueprints();
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public void Start()
        {
=======
        public MaterialMonitor()
        {
            configuration = MaterialMonitorConfiguration.FromFile();
            //if (configuration != null && configuration.Personality != null)
            //{
            //    personality = Personality.FromName(configuration.Personality);
            //}
            //if (personality == null)
            //{ 
            //    personality = Personality.Default();
            //}
            //scriptResolver = new ScriptResolver(personality.Scripts);
            //subtitles = configuration.Subtitles;
            //subtitlesOnly = configuration.SubtitlesOnly;
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        ///// <summary>
        ///// Change the personality for the speech responder
        ///// </summary>
        ///// <returns>true if the speech responder is now using the new personality, otherwise false</returns>
        //public bool SetPersonality(string newPersonality)
        //{
        //    SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
        //    if (newPersonality == configuration.Personality)
        //    {
        //        // Already set to this personality
        //        return true;
        //    }

        //    // Ensure that this personality exists
        //    Personality personality = Personality.FromName(newPersonality);
        //    if (personality != null)
        //    {
        //        // Yes it does; use it
        //        configuration.Personality = newPersonality;
        //        configuration.ToFile();
        //        scriptResolver = new ScriptResolver(personality.Scripts);
        //        Logging.Debug("Changed personality to " + newPersonality);
        //        return true;
        //    }
        //    else
        //    {
        //        // No it does not; ignore it
        //        return false;
        //    }
        //}

        public void Start()
        {
            // We don't actively do anything, just listen to events, so nothing to do here
>>>>>>> Add material monitor.
        }

        public void Stop()
        {
        }

        public void Reload()
        {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
            readMaterials();
=======
            configuration = MaterialMonitorConfiguration.FromFile();
>>>>>>> Add material monitor.
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is MaterialInventoryEvent)
            {
                handleMaterialInventoryEvent((MaterialInventoryEvent)@event);
            }
            else if (@event is MaterialCollectedEvent)
=======
        public void Handle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));
            // Handle the events that we care about
            if (@event is MaterialCollectedEvent)
>>>>>>> Add material monitor.
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

<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        // Flush any pending events
        public void PostHandle(Event @event)
        {
            Event pendingEvent;
            while (pendingEvents.TryDequeue(out pendingEvent))
            {
                EDDI.Instance.eventHandler(pendingEvent);
            }
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

=======
>>>>>>> Add material monitor.
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

<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        public void HandleProfile(JObject profile)
        {
        }

=======
>>>>>>> Add material monitor.
        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string name, int amount)
        {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
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
=======
            int oldInventory = 0;
            inventory.TryGetValue(name, out oldInventory);
            int newInventory = oldInventory + amount;
            inventory[name] = newInventory;
            Logging.Debug(name + ": " + oldInventory + "->" + newInventory);

            Limits limits;
            if (configuration.limits.TryGetValue(name, out limits))
            {
                // We have limits for this material; carry out relevant checks
                if (limits.maximum.HasValue)
                {
                    if (oldInventory <= limits.maximum && newInventory > limits.maximum)
                    {
                        // We have crossed the high water threshold for this material
                        EDDI.Instance.eventHandler(new MaterialInventoryEvent(DateTime.Now, Material.FromEDName(name), "Maximum", (int)limits.maximum, newInventory, "Increase"));
                    }
                }
                if (limits.desired.HasValue)
                {
                    if (oldInventory < limits.desired && newInventory >= limits.desired)
                    {
                        // We have crossed the desired threshold for this material
                        EDDI.Instance.eventHandler(new MaterialInventoryEvent(DateTime.Now, Material.FromEDName(name), "Desired", (int)limits.maximum, newInventory, "Increase"));
                    }
                }
            }
>>>>>>> Add material monitor.
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string name, int amount)
        {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
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

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string name, int amount)
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

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables["materials"] = inventory;

            return variables;
        }

        public void writeMaterials()
        {
            // Write material configuration with current inventory
            MaterialMonitorConfiguration configuration = new MaterialMonitorConfiguration();
            configuration.materials = inventory;
            configuration.ToFile();
        }

        private void readMaterials()
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

        private void populateMaterialBlueprints()
        {
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
=======
            // Update in-memory stats
            int oldInventory = 0;
            inventory.TryGetValue(name, out oldInventory);
            int newInventory = oldInventory - amount;
            inventory[name] = newInventory;
            Logging.Debug(name + ": " + oldInventory + "->" + newInventory);

            // Check for events
            Limits limits;
            if (configuration.limits.TryGetValue(name, out limits))
            {
                // We have limits for this material; carry out relevant checks
                if (limits.minimum.HasValue)
                {
                    if (oldInventory >= limits.minimum && newInventory < limits.minimum)
                    {
                        // We have crossed the low water threshold for this material
                        EDDI.Instance.eventHandler(new MaterialInventoryEvent(DateTime.Now, Material.FromEDName(name), "Minimum", (int)limits.maximum, newInventory, "Decrease"));
                    }
                }
                if (limits.desired.HasValue)
                {
                    if (oldInventory >= limits.desired && newInventory < limits.desired)
                    {
                        // We have crossed the desired threshold for this material
                        EDDI.Instance.eventHandler(new MaterialInventoryEvent(DateTime.Now, Material.FromEDName(name), "Desired", (int)limits.maximum, newInventory, "Decrease"));
>>>>>>> Add material monitor.
                    }
                }
            }
        }
    }
}
