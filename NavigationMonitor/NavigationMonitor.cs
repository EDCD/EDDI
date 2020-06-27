using Eddi;
using EddiConfigService;
using EddiDataDefinitions;
using EddiEvents;
using EddiNavigationService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiNavigationMonitor
{
    public class NavigationMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Bookmark> bookmarks { get; private set; }

        private NavigationMonitorConfiguration navConfig = new NavigationMonitorConfiguration();

        // Navigation route data
        public string navDestination;
        public string navRouteList;
        public decimal navRouteDistance;

        private DateTime updateDat;

        private static readonly object bookmarksLock = new object();
        public event EventHandler BookmarksUpdatedEvent;

        public string MonitorName()
        {
            return "Navigation monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.NavigationMonitor.navigation_monitor_name;
        }

        public string MonitorDescription()
        {
            return Properties.NavigationMonitor.navigation_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public NavigationMonitor()
        {
            bookmarks = new ObservableCollection<Bookmark>();
            BindingOperations.CollectionRegistering += Bookmarks_CollectionRegistering;
            initializeNavigationMonitor();
        }

        public void initializeNavigationMonitor()
        {
            readBookmarks();
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void Bookmarks_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock); });
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
            readBookmarks();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(bookmarks); });
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
            if (@event is CarrierJumpedEvent)
            {
                handleCarrierJumpedEvent((CarrierJumpedEvent)@event);
            }
            else if (@event is LocationEvent)
            {
                handleLocationEvent((LocationEvent)@event);
            }
            else if (@event is JumpedEvent)
            {
                handleJumpedEvent((JumpedEvent)@event);
            }
            else if (@event is NavRouteEvent)
            {
                handleNavRouteEvent((NavRouteEvent)@event);
            }
            else if (@event is RouteDetailsEvent)
            {
                handleRouteDetailsEvent((RouteDetailsEvent)@event);
            }
        }

        private void handleCarrierJumpedEvent(CarrierJumpedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
            }
        }
        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.system, updateDat);
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            updateDat = @event.timestamp;
            if (_handleNavRouteEvent(@event))
            {
                writeBookmarks();
            }
        }

        private bool _handleNavRouteEvent(NavRouteEvent @event)
        {
            navDestination = null;
            navRouteList = null;
            navRouteDistance = 0;
            string station = null;
            StarSystem curr = EDDI.Instance?.CurrentStarSystem;
            List<NavRouteInfo> route = @event.navRoute;
            List<string> routeList = new List<string>();

            if (route.Count > 1 && route[0].starSystem == curr.systemname)
            {
                routeList.Add(route[0].starSystem);
                for (int i = 0; i < route.Count - 1; i++)
                {
                    navRouteDistance += CalculateDistance(route[i], route[i + 1]);
                    routeList.Add(route[i + 1].starSystem);
                }
                navDestination = route[route.Count - 1].starSystem;
                navRouteList = string.Join("_", routeList);

                if (navDestination == navConfig.searchSystem) { station = navConfig.searchStation; }
                UpdateDestinationData(navDestination, station, navRouteDistance);

                // Get up-to-date configuration data
                navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                List<long> missionids = new List<long>();
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                List<NavRouteInfo> route = @event.navRoute;
                List<string> routeList = new List<string>();
                decimal nextSystemDistance = 0;
                int count = 0;

                navDestination = null;
                navRouteList = null;
                navRouteDistance = 0;
                if (navConfig.searchQuery != "cancel")
                {
                    count = route.Count;
                    if (count > 1 && route[0].starSystem == curr.systemname)
                    {
                        routeList.Add(route[0].starSystem);
                        for (int i = 0; i < count - 1; i++)
                        {
                            navRouteDistance += CalculateDistance(route[i], route[i + 1]);
                            if (i == 0) { nextSystemDistance = navRouteDistance; }
                            routeList.Add(route[i + 1].starSystem);
                        }
                        navDestination = route[count - 1].starSystem;
                        navRouteList = string.Join("_", routeList);
                        UpdateDestinationData(navDestination, navRouteDistance);

                        // Get mission IDs for 'set' system
                        missionids = NavigationService.Instance.GetSystemMissionIds(navDestination);
                    }
                }
                EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "nav", navDestination, null, navRouteList, count, nextSystemDistance, navRouteDistance, missionids));

                // Update the navigation configuration 
                writeBookmarks();
            }
        }
    
        private void handleRouteDetailsEvent(RouteDetailsEvent @event)
        {

        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<Bookmark>(bookmarks),
                ["navRouteList"] = navRouteList
            };
            return variables;
        }

        public void writeBookmarks()
        {
            lock (bookmarksLock)
            {
                // Write bookmarks configuration with current list
                navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.bookmarks = bookmarks;
                navConfig.navDestination = navDestination;
                navConfig.navRouteDistance = navRouteDistance;
                navConfig.navRouteList = navRouteList;
                if (navConfig.searchQuery == "cancel") { navConfig.searchQuery = null; }
                navConfig.updatedat = updateDat;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(BookmarksUpdatedEvent, bookmarks);
        }

        private void readBookmarks()
        {
            lock (bookmarksLock)
            {
                // Obtain current bookmarks list from configuration
                navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navDestination = navConfig.navDestination;
                navRouteDistance = navConfig.navRouteDistance;
                navRouteList = navConfig.navRouteList;
                updateDat = navConfig.updatedat;

                // Build a new bookmark list
                bookmarks.Clear();
                foreach (Bookmark bookmark in navConfig.bookmarks)
                {
                    bookmarks.Add(bookmark);
                }
            }
        }

        private void RemoveBookmark(Bookmark bookmark)
        {
            _RemoveBookmark(bookmark);
        }

        public void _RemoveBookmark(Bookmark bookmark)
        {
            string system = bookmark.system.ToLowerInvariant();
            string body = bookmark.body?.ToLowerInvariant();
            string poi = bookmark.poi?.ToLowerInvariant();
            lock (bookmarksLock)
            {
                for (int i = 0; i < bookmarks.Count; i++)
                {
                    if (bookmarks[i].system.ToLowerInvariant() == system
                        && bookmarks[i].body?.ToLowerInvariant() == body
                        && bookmarks[i].poi?.ToLowerInvariant() == poi)
                    {
                        bookmarks.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public decimal CalculateDistance(NavRouteInfo curr, NavRouteInfo dest)
        {
            double square(double x) => x * x;
            decimal distance = 0;
            if (curr?.x != null && dest?.x != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(square((double)(curr.x - dest.x))
                            + square((double)(curr.y - dest.y))
                            + square((double)(curr.z - dest.z))), 2);
            }
            return distance;
        }

        public void UpdateDestinationData(string system, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistanceLy = distance;
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
