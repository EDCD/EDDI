﻿using Eddi;
using EddiCore;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Utilities;

namespace EddiNavigationMonitor
{
    public class NavigationMonitor : EDDIMonitor
    {
        // Engage guidance system
        private bool running;

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
            return true;
        }

        public void Start()
        {
            _start();
        }

        public void Stop()
        {
            running = false;
        }

        public void Reload()
        {
            readBookmarks();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public void _start()
        {
            running = true;

            while (running)
            {
                navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                if (navConfig.guidanceSystem)
                {
                    decimal distance = 0;
                    decimal heading = 0;
                    decimal headingError = 0;
                    decimal slope = 0;
                    decimal slopeError = 0;

                    Bookmark bookmark = navConfig.bookmarks.FirstOrDefault(b => b.isset);
                    if (bookmark?.isset ?? false)
                    {
                        StarSystem currentSystem = EDDI.Instance.CurrentStarSystem;
                        if (currentSystem?.systemname == bookmark.system)
                        {
                            
                            Status currentStatus = NavigationService.Instance.currentStatus;

                            // Activate guidance system when near surface of the bookmark body
                            if (currentStatus?.near_surface ?? false)
                            {
                                Body currentBody = EDDI.Instance.CurrentStellarBody;
                                if (currentBody != null && currentBody.shortname == bookmark.body)
                                {
                                    heading = CalculateHeading(currentStatus, bookmark.latitude, bookmark.longitude);
                                    headingError = heading - (decimal)currentStatus.heading;
                                    distance = CalculateDistance(currentStatus, bookmark.latitude, bookmark.longitude);

                                    if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                                    {
                                        if (currentStatus?.slope != null)
                                        {
                                            slope = (decimal)Math.Round(Math.Atan2((double)currentStatus.altitude, (double)distance) * 180 / Math.PI, 4);
                                            slopeError = slope - (decimal)currentStatus.slope;
                                        }
                                    }

                                    // Guidance system inactive when destination reached
                                    if (distance < 0.5M)
                                    {
                                        bookmark.isset = false;
                                        EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.Now, "complete", null, null, null, null, null));
                                    }
                                    else
                                    {
                                        EDDI.Instance.enqueueEvent(new GuidanceSystemEvent(DateTime.Now, "update", heading, headingError, slope, slopeError, distance));
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(3000);
            }
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
            if (@event is TouchdownEvent)
            {
                handleTouchdownEvent((TouchdownEvent)@event);
            }
            if (@event is LiftoffEvent)
            {
                handleLiftoffEvent((LiftoffEvent)@event);
            }
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
            else if (@event is EnteredNormalSpaceEvent)
            {
                handleEnteredNormalSpaceEvent((EnteredNormalSpaceEvent)@event);
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

        private void UpdateBookmarkSetStatus (string system, string station = null)
        {
            // Update bookmark 'set' status
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            Bookmark bookmark = navConfig.bookmarks.FirstOrDefault(b => b.isset);
            if (bookmark?.system == system)
            {
                if (bookmark.body is null && !bookmark.isstation || !string.IsNullOrEmpty(station) && bookmark.poi == station)
                {
                    bookmark.isset = false;
                }

                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;

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

        public void CalculateCoordinates(Status curr, ref decimal? latitude, ref decimal? longitude)
        {
            if (curr?.slope != null)
            {
                // Convert latitude, longitude & slope to radians
                double currLat = (double)curr.latitude * Math.PI / 180;
                double currLong = (double)curr.longitude * Math.PI / 180;
                double slope = -(double)curr.slope * Math.PI / 180;
                double altitude = (double)curr.altitude / 1000;

                // Determine minimum slope
                double radius = (double)curr.planetradius / 1000;
                double minSlope = Math.Acos(radius / (altitude + radius));
                if (slope > minSlope)
                {
                    // Calculate the orbital cruise 'point to' position using Laws of Sines & Haversines 
                    double a = Math.PI / 2 - slope;
                    double path = altitude / Math.Cos(a);
                    double c = Math.Asin(path * Math.Sin(a) / radius);
                    double heading = (double)curr.heading * Math.PI / 180;
                    double Lat = Math.Asin(Math.Sin(currLat) * Math.Cos(c) + Math.Cos(currLat) * Math.Sin(c) * Math.Cos(heading));
                    double Lon = currLong + Math.Atan2(Math.Sin(heading) * Math.Sin(c) * Math.Cos(Lat),
                        Math.Cos(c) - Math.Sin(currLat) * Math.Sin(Lat));

                    // Convert position to degrees
                    latitude = (decimal)Math.Round(Lat * 180 / Math.PI, 4);
                    longitude = (decimal)Math.Round(Lon * 180 / Math.PI, 4);
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

        public decimal CalculateDistance(Status curr, decimal? latitude = null, decimal? longitude = null)
        {
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            double distance = 0;
            
            if (curr?.altitude != null && curr?.latitude != null && curr?.longitude != null)
            {
                if (latitude == null || longitude == null)
                {
                    latitude = navConfig?.tdLat;
                    longitude = navConfig?.tdLong;
                }

                if (latitude != null && longitude != null)
                {
                    double square(double x) => x * x;
                    double radius = (double)curr.planetradius / 1000;

                    // Convert latitude & longitude to radians
                    double lat1 = (double)curr.latitude * Math.PI / 180;
                    double lat2 = (double)latitude * Math.PI / 180;
                    double deltaLat = lat2 - lat1;
                    double deltaLong = (double)(longitude - curr.longitude) * Math.PI / 180;

                    // Calculate distance traveled using Law of Haversines
                    double a = square(Math.Sin(deltaLat / 2)) + Math.Cos(lat2) * Math.Cos(lat1) * square(Math.Sin(deltaLong / 2));
                    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    distance = c * radius;
                }
            }
            return (decimal)distance;
        }

        public decimal CalculateHeading(Status curr, decimal? latitude, decimal? longitude)
        {
            double heading = 0;

            if (curr?.altitude != null && curr?.latitude != null && curr?.longitude != null)
            {
                if (latitude != null && longitude != null)
                {
                    // Convert latitude & longitude to radians
                    double lat1 = (double)curr.latitude * Math.PI / 180;
                    double lat2 = (double)latitude * Math.PI / 180;
                    double deltaLong = (double)(longitude - curr.longitude) * Math.PI / 180;

                    // Calculate heading using Law of Haversines
                    double x = Math.Sin(deltaLong) * Math.Cos(lat2);
                    double y = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLong);
                    heading = Math.Atan2(y, x) * 180 / Math.PI;
                }
            }
            return (decimal)heading;
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
