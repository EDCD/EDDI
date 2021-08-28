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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiNavigationMonitor
{
    public class NavigationMonitor : EDDIMonitor
    {
        // A token source so that we can cancel the guidance system when desired
        private CancellationTokenSource guidanceCancellationTokenSource = new CancellationTokenSource();

        // Observable collection for us to handle changes
        public ObservableCollection<NavBookmark> bookmarks { get; private set; }
        private static readonly object bookmarksLock = new object();

        private NavigationMonitorConfiguration navConfig = new NavigationMonitorConfiguration();

        // Navigation route data
        public string navDestination;
        public string navRouteList;
        public decimal navRouteDistance;
        private DateTime updateDat;
        private bool guidanceEngaged;

        public static event EventHandler BookmarksUpdatedEvent;

        private static readonly object statusLock = new object();
        public Status currentStatus { get; private set; }
        private Status lastStatus { get; set; }

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

            StatusMonitor.StatusUpdatedEvent += (s, e) =>
            {
                if (s is Status status)
                {
                    lock (statusLock)
                    {
                        OnStatusUpdated(status);
                    }
                }
            };
            initializeNavigationMonitor();
        }

        private void OnStatusUpdated(Status status)
        {
            lastStatus = currentStatus;
            currentStatus = status;

            if (lastStatus != null && currentStatus != null)
            {
                if (currentStatus.near_surface && !lastStatus.near_surface)
                {
                    updateDat = status.timestamp;
                    EngageGuidanceSystem(EDDI.Instance.CurrentStarSystem?.systemname, status.bodyname);
                }
                else if (lastStatus.near_surface && !currentStatus.near_surface)
                {
                    updateDat = status.timestamp;
                    DisengageGuidanceSystem();
                }
            }
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
        {
            guidanceCancellationTokenSource.Cancel();
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
            else if(@event is SRVTurretDeployableEvent srvTurretDeployableEvent)
            {
                handleSRVTurretDeployableEvent(srvTurretDeployableEvent);
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
        }

        private void handleBookmarkDetailsEvent(BookmarkDetailsEvent @event) 
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (@event.isset)
                {
                    EngageGuidanceSystem(@event.system, @event.body);
                }
                else
                {
                    DisengageGuidanceSystem();
                }
            }
        }

        private void handleCarrierJumpedEvent(CarrierJumpedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
                UpdateBookmarkSetStatus(@event.systemname);
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
                UpdateBookmarkSetStatus(@event.systemname);
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.system, updateDat);
                UpdateBookmarkSetStatus(@event.system);
            }
        }

        private void handleEnteredNormalSpaceEvent(EnteredNormalSpaceEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                NavigationService.Instance.UpdateSearchDistance(@event.systemname, updateDat);
                if (@event.bodytype == "Station")
                {
                    UpdateBookmarkSetStatus(@event.systemname, @event.bodyname);
                }
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;

                // Get up-to-date configuration data
                navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                List<NavWaypoint> route = @event.route;
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
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;

                if (@event.playercontrolled)
                {
                    navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                    navConfig.tdLat = @event.latitude;
                    navConfig.tdLong = @event.longitude;
                    navConfig.tdPOI = @event.nearestdestination;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void handleLiftoffEvent(LiftoffEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;

                if (@event.playercontrolled)
                {
                    navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                    navConfig.tdLat = null;
                    navConfig.tdLong = null;
                    navConfig.tdPOI = null;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void handleSRVTurretDeployableEvent(SRVTurretDeployableEvent @event) 
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (@event.deployable)
                {
                    EngageGuidanceSystem(EDDI.Instance.CurrentStarSystem?.systemname, EDDI.Instance.CurrentStellarBody.bodyname);
                }
                else
                {
                    DisengageGuidanceSystem();
                }
            }
        }

        private void DisengageGuidanceSystem()
        {
            if (guidanceEngaged)
            {
                guidanceEngaged = false;
                guidanceCancellationTokenSource.Cancel();
                EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, "disengaged", null, null, null, null, null));
            }
        }

        private void EngageGuidanceSystem(string systemname, string bodyname)
        {
            // Find a guidance-eligible bookmark that matches a given systemname and bodyname, if any
            var navBookmark = bookmarks.FirstOrDefault(b =>
                b.isset &&
                string.Equals(b.systemname, systemname, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(b.bodyname, bodyname, StringComparison.InvariantCultureIgnoreCase) &&
                b.latitude != null &&
                b.longitude != null);
            if (navBookmark != null)
            {
                // Activate guidance system when near an eligible bookmark
                EngageGuidanceSystem(navBookmark);
            }
        }

        private void EngageGuidanceSystem(NavBookmark navBookmark)
        {
            guidanceCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => GuidanceSystem(navBookmark), guidanceCancellationTokenSource.Token);
        }

        private void GuidanceSystem(NavBookmark navBookmark)
        {
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            if (navBookmark is null || !navConfig.guidanceSystemEnabled) { return; }

            // Wait as needed until our status also shows us near the surface of the body
            while (currentStatus == null || !currentStatus.near_surface || navBookmark.bodyname != currentStatus.bodyname)
            {
                Thread.Sleep(250);
            }

            while (navConfig.guidanceSystemEnabled && navBookmark.isset && navBookmark.bodyname == currentStatus.bodyname)
            {
                // Guidance system is active and tracking a bookmark.
                if (!guidanceEngaged)
                {
                    guidanceEngaged = true;
                    EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, "engaged", null, null, null, null, null));
                }

                // Determine our distance
                decimal? surfaceDistanceKm = SurfaceDistanceKm(currentStatus, navBookmark?.latitude, navBookmark?.longitude);
                if (surfaceDistanceKm < 3.0M)
                {
                    // We've arrived - guidance system deactivated
                    guidanceEngaged = false;
                    EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, "complete", null, null, null, null, null));
                    return;
                }

                if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE || (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE && currentStatus.gliding))
                {
                    // Determine our heading
                    decimal? heading = SurfaceHeadingDegrees(currentStatus, navBookmark?.latitude, navBookmark?.longitude);
                    decimal? headingError = heading - currentStatus?.heading;

                    // Determine our slope
                    // Our calculated slope from the status object is not guaranteed to be accurate if we're in normal space and not gliding
                    // (since we may be moving in a direction other than where we are pointing) so we disregard slope unless we're gliding.
                    decimal? slope = null;
                    decimal? slopeError = null;
                    if (currentStatus.near_surface && currentStatus?.slope != null && currentStatus.altitude != null && surfaceDistanceKm != null)
                    {
                        var altitudeKm = (double)currentStatus.altitude / 1000;
                        slope = (decimal)Math.Round(Math.Atan2(altitudeKm, (double)surfaceDistanceKm) * 180 / Math.PI, 4) * -1;
                        slopeError = slope - currentStatus.slope;
                    }

                    // We're already navigating to this bookmark, send an update event
                    EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.UtcNow, "update", heading, headingError, slope, slopeError, surfaceDistanceKm));
                    Thread.Sleep(3000);
                }
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<NavBookmark>(bookmarks),
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
                foreach (NavBookmark bookmark in navConfig.bookmarks)
                {
                    bookmarks.Add(bookmark);
                }
            }
        }

        private void UpdateBookmarkSetStatus(string system, string station = null)
        {
            // Update bookmark 'set' status
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            NavBookmark navBookmark = navConfig.bookmarks.FirstOrDefault(b => b.isset);
            if (navBookmark != null && navBookmark.systemname == system)
            {
                if ((navBookmark.bodyname is null && !navBookmark.isstation) || (!string.IsNullOrEmpty(station) && navBookmark.poi == station))
                {
                    navBookmark.isset = false;
                }

                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        public void RemoveBookmarkAt(int index)
        {
            lock (bookmarksLock)
            {
                bookmarks.RemoveAt(index);
            }
        }

        public static void SurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        public static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }

        public static decimal? SurfaceHeadingDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceHeadingDegrees(curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
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
