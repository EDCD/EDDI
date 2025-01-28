using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    public class EncodedMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.encoded;
        public Dictionary<string, object> SpanshQueryFilter => 
            new Dictionary<string, object>
            {
                { "material_trader", new { value = new[] { "Encoded" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class GuardianTechBroker : IQueryResolver
    {
        public QueryType Type => QueryType.guardian;
        public Dictionary<string, object> SpanshQueryFilter => 
            new Dictionary<string, object>
            {
                { "technology_broker", new { value = new[] { "Guardian" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class HumanTechBroker : IQueryResolver
    {
        public QueryType Type => QueryType.human;
        public Dictionary<string, object> SpanshQueryFilter =>
            new Dictionary<string, object>
            {
                { "technology_broker", new { value = new[] { "Human" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class InterstellarFactors : IQueryResolver
    {
        public QueryType Type => QueryType.facilitator;
        public Dictionary<string, object> SpanshQueryFilter =>
            new Dictionary<string, object>
            {
                { "services", new { value = new[] { "Interstellar Factors Contact" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class ManufacturedMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.manufactured;
        public Dictionary<string, object> SpanshQueryFilter =>
            new Dictionary<string, object>
            {
                { "material_trader", new { value = new[] { "Manufactured" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class RawMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.raw;

        public Dictionary<string, object> SpanshQueryFilter => new Dictionary<string, object>
        {
            { "material_trader", new { value = new[] { "Raw" } } }
        };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    [UsedImplicitly]
    public class ScorpionSrvVendor : IQueryResolver
    {
        public QueryType Type => QueryType.scorpion;
        public Dictionary<string, object> SpanshQueryFilter => new Dictionary<string, object>
            {
                { "system_primary_economy", new { value = new[] { "Military" } } },
                { "type", new { value = new[] { "Planetary Port" } } },
                { "services", new { value = new[] { "Outfitting" } } }
            };
        public RouteDetailsEvent Resolve ( Query query, StarSystem startSystem ) => new ServiceQueryResolver ().Resolve ( Type, SpanshQueryFilter, query, startSystem );
    }

    #region ServiceQueryResolver
    internal class ServiceQueryResolver
    {
        public RouteDetailsEvent Resolve ( QueryType queryType, Dictionary<string, object> spanshQueryFilter,
            [ NotNull ] Query query,
            [ NotNull ] StarSystem startSystem ) => GetServiceSystem( queryType,
            startSystem, spanshQueryFilter, 
            query.NumericArg is null ? (int?)null : Convert.ToInt32( Math.Round( (decimal)query.NumericArg ) ),
            query.BooleanArg
        );

        /// <summary> Route to the nearest star system that offers a specific service </summary>
        /// <returns> The query result </returns>
        private static RouteDetailsEvent GetServiceSystem ( QueryType serviceQuery, [ NotNull ] StarSystem startSystem,
            Dictionary<string, object> spanshQueryFilter = null, int? maxDistanceOverride = null,
            bool? prioritizeOrbitalStationsOverride = null )
        {
            if ( spanshQueryFilter is null )
            {
                Logging.Error($"No search filter has been defined for '{serviceQuery}' navigation searches.");
                return null;
            }
            if ( startSystem.x is null || startSystem.y is null || startSystem.z is null )
            {
                Logging.Warn( "Start system coordinates are unknown." );
                return null;
            }

            var fromX = Convert.ToDecimal( startSystem.x );
            var fromY = Convert.ToDecimal( startSystem.y );
            var fromZ = Convert.ToDecimal( startSystem.z );

            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            var maxStationDistance = maxDistanceOverride ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            var prioritizeOrbitalStations = prioritizeOrbitalStationsOverride ?? navConfig.prioritizeOrbitalStations;

            // Add configured and situational search filter parameters
            if ( prioritizeOrbitalStations && serviceQuery != QueryType.scorpion )
            {
                spanshQueryFilter.Add( "is_planetary", new { value = false } );
            }
            var shipSize = EDDI.Instance.CurrentShip?.Size ?? LandingPadSize.Large;
            if ( shipSize.sizeIndex == 3 )
            {
                spanshQueryFilter.Add( "has_large_pad", new { value = true } );
            }
            spanshQueryFilter.Add( "distance_to_arrival", new { comparison = "<=>", value = new[] { 0, maxStationDistance } } );
            
            var searchResult = EDDI.Instance.DataProvider.FetchStationWaypoint( fromX, fromY, fromZ, spanshQueryFilter );
            if ( searchResult != null )
            {
                searchResult.visited = searchResult.systemAddress == startSystem.systemAddress;

                // Update the navRouteList
                var navRouteList = new NavWaypointCollection(Convert.ToDecimal(startSystem.x), Convert.ToDecimal(startSystem.y), Convert.ToDecimal(startSystem.z));
                navRouteList.Waypoints.Add( new NavWaypoint( startSystem ) { visited = true } );
                if ( startSystem.systemAddress != searchResult.systemAddress )
                {
                    navRouteList.Waypoints.Add( searchResult );
                }

                // Get mission IDs for 'service' system 
                var missionids = NavigationService.GetSystemMissionIds( searchResult.systemName );

                return new RouteDetailsEvent( DateTime.UtcNow, serviceQuery.ToString(), searchResult.systemName, searchResult.systemAddress, searchResult.stationName, searchResult.marketID, navRouteList, missionids.Count, missionids );
            }
            else
            {
                Logging.Error( $"No navigation query filter found for query type {serviceQuery}." );
            }

            return null;
        }
    }
    #endregion
}
