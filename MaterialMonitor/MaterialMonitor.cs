using System.Collections.Generic;
using Eddi;
using System.Windows.Controls;
using System;
using System.Text.RegularExpressions;
using System.IO;
using EddiEvents;
using Utilities;
using Newtonsoft.Json;
using EddiDataDefinitions;

namespace EddiMaterialMonitor
{
    /// <summary>
    /// A monitor that keeps track of the number of materials held and sends events on user-defined changes
    /// </summary>
    public class MaterialMonitor : EDDIMonitor
    {
        // The file to log speech
        public static readonly string LogFile = Constants.DATA_DIR + @"\materialsresponder.out";

        // Configuration of the responder
        private MaterialMonitorConfiguration configuration;

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
            if (@event is MaterialCollectedEvent)
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
            Limits limits;
            if (!configuration.limits.TryGetValue(name, out limits))
            {
                // No information for the current material - create one and set it to 0
                limits = new Limits(0, null, null, null);
            }
            int previous = limits.current;
            limits.current += amount;
            Logging.Debug(name + ": " + previous + "->" + limits.current);
            configuration.limits[name] = limits;

            if (limits.maximum.HasValue)
            {
                if (previous <= limits.maximum && limits.current > limits.maximum)
                {
                    // We have crossed the high water threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Maximum", (int)limits.maximum, limits.current, "Increase"));
                }
            }
            if (limits.desired.HasValue)
            {
                if (previous < limits.desired && limits.current >= limits.desired)
                {
                    // We have crossed the desired threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)limits.desired, limits.current, "Increase"));
                }
            }
            // Update configuration information
            configuration.ToFile();
        }

        /// <summary>
        /// Decrement the current amount of a material, potentially triggering events as a result
        /// </summary>
        private void decMaterial(string name, int amount)
        {
            Limits limits;
            if (!configuration.limits.TryGetValue(name, out limits))
            {
                // No information for the current material - create one and set it to amount
                limits = new Limits(amount, null, null, null);
            }
            int previous = limits.current;
            limits.current -= amount;
            Logging.Debug(name + ": " + previous + "->" + limits.current);
            configuration.limits[name] = limits;


            // We have limits for this material; carry out relevant checks
            if (limits.minimum.HasValue)
            {
                if (previous >= limits.minimum && limits.current < limits.minimum)
                {
                    // We have crossed the low water threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Minimum", (int)limits.minimum, limits.current, "Decrease"));
                }
            }
            if (limits.desired.HasValue)
            {
                if (previous >= limits.desired && limits.current < limits.desired)
                {
                    // We have crossed the desired threshold for this material
                    EDDI.Instance.eventHandler(new MaterialThresholdEvent(DateTime.Now, Material.FromName(name), "Desired", (int)limits.desired, limits.current, "Decrease"));
                }
            }
            // Update configuration information
            configuration.ToFile();
        }

        /// <summary>
        /// Set the current amount of a material
        /// </summary>
        private void setMaterial(string name, int amount)
        {
            Limits limits;
            if (!configuration.limits.TryGetValue(name, out limits))
            {
                // No information for the current material - create one and set it to amount
                limits = new Limits(amount, null, null, null);
            }
            configuration.ToFile();
        }
    }
}
