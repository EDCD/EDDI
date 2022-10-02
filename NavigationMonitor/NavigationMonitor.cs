using EddiCompanionAppService;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiStarMapService;
using EddiStatusService;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiNavigationMonitor
{
    [UsedImplicitly]
    public class NavigationMonitor : EDDIMonitor
    {
        public FleetCarrier FleetCarrier => EDDI.Instance.FleetCarrier;

        #region Collections

        // Observable collection for us to handle changes to Bookmarks
        public ObservableCollection<NavBookmark> Bookmarks = new ObservableCollection<NavBookmark>();
        public static event EventHandler BookmarksUpdatedEvent;

        public ObservableCollection<NavPOIBookmark> GalacticPOIs = new ObservableCollection<NavPOIBookmark>();

        // Navigation route data
        public NavWaypointCollection NavRouteList = new NavWaypointCollection() { FillVisitedGaps = true };
        public static event EventHandler NavRouteUpdatedEvent;

        // Plotted carrier route data
        public NavWaypointCollection CarrierPlottedRoute = new NavWaypointCollection() { FillVisitedGaps = true };
        public static event EventHandler CarrierPlottedRouteUpdatedEvent;

        // Plotted ship route data
        public NavWaypointCollection PlottedRouteList = new NavWaypointCollection();
        public static event EventHandler PlottedRouteUpdatedEvent;

        #endregion

        public static readonly object navConfigLock = new object();

        private DateTime updateDat;
        
        internal Status currentStatus { get; set; }

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
            StatusService.StatusUpdatedEvent += OnStatusUpdated;
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void LoadMonitor()
        {
            ReadNavConfig();

            // Build a Galactic POI list
            GalacticPOIs = EDAstro.GetPOIs();
            GetBookmarkExtras(GalacticPOIs);

            // Retrieve the latest Fleet Carrier data
            try
            {
                RefreshFleetCarrierFromFrontierAPI();
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to obtain Frontier API fleet carrier: " + ex);
            }
        }

        private void NavigationMonitor_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(NavRouteList.Waypoints, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(PlottedRouteList.Waypoints, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(CarrierPlottedRoute.Waypoints, navConfigLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRouteList.Waypoints, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRouteList.Waypoints, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(CarrierPlottedRoute.Waypoints, navConfigLock); });
            }
        }

        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            LoadMonitor();
        }

        public void Stop()
        { }

        public void Reload()
        {
            LoadMonitor();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PreHandle(Event @event)
        {
            if (@event.timestamp >= updateDat)
            {
                Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

                // Handle the events that we care about

                if (@event is CarrierJumpRequestEvent carrierJumpRequestEvent)
                {
                    handleCarrierJumpRequestEvent(carrierJumpRequestEvent);
                }
                else if (@event is CarrierJumpCancelledEvent carrierJumpCancelledEvent)
                {
                    handleCarrierJumpCancelledEvent(carrierJumpCancelledEvent);
                }
                else if (@event is CarrierJumpedEvent carrierJumpedEvent)
                {
                    handleCarrierJumpedEvent(carrierJumpedEvent);
                }
                else if (@event is CarrierJumpEngagedEvent carrierJumpEngagedEvent)
                {
                    handleCarrierJumpEngagedEvent(carrierJumpEngagedEvent);
                }
                else if (@event is CarrierPurchasedEvent carrierPurchasedEvent)
                {
                    handleCarrierPurchasedEvent(carrierPurchasedEvent);
                }
                else if (@event is CarrierStatsEvent carrierStatsEvent)
                {
                    handleCarrierStatsEvent(carrierStatsEvent);
                }
                else if (@event is CommodityPurchasedEvent commodityPurchasedEvent)
                {
                    handleCommodityPurchasedEvent(commodityPurchasedEvent);
                }
                else if (@event is CommoditySoldEvent commoditySoldEvent)
                {
                    handleCommoditySoldEvent(commoditySoldEvent);
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
        }

        public void PostHandle(Event @event)
        {
            if (@event is CarrierJumpRequestEvent
                     || @event is CarrierJumpEngagedEvent
                     || @event is CarrierJumpedEvent
                     || @event is CarrierPurchasedEvent
                     || @event is CarrierStatsEvent)
            {
                RefreshFleetCarrierFromFrontierAPI();
            }
            else if (@event is NavRouteEvent navRouteEvent)
            {
                posthandleNavRouteEvent(navRouteEvent);
            }
            else if (@event is LiftoffEvent liftoffEvent)
            {
                posthandleLiftoffEvent(liftoffEvent);
            }
            else if (@event is TouchdownEvent touchdownEvent)
            {
                posthandleTouchdownEvent(touchdownEvent);
            }
            else if (@event is UndockedEvent undockedEvent)
            {
                posthandleUndockedEvent(undockedEvent);
            }
        }

        #region handledEvents

        private void handleCarrierJumpRequestEvent(CarrierJumpRequestEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier() { carrierID = @event.carrierId };
            updatedCarrier.nextStarSystem = @event.systemname;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierJumpCancelledEvent(CarrierJumpCancelledEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier() { carrierID = @event.carrierId };
            updatedCarrier.nextStarSystem = null;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierJumpedEvent(CarrierJumpedEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier() { carrierID = @event.carrierId, name = @event.carriername };
            updatedCarrier.currentStarSystem = @event.systemname;
            updatedCarrier.Market.name = @event.carriername;
            updatedCarrier.nextStarSystem = null;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            CarrierPlottedRoute.UpdateVisitedStatus(@event.systemAddress);
            updateNavigationData(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierJumpEngagedEvent(CarrierJumpEngagedEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier() { carrierID = @event.carrierId };
            updatedCarrier.currentStarSystem = @event.systemname;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            CarrierPlottedRoute.UpdateVisitedStatus(@event.systemAddress);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierPurchasedEvent(CarrierPurchasedEvent @event)
        {
            EDDI.Instance.FleetCarrier = new FleetCarrier() { carrierID = @event.carrierId, callsign = @event.callsign, currentStarSystem = @event.systemname };
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierStatsEvent(CarrierStatsEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier() { carrierID = @event.carrierId, callsign = @event.callsign, name = @event.name };
            updatedCarrier.name = @event.name;
            updatedCarrier.dockingAccess = @event.dockingAccess;
            updatedCarrier.notoriousAccess = @event.notoriousAccess;
            updatedCarrier.fuel = @event.fuel;
            updatedCarrier.usedCapacity = @event.usedCapacity;
            updatedCarrier.freeCapacity = @event.freeCapacity;
            updatedCarrier.bankBalance = @event.bankBalance;
            updatedCarrier.bankReservedBalance = @event.bankReservedBalance;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            if (!@event.fromLoad && @event.timestamp >= updateDat) 
            {
                updateDat = @event.timestamp;
                WriteNavConfig(); 
            }
        }

        private void handleCommodityPurchasedEvent(CommodityPurchasedEvent @event)
        {
            if (FleetCarrier != null && @event.marketid == FleetCarrier?.carrierID)
            {
                if (@event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium")
                {
                    FleetCarrier.fuelInCargo -= @event.amount;
                    if (!@event.fromLoad && @event.timestamp >= updateDat)
                    {
                        updateDat = @event.timestamp;
                        WriteNavConfig();
                    }
                }
            }
        }

        private void handleCommoditySoldEvent(CommoditySoldEvent @event)
        {
            if (FleetCarrier != null && @event.marketid == FleetCarrier?.carrierID)
            {
                if (@event.commodityDefinition?.edname?.ToLowerInvariant() == "tritium")
                {
                    FleetCarrier.fuelInCargo += @event.amount;
                    if (!@event.fromLoad && @event.timestamp >= updateDat)
                    {
                        updateDat = @event.timestamp;
                        WriteNavConfig();
                    }
                }
            }
        }

        private void handleDockedEvent(DockedEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                lock (navConfigLock)
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    // Check if we're at a planetary location and capture our location if true
                    if ((currentStatus?.near_surface ?? false) &&
                        new Station { Model = @event.stationModel }.IsPlanetary())
                    {

                        navConfig.tdLat = currentStatus.latitude;
                        navConfig.tdLong = currentStatus.longitude;
                        navConfig.tdPOI = @event.station;

                        // If we are at our fleet carrier, make sure that the carrier location is up to date.
                        if (@event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID)
                        {
                            FleetCarrier.currentStarSystem = @event.system;
                            CarrierPlottedRoute.UpdateVisitedStatus(@event.systemAddress);
                            navConfig.fleetCarrier = FleetCarrier;
                            navConfig.carrierPlottedRoute = CarrierPlottedRoute;
                        }
                    }

                    navConfig.updatedat = updateDat;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                // If we are at our fleet carrier, make sure that the carrier location is up to date.
                lock (navConfigLock)
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    if (@event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID)
                    {
                        FleetCarrier.currentStarSystem = @event.systemname;
                        CarrierPlottedRoute.UpdateVisitedStatus(@event.systemAddress);
                        navConfig.fleetCarrier = FleetCarrier;
                        navConfig.carrierPlottedRoute = CarrierPlottedRoute;
                    }
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleEnteredNormalSpaceEvent(EnteredNormalSpaceEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleNavRouteEvent(NavRouteEvent @event)
        {
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null)
                {
                    if (routeList.Count > 1 && routeList[0].systemName == EDDI.Instance?.CurrentStarSystem?.systemname)
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
                    updateDat = @event.timestamp;
                    WriteNavConfig();
                }
            }
        }

        private void posthandleNavRouteEvent(NavRouteEvent @event)
        {
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null)
                {
                    if (routeList.Count == 0)
                    {
                        UpdateDestinationData(null, 0);
                        NavRouteList.Waypoints.Clear();
                    }

                    // Raise on UI thread
                    RaiseOnUIThread(NavRouteUpdatedEvent, NavRouteList);

                    // Update the navigation configuration 
                    updateDat = @event.timestamp;
                    WriteNavConfig();
                }
            }
        }

        private void posthandleTouchdownEvent(TouchdownEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                lock (navConfigLock)
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    navConfig.tdLat = @event.latitude;
                    navConfig.tdLong = @event.longitude;
                    navConfig.tdPOI = @event.nearestdestination;
                    navConfig.updatedat = updateDat;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void posthandleLiftoffEvent(LiftoffEvent @event)
        {
            updateNavigationData(@event.timestamp, @event.systemAddress);
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                lock (navConfigLock)
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    navConfig.tdLat = null;
                    navConfig.tdLong = null;
                    navConfig.tdPOI = null;
                    navConfig.updatedat = updateDat;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void posthandleUndockedEvent(UndockedEvent @event)
        {
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                lock (navConfigLock)
                {
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    navConfig.tdLat = null;
                    navConfig.tdLong = null;
                    navConfig.tdPOI = null;
                    navConfig.updatedat = updateDat;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
        }

        private void handleRouteDetailsEvent(RouteDetailsEvent routeDetailsEvent)
        {

            if (routeDetailsEvent.routetype == QueryType.carrier.ToString())
            {
                if (routeDetailsEvent.Route?.Waypoints.GetHashCode() == CarrierPlottedRoute.Waypoints.GetHashCode())
                {
                    // Displayed route is correct, nothing to do here
                }
                else if (routeDetailsEvent.Route != null)
                {
                    CarrierPlottedRoute.Waypoints.Clear();
                    Thread.Sleep(5); // A small delay helps ensure that any straggling entries are removed from the UI DataGrid
                    CarrierPlottedRoute.AddRange(routeDetailsEvent.Route.Waypoints);
                    CarrierPlottedRoute.FillVisitedGaps = routeDetailsEvent.Route.FillVisitedGaps;
                }
                else
                {
                    CarrierPlottedRoute.Waypoints.Clear();
                }

                // Raise on UI thread
                RaiseOnUIThread(CarrierPlottedRouteUpdatedEvent, CarrierPlottedRoute);
            }
            else
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
                        Thread.Sleep(5); // A small delay helps ensure that any straggling entries are removed from the UI DataGrid
                        PlottedRouteList.AddRange(routeDetailsEvent.Route.Waypoints);
                        PlottedRouteList.FillVisitedGaps = routeDetailsEvent.Route.FillVisitedGaps;
                        PlottedRouteList.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions
                            ?.ToList());
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
            }

            // Update the navigation configuration 
            if (!routeDetailsEvent.fromLoad && routeDetailsEvent.timestamp >= updateDat)
            {
                updateDat = routeDetailsEvent.timestamp;
                WriteNavConfig();
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
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        #endregion

        public IDictionary<string, object> GetVariables()
        {
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<NavBookmark>(Bookmarks),
                ["navRouteList"] = NavRouteList,
                ["orbitalpriority"] = navConfig.prioritizeOrbitalStations,
            };
            return variables;
        }

        public void WriteNavConfig()
        {
            lock (navConfigLock)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Bookmarks
                navConfig.bookmarks = Bookmarks;

                // In-game routing
                navConfig.navRouteList = NavRouteList;

                // Plotted Routes
                navConfig.plottedRouteList = PlottedRouteList;
                navConfig.carrierPlottedRoute = CarrierPlottedRoute;

                // Fleet Carrier
                navConfig.fleetCarrier = FleetCarrier;

                // Misc
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

                // Restore our bookmarks
                Bookmarks = navConfig.bookmarks;
                GetBookmarkExtras(Bookmarks);

                // Restore our in-game routing
                NavRouteList = navConfig.navRouteList;

                // Restore our plotted routes
                PlottedRouteList = navConfig.plottedRouteList;
                CarrierPlottedRoute = navConfig.carrierPlottedRoute;

                // Restore our Fleet Carrier data
                EDDI.Instance.FleetCarrier = navConfig.fleetCarrier;

                // Misc
                updateDat = navConfig.updatedat;
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

        private void updateNavigationData(DateTime timestamp, ulong? systemAddress, decimal? x = null, decimal? y = null,
    decimal? z = null)
        {
            if (systemAddress is null) { return; }

            // Distances Data
            if (x != null && y != null && z != null)
            {
                foreach (var navBookmark in Bookmarks.AsParallel())
                {
                    navBookmark.distanceLy =
                        Functions.StellarDistanceLy(x, y, z, navBookmark.x, navBookmark.y, navBookmark.z);
                    if (navBookmark.systemAddress == systemAddress)
                    {
                        navBookmark.visitLog.Add(timestamp);
                    }
                }
                foreach (var poiBookmark in GalacticPOIs.AsParallel())
                {
                    poiBookmark.distanceLy =
                        Functions.StellarDistanceLy(x, y, z, poiBookmark.x, poiBookmark.y, poiBookmark.z);
                    if (poiBookmark.systemAddress == systemAddress)
                    {
                        poiBookmark.visitLog.Add(timestamp);
                    }
                }
                Application.Current.Dispatcher?.Invoke(() =>
                {
                    var configWindow = ConfigurationTabItem();
                    if (configWindow.TryFindResource(nameof(GalacticPOIControl.POIView)) is ICollectionView poiView)
                    {
                        poiView.Refresh();
                    }
                });

                NavigationService.Instance.SearchDistanceLy = Functions.StellarDistanceLy(x, y, z,
                    NavigationService.Instance.SearchStarSystem?.x, NavigationService.Instance.SearchStarSystem?.y,
                    NavigationService.Instance.SearchStarSystem?.z) ?? 0;
            }

            // Visited Data
            NavRouteList.UpdateVisitedStatus((ulong)systemAddress);
            PlottedRouteList.UpdateVisitedStatus((ulong)systemAddress);
            if (PlottedRouteList.GuidanceEnabled && PlottedRouteList.Waypoints.All(w => w.visited))
            {
                // Deactivate guidance once we've reached our destination.
                NavigationService.Instance.NavQuery(QueryType.cancel);
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
            //// Not required for observable collections
            //if (handler != null)
            //{
            //    SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            //    if (uiSyncContext == null)
            //    {
            //        handler(sender, EventArgs.Empty);
            //    }
            //    else
            //    {
            //        uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
            //    }
            //}
        }

        private async void GetBookmarkExtras<T>(ObservableCollection<T> bookmarks) where T : NavBookmark
        {
            // Retrieve extra details to supplement our bookmarks

            var getSystems = Task.Run( () =>
            {
                var bookmarkSystems = bookmarks.Select(n => new StarSystem()
                {
                    systemname = n.systemname, 
                    systemAddress = n.systemAddress
                }).ToList();
                var dataProviderService = new DataProviderService(new StarMapService());
                return dataProviderService.syncFromStarMapService(bookmarkSystems);
            });
            await Task.WhenAll(getSystems);
            foreach (var system in getSystems.Result)
            {
                var poi = bookmarks.FirstOrDefault(s =>
                    s.systemname == system.systemname);
                if (poi != null)
                {
                    poi.systemAddress = system.systemAddress;
                    poi.visitLog = system.visitLog;
                }
            }
        }

        /// <summary>Obtain fleet carrier information from the companion API and use it to refresh our own data</summary>
        private void RefreshFleetCarrierFromFrontierAPI()
        {
            if (CompanionAppService.Instance?.CurrentState == CompanionAppService.State.Authorized)
            {
                try
                {
                    var frontierApiCarrier = CompanionAppService.Instance.FleetCarrier();
                    if (frontierApiCarrier != null)
                    {
                        // Update our Fleet Carrier object
                        var updatedCarrier = FleetCarrier.FromFrontierApiFleetCarrier(FleetCarrier, frontierApiCarrier, frontierApiCarrier.timestamp, updateDat, out bool carrierMatches);

                        // Stop if the carrier returned from the profile does not match our expected carrier id
                        if (!carrierMatches) { return; }

                        EDDI.Instance.FleetCarrier = updatedCarrier ?? FleetCarrier;

                        updateDat = frontierApiCarrier.timestamp > updateDat ? frontierApiCarrier.timestamp : updateDat;
                        WriteNavConfig();
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Exception obtaining fleet carrier Frontier API data", ex);
                    return;
                }
            }
        }
        }
    }
}
