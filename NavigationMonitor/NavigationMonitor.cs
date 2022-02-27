using Eddi;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusMonitor;
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
        // Observable collection for us to handle changes to Bookmarks
        public readonly ObservableCollection<NavBookmark> Bookmarks = new ObservableCollection<NavBookmark>();
        public static readonly object navConfigLock = new object();
        public static event EventHandler BookmarksUpdatedEvent;

        // Navigation route data
        public ObservableCollection<NavWaypoint> NavRouteList = new ObservableCollection<NavWaypoint>();
        private decimal navRouteDistance;
        public static event EventHandler NavRouteUpdatedEvent;

        // Plotted route data
        public ObservableCollection<NavWaypoint> PlottedRouteList = new ObservableCollection<NavWaypoint>();
        private decimal plottedRouteDistance;
        public static event EventHandler PlottedRouteUpdatedEvent;
        public bool GuidanceEnabled;

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
            BindingOperations.CollectionRegistering += NavigationMonitor_CollectionRegistering;
            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
            initializeNavigationMonitor();
        }

        public void initializeNavigationMonitor()
        {
            ReadNavConfig();
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void NavigationMonitor_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(NavRouteList, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(PlottedRouteList, navConfigLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRouteList, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRouteList, navConfigLock); });
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
            ReadNavConfig();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRouteList, navConfigLock); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRouteList, navConfigLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(Bookmarks); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(NavRouteList); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(PlottedRouteList); });
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
            else if (@event is RouteDetailsEvent routeDetailsEvent)
            {
                handleRouteDetailsEvent(routeDetailsEvent);
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
                UpdateVisitedStatus(ref NavRouteList, (ulong)(@event.systemAddress ?? 0));
                UpdateVisitedStatus(ref PlottedRouteList, (ulong)(@event.systemAddress ?? 0));
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
                UpdateVisitedStatus(ref NavRouteList, (ulong)(@event.systemAddress ?? 0));
                UpdateVisitedStatus(ref PlottedRouteList, (ulong)(@event.systemAddress ?? 0));
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                UpdateVisitedStatus(ref NavRouteList, (ulong)@event.systemAddress);
                UpdateVisitedStatus(ref PlottedRouteList, (ulong)@event.systemAddress);
            }
        }

        private void handleEnteredNormalSpaceEvent(EnteredNormalSpaceEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                UpdateVisitedStatus(ref NavRouteList, (ulong)@event.systemAddress);
                UpdateVisitedStatus(ref PlottedRouteList, (ulong)@event.systemAddress);
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                var routeDistance = 0M;

                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null && routeList.Count > 1 && routeList[0].systemName == EDDI.Instance?.CurrentStarSystem?.systemname)
                {
                    routeList[0].visited = true;
                    CalculateRouteDistances(ref routeList, ref routeDistance);

                    // Supplement w/ mission info
                    var missionMultiDestinationSystems = ConfigService.Instance.missionMonitorConfiguration.missions
                        .Where(m => m.destinationsystems != null)
                        .SelectMany(m => m.destinationsystems.Select(s => s.systemName));
                    var missionSingleDestinationSystems = ConfigService.Instance.missionMonitorConfiguration.missions
                        .Where(m => !string.IsNullOrEmpty(m.destinationsystem))
                        .Select(m => m.destinationsystem);
                    var missionSystems = missionMultiDestinationSystems.Concat(missionSingleDestinationSystems).Distinct();
                    foreach (var system in missionSystems)
                    {
                        foreach (var waypoint in routeList)
                        {
                            if (waypoint.systemName == system)
                            {
                                waypoint.isMissionSystem = true;
                            }
                        }
                    }

                    UpdateDestinationData(routeList.Last().systemName, routeDistance);
                    NavRouteList.Clear();
                    foreach (var waypoint in routeList)
                    {
                        NavRouteList.Add(waypoint);
                    }
                }

                // Raise on UI thread
                RaiseOnUIThread(NavRouteUpdatedEvent, NavRouteList);

                // Update the navigation configuration 
                WriteNavConfig();
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

        private void handleRouteDetailsEvent(RouteDetailsEvent routeDetailsEvent)
        {
            if (routeDetailsEvent.Route != null)
            {
                var routeList = routeDetailsEvent.Route;
                var routeDistance = 0M;
                CalculateRouteDistances(ref routeList, ref routeDistance);
                plottedRouteDistance = routeDistance;
                PlottedRouteList.Clear();
                foreach (var waypoint in routeDetailsEvent.Route)
                {
                    PlottedRouteList.Add(waypoint);
                }
            }
            else
            {
                PlottedRouteList.Clear();
                plottedRouteDistance = 0M;
            }

            if (routeDetailsEvent.routetype == "set")
            {
                GuidanceEnabled = true;
            }
            else if (routeDetailsEvent.routetype == "cancel")
            {
                GuidanceEnabled = true;
            }

            // Raise on UI thread
            RaiseOnUIThread(PlottedRouteUpdatedEvent, PlottedRouteList);

            // Update the navigation configuration 
            WriteNavConfig();
        }

        public IDictionary<string, object> GetVariables()
        {
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<NavBookmark>(Bookmarks),
                ["navRouteList"] = NavRouteList,
                ["orbitalpriority"] = navConfig.prioritizeOrbitalStations
            };
            return variables;
        }

        public void WriteNavConfig()
        {
            lock (navConfigLock)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.bookmarks = Bookmarks;
                navConfig.navRouteList = NavRouteList.ToList();
                navConfig.navRouteDistance = navRouteDistance;
                navConfig.plottedRouteList = PlottedRouteList.ToList();
                navConfig.plottedRouteDistance = plottedRouteDistance;
                navConfig.routeGuidanceEnabled = GuidanceEnabled;
                navConfig.updatedat = updateDat;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(BookmarksUpdatedEvent, Bookmarks);
            RaiseOnUIThread(NavRouteUpdatedEvent, NavRouteList);
            RaiseOnUIThread(PlottedRouteUpdatedEvent, PlottedRouteList);
        }

        private void ReadNavConfig()
        {
            lock (navConfigLock)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Obtain current bookmarks list from configuration
                updateDat = navConfig.updatedat;

                // Build a new bookmark list
                Bookmarks.Clear();
                foreach (var bookmark in navConfig.bookmarks)
                {
                    Bookmarks.Add(bookmark);
                }

                // Restore our in-game routing
                NavRouteList.Clear();
                if (navConfig.navRouteList != null)
                {
                    foreach (var waypoint in navConfig.navRouteList)
                    {
                        NavRouteList.Add(waypoint);
                    }
                }
                navRouteDistance = navConfig.navRouteDistance;

                // Restore our plotted routing
                PlottedRouteList.Clear();
                if (navConfig.plottedRouteList != null)
                {
                    foreach (var waypoint in navConfig.plottedRouteList)
                    {
                        PlottedRouteList.Add(waypoint);
                    }
                }
                plottedRouteDistance = navConfig.plottedRouteDistance;
                GuidanceEnabled = navConfig.routeGuidanceEnabled;
            }
        }

        public void RemoveBookmarkAt(int index)
        {
            lock (navConfigLock)
            {
                Bookmarks.RemoveAt(index);
                // Make sure the UI is up to date
                RaiseOnUIThread(BookmarksUpdatedEvent, Bookmarks);
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

                foreach (var bookmark in Bookmarks)
                {
                    CheckBookmarkPosition(bookmark, currentStatus);
                }
            }
        }

        public void CheckBookmarkPosition(NavBookmark bookmark, Status status, bool emitEvent = true)
        {
            if (bookmark is null || status is null) { return; }

            // Calculate our position relative to the bookmark and whether we're nearby
            if (currentStatus.bodyname == bookmark.bodyname && currentStatus.near_surface)
            {
                // Update our bookmark heading and distance
                var surfaceDistanceKm = bookmark.useStraightPath
                    ? SurfaceConstantHeadingDistanceKm(currentStatus, bookmark.latitude, bookmark.longitude)
                    : SurfaceShortestPathDistanceKm(currentStatus, bookmark.latitude, bookmark.longitude);
                if (surfaceDistanceKm != null)
                {
                    var trueDistanceKm = (decimal) Math.Sqrt(Math.Pow((double)surfaceDistanceKm, 2) +
                                                             Math.Pow((double?) (status.altitude / 1000) ?? 0, 2));
                    bookmark.distanceKm = trueDistanceKm;
                    bookmark.heading = bookmark.useStraightPath
                        ? SurfaceConstantHeadingDegrees(currentStatus, bookmark.latitude, bookmark.longitude)
                        : SurfaceShortestPathDegrees(currentStatus, bookmark.latitude, bookmark.longitude);

                    var trueDistanceMeters = trueDistanceKm * 1000;
                    if (!bookmark.nearby && trueDistanceMeters < bookmark.arrivalRadiusMeters)
                    {
                        // We've entered the nearby radius of the bookmark
                        bookmark.nearby = true;
                        if (emitEvent)
                        {
                            EDDI.Instance.enqueueEvent(new NearBookmarkEvent(status.timestamp, true, bookmark));
                        }
                    }
                    else if (bookmark.nearby && trueDistanceMeters >= bookmark.arrivalRadiusMeters * 1.1M)
                    {
                        // We've left the nearby radius of the bookmark
                        // (calculated at 110% of the arrival radius to prevent bouncing between nearby and not)
                        bookmark.nearby = false;
                        if (emitEvent)
                        {
                            EDDI.Instance.enqueueEvent(new NearBookmarkEvent(status.timestamp, false, bookmark));
                        }
                    }
                }
            }
            else if (bookmark.heading != null || bookmark.distanceKm != null)
            {
                // We're not at the body, clear bookmark position data
                bookmark.heading = null;
                bookmark.distanceKm = null;
                bookmark.nearby = false;
            }
        }

        private static decimal? SurfaceConstantHeadingDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceConstantHeadingDegrees(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceConstantHeadingDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceConstantHeadingDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceShortestPathDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceHeadingDegrees(curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceShortestPathDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        public bool IsNavRouteComplete(List<NavWaypoint> routeList, bool completeWithFinalWaypoint = false)
        {
            return completeWithFinalWaypoint
                ? routeList.LastOrDefault()?.visited ?? true
                : routeList.All(w => w.visited);
        }

        private void CalculateRouteDistances(ref List<NavWaypoint> routeList, ref decimal routeDistance)
        {
            // Calculate distance of each hop and total distance traveled
            navRouteDistance = 0M;
            routeList.First().distanceTraveled = 0;
            for (int i = 0; i < routeList.Count - 1; i++)
            {
                routeList[i + 1].distance = Functions.StellarDistanceLy(routeList[i].x, routeList[i].y, routeList[i].z,
                    routeList[i + 1].x, routeList[i + 1].y, routeList[i + 1].z) ?? 0;
                navRouteDistance += routeList[i + 1].distance;
                routeList[i + 1].distanceTraveled = navRouteDistance;
            }

            // Calculate distance remaining
            foreach (var waypoint in routeList)
            {
                waypoint.distanceRemaining = navRouteDistance - waypoint.distanceTraveled;
            }
        }

        private void UpdateVisitedStatus(ref ObservableCollection<NavWaypoint> routeList, ulong systemAddress)
        {
            foreach (var waypoint in routeList)
            {
                if (!waypoint.visited && waypoint.systemAddress == systemAddress)
                {
                    waypoint.visited = true;
                }
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
