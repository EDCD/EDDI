using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiCompanionAppService;
using Utilities;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EddiPlayerPPlayMonitor
{
    public class PlayerPPlayMonitor : EDDIMonitor
    {
        
        public string MonitorName()
        {
            return "Player And PowerPlay Monitor";
        }

        public string MonitorVersion()
        {
            return "0.0.1";
        }

        public string MonitorDescription()
        {
            return "Track information about you (your commander) and your PowerPlay.";
        }

        public bool IsRequired()
        {
            return true;
        }



        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public PlayerPPlayMonitor()
        {
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public IDictionary<string, object> GetVariables()
        {
            return null;
        }


        public void HandleProfile(JObject profile)
        {
        }

        public void PostHandle(Event @event)
        {
			
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is PowerJoinedEvent)
            {
                handlePowerJoinedEvent((PowerJoinedEvent)@event);
            }
            else if (@event is PowerLeftEvent)
            {
                handlePowerLeftEvent((PowerLeftEvent)@event);
            }
            else if (@event is PowerDefectedEvent)
            {
                handlePowerDefectedEvent((PowerDefectedEvent)@event);
            }
            else if (@event is PowerPreparationVoteCast)
            {
                handlePowerPreparationVoteCast((PowerPreparationVoteCast)@event);
            }
            else if (@event is PowerSalaryClaimedEvent)
            {
                handlePowerSalaryClaimedEvent((PowerSalaryClaimedEvent)@event);
            }
            else if (@event is PowerCommodityObtainedEvent)
            {
                handlePowerCommodityObtainedEvent((PowerCommodityObtainedEvent)@event);
            }
            else if (@event is PowerCommodityDeliveredEvent)
            {
                handlePowerCommodityDeliveredEvent((PowerCommodityDeliveredEvent)@event);
            }
            else if (@event is PowerCommodityFastTrackedEvent)
            {
                handlePowerCommodityFastTrackedEvent((PowerCommodityFastTrackedEvent)@event);
            }
            else if (@event is PowerVoucherReceivedEvent)
            {
                handlePowerVoucherReceivedEvent((PowerVoucherReceivedEvent)@event);
            }

        }
			
			
		private void handlePowerJoinedEvent(PowerJoinedEvent @event)
		{
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string NewObedience = @event.power;
            if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
            eddiConfiguration.PowerPlayObedience = NewObedience;
            eddiConfiguration.ToFile();

            EDDI.Instance.Cmdr.powerplay = NewObedience;
            // not needed
            //EDDI.Instance.refreshProfile();

            // don't work a don't understand what to do
            //MainWindow.eddiPowerPlayObedience.Text = NewObedience;

        }
			
		private void handlePowerLeftEvent(PowerLeftEvent @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.PowerPlayObedience = "None";
            eddiConfiguration.ToFile();

            EDDI.Instance.Cmdr.powerplay = "None";

        }
			
		private void handlePowerDefectedEvent(PowerDefectedEvent @event)
        { 
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string NewObedience = @event.topower;
            if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
            eddiConfiguration.PowerPlayObedience = NewObedience;
            eddiConfiguration.ToFile();

            EDDI.Instance.Cmdr.powerplay = NewObedience;

        }
			
		private void handlePowerPreparationVoteCast(PowerPreparationVoteCast @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (@event.power != eddiConfiguration.PowerPlayObedience)
            {
                string NewObedience = @event.power;
                if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
                eddiConfiguration.PowerPlayObedience = NewObedience;
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.powerplay = NewObedience;
             }
        }

        private void handlePowerSalaryClaimedEvent(PowerSalaryClaimedEvent @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (@event.power != eddiConfiguration.PowerPlayObedience)
            {
                string NewObedience = @event.power;
                if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
                eddiConfiguration.PowerPlayObedience = NewObedience;
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.powerplay = NewObedience;
            }
        }
			
		private void handlePowerCommodityObtainedEvent(PowerCommodityObtainedEvent @event)
		{
				
		}

        private void handlePowerCommodityDeliveredEvent(PowerCommodityDeliveredEvent @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (@event.power != eddiConfiguration.PowerPlayObedience)
            {
                string NewObedience = @event.power;
                if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
                eddiConfiguration.PowerPlayObedience = NewObedience;
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.powerplay = NewObedience;
            }
        }
			
		private void handlePowerCommodityFastTrackedEvent(PowerCommodityFastTrackedEvent @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (@event.power != eddiConfiguration.PowerPlayObedience)
            {
                string NewObedience = @event.power;
                if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
                eddiConfiguration.PowerPlayObedience = NewObedience;
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.powerplay = NewObedience;
            }
        }

        private void handlePowerVoucherReceivedEvent(PowerVoucherReceivedEvent @event)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (@event.power != eddiConfiguration.PowerPlayObedience)
            {
                string NewObedience = @event.power;
                if (NewObedience == "A. Lavigny-Duval") { NewObedience = "Arissa Lavigny-Duval"; }
                eddiConfiguration.PowerPlayObedience = NewObedience;
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.powerplay = NewObedience;
            }
        }
    }
}
