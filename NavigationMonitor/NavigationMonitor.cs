using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiStarMapService;
using EddiStatusService;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    [UsedImplicitly]
    public class NavigationMonitor : IEddiMonitor
    {
        public FleetCarrier FleetCarrier => EDDI.Instance.FleetCarrier;

        #region Collections

        // Observable collection for us to handle changes to Bookmarks
        public ObservableCollection<NavBookmark> Bookmarks = new ObservableCollection<NavBookmark>();

        public ObservableCollection<NavBookmark> GalacticPOIs = new ObservableCollection<NavBookmark>();

        // Navigation route data
        public NavWaypointCollection NavRoute = new NavWaypointCollection() { FillVisitedGaps = true };

        // Plotted carrier route data
        public NavWaypointCollection CarrierPlottedRoute = new NavWaypointCollection() { FillVisitedGaps = true };

        // Plotted ship route data
        public NavWaypointCollection PlottedRoute = new NavWaypointCollection();

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
            LoadMonitor();
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void LoadMonitor()
        {
            ReadNavConfig();
            GetGalacticPOIs();
        }

        private void GetGalacticPOIs()
        {
            // Build a Galactic POI list
            GalacticPOIs = EDAstro.GetPOIs();
            GetBookmarkExtras(GalacticPOIs);
        }

        private void NavigationMonitor_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(GalacticPOIs, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(NavRoute.Waypoints, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(PlottedRoute.Waypoints, navConfigLock);
                BindingOperations.EnableCollectionSynchronization(CarrierPlottedRoute.Waypoints, navConfigLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(Bookmarks, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(GalacticPOIs, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(NavRoute.Waypoints, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(PlottedRoute.Waypoints, navConfigLock); });
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(CarrierPlottedRoute.Waypoints, navConfigLock); });
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
            LoadMonitor();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return ConfigurationWindow.Instance;
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PreHandle(Event @event)
        {
            if (@event.timestamp >= updateDat)
            {
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
                || @event is CarrierStatsEvent
                || @event is CommanderContinuedEvent)
            {
                if (!@event.fromLoad)
                {
                    EDDI.Instance.RefreshFleetCarrierFromFrontierAPI();
                }
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
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier(@event.carrierId);
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
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier(@event.carrierId);
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
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier(@event.carrierId) { name = @event.carriername };
            updatedCarrier.currentStarSystem = @event.systemname;
            updatedCarrier.Market.name = @event.carriername;
            updatedCarrier.nextStarSystem = null;
            EDDI.Instance.FleetCarrier = updatedCarrier;

            UpdateStellarLocationData(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad);
        }

        private void handleCarrierJumpEngagedEvent(CarrierJumpEngagedEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier(@event.carrierId);
            updatedCarrier.currentStarSystem = @event.systemname;
            EDDI.Instance.FleetCarrier = updatedCarrier;
            UpdateCarrierRouteLocationData(@event.timestamp, @event.systemname, @event.systemAddress, @event.fromLoad);
        }

        private void UpdateCarrierRouteLocationData(DateTime timestamp, string systemName, ulong systemAddress, bool fromLoad)
        {
            var system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName, true, false, false, false, false);
            if (systemAddress == system.systemAddress)
            {
                CarrierPlottedRoute.UpdateLocationData(system.systemAddress, system.x, system.y, system.z);
                if (!fromLoad && timestamp >= updateDat)
                {
                    updateDat = timestamp;
                    WriteNavConfig();
                }
            }
        }

        private void handleCarrierPurchasedEvent(CarrierPurchasedEvent @event)
        {
            EDDI.Instance.FleetCarrier = new FleetCarrier(@event.carrierId) { callsign = @event.callsign, currentStarSystem = @event.systemname };
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                updateDat = @event.timestamp;
                WriteNavConfig();
            }
        }

        private void handleCarrierStatsEvent(CarrierStatsEvent @event)
        {
            var updatedCarrier = FleetCarrier?.Copy() ?? new FleetCarrier(@event.carrierID) { callsign = @event.callsign, name = @event.name };
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
            if ( !@event.fromLoad && @event.timestamp >= updateDat )
            {
                // Check if we're at a planetary location and capture our location if true
                if ( ( currentStatus?.near_surface ?? false ) &&
                     new Station { Model = @event.stationModel }.IsPlanetary() )
                {
                    lock ( navConfigLock )
                    {
                        var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                        navConfig.tdLat = currentStatus.latitude;
                        navConfig.tdLong = currentStatus.longitude;
                        navConfig.tdPOI = @event.station;
                        navConfig.updatedat = updateDat;
                        ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    }

                    // If we are at our fleet carrier, make sure that the carrier location is up to date.
                    if ( @event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID )
                    {
                        FleetCarrier.currentStarSystem = @event.system;
                        UpdateCarrierRouteLocationData( @event.timestamp, @event.system, @event.systemAddress, @event.fromLoad );
                    }
                }
            }
        }

        private void handleLocationEvent(LocationEvent @event)
        {
            UpdateStellarLocationData( @event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad );

            if ( !@event.fromLoad && @event.timestamp >= updateDat)
            {
                // If we are at our fleet carrier, make sure that the carrier location is up to date.
                if ( @event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID )
                {
                    FleetCarrier.currentStarSystem = @event.systemname;
                    UpdateCarrierRouteLocationData( @event.timestamp, @event.systemname, @event.systemAddress, @event.fromLoad );
                }
            }
        }

        private void handleJumpedEvent(JumpedEvent @event)
        {
            UpdateStellarLocationData(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad );
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
                        // Update the Nav Route
                        routeList[0].visited = true;
                        NavRoute.Waypoints.Clear();
                        NavRoute.AddRange(routeList);
                        NavRoute.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions?.ToList());

                        // Update destination data
                        var start = routeList.FirstOrDefault();
                        var end = routeList.LastOrDefault();
                        UpdateDestinationData( start, end );
                    }

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
                        UpdateDestinationData(null, null);
                        NavRoute.Waypoints.Clear();
                    }
                    
                    // Update the navigation configuration 
                    updateDat = @event.timestamp;
                    WriteNavConfig();
                }
            }
        }

        private void posthandleTouchdownEvent(TouchdownEvent @event)
        {
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
            }
            else
            {
                if (routeDetailsEvent.Route?.Waypoints.GetHashCode() == PlottedRoute.Waypoints.GetHashCode())
                {
                    // Displayed route is correct, nothing to do here
                }
                else if (routeDetailsEvent.Route != null)
                {
                    PlottedRoute.Waypoints.Clear();
                    Thread.Sleep(5); // A small delay helps ensure that any straggling entries are removed from the UI DataGrid
                    PlottedRoute.AddRange(routeDetailsEvent.Route.Waypoints);
                    PlottedRoute.FillVisitedGaps = routeDetailsEvent.Route.FillVisitedGaps;
                    PlottedRoute.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions
                        ?.ToList());
                }
                else
                {
                    PlottedRoute.Waypoints.Clear();
                }

                if (routeDetailsEvent.routetype == QueryType.set.ToString())
                {
                    PlottedRoute.GuidanceEnabled = true;
                }
                else if (routeDetailsEvent.routetype == QueryType.cancel.ToString())
                {
                    PlottedRoute.GuidanceEnabled = false;
                }
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
            var wp = PlottedRoute.Waypoints.FirstOrDefault(w => w.systemAddress == (ulong?)@event.systemAddress);
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

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( navConfigLock )
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                return new Dictionary<string, Tuple<Type, object>>
                {
                    // Bookmark info
                    ["bookmarks"] = new Tuple<Type, object>(typeof(List<NavBookmark>), Bookmarks.ToList() ),
                    ["galacticPOIs"] = new Tuple<Type, object>(typeof(NavBookmark), GalacticPOIs ),

                    // Route plotting info
                    ["navRoute"] = new Tuple<Type, object>(typeof(NavWaypointCollection), NavRoute ),
                    ["carrierPlottedRoute"] = new Tuple<Type, object>(typeof(NavWaypointCollection), CarrierPlottedRoute ),
                    ["shipPlottedRoute"] = new Tuple<Type, object>(typeof(NavWaypointCollection), PlottedRoute ),

                    // NavConfig info
                    ["orbitalpriority"] = new Tuple<Type, object>(typeof(bool), navConfig.prioritizeOrbitalStations ),
                    ["maxStationDistance"] = new Tuple<Type, object>(typeof(int?), navConfig.maxSearchDistanceFromStarLs )
                };                
            }
        }

        public void WriteNavConfig()
        {
            lock (navConfigLock)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Bookmarks
                navConfig.bookmarks = Bookmarks;

                // In-game routing
                navConfig.navRouteList = NavRoute;

                // Plotted Routes
                navConfig.plottedRouteList = PlottedRoute;
                navConfig.carrierPlottedRoute = CarrierPlottedRoute;

                // Misc
                navConfig.updatedat = updateDat;

                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void ReadNavConfig()
        {
            lock (navConfigLock)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Restore our bookmarks
                Bookmarks = navConfig.bookmarks ?? new ObservableCollection<NavBookmark>();
                GetBookmarkExtras(Bookmarks);

                // Restore our in-game routing
                NavRoute = navConfig.navRouteList ?? new NavWaypointCollection(true);

                // Restore our plotted routes
                PlottedRoute = navConfig.plottedRouteList ?? new NavWaypointCollection();
                CarrierPlottedRoute = navConfig.carrierPlottedRoute ?? new NavWaypointCollection(true);

                // Misc
                updateDat = navConfig.updatedat;
            }
        }

        public void RemoveBookmarkAt(int index)
        {
            lock (navConfigLock)
            {
                Bookmarks.RemoveAt(index);
            }
        }

        private void UpdateStellarLocationData(DateTime timestamp, ulong? systemAddress, decimal? x, decimal? y, decimal? z, bool fromLoad = false)
        {
            if (systemAddress is null || x == null || y == null || z == null ) { return; }
            
            // Route Data
            NavRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            PlottedRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            CarrierPlottedRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            if ( PlottedRoute.GuidanceEnabled && PlottedRoute.Waypoints.All( w => w.visited ) )
            {
                // Deactivate guidance once we've reached our destination.
                NavigationService.Instance.NavQuery( QueryType.cancel, null, null, null, null, true );
            }

            // Bookmarks data
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
            // We need to refresh a collection view for galactic POIs
            Application.Current.Dispatcher.Invoke( () =>
            {
                if ( ConfigurationWindow.Instance.TryFindResource( nameof( GalacticPOIControl.POIView ) ) is ICollectionView poiView )
                {
                    poiView.Refresh();
                }
            } );

            // Search Data
            NavigationService.Instance.SearchDistanceLy = Functions.StellarDistanceLy(x, y, z,
                NavigationService.Instance.SearchStarSystem?.x, NavigationService.Instance.SearchStarSystem?.y,
                NavigationService.Instance.SearchStarSystem?.z) ?? 0;

            // Save to Config
            if ( !fromLoad && timestamp >= updateDat )
            {
                updateDat = timestamp;
                WriteNavConfig();
            }
        }

        private void UpdateDestinationData(NavWaypoint routeStart, NavWaypoint routeDestination)
        {
            if ( routeDestination is null )
            {
                EDDI.Instance.updateDestinationSystem( null );
                EDDI.Instance.DestinationDistanceLy = 0;
                return;
            }

            EDDI.Instance.updateDestinationSystem( routeDestination.systemName );
            var distance = Functions.StellarDistanceLy(
                routeStart?.x, routeStart?.y, routeStart?.z, 
                routeDestination.x, routeDestination.y, routeDestination.z) ?? 0;
            EDDI.Instance.DestinationDistanceLy = distance;

            NavRoute.UpdateDestinationData( routeDestination.x, routeDestination.y, routeDestination.z );
            PlottedRoute.UpdateDestinationData( routeDestination.x, routeDestination.y, routeDestination.z );
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
                    else if (bookmark.nearby && trueDistanceMeters >= (bookmark.arrivalRadiusMeters * 1.1M))
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
            var radiusMeters = curr.planetradius ?? (EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000);
            return Functions.SurfaceConstantHeadingDegrees(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceConstantHeadingDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? (EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000);
            return Functions.SurfaceConstantHeadingDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceShortestPathDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceHeadingDegrees(curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceShortestPathDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? (EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000);
            return Functions.SurfaceDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private async void GetBookmarkExtras<T>(ObservableCollection<T> bookmarks) where T : NavBookmark
        {
            // Retrieve extra details to supplement our bookmarks

            var bookmarkSystems = bookmarks.Select(n => new StarSystem()
            {
                systemname = n.systemname,
                systemAddress = n.systemAddress
            }).ToList();
            var visitedBookmarkSystems = await Task.Run(() =>
            {
                return new DataProviderService(new StarMapService(null, true))
                    .syncFromStarMapService(bookmarkSystems)
                    .Where(s => s.visits > 0);
            }).ConfigureAwait(false);
            foreach (var system in visitedBookmarkSystems)
            {
                var poi = bookmarks.FirstOrDefault(s => s.systemAddress == system.systemAddress) ??
                          bookmarks.FirstOrDefault(s => s.systemname == system.systemname);
                if (poi != null)
                {
                    poi.systemAddress = system.systemAddress;
                    poi.visitLog = system.visitLog;
                    bookmarks.Remove(poi);
                    bookmarks.Add(poi);
                }
            }
        }
    }
}
