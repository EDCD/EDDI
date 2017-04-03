using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiCargoMonitor
{
    /**
     * Monitor cargo for the current ship
     * Missing: there is no event for when a drone is fired, so we cannot keep track of this individually.  Instead we have to rely
     * on the inventory events to give us information on the number of drones in-ship.
     */
    public class CargoMonitor : EDDIMonitor
    {
        // List of cargo for the current ship
        public List<Cargo> cargo { get; private set; }

        public void HandleProfile(JObject profile)
        {
            throw new NotImplementedException();
        }

        public bool IsRequired()
        {
            return true;
        }

        public string MonitorDescription()
        {
            throw new NotImplementedException();
        }

        public string MonitorName()
        {
            return "Cargo monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public void PostHandle(Event @event)
        {
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is CargoInventoryEvent)
            {

            }
            else if (@event is CommodityCollectedEvent)
            {

            }
            else if (@event is CommodityEjectedEvent)
            {

            }
            else if (@event is CommodityPurchasedEvent)
            {

            }
            else if (@event is CommodityRefinedEvent)
            {

            }
            else if (@event is CommoditySoldEvent)
            {

            }
            else if (@event is PowerCommodityObtainedEvent)
            {

            }
            else if (@event is PowerCommodityDeliveredEvent)
            {

            }
            else if (@event is LimpetPurchasedEvent)
            {

            }
            else if (@event is LimpetSoldEvent)
            {

            }
            else if (@event is MissionAbandonedEvent)
            {
                // If we abandon a mission with cargo it becomes stolen
            }
            else if (@event is MissionAcceptedEvent)
            {
                // Check to see if this is a cargo mission and update our inventory accordingly
            }
            else if (@event is MissionCompletedEvent)
            {
                // Check to see if this is a cargo mission and update our inventory accordingly
            }
            else if (@event is MissionFailedEvent)
            {
                // If we fail a mission with cargo it becomes stolen
            }
        }

        public void Reload()
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables["cargo"] = cargo;

            return variables;
        }
    }
}
