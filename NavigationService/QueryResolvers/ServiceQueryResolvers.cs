using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiNavigationService.QueryResolvers
{
    [UsedImplicitly]
    public class EncodedMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.encoded;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve( Type, query );
    }

    [UsedImplicitly]
    public class GuardianTechBroker : IQueryResolver
    {
        public QueryType Type => QueryType.guardian;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    [UsedImplicitly]
    public class HumanTechBroker : IQueryResolver
    {
        public QueryType Type => QueryType.human;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    [UsedImplicitly]
    public class InterstellarFactors : IQueryResolver
    {
        public QueryType Type => QueryType.facilitator;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    [UsedImplicitly]
    public class ManufacturedMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.manufactured;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    [UsedImplicitly]
    public class RawMaterialsTrader : IQueryResolver
    {
        public QueryType Type => QueryType.raw;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    [UsedImplicitly]
    public class ScorpionSrvVendor : IQueryResolver
    {
        public QueryType Type => QueryType.scorpion;
        public RouteDetailsEvent Resolve ( Query query ) => new ServiceQueryResolver ().Resolve ( Type, query );
    }

    #region ServiceQueryResolver
    internal class ServiceQueryResolver
    {
        private static readonly Dictionary<QueryType, ServiceFilter> ServiceFilters =
            new Dictionary<QueryType, ServiceFilter>()
            {
                // Encoded materials trader
                {
                    QueryType.encoded, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = new List<Economy> { Economy.HighTech, Economy.Military },
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.MaterialTrader },
                        stationModels = null,
                        cubeLy = 40
                    }
                },
                // Interstellar Factors
                {
                    QueryType.facilitator, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = null,
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.Facilitator },
                        stationModels = null,
                        cubeLy = 25
                    }
                },
                // Manufactured materials trader
                {
                    QueryType.manufactured, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = new List<Economy> { Economy.Industrial },
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.MaterialTrader },
                        stationModels = null,
                        cubeLy = 40
                    }
                },
                // Raw materials trader
                {
                    QueryType.raw, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = new List<Economy> { Economy.Extraction, Economy.Refinery },
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.MaterialTrader },
                        stationModels = null,
                        cubeLy = 40
                    }
                },
                // Guardian tech broker
                {
                    QueryType.guardian, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = new List<Economy> { Economy.HighTech },
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.TechBroker },
                        stationModels = null,
                        cubeLy = 80
                    }
                },
                // Human tech broker
                {
                    QueryType.human, new ServiceFilter
                    {
                        systemEconomies = null,
                        stationEconomies = new List<Economy> { Economy.Industrial },
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.TechBroker },
                        stationModels = null,
                        cubeLy = 80
                    }
                },
                // Scorpion SRV vender
                {
                    QueryType.scorpion, new ServiceFilter
                    {
                        systemEconomies = new List<Economy> { Economy.Military },
                        stationEconomies = null,
                        minPopulation = 0,
                        security = null,
                        services = new List<StationService> { StationService.Outfitting },
                        stationModels = new List<StationModel> { StationModel.CraterPort },
                        cubeLy = 40
                    }
                }
            };

        public RouteDetailsEvent Resolve ( QueryType queryType, Query query ) => GetServiceSystem ( 
            queryType,
            query.NumericArg is null ? (int?)null : Convert.ToInt32 ( Math.Round ( (decimal)query.NumericArg ) ),
            query.BooleanArg 
            );

        /// <summary> Route to the nearest star system that offers a specific service </summary>
        /// <returns> The query result </returns>
        private static RouteDetailsEvent GetServiceSystem ( QueryType serviceQuery, int? maxDistanceOverride = null, bool? prioritizeOrbitalStationsOverride = null )
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            int maxStationDistance = maxDistanceOverride ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            bool prioritizeOrbitalStations = prioritizeOrbitalStationsOverride ?? navConfig.prioritizeOrbitalStations;

            var currentSystem = EDDI.Instance.CurrentStarSystem;
            if ( currentSystem != null )
            {
                var shipSize = EDDI.Instance.CurrentShip?.Size ?? LandingPadSize.Large;
                if ( ServiceFilters.TryGetValue ( serviceQuery, out var filter ) )
                {
                    // Scorpions are only found at Surface Ports
                    if ( serviceQuery is QueryType.scorpion )
                    { prioritizeOrbitalStations = false; }

                    var serviceStarSystem = GetServiceSystem(serviceQuery, maxStationDistance, prioritizeOrbitalStations);

                    if ( serviceStarSystem is null && prioritizeOrbitalStations )
                    {
                        serviceStarSystem = GetServiceSystem ( serviceQuery, maxStationDistance, false );
                    }

                    if ( serviceStarSystem != null )
                    {
                        var searchSystem = serviceStarSystem;

                        // Find stations which meet the search preference and filter requirements
                        var serviceStations = FilterSystemStations(serviceQuery, prioritizeOrbitalStations, serviceStarSystem, maxStationDistance, filter, shipSize);

                        // Build list to find the station nearest to the main star
                        var nearestList = new SortedList<decimal, string> ();
                        foreach ( var station in serviceStations )
                        {
                            if ( !nearestList.ContainsKey ( station.distancefromstar ?? 0 ) )
                            {
                                nearestList.Add ( station.distancefromstar ?? 0, station.name );
                            }
                        }

                        // Station is nearest to the main star which meets the service query
                        var searchStation = nearestList.Values.FirstOrDefault();

                        // Update the navRouteList
                        var navRouteList = new NavWaypointCollection();
                        navRouteList.Waypoints.Add ( new NavWaypoint ( currentSystem ) { visited = true } );
                        if ( currentSystem.systemname != searchSystem.systemname )
                        {
                            navRouteList.Waypoints.Add ( new NavWaypoint ( searchSystem )
                            {
                                visited = searchSystem.systemname == currentSystem.systemname,
                                stationName = searchStation
                            } );
                        }

                        // Get mission IDs for 'service' system 
                        var missionids = NavigationService.GetSystemMissionIds(searchSystem.systemname);

                        return new RouteDetailsEvent ( DateTime.UtcNow, serviceQuery.ToString (), searchSystem.systemname, searchStation, navRouteList, missionids.Count, missionids );
                    }
                }
                else
                {
                    Logging.Error ( $"No navigation query filter found for query type {serviceQuery}." );
                }
            }
            else
            {
                Logging.Warn ( "Unable to obtain navigation service result - current star system is not known." );
            }
            return null;
        }

        private static StarSystem GetServiceSystem ( QueryType serviceQuery, int maxStationDistance, bool prioritizeOrbitalStations )
        {
            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if ( currentSystem != null )
            {
                // Get the filter parameters
                var shipSize = EDDI.Instance.CurrentShip?.Size ?? LandingPadSize.Large;
                if ( ServiceFilters.TryGetValue ( serviceQuery, out ServiceFilter filter ) )
                {
                    int cubeLy = filter.cubeLy;
                    var checkedSystems = new List<string> ();
                    var maxTries = 5;

                    while ( maxTries > 0 )
                    {
                        var cubeSystems = NavigationService.Instance.EdsmService.GetStarMapSystemsCube(currentSystem.systemname, cubeLy);
                        if ( cubeSystems?.Any () ?? false )
                        {
                            // Filter systems using search parameters
                            cubeSystems = cubeSystems.Where ( s => s.population >= filter.minPopulation ).ToList ();
                            cubeSystems = cubeSystems
                                .Where ( s => filter.security?.Any ( filterSecurity => s.securityLevel == filterSecurity ) ?? true ).ToList ();
                            cubeSystems = cubeSystems
                                .Where ( s => filter.systemEconomies?.Any ( filterEconomy => s.Economies.Any ( stationEconomy => filterEconomy == stationEconomy ) ) ?? true )
                                .ToList ();

                            // Retrieve systems in current shell which have not been previously checked
                            var systemNames =
                                cubeSystems.Select(s => s.systemname).Except(checkedSystems).ToList();
                            if ( systemNames.Count > 0 )
                            {
                                var starSystems = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames.ToArray(), true, true, false, true, false);
                                checkedSystems.AddRange ( systemNames );

                                var nearestList = new SortedList<decimal, string> ();
                                foreach ( var starsystem in starSystems )
                                {
                                    // Find stations which meet the search preference and filter requirements
                                    var stations = FilterSystemStations(serviceQuery, prioritizeOrbitalStations, starsystem, maxStationDistance, filter, shipSize);

                                    // Build list to find the 'service' system nearest to the current system, meeting station requirements
                                    if ( stations.Count > 0 )
                                    {
                                        decimal distance = NavigationService.CalculateDistance(currentSystem, starsystem);
                                        if ( !nearestList.ContainsKey ( distance ) )
                                        {
                                            nearestList.Add ( distance, starsystem.systemname );
                                        }
                                    }
                                }

                                // Nearest 'service' system
                                var serviceSystem = nearestList.Values.FirstOrDefault();
                                if ( serviceSystem != null )
                                {
                                    return starSystems.FirstOrDefault ( s => s.systemname == serviceSystem );
                                }
                            }
                        }

                        // Increase search radius in 10 Ly increments (up from the starting shell size) until the required 'service' is found
                        cubeLy += 10;
                        maxTries--;
                    }
                }
            }
            return null;
        }

        private static List<Station> FilterSystemStations ( QueryType serviceQuery, bool prioritizeOrbitalStations, StarSystem serviceStarSystem,
            int maxStationDistance, ServiceFilter filter, LandingPadSize shipSize )
        {
            bool EconomyFilter ( Station station )
            {
                bool? testStationEconomy ( Economy economy )
                {
                    switch ( serviceQuery )
                    {
                        case QueryType.encoded:
                        case QueryType.raw:
                        case QueryType.manufactured:
                            {
                                // If the station could theoretically qualify for multiple types of material traders,
                                // the precedence is Encoded, Raw, Manufactured
                                if ( ServiceFilters[ QueryType.encoded ].stationEconomies?.Contains ( economy ) ?? false )
                                {
                                    return serviceQuery == QueryType.encoded;
                                }
                                if ( ServiceFilters[ QueryType.raw ].stationEconomies?.Contains ( economy ) ?? false )
                                {
                                    return serviceQuery == QueryType.raw;
                                }
                                if ( ServiceFilters[ QueryType.manufactured ].stationEconomies?.Contains ( economy ) ?? false )
                                {
                                    return serviceQuery == QueryType.manufactured;
                                }
                                break;
                            }
                        case QueryType.guardian:
                        case QueryType.human:
                            {
                                // If the station could theoretically qualify for multiple types of tech brokers,
                                // the precedence is Guardian, Human
                                if ( ServiceFilters[ QueryType.guardian ].stationEconomies?.Contains ( economy ) ?? false )
                                {
                                    return serviceQuery == QueryType.guardian;
                                }
                                if ( ServiceFilters[ QueryType.human ].stationEconomies?.Contains ( economy ) ?? false )
                                {
                                    return serviceQuery == QueryType.human;
                                }
                                break;
                            }
                    }
                    return filter.stationEconomies?.Contains ( economy );
                }

                // Evaluate each economy in turn - the primary economy takes precedence over the secondary economy.
                // If the results are inconclusive (for example, if there are no filter economies) then pass the station along.
                return testStationEconomy ( station.economyShares[ 0 ].economy ) ??
                       testStationEconomy ( station.economyShares[ 1 ].economy ) ??
                       true;
            }

            // Prioritize orbital stations as appropriate
            var serviceStations = prioritizeOrbitalStations ? serviceStarSystem.orbitalstations : serviceStarSystem.stations;

            // Apply our service filters
            serviceStations = serviceStations.Where ( s => filter.services?.All ( svc => s.stationServices.Contains ( svc ) ) ?? true ).ToList ();

            // Apply our station model filter
            serviceStations = serviceStations.Where ( s => filter.stationModels?.Contains ( s.Model ) ?? true ).ToList ();

            // Apply our economy filters
            serviceStations = serviceStations.Where ( EconomyFilter ).ToList ();

            // Apply our distance filter
            serviceStations = serviceStations.Where ( s => s.distancefromstar <= maxStationDistance ).ToList ();

            // Apply our landing pad filter
            if ( serviceQuery == QueryType.facilitator )
            {
                serviceStations = serviceStations.Where ( s => s.LandingPadCheck ( shipSize ) ).ToList ();
            }

            return serviceStations;
        }
    }
    #endregion
}
