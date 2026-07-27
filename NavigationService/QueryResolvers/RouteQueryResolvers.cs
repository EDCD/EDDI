using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    internal class SetQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.set;
        public static Dictionary<string, object> SpanshQueryFilter => null;

        public Task<RouteDetailsEvent> ResolveAsync ( Query query, StarSystem startSystem ) =>
            SetRouteAsync( startSystem, query.StringArg0, query.StringArg1, query.RuntimeContext );

        private static async Task<RouteDetailsEvent> SetRouteAsync ( StarSystem startSystem, string systemName, string stationName, INavigationRuntimeContext runtimeContext )
        {
            // Disregard commands to set a route to the current star system.
            if ( startSystem.systemname == systemName ) { return null; }

            if ( string.IsNullOrEmpty ( systemName ) )
            {
                // Use our saved route if a named system is not provided
                var navRouteList = runtimeContext.NavigationConfiguration.plottedRouteList ?? new NavWaypointCollection ();
                navRouteList.UpdateLocationData( startSystem.systemAddress, startSystem.x, startSystem.y, startSystem.z );
                var firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault ( w => !w.visited );
                if ( firstUnvisitedWaypoint != null )
                {
                    return new RouteDetailsEvent ( DateTime.UtcNow, nameof(QueryType.set), firstUnvisitedWaypoint.systemName, firstUnvisitedWaypoint.systemAddress, firstUnvisitedWaypoint.stationName, firstUnvisitedWaypoint.marketID, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint.missionids );
                }
            }
            else
            {
                // Set a course to a named system (and optionally station)
                var neutronRoute = await NavigationService.Instance.NavQueryAsync(QueryType.neutron, systemName).ConfigureAwait(false);
                if ( neutronRoute == null || neutronRoute.Route.Waypoints.Count == 1 ) { return null; }
                var navRouteList = neutronRoute.Route;
                navRouteList.UpdateLocationData( startSystem.systemAddress, startSystem.x, startSystem.y, startSystem.z );
                foreach ( var wp in navRouteList.Waypoints )
                {
                    wp.missionids = NavigationService.GetSystemMissionIds( wp.systemName, runtimeContext );
                }
                var firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault( w => !w.visited );
                var lastWaypoint = navRouteList.Waypoints.LastOrDefault();

                if ( lastWaypoint != null )
                {
                    var dest = string.IsNullOrEmpty( stationName )
                        ? await runtimeContext.DataProvider.GetOrFetchSystemWaypointAsync( lastWaypoint.systemName ).ConfigureAwait(false)
                        : await runtimeContext.DataProvider.FetchStationWaypointAsync( lastWaypoint.x, lastWaypoint.y, lastWaypoint.z,
                            new Dictionary<string, object> { { "name", new { value = new[] { stationName } } } } ).ConfigureAwait(false);

                    return new RouteDetailsEvent( DateTime.UtcNow, nameof(QueryType.set), firstUnvisitedWaypoint?.systemName, firstUnvisitedWaypoint?.systemAddress, firstUnvisitedWaypoint?.systemAddress == dest.systemAddress ? dest.stationName : null, firstUnvisitedWaypoint?.systemAddress == dest.systemAddress ? dest.marketID : null, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint?.missionids ?? [ ] );

                }
            }
            return null;
        }
    }

    [UsedImplicitly]
    internal class CancelQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.cancel;
        public static Dictionary<string, object> SpanshQueryFilter => null;

        public Task<RouteDetailsEvent> ResolveAsync ( Query query, StarSystem startSystem ) =>
            CancelRouteAsync( query.RuntimeContext );

        private static async Task<RouteDetailsEvent> CancelRouteAsync ( INavigationRuntimeContext runtimeContext )
        {
            // Get up-to-date configuration data
            var navConfig = runtimeContext.NavigationConfiguration;

            // Save updated route data to the configuration
            navConfig.plottedRouteList.GuidanceEnabled = false;
            runtimeContext.NavigationConfiguration = navConfig;

            // Update Voice Attack & Cottle variables
            await runtimeContext.UpdateDestinationSystemAsync( null ).ConfigureAwait(false);
            runtimeContext.UpdateDestinationDistance( 0 );

            return new RouteDetailsEvent ( DateTime.UtcNow, nameof(QueryType.cancel), null, null, null, null, navConfig.plottedRouteList, navConfig.plottedRouteList.Waypoints.Count, null );
        }
    }

    [UsedImplicitly]
    public class UpdateQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.update;
        public static Dictionary<string, object> SpanshQueryFilter => null;

        public Task<RouteDetailsEvent> ResolveAsync ( Query query, StarSystem currentSystem ) =>
            RefreshLastNavigationQueryAsync( currentSystem, query.RuntimeContext );

        /// <summary> Repeat the last mission query and return an updated result if different from the prior result, either relative to your current location or to a named system </summary>
        /// <returns> The star system result from the repeated query </returns>
        private static async Task<RouteDetailsEvent> RefreshLastNavigationQueryAsync ( [ NotNull ] StarSystem currentSystem, INavigationRuntimeContext runtimeContext )
        {
            var config = runtimeContext.NavigationConfiguration;
            if ( config is null ) { return null; }
            if ( !( config.plottedRouteList?.GuidanceEnabled ?? false ) ) { return null; }

            if ( NavigationService.Instance.LastQuery.Group () == QueryGroup.missions )
            {
                var missionsList = runtimeContext.MissionConfiguration?.missions?.ToList() ?? [ ];
                if ( missionsList
                    .Where ( m => m != null && m.statusDef == MissionStatus.Active )
                    .Any ( m => m.destinationsystem == currentSystem.systemname ) )
                {
                    // We still have active missions at the current location
                    return null;
                }
            }

            var currentPlottedRoute = runtimeContext.NavigationConfiguration?.plottedRouteList;
            if ( currentPlottedRoute is null ) { return null; }

            if ( currentPlottedRoute.UnvisitedWaypoints.Count == 0 )
            {
                // The current route has already been completed
                return null;
            }

            var currentWaypoint = currentPlottedRoute.Waypoints.FirstOrDefault( w => w.systemAddress == currentSystem.systemAddress );
            if ( currentWaypoint != null )
            {
                // We're currently visiting a waypoint on the plotted route and need to update to the next waypoint
                currentPlottedRoute.NextWaypoint = null; 
                return new RouteDetailsEvent( DateTime.UtcNow, nameof(QueryType.update), currentPlottedRoute.NextWaypoint?.systemName, currentPlottedRoute.NextWaypoint?.systemAddress, currentPlottedRoute.NextWaypoint?.stationName, currentPlottedRoute.NextWaypoint?.marketID, currentPlottedRoute, currentPlottedRoute.Waypoints.Count, currentPlottedRoute.NextWaypoint?.missionids );
            }

            var destinationWaypoint = currentPlottedRoute.Waypoints
                .Where( w => !w.visited )
                .FirstOrDefault( w => w.systemAddress == runtimeContext.GameState.DestinationStarSystem?.systemAddress );
            if ( destinationWaypoint != null )
            {
                // We're making our way towards a valid destination waypoint
                currentPlottedRoute.NextWaypoint = destinationWaypoint;
                return new RouteDetailsEvent( DateTime.UtcNow, nameof(QueryType.update),
                    destinationWaypoint.systemName, destinationWaypoint.systemAddress, destinationWaypoint.stationName,
                    destinationWaypoint.marketID, currentPlottedRoute,
                    currentPlottedRoute.Waypoints.Count, destinationWaypoint.missionids );
            }

            // We've strayed, recalculate the route
            currentPlottedRoute.NextWaypoint = null;
            if ( Enum.TryParse( config?.searchQuery, out QueryType lastQuery ) )
            {
                var @event = await NavigationService.Instance.NavQueryAsync(lastQuery, config?.searchQuerySystemArg, config?.searchQuerySystemArg, config?.maxSearchDistanceFromStarLs, config?.prioritizeOrbitalStations).ConfigureAwait(false);
                if ( @event is null )
                { return null; }
                runtimeContext.EnqueueEvent( new RouteDetailsEvent( DateTime.UtcNow, nameof( QueryType.recalculating ), @event.system, @event.systemAddress, @event.station, @event.marketId, @event.Route, @event.count, @event.missionids ) );
                return new RouteDetailsEvent( DateTime.UtcNow, config?.searchQuery, @event.system, @event.systemAddress, @event.station, @event.marketId, @event.Route, @event.count, @event.missionids );
            }
            
            return null;
        }
    }
}
