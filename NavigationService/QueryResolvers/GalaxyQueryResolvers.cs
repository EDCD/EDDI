using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiSpanshService;
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
        public RouteDetailsEvent Resolve ( Query query ) => GetNearestScoopSystem(query.NumericArg);

        /// <summary> Route to the nearest star system that is eligible for fuel scoop refueling </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestScoopSystem ( decimal? searchRadius = null )
        {
            if ( searchRadius is null )
            {
                searchRadius = EDDI.Instance.CurrentShip?.JumpDetails ( "total" )?.distance ?? 100;
            }

            // We'll search in progressive spherical shells out to a maximum radius of 100 ly
            // (the maximum from EDSM for a spherical system search)
            string searchSystem = null;
            int searchCount = 0;
            int searchIncrement = (int)Math.Ceiling(Math.Min((decimal)searchRadius, 100) / 4);
            var navRouteList = new NavWaypointCollection();

            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if ( currentSystem != null )
            {
                if ( currentSystem.scoopable )
                {
                    searchSystem = currentSystem.systemname;
                    navRouteList.Waypoints.Add ( new NavWaypoint ( currentSystem ) { visited = true } );
                    searchCount = 1;
                }
                else
                {
                    for ( int i = 0; i < 4; i++ )
                    {
                        int startRadius = i * searchIncrement;
                        var endRadius = (i + 1) * searchIncrement;
                        var sphereSystems = NavigationService.Instance.EdsmService.GetStarMapSystemsSphere(currentSystem.systemname, startRadius, endRadius) ?? new List<Dictionary<string, object>>();
                        sphereSystems = sphereSystems.Where ( kvp => ( kvp[ "system" ] as StarSystem )?.scoopable ?? false ).ToList ();
                        searchCount = sphereSystems.Count;
                        if ( searchCount > 0 )
                        {
                            var nearestList = new SortedList<decimal, StarSystem>();
                            foreach ( Dictionary<string, object> system in sphereSystems )
                            {
                                decimal distance = (decimal)system["distance"];
                                if ( !nearestList.ContainsKey ( distance ) )
                                {
                                    nearestList.Add ( distance, system[ "system" ] as StarSystem );
                                }
                            }

                            // Nearest 'scoopable' system
                            searchSystem = nearestList.Values.FirstOrDefault ()?.systemname;

                            // Update the navRouteList
                            navRouteList.Waypoints.Add ( new NavWaypoint ( currentSystem ) { visited = true } );
                            if ( currentSystem.systemname != nearestList.Values.FirstOrDefault ()?.systemname )
                            {
                                navRouteList.Waypoints.Add ( new NavWaypoint ( nearestList.Values.FirstOrDefault () ) { visited = nearestList.Values.FirstOrDefault ()?.systemname == currentSystem.systemname } );
                            }

                            break;
                        }
                    }
                }
            }
            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.scoop.ToString (), searchSystem, null, navRouteList, searchCount, null );
        }
    }

    [UsedImplicitly]
    internal class NeutronRouteResolver : IQueryResolver
    {
        public QueryType Type => QueryType.neutron;
        public RouteDetailsEvent Resolve ( Query query ) => GetNeutronRoute(query.StringArg0);

        /// <summary> Obtains a neutron star route between the current star system and a named star system </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNeutronRoute ( string targetSystemName, bool isSupercharged = false, bool useSupercharge = true, bool useInjections = false, bool excludeSecondary = false, bool fromUIquery = false )
        {
            var plottedRouteList = new NavWaypointCollection();
            var currentSystemName = EDDI.Instance.CurrentStarSystem?.systemname;
            if ( EDDI.Instance.CurrentStarSystem == null )
            {
                Logging.Debug ( "Neutron route plotting is not available, current star system is unknown." );
            }
            else if ( string.IsNullOrEmpty ( targetSystemName ) )
            {
                Logging.Debug ( "Neutron route plotting is not available, target star system is unknown." );
            }
            else if ( targetSystemName == currentSystemName )
            {
                Logging.Debug ( "Neutron route plotting is not available, the target star system name matches the current star system." );
            }
            else
            {
                var cargoCarriedTons = ConfigService.Instance.cargoMonitorConfiguration.cargocarried;
                var shipId = ConfigService.Instance.shipMonitorConfiguration.currentshipid;
                var ship = ConfigService.Instance.shipMonitorConfiguration.shipyard.FirstOrDefault(s =>
                    s.LocalId == shipId);
                var spanshService = new SpanshService();
                plottedRouteList = spanshService.GetGalaxyRoute ( currentSystemName, targetSystemName, ship, cargoCarriedTons,
                    isSupercharged, useSupercharge, useInjections, excludeSecondary, fromUIquery );
            }

            if ( plottedRouteList == null || plottedRouteList.Waypoints.Count <= 1 )
            { return null; }

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
            var searchSystem = plottedRouteList.Waypoints[1].systemName;

            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.neutron.ToString (), searchSystem, null, plottedRouteList, plottedRouteList.Waypoints.Count, null );
        }

    }

    [UsedImplicitly]
    internal class CarrierRouteResolver : IQueryResolver
    {
        public QueryType Type => QueryType.carrier;
        public RouteDetailsEvent Resolve ( Query query ) => GetCarrierRoute( query.StringArg0, query.StringArg1, (long)Math.Round ( query.NumericArg ?? 0, 0 ) );

        /// <summary> Obtains a carrier route between the current carrier star system and a named star system </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetCarrierRoute ( string targetSystemName, string startingSystemName, long? usedCarrierCapacity = 0, string[] refuelDestinations = null, bool fromUIquery = false )
        {
            if ( string.IsNullOrEmpty ( startingSystemName ) )
            {
                Logging.Warn ( "Invalid query: starting star system is not identified." );
                return null;
            }
            else if ( string.IsNullOrEmpty ( targetSystemName ) )
            {
                Logging.Warn ( "Invalid query: target star system is not identified." );
                return null;
            }

            var spanshService = new SpanshService();
            usedCarrierCapacity = usedCarrierCapacity ?? EDDI.Instance.FleetCarrier?.usedCapacity;
            if ( usedCarrierCapacity is null )
            { return null; }
            var plottedRouteList = spanshService.GetCarrierRoute(startingSystemName, new[] { targetSystemName }, Convert.ToInt64(usedCarrierCapacity), false, refuelDestinations, fromUIquery);

            if ( plottedRouteList == null || plottedRouteList.Waypoints.Count <= 1 )
            { return null; }

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
            var searchSystem = plottedRouteList.Waypoints[1].systemName;

            return new RouteDetailsEvent ( DateTime.UtcNow, QueryType.carrier.ToString (), searchSystem, null, plottedRouteList, plottedRouteList.Waypoints.Count, null );
        }
    }
}