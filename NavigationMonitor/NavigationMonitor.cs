using Eddi;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiNavigationService;
using EddiStatusMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public readonly ObservableCollection<NavBookmark> bookmarks;
        private static readonly object bookmarksLock = new object();
        public static event EventHandler BookmarksUpdatedEvent;

        // Navigation route data
        private string navDestination;
        private string navRouteList;
        private decimal navRouteDistance;
        private DateTime updateDat;

        private Status currentStatus { get; set; }

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
            bookmarks = new ObservableCollection<NavBookmark>();
            BindingOperations.CollectionRegistering += Bookmarks_CollectionRegistering;
            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
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
            return false;
        }

        public void Start()
        { }

        public void Stop()
        { }

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

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is BookmarkDetailsEvent bookmarkDetailsEvent)
            {
                handleBookmarkDetailsEvent(bookmarkDetailsEvent);
            }
            else if (@event is CarrierJumpedEvent carrierJumpedEvent)
            {
                handleCarrierJumpedEvent(carrierJumpedEvent);
            }
            else if (@event is DockedEvent dockedEvent)
            {
                handleDockedEvent(dockedEvent);
            }
            else if (@event is EnteredNormalSpaceEvent enteredNormalSpaceEvent)
            {
                handleEnteredNormalSpaceEvent(enteredNormalSpaceEvent);
            }
            else if (@event is JumpedEvent jumpedEvent)
            {
                handleJumpedEvent(jumpedEvent);
            }
            else if (@event is LocationEvent locationEvent)
            {
                handleLocationEvent(locationEvent);
            }
            else if (@event is NavRouteEvent navRouteEvent)
            {
                handleNavRouteEvent(navRouteEvent);
            }
        }

        public void PostHandle(Event @event)
        {
            if (@event is TouchdownEvent touchdownEvent)
            {
                handleTouchdownEvent(touchdownEvent);
            }
            else if (@event is LiftoffEvent liftoffEvent)
            {
                handleLiftoffEvent(liftoffEvent);
            }
            else if (@event is UndockedEvent undockedEvent)
            {
                handleUndockedEvent(undockedEvent);
            }
        }

        private void handleBookmarkDetailsEvent(BookmarkDetailsEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
            }
        }

        private void handleCarrierJumpedEvent(CarrierJumpedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
            }
        }

        private void handleDockedEvent(DockedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;

                // Check if we're at a planetary location and capture our location if true
                if ((currentStatus?.near_surface ?? false) && new Station { Model = @event.stationModel }.IsPlanetary())
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    navConfig.tdLat = currentStatus.latitude;
                    navConfig.tdLong = currentStatus.longitude;
                    navConfig.tdPOI = @event.station;
                    navConfig.updatedat = updateDat;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.system, updateDat);
            }
        }

        private void handleEnteredNormalSpaceEvent(EnteredNormalSpaceEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;

                List<NavRouteInfoItem> route = @event.route;
                List<string> routeList = new List<string>();

                navDestination = null;
                navRouteList = null;
                navRouteDistance = 0;

                if (route.Count > 1 && route[0].systemname == EDDI.Instance?.CurrentStarSystem?.systemname)
                {
                    routeList.Add(route[0].systemname);
                    for (int i = 0; i < route.Count - 1; i++)
                    {
                        navRouteDistance += Functions.StellarDistanceLy(route[i].x, route[i].y, route[i].z, route[i + 1].x, route[i + 1].y, route[i + 1].z) ?? 0;
                        routeList.Add(route[i + 1].systemname);
                    }
                    navDestination = route[route.Count - 1].systemname;
                    navRouteList = string.Join("_", routeList);
                    UpdateDestinationData(navDestination, navRouteDistance);
                }

                // Update the navigation configuration 
                writeBookmarks();
            }
        }

        private void handleTouchdownEvent(TouchdownEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;

                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.tdLat = @event.latitude;
                navConfig.tdLong = @event.longitude;
                navConfig.tdPOI = @event.nearestdestination;
                navConfig.updatedat = updateDat;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void handleLiftoffEvent(LiftoffEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.tdLat = null;
                navConfig.tdLong = null;
                navConfig.tdPOI = null;
                navConfig.updatedat = updateDat;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void handleUndockedEvent(UndockedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.tdLat = null;
                navConfig.tdLong = null;
                navConfig.tdPOI = null;
                navConfig.updatedat = updateDat;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<NavBookmark>(bookmarks),
                ["navRouteList"] = navRouteList,
                ["orbitalpriority"] = navConfig.prioritizeOrbitalStations
            };
            return variables;
        }

        public void writeBookmarks()
        {
            lock (bookmarksLock)
            {
                // Write bookmarks configuration with current list
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.bookmarks = bookmarks;
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
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Obtain current bookmarks list from configuration
                updateDat = navConfig.updatedat;

                // Build a new bookmark list
                bookmarks.Clear();
                foreach (NavBookmark bookmark in navConfig.bookmarks)
                {
                    bookmarks.Add(bookmark);
                }
            }
        }

        public void RemoveBookmarkAt(int index)
        {
            lock (bookmarksLock)
            {
                bookmarks.RemoveAt(index);
                // Make sure the UI is up to date
                RaiseOnUIThread(BookmarksUpdatedEvent, bookmarks);
            }
        }

        public void UpdateDestinationData(string system, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistanceLy = distance;
        }

        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                LockManager.GetLock(nameof(currentStatus), () => 
                {
                    currentStatus = status;
                });
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
