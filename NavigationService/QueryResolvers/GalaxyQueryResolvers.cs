using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    internal class NearestScoopSystemResolver : IQueryResolver
    {
        public QueryType Type => QueryType.scoop;
        public Dictionary<string, object> SpanshQueryFilter =>
            new Dictionary<string, object>
            {
                { "type", new { value = new[] { "Star" } } },
                { "subtype", new { value = new[] {
                    "A (Blue-White super giant) Star",
                    "A (Blue-White) Star",
                    "B (Blue-White super giant) Star",
                    "B (Blue-White) Star",
                    "F (White super giant) Star",
                    "F (White) Star",
                    "G (White-Yellow super giant) Star",
                    "G (White-Yellow) Star",
                    "K (Yellow-Orange giant) Star",
                    "K (Yellow-Orange) Star",
                    "M (Red dwarf) Star",
                    "M (Red giant) Star",
                    "M (Red super giant) Star",
                    "O (Blue-White) Star"
                } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetNearestScoopSystem(startSystem, SpanshQueryFilter);

        /// <summary> Route to the nearest star system that is eligible for fuel scoop refueling </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestScoopSystem ( [ NotNull ] StarSystem startSystem, [ NotNull ] Dictionary<string, object> searchFilter )
        {
            if ( startSystem.x is null || startSystem.y is null || startSystem.z is null )
            {
                Logging.Warn("Start system coordinates are unknown.");
                return null;
            }

            var fromX = Convert.ToDecimal( startSystem.x );
            var fromY = Convert.ToDecimal( startSystem.y );
            var fromZ = Convert.ToDecimal( startSystem.z );

            var navRouteList = new NavWaypointCollection(fromX, fromY, fromZ);
            navRouteList.Waypoints.Add( new NavWaypoint( startSystem ) { visited = true } );
            if ( !startSystem.scoopable )
            {
                var searchSystem = EDDI.Instance.DataProvider.FetchBodyWaypoint( fromX, fromY, fromZ, searchFilter );
                navRouteList.Waypoints.Add( searchSystem );
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.scoop.ToString (), navRouteList.Waypoints.LastOrDefault()?.systemName, navRouteList.Waypoints.LastOrDefault()?.systemAddress, null, null, navRouteList, navRouteList.Waypoints.Count, null );
        }
    }

    [UsedImplicitly]
    internal class NeutronRouteResolver : IQueryResolver
    {
        public QueryType Type => QueryType.neutron;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetNeutronRoute( query.StringArg0, startSystem );

        /// <summary> Obtains a neutron star route between the current star system and a named star system </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNeutronRoute ( string targetSystemName, StarSystem startSystem, bool isSupercharged = false, bool useSupercharge = true, bool useInjections = false, bool excludeSecondary = false, bool fromUIquery = false )
        {
            if ( string.IsNullOrEmpty( targetSystemName ) )
            {
                Logging.Warn( "Neutron route plotting is not available, target star system is unknown." );
                return null;
            }
            else if ( targetSystemName == startSystem.systemname )
            {
                Logging.Warn( "Neutron route plotting is not available, the target star system name matches the current star system." );
                return null;
            }

            var cargoCarriedTons = ConfigService.Instance.cargoMonitorConfiguration.cargocarried;
            var shipId = ConfigService.Instance.shipMonitorConfiguration.currentshipid;
            var ship = ConfigService.Instance.shipMonitorConfiguration.shipyard.FirstOrDefault(s => s.LocalId == shipId);
            var plottedRouteList = EDDI.Instance.DataProvider.FetchGalaxyRoute ( startSystem.systemname, targetSystemName, ship, cargoCarriedTons,
                isSupercharged, useSupercharge, useInjections, excludeSecondary, fromUIquery );
            if ( plottedRouteList == null || plottedRouteList.Waypoints.Count <= 1 ) { return null; }
            plottedRouteList.UpdateLocationData( startSystem.systemAddress, startSystem.x, startSystem.y, startSystem.z );

            // Sanity check - if we're already navigating to the plotted route destination then the number of jumps
            // must be equal or less then the already plotted route and the total route distance must be less also.
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if ( plottedRouteList.Waypoints.LastOrDefault ()?.systemAddress ==
                config.navRouteList.Waypoints.LastOrDefault ()?.systemAddress
                && plottedRouteList.Waypoints.Count >= config.navRouteList.Waypoints.Count )
            {
                plottedRouteList = config.navRouteList;
            }

            plottedRouteList.Waypoints.First ().visited = true;
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.neutron.ToString (), plottedRouteList.Waypoints[ 1 ]?.systemName, plottedRouteList.Waypoints[ 1 ]?.systemAddress, null, null, plottedRouteList, plottedRouteList.Waypoints.Count, null );
        }

    }

    [UsedImplicitly]
    internal class CarrierRouteResolver : IQueryResolver
    {
        public QueryType Type => QueryType.carrier;
        public Dictionary<string, object> SpanshQueryFilter => null;
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => GetCarrierRoute( query.StringArg0, startSystem, (long)Math.Round ( query.NumericArg ?? 0, 0 ) );

        /// <summary> Obtains a carrier route between the current carrier star system and a named star system </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetCarrierRoute ( [NotNull] string targetSystemName, [NotNull] StarSystem startSystem, long? usedCarrierCapacity = 0, string[] refuelDestinations = null, bool fromUIquery = false )
        {
            usedCarrierCapacity = usedCarrierCapacity ?? EDDI.Instance.FleetCarrier?.usedCapacity;
            if ( usedCarrierCapacity is null ) { return null; }

            var plottedRouteList = EDDI.Instance.DataProvider.FetchCarrierRoute(startSystem.systemname, new[] { targetSystemName }, Convert.ToInt64(usedCarrierCapacity), false, refuelDestinations, fromUIquery);
            if ( plottedRouteList == null || plottedRouteList.Waypoints.Count <= 1 ) { return null; }
            plottedRouteList.UpdateLocationData( startSystem.systemAddress, startSystem.x, startSystem.y, startSystem.z );

            // Sanity check - if we're already navigating to the plotted route destination then the number of jumps
            // must be equal or less then the already plotted route and the total route distance must be less also.
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if ( plottedRouteList.Waypoints.LastOrDefault ()?.systemAddress ==
                config.navRouteList.Waypoints.LastOrDefault ()?.systemAddress
                && plottedRouteList.Waypoints.Count >= config.navRouteList.Waypoints.Count )
            {
                plottedRouteList = config.navRouteList;
            }

            plottedRouteList.Waypoints.First ().visited = true;
            
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.carrier.ToString (), plottedRouteList.Waypoints[ 1 ]?.systemName, plottedRouteList.Waypoints[ 1 ]?.systemAddress, null, null, plottedRouteList, plottedRouteList.Waypoints.Count, null );
        }
    }
}