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
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using JetBrains.Annotations;
using Utilities;

namespace EddiNavigationMonitor
{
    [UsedImplicitly]
    public class NavigationMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes to Bookmarks
        public ObservableCollection<NavBookmark> Bookmarks = new ObservableCollection<NavBookmark>();
        public static event EventHandler BookmarksUpdatedEvent;

        // Navigation route data
        public NavWaypointCollection NavRouteList = new NavWaypointCollection() { FillVisitedGaps = true };
        public static event EventHandler NavRouteUpdatedEvent;

        // Plotted route data
        public NavWaypointCollection PlottedRouteList = new NavWaypointCollection();
        public static event EventHandler PlottedRouteUpdatedEvent;

        public static readonly object navConfigLock = new object();

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

            // Make sure the UI is up to date
            RaiseOnUIThread(BookmarksUpdatedEvent, Bookmarks);
            RaiseOnUIThread(NavRouteUpdatedEvent, NavRouteList);
            RaiseOnUIThread(PlottedRouteUpdatedEvent, PlottedRouteList);

            Logging.Info($"Initialized {MonitorName()}");
        }

        private void NavigationMonitor_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(NavRouteList.Waypoints, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(PlottedRouteList.Waypoints, navConfigLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRouteList.Waypoints, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRouteList.Waypoints, navConfigLock); });
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
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRouteList.Waypoints, navConfigLock); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRouteList.Waypoints, navConfigLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(Bookmarks); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(NavRouteList.Waypoints); });
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(PlottedRouteList.Waypoints); });
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
            else if (@event is FSDTargetEvent fsdTargetEvent)
            {
                handleFSDTargetEvent(fsdTargetEvent);
            }
        }

        private void handleFSDTargetEvent(FSDTargetEvent @event)
        {
            // Update our plotted route star class data if this event provides new details about the targeted star class.
            var wp = PlottedRouteList.Waypoints.FirstOrDefault(w => w.systemAddress == (ulong?)@event.systemAddress);
            if (wp != null && wp.stellarclass != @event.starclass)
            {
                wp.stellarclass = @event.starclass;
                wp.isScoopable = !string.IsNullOrEmpty(@event.starclass) && "KGBFOAM".Contains(@event.starclass);
                wp.hasNeutronStar = !string.IsNullOrEmpty(@event.starclass) && "N".Contains(@event.starclass);
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

        private void updateNavigationData(ulong systemAddress, decimal? x, decimal? y, decimal? z)
        {
            NavRouteList.UpdateVisitedStatus(systemAddress);
            PlottedRouteList.UpdateVisitedStatus(systemAddress);
            NavigationService.Instance.SearchDistanceLy = Functions.StellarDistanceLy(x, y, z,
                NavigationService.Instance.SearchStarSystem?.x, NavigationService.Instance.SearchStarSystem?.y, NavigationService.Instance.SearchStarSystem?.z) ?? 0;
            if (PlottedRouteList.GuidanceEnabled && PlottedRouteList.Waypoints.All(w => w.visited))
            {
                // Deactivate guidance once we've reached our destination.
                NavigationService.Instance.NavQuery(QueryType.cancel);
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
                updateNavigationData(@event.systemAddress ?? 0, @event.x, @event.y, @event.z);
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
                updateNavigationData(@event.systemAddress ?? 0, @event.x, @event.y, @event.z);
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                updateNavigationData(@event.systemAddress, @event.x, @event.y, @event.z);
            }
        }

        private void handleEnteredNormalSpaceEvent(EnteredNormalSpaceEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                NavRouteList.UpdateVisitedStatus(@event.systemAddress);
                PlottedRouteList.UpdateVisitedStatus(@event.systemAddress);
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (@event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;

                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null && routeList.Count > 1 && routeList[0].systemName == EDDI.Instance?.CurrentStarSystem?.systemname)
                {
                    routeList[0].visited = true;
                    UpdateDestinationData(routeList.Last().systemName, NavRouteList.RouteDistance);
                    NavRouteList.Waypoints.Clear();
                    NavRouteList.AddRange(routeList);
                    NavRouteList.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions?.ToList());
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
            if (!PlottedRouteList.GuidanceEnabled)
            {
                if (routeDetailsEvent.Route?.Waypoints.GetHashCode() == PlottedRouteList.Waypoints.GetHashCode())
                {
                    // Displayed route is correct, nothing to do here
                }
                else if (routeDetailsEvent.Route != null)
                {
                    PlottedRouteList.Waypoints.Clear();
                    PlottedRouteList.AddRange(routeDetailsEvent.Route.Waypoints);
                    PlottedRouteList.FillVisitedGaps = routeDetailsEvent.Route.FillVisitedGaps;
                    PlottedRouteList.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions?.ToList());
                }
                else
                {
                    PlottedRouteList.Waypoints.Clear();
                }
            }

            if (routeDetailsEvent.routetype == QueryType.set.ToString())
            {
                PlottedRouteList.GuidanceEnabled = true;
            }
            else if (routeDetailsEvent.routetype == QueryType.cancel.ToString())
            {
                PlottedRouteList.GuidanceEnabled = false;
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
                navConfig.navRouteList = NavRouteList;
                navConfig.plottedRouteList = PlottedRouteList;
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
                Bookmarks = navConfig.bookmarks;

                // Restore our in-game routing
                NavRouteList = navConfig.navRouteList;

                // Restore our plotted routing
                PlottedRouteList = navConfig.plottedRouteList;
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
