using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    internal class SetQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.set;
        public RouteDetailsEvent Resolve ( Query query ) => SetRoute ( query.StringArg0, query.StringArg1 );

        private static RouteDetailsEvent SetRoute ( string system, string station = null )
        {
            NavWaypointCollection navRouteList;
            NavWaypoint firstUnvisitedWaypoint;
            // Use our saved route if a named system is not provided
            if ( string.IsNullOrEmpty ( system ) )
            {
                navRouteList = ConfigService.Instance.navigationMonitorConfiguration.plottedRouteList ?? new NavWaypointCollection ();
                firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault ( w => !w.visited );
                if ( firstUnvisitedWaypoint != null )
                {
                    return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.set.ToString (), firstUnvisitedWaypoint.systemName, firstUnvisitedWaypoint.stationName, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint.missionids );
                }
            }

            // Disregard commands to set a route to the current star system.
            var curr = EDDI.Instance?.CurrentStarSystem;
            if ( curr?.systemname == system )
            { return null; }

            // Set a course to a named system (and optionally station)
            var neutronRoute = NavigationService.Instance.NavQuery(QueryType.neutron, system);
            if ( neutronRoute == null || neutronRoute.Route.Waypoints.Count <= 1 )
            { return null; }

            navRouteList = neutronRoute.Route;
            foreach ( var wp in navRouteList.Waypoints )
            {
                wp.missionids = NavigationService.GetSystemMissionIds ( wp.systemName );
            }
            firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault ( w => !w.visited );
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.set.ToString (), firstUnvisitedWaypoint?.systemName, firstUnvisitedWaypoint?.systemName == system ? station : null, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint?.missionids ?? new List<long> () );
        }
    }

    [UsedImplicitly]
    internal class CancelQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.cancel;
        public RouteDetailsEvent Resolve ( Query query ) => CancelRoute ();

        private static RouteDetailsEvent CancelRoute ()
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            // Save updated route data to the configuration
            navConfig.plottedRouteList.GuidanceEnabled = false;
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;

            // Update Voice Attack & Cottle variables
            EDDI.Instance.updateDestinationSystem ( null );
            EDDI.Instance.DestinationDistanceLy = 0;

            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.cancel.ToString (), null, null, navConfig.plottedRouteList, navConfig.plottedRouteList.Waypoints.Count, null );
        }
    }

    [UsedImplicitly]
    public class UpdateQueryResolver : IQueryResolver
    {
        public QueryType Type => QueryType.update;
        public RouteDetailsEvent Resolve ( Query query ) => RefreshLastNavigationQuery ();

        /// <summary> Repeat the last mission query and return an updated result if different from the prior result, either relative to your current location or to a named system </summary>
        /// <returns> The star system result from the repeated query </returns>
        private static RouteDetailsEvent RefreshLastNavigationQuery ()
        {
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if ( !( config?.plottedRouteList?.GuidanceEnabled ?? false ) )
            { return null; }

            if ( NavigationService.Instance.LastQuery.Group () == QueryGroup.missions )
            {
                var missionsList = ConfigService.Instance.missionMonitorConfiguration?.missions?.ToList() ?? new List<Mission>();
                if ( missionsList
                    .Where ( m => m != null && m.statusDef == MissionStatus.Active )
                    .Any ( m => m.destinationsystem == EDDI.Instance.CurrentStarSystem?.systemname ) )
                {
                    // We still have active missions at the current location
                    return null;
                }
            }

            var currentPlottedRoute = ConfigService.Instance.navigationMonitorConfiguration?.plottedRouteList;
            if ( !currentPlottedRoute?.UnvisitedWaypoints.Any() ?? false )
            {
                // The current route has already been completed
                return null;
            }

            var currentWaypoint = currentPlottedRoute?.Waypoints.FirstOrDefault( w =>
                w.systemAddress == EDDI.Instance.CurrentStarSystem?.systemAddress );
            if ( currentWaypoint != null )
            {
                // We're following the plotted route
                return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.update.ToString (), currentPlottedRoute.NearestUnvisitedWaypoint.systemName, currentPlottedRoute.NearestUnvisitedWaypoint.stationName, currentPlottedRoute, currentPlottedRoute.Waypoints.Count, currentPlottedRoute.NearestUnvisitedWaypoint.missionids );
            }

            var destinationWaypoint = currentPlottedRoute?.Waypoints.FirstOrDefault( w =>
                    w.systemAddress == EDDI.Instance.DestinationStarSystem?.systemAddress );
            if ( destinationWaypoint != null )
            {
                // We're making our way back to the plotted route
                return new RouteDetailsEvent( DateTime.UtcNow, QueryType.update.ToString(), currentPlottedRoute.DestinationWaypoint?.systemName, null, currentPlottedRoute, currentPlottedRoute.Waypoints.Count, currentPlottedRoute.DestinationWaypoint?.missionids );
            }

            // We've strayed, recalculate the route
            Enum.TryParse( config?.searchQuery, out QueryType lastQuery );
            var @event = NavigationService.Instance.NavQuery(lastQuery, config?.searchQuerySystemArg, config?.searchQuerySystemArg, config?.maxSearchDistanceFromStarLs, config?.prioritizeOrbitalStations);
            if ( @event is null ) { return null; }
            EDDI.Instance.enqueueEvent( new RouteDetailsEvent( DateTime.UtcNow, QueryType.recalculating.ToString(), @event.system, @event.station, @event.Route, @event.count, @event.missionids ) );
            return new RouteDetailsEvent( DateTime.UtcNow, config?.searchQuery, @event.system, @event.station, @event.Route, @event.count, @event.missionids );
        }
    }
}
