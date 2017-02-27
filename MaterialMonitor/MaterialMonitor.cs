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

namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : EDDIMonitor
    {
        // The file to log speech
        public static readonly string LogFile = Constants.DATA_DIR + @"\materialsresponder.out";

        private MaterialMonitorConfiguration configuration;
        private IDictionary<string, MaterialAmount> inventory;

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

        public MaterialMonitor()
        {
            configuration = MaterialMonitorConfiguration.FromFile();
            // Easier for us to use this internally as a dictionary
            inventory = configuration.materials.ToDictionary(m => m.material);
            
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
        }

        public void Stop()
        {
        }

        public void Reload()
        {
            configuration = MaterialMonitorConfiguration.FromFile();
            inventory = configuration.materials.ToDictionary(m => m.material);
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Handle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));
            // Handle the events that we care about

            if (@event is MaterialInventoryEvent)
            {
                handleMaterialInventoryEvent((MaterialInventoryEvent)@event);
            }
            else if (@event is MaterialDiscardedEvent)
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

        private void handleMaterialInventoryEvent(MaterialInventoryEvent @event)
        {
            List<string> knownNames = new List<string>();
            foreach (MaterialAmount materialAmount in @event.inventory)
            {
                setMaterial(materialAmount.material, materialAmount.amount);
                knownNames.Add(materialAmount.material);
            }

            // Also remove any items for which we have neither inventory nor limits
            inventory = inventory.Where(i => (i.Value.amount != 0 || i.Value.desired.HasValue || i.Value.minimum.HasValue || i.Value.maximum.HasValue)).ToDictionary(i => i.Key, i => i.Value);

            // Update configuration information
            configuration.materials = inventory.Values.ToList();
            configuration.ToFile();
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

        /// <summary>
        /// Increment the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void incMaterial(string name, int amount)
        {
            Material material = Material.FromName(name);
            MaterialAmount ma;
            if (!inventory.TryGetValue(name, out ma))
            {
                // No information for the current material - create one and set it to 0
                ma = new MaterialAmount(material, 0);
            }
            int previous = ma.amount;
            ma.amount -= amount;
            Logging.Debug(name + ": " + previous + "->" + ma.amount);
            inventory[name] = ma;

            if (ma.maximum.HasValue)
            {
                if (previous <= ma.maximum && ma.amount > ma.maximum)
                {
                    // We have crossed the high water threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Maximum", (int)ma.maximum, ma.amount, "Increase"));
                }
            }
            if (ma.desired.HasValue)
            {
                if (previous < ma.desired && ma.amount >= ma.desired)
                {
                    // We have crossed the desired threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)ma.desired, ma.amount, "Increase"));
                }
            }

            // Update configuration information
            configuration.materials = inventory.Values.ToList();
            configuration.ToFile();
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string name, int amount)
        {
            Material material = Material.FromName(name);
            MaterialAmount ma;
            if (!inventory.TryGetValue(name, out ma))
            {
                // No information for the current material - create one and set it to amount
                ma = new MaterialAmount(material, amount);
            }
            int previous = ma.amount;
            ma.amount -= amount;
            Logging.Debug(name + ": " + previous + "->" + ma.amount);
            inventory[name] = ma;

            // We have limits for this material; carry out relevant checks
            if (ma.minimum.HasValue)
            {
                if (previous >= ma.minimum && ma.amount < ma.minimum)
                {
                    // We have crossed the low water threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Minimum", (int)ma.minimum, ma.amount, "Decrease"));
                }
            }
            if (ma.desired.HasValue)
            {
                if (previous >= ma.desired && ma.amount < ma.desired)
                {
                    // We have crossed the desired threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)ma.desired, ma.amount, "Decrease"));
                }
            }
            // Update configuration information
            configuration.materials = inventory.Values.ToList();
            configuration.ToFile();
        }

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string name, int amount)
        {
            Material material = Material.FromName(name);
            MaterialAmount ma;
            if (!inventory.TryGetValue(name, out ma))
            {
                // No information for the current material - create one and set it to amount
                ma = new MaterialAmount(material, amount);
                inventory[name] = ma;
            }
            ma.amount = amount;
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables["materials"] = configuration.materials;

            return variables;
        }
    }
}
