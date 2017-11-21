using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        // Observable collection for us to handle changes
        public ObservableCollection<Cargo> inventory = new ObservableCollection<Cargo>();

        public string MonitorName()
        {
            return "Cargo monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Track information on your cargo.";
        }

        public bool IsRequired()
        {
            return true;
        }

        public CargoMonitor()
        {
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
        }

        public UserControl ConfigurationTabItem()
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
            if (@event is CargoInventoryEvent)
            {
                handleCargoInventoryEvent((CargoInventoryEvent)@event);
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
            // TODO Powerplay events
        }

        private void handleCargoInventoryEvent(CargoInventoryEvent @event)
        {
            //// CargoInventoryEvent does not contain stolen, missionid, or cost information so merge it here
            //foreach (Cargo cargo in @event.inventory)
            //{
            //    bool added = false;
            //    foreach (Cargo inventoryCargo in inventory)
            //    {
            //        if (inventoryCargo.commodity == cargo.commodity)
            //        {
            //            // Match of commodity
            //            added = true;
            //        }
            //    }
            //    if (!added)
            //    {
            //        // We haven't heard of this cargo so add it to the inventory directly
            //        AddCargo(cargo);
            //    }
            //}
            //inventory.Clear();
            //foreach (Cargo cargo in @event.inventory)
            //{
            //    inventory.Add(cargo);
            //}
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["cargo"] = new List<Cargo>(inventory)
            };
            return variables;
        }

        public void AddCargo(Cargo cargo)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                addCargo(cargo);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    addCargo(cargo);
                }));
            }
        }

        public void RemoveCargo(Cargo cargo)
        {
            // If we were started from VoiceAttack then we might not have an application; check here and create if it doesn't exist
            if (Application.Current == null)
            {
                new Application();
            }

            // Run this on the dispatcher to ensure that we can update it whilst reflecting changes in the UI
            if (Application.Current.Dispatcher.CheckAccess())
            {
                removeCargo(cargo);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    removeCargo(cargo);
                }));
            }
        }

        private void addCargo(Cargo cargo)
        {
            foreach (Cargo inventoryCargo in inventory)
            {
                if (inventoryCargo.commodity == cargo.commodity)
                {
                    // Matching commodity; see if the details match
                    if (inventoryCargo.missionid != null)
                    {
                        // Mission-specific cargo
                        if (inventoryCargo.missionid == cargo.missionid)
                        {
                            // Both for the same mission - add to this
                            inventoryCargo.amount += cargo.amount;
                            return;
                        }
                        // Different mission; skip
                        continue;
                    }

                    if (inventoryCargo.stolen == cargo.stolen)
                    {
                        // Both of the same legality
                        if (inventoryCargo.price == cargo.price)
                        {
                            // Same cost basis - add to this
                            inventoryCargo.amount += cargo.amount;
                            return;
                        }
                    }
                }
            }
            // No matching cargo - add entry
            inventory.Add(cargo);
        }

        private void removeCargo(Cargo cargo)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                Cargo inventoryCargo = inventory[i];
                if (inventoryCargo.commodity == cargo.commodity)
                {
                    // Matching commodity; see if the details match
                    if (inventoryCargo.missionid != null)
                    {
                        // Mission-specific cargo
                        if (inventoryCargo.missionid == cargo.missionid)
                        {
                            // Both for the same mission - remove from this
                            if (inventoryCargo.amount == cargo.amount)
                            {
                                inventory.RemoveAt(i);
                                return;
                            }
                            else
                            {
                                inventoryCargo.amount -= cargo.amount;
                            }
                            return;
                        }
                        // Different mission; skip
                        continue;
                    }

                    if (inventoryCargo.stolen == cargo.stolen)
                    {
                        // Both of the same legality
                        if (inventoryCargo.price == cargo.price)
                        {
                            // Same cost basis - remove from this
                            if (inventoryCargo.amount == cargo.amount)
                            {
                                inventory.RemoveAt(i);
                                return;
                            }
                            else
                            {
                                inventoryCargo.amount -= cargo.amount;
                            }
                            return;
                        }
                    }
                }
            }
            // No matching cargo - ignore
            Logging.Debug("Did not find match for cargo " + JsonConvert.SerializeObject(cargo));
        }
    }
}
