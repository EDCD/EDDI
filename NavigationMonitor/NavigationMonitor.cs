using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiNavigationService;
using JetBrains.Annotations;
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
    [UsedImplicitly]
    public class NavigationMonitor : IEddiMonitor
    {
        public FleetCarrier FleetCarrier => EDDI.Instance.FleetCarrier;

        #region Collections

        // Observable collection for us to handle changes to Bookmarks
        public ObservableCollection<NavBookmark> Bookmarks = [ ];

        public readonly ObservableCollection<NavBookmark> GalacticPOIs = [ ];

        // Navigation route data
        public NavWaypointCollection NavRoute = new() { FillVisitedGaps = true };

        // Plotted carrier route data
        public NavWaypointCollection CarrierPlottedRoute = new() { FillVisitedGaps = true };

        // Plotted ship route data
        public NavWaypointCollection PlottedRoute = new();

        #endregion

        public static readonly object navConfigLock = new();

        private DateTime updateDat;

        internal Status currentStatus { get; private set; }

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
            LoadMonitor();
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void LoadMonitor()
        {
            ReadNavConfig();
            Task.Run( async () =>
            {
                await GetBookmarkExtrasAsync( Bookmarks ).ConfigureAwait(false);
                await GetGalacticPOIsAsync().ConfigureAwait(false);
            } );
        }

        private async Task GetGalacticPOIsAsync()
        {
            // Build a Galactic POI list
            foreach (var navBookmark in await EDAstro.GetPOIsAsync().ConfigureAwait(false) )
            {
                GalacticPOIs.Add( navBookmark );
            }
            await GetBookmarkExtrasAsync( GalacticPOIs ).ConfigureAwait(false);
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

        public Task HandleProfileAsync(JObject profile)
        {
            return Task.CompletedTask;
        }

        public async Task PreHandleAsync(Event @event)
        {
            // Handle the events that we care about
            if (@event is CarrierJumpedEvent carrierJumpedEvent)
            {
                handleCarrierJumpedEvent(carrierJumpedEvent);
            }
            else if (@event is CarrierJumpEngagedEvent carrierJumpEngagedEvent)
            {
                handleCarrierJumpEngagedEvent(carrierJumpEngagedEvent);
            }
            else if (@event is DockedEvent dockedEvent)
            {
                handleDockedEvent( dockedEvent );
            }
            else if (@event is JumpedEvent jumpedEvent)
            {
                handleJumpedEvent(jumpedEvent);
            }
            else if (@event is LocationEvent locationEvent)
            {
                await handleLocationEventAsync( locationEvent ).ConfigureAwait( false );
            }
            else if (@event is NavRouteEvent navRouteEvent)
            {
                await handleNavRouteEventAsync(navRouteEvent).ConfigureAwait( false );
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

        public Task PostHandleAsync ( Event @event )
        {
            if (@event is NavRouteEvent navRouteEvent)
            {
                posthandleNavRouteEventAsync( navRouteEvent ).SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
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

            return Task.CompletedTask;
        }

        #region handledEvents

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            UpdateStellarLocationDataAsync(@event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad)
                .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
        }

        private void handleCarrierJumpEngagedEvent(CarrierJumpEngagedEvent @event)
        {
            UpdateCarrierRouteLocationDataAsync(@event.timestamp, @event.systemname, @event.systemAddress, @event.fromLoad)
                .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
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
                        UpdateCarrierRouteLocationDataAsync( @event.timestamp, @event.system, @event.systemAddress, @event.fromLoad )
                            .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
                    }
                }
            }
        }

        private async Task handleLocationEventAsync(LocationEvent @event)
        {
            await UpdateStellarLocationDataAsync( @event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad ).ConfigureAwait(false);

            if ( !@event.fromLoad && @event.timestamp >= updateDat)
            {
                // If we are at our fleet carrier, make sure that the carrier location is up to date.
                if ( @event.marketId != null && FleetCarrier != null && @event.marketId == FleetCarrier.carrierID )
                {
                    await UpdateCarrierRouteLocationDataAsync( @event.timestamp, @event.systemname, @event.systemAddress, @event.fromLoad ).ConfigureAwait(false);
                }
            }
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            UpdateStellarLocationDataAsync( @event.timestamp, @event.systemAddress, @event.x, @event.y, @event.z, @event.fromLoad )
                .SafeFireAndForget( ex => Logging.Error( ex.Message, ex ) );
        }

        private async Task handleNavRouteEventAsync(NavRouteEvent @event)
        {
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null)
                {
                    if (routeList.Count > 1 && routeList[0].systemAddress == EDDI.Instance.CurrentStarSystem?.systemAddress)
                    {
                        // Update the Nav Route
                        routeList[0].visited = true;
                        NavRoute.Waypoints.Clear();
                        NavRoute.AddRange(routeList);
                        NavRoute.PopulateMissionIds(ConfigService.Instance.missionMonitorConfiguration.missions?.ToList());

                        // Update destination data
                        var start = routeList.FirstOrDefault();
                        var end = routeList.LastOrDefault();
                        await UpdateDestinationDataAsync( start, end ).ConfigureAwait(false);
                    }

                    // Update the navigation configuration 
                    updateDat = @event.timestamp;
                    WriteNavConfig();
                }
            }
        }

        private async Task posthandleNavRouteEventAsync(NavRouteEvent @event)
        {
            if (!@event.fromLoad && @event.timestamp >= updateDat)
            {
                var routeList = @event.route?.Select(r => new NavWaypoint(r)).ToList();
                if (routeList != null)
                {
                    if (routeList.Count == 0)
                    {
                        await UpdateDestinationDataAsync(null, null).ConfigureAwait(false);
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
            if (routeDetailsEvent.routetype == nameof(QueryType.carrier))
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

                if (routeDetailsEvent.routetype == nameof(QueryType.set))
                {
                    PlottedRoute.GuidanceEnabled = true;
                }
                else if (routeDetailsEvent.routetype == nameof(QueryType.cancel))
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

        private async Task UpdateCarrierRouteLocationDataAsync ( DateTime timestamp, string systemName, ulong systemAddress, bool fromLoad )
        {
            var system = await EDDI.Instance.DataProvider.GetOrFetchSystemWaypointAsync( systemName ).ConfigureAwait(false);
            if ( systemAddress == system?.systemAddress )
            {
                CarrierPlottedRoute.UpdateLocationData( system.systemAddress, system.x, system.y, system.z );
                if ( !fromLoad && timestamp >= updateDat )
                {
                    updateDat = timestamp;
                    WriteNavConfig();
                }
            }
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            lock ( navConfigLock )
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                return new Dictionary<string, Tuple<Type, object>>
                {
                    // Bookmark info
                    ["bookmarks"] = new(typeof(List<NavBookmark>), Bookmarks.ToList() ),
                    ["galacticPOIs"] = new(typeof(NavBookmark), GalacticPOIs ),

                    // Route plotting info
                    ["navRoute"] = new(typeof(NavWaypointCollection), NavRoute ),
                    ["carrierPlottedRoute"] = new(typeof(NavWaypointCollection), CarrierPlottedRoute ),
                    ["shipPlottedRoute"] = new(typeof(NavWaypointCollection), PlottedRoute ),

                    // NavConfig info
                    ["orbitalpriority"] = new(typeof(bool), navConfig.prioritizeOrbitalStations ),
                    ["maxStationDistance"] = new(typeof(int?), navConfig.maxSearchDistanceFromStarLs )
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
                Bookmarks = navConfig.bookmarks ?? [ ];

                // Restore our in-game routing
                NavRoute = navConfig.navRouteList ?? new NavWaypointCollection(null, true);

                // Restore our plotted routes
                CarrierPlottedRoute = navConfig.carrierPlottedRoute ?? new NavWaypointCollection(null, true);
                PlottedRoute = navConfig.plottedRouteList ?? new NavWaypointCollection();

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

        private async Task UpdateStellarLocationDataAsync(DateTime timestamp, ulong? systemAddress, decimal? x, decimal? y, decimal? z, bool fromLoad = false)
        {
            if (systemAddress is null || x == null || y == null || z == null ) { return; }
            
            // Route Data
            NavRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            PlottedRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            CarrierPlottedRoute.UpdateLocationData( (ulong)systemAddress, x, y, z );
            if ( PlottedRoute.GuidanceEnabled && PlottedRoute.Waypoints.All( w => w.visited ) )
            {
                // Deactivate guidance once we've reached our destination.
                await NavigationService.Instance.NavQueryAsync( QueryType.cancel, null, null, null, null, true ).ConfigureAwait(false);
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
            await Application.Current.Dispatcher.InvokeAsync( () =>
            {
                ConfigurationWindow.Instance.refreshGalacticPOIs();
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

        private static async Task UpdateDestinationDataAsync(NavWaypoint routeStart, NavWaypoint routeDestination)
        {
            if ( routeDestination is null )
            {
                await EDDI.Instance.updateDestinationSystemAsync( null ).ConfigureAwait(false);
                EDDI.Instance.DestinationDistanceLy = 0;
                return;
            }

            await EDDI.Instance.updateDestinationSystemAsync( routeDestination.systemAddress, routeDestination.systemName ).ConfigureAwait(false);
            var distance = Functions.StellarDistanceLy(
                routeStart?.x, routeStart?.y, routeStart?.z, 
                routeDestination.x, routeDestination.y, routeDestination.z) ?? 0;
            EDDI.Instance.DestinationDistanceLy = distance;
        }

        public Task HandleStatusAsync(Status status)
        {
            currentStatus = status;
            foreach ( var bookmark in Bookmarks )
            {
                CheckBookmarkPosition( bookmark, currentStatus );
            }

            return Task.CompletedTask;
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
            var radiusMeters = curr.planetradius ?? (EDDI.Instance.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000);
            return Functions.SurfaceConstantHeadingDegrees(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceConstantHeadingDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? (EDDI.Instance.CurrentStarSystem?.bodies
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
            var radiusMeters = curr.planetradius ?? (EDDI.Instance.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000);
            return Functions.SurfaceDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static async Task GetBookmarkExtrasAsync<T>(ObservableCollection<T> bookmarks) where T : NavBookmark
        {
            // Retrieve extra details to supplement our bookmarks

            var bookmarkSystems = bookmarks.Select(n => new StarSystem()
            {
                systemname = n.systemname,
                systemAddress = n.systemAddress
            }).ToList();
            var retrievedBookmarkSystems = await EDDI.Instance.DataProvider
                .SyncFromStarMapServiceAsync( bookmarkSystems ).ConfigureAwait( false );
            foreach (var system in retrievedBookmarkSystems.Where( s => s.visits > 0 ) )
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
