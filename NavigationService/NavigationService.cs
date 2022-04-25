using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiSpanshService;
using EddiStarMapService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiNavigationService
{
    public class NavigationService : INotifyPropertyChanged
    {
        private static readonly Dictionary<QueryType, ServiceFilter> ServiceFilters =
            new Dictionary<QueryType, ServiceFilter>()
            {
                // Encoded materials trader
                {
                    QueryType.encoded, new ServiceFilter
                    {
                        econ = new List<string> { "High Tech", "Military" },
                        population = 1000000,
                        security = new List<string> { "Medium", "High" },
                        service = StationService.FromName("Material Trader"),
                        cubeLy = 40
                    }
                },
                // Interstellar Factors
                {
                    QueryType.facilitator, new ServiceFilter
                    {
                        econ = new List<string>(),
                        population = 0,
                        security = new List<string> { "Low" },
                        service = StationService.FromName("Interstellar Factors Contact"),
                        cubeLy = 25
                    }
                },
                // Manufactured materials trader
                {
                    QueryType.manufactured, new ServiceFilter
                    {
                        econ = new List<string> { "Industrial" },
                        population = 1000000,
                        security = new List<string>() { "Medium", "High" },
                        service = StationService.FromName("Material Trader"),
                        cubeLy = 40
                    }
                },
                // Raw materials trader
                {
                    QueryType.raw, new ServiceFilter
                    {
                        econ = new List<string> { "Extraction", "Refinery" },
                        population = 1000000,
                        security = new List<string> { "Medium", "High" },
                        service = StationService.FromName("Material Trader"),
                        cubeLy = 40
                    }
                },
                // Guardian tech broker
                {
                    QueryType.guardian, new ServiceFilter
                    {
                        econ = new List<string> { "High Tech" },
                        population = 10000000,
                        security = new List<string> { "High" },
                        service = StationService.FromName("Technology Broker"),
                        cubeLy = 80
                    }
                },
                // Human tech broker
                {
                    QueryType.human, new ServiceFilter
                    {
                        econ = new List<string> { "Industrial" },
                        population = 10000000,
                        security = new List<string> { "High" },
                        service = StationService.FromName("Technology Broker"),
                        cubeLy = 80
                    }
                }
            };

        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;
        private static NavigationService instance;
        private static readonly object instanceLock = new object();

        // Search variables
        public StarSystem SearchStarSystem { get; private set; }
        public Station SearchStation { get; private set; }
        public decimal SearchDistanceLy { get; set; }

        // Last query variables
        public QueryType LastQuery
        {
            get => _lastQuery;
            set
            {
                _lastQuery = value;
                OnPropertyChanged();
            }
        }
        private QueryType _lastQuery;

        public string LastQuerySystemArg
        {
            get => _lastQuerySystemArg;
            set
            {
                _lastQuerySystemArg = value; 
                OnPropertyChanged();
            }
        }
        private string _lastQuerySystemArg;

        public string LastQueryStationArg
        {
            get => _lastQueryStationArg;
            set
            {
                _lastQueryStationArg = value;
                OnPropertyChanged();
            }
        }
        private string _lastQueryStationArg;

        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                _isWorking = value;
                OnPropertyChanged();
            }
        }
        private bool _isWorking;

        public NavigationService(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);

            // Remember our last query
            var configuration = ConfigService.Instance.navigationMonitorConfiguration;
            if (Enum.TryParse(configuration.searchQuery, true, out QueryType queryType))
            {
                if (queryType.Group() != null)
                {
                    LastQuery = queryType;
                    LastQuerySystemArg = configuration.searchQuerySystemArg;
                    LastQueryStationArg = configuration.searchQueryStationArg;
                }
            }
        }

        public static NavigationService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No Navigation instance: creating one");
                            instance = new NavigationService(new StarMapService());
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary> Obtains the result from various navigation queries </summary>
        /// <param name="queryType">The type of query</param>
        /// <param name="systemArg">The query system argument</param>
        /// <param name="stationArg">The query station argument</param>
        /// <param name="distanceArg">The query distance argument</param>
        /// <param name="prioritizeOrbitalStationArg">The query prioritizeOrbitalStation argument</param>
        /// <returns>The query result</returns>
        public RouteDetailsEvent NavQuery(QueryType queryType, string systemArg = null, string stationArg = null, decimal? distanceArg = null, bool? prioritizeOrbitalStationArg = null)
        {
            IsWorking = true;
            RouteDetailsEvent result = null;

            try
            {
                // Resolve the current search query
                switch (queryType)
                {
                    // Services Searches
                    case QueryType.encoded:
                    case QueryType.facilitator:
                    case QueryType.guardian:
                    case QueryType.human:
                    case QueryType.manufactured:
                    case QueryType.raw:
                    {
                        result = GetServiceSystem(queryType, distanceArg is null ? (int?)null : Convert.ToInt32(Math.Round((decimal)distanceArg)), prioritizeOrbitalStationArg);
                        break;
                    }

                    // Route Management Searches
                    case QueryType.cancel:
                    {
                        result = CancelRoute();
                        break;
                    }
                    case QueryType.set:
                    {
                        result = SetRoute(systemArg, stationArg);
                        break;
                    }
                    case QueryType.update:
                    {
                        result = RefreshLastNavigationQuery();
                        break;
                    }

                    // Mission Route Searches
                    case QueryType.expiring:
                    {
                        result = GetExpiringMissionRoute();
                        break;
                    }
                    case QueryType.farthest:
                    {
                        result = GetFarthestMissionRoute();
                        break;
                    }
                    case QueryType.most:
                    {
                        result = GetMostMissionRoute(systemArg);
                        break;
                    }
                    case QueryType.nearest:
                    {
                        result = GetNearestMissionRoute();
                        break;
                    }
                    case QueryType.route:
                    {
                        result = GetRNNAMissionRoute(systemArg);
                        break;
                    }
                    case QueryType.source:
                    {
                        result = GetMissionCargoSourceRoute(systemArg);
                        break;
                    }

                    // Galaxy Searches
                    case QueryType.neutron:
                    {
                        result = GetNeutronRoute(systemArg);
                        break;
                    }
                    case QueryType.scoop:
                    {
                        result = GetNearestScoopSystem(distanceArg);
                        break;
                    }
                    default:
                    {
                        IsWorking = false;
                        Logging.Error($"{queryType} has not been configured in NavigationService.cs");
                        result = new RouteDetailsEvent(DateTime.UtcNow, queryType.ToString(), null, null, null, 0, null);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("Nav query failed", e);
                return null;
            }

            // Keep track of the query (excluding route management queries)
            if (result != null)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                if (queryType.Group() != null)
                {
                    LastQuery = queryType;
                    LastQuerySystemArg = systemArg;
                    LastQueryStationArg = stationArg;

                    // Save query data
                    navConfig.searchQuery = LastQuery.ToString();
                    navConfig.searchQuerySystemArg = LastQuerySystemArg;
                    navConfig.searchQueryStationArg = LastQueryStationArg;
                }

                // Save the route data
                navConfig.plottedRouteList = result.Route;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;

                // Update the global `SearchSystem` and `SearchStation` variables
                UpdateSearchData(result.system, result.station);
            }

            IsWorking = false;
            return result;
        }

        private RouteDetailsEvent CancelRoute()
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            // Save updated route data to the configuration
            navConfig.plottedRouteList.GuidanceEnabled = false;
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;

            // Update Voice Attack & Cottle variables
            EDDI.Instance.updateDestinationSystem(null);
            EDDI.Instance.DestinationDistanceLy = 0;

            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.cancel.ToString(), null, null, navConfig.plottedRouteList, navConfig.plottedRouteList.Waypoints.Count, null);
        }

        private RouteDetailsEvent SetRoute(string system, string station = null)
        {
            NavWaypointCollection navRouteList = new NavWaypointCollection();
            NavWaypoint firstUnvisitedWaypoint;
            // Use our saved route if a named system is not provided
            if (string.IsNullOrEmpty(system))
            {
                navRouteList = ConfigService.Instance.navigationMonitorConfiguration.plottedRouteList ?? new NavWaypointCollection();
                firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault(w => !w.visited);
                if (firstUnvisitedWaypoint != null)
                {
                    return new RouteDetailsEvent(DateTime.UtcNow, QueryType.set.ToString(), firstUnvisitedWaypoint.systemName, firstUnvisitedWaypoint.stationName, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint.missionids);
                }
            }
            
            // Disregard commands to set a route to the current star system.
            var curr = EDDI.Instance?.CurrentStarSystem;
            if (curr?.systemname == system) { return null; }

            // Set a course to a named system (and optionally station)
            var neutronRoute = NavQuery(QueryType.neutron, system);
            navRouteList = neutronRoute.Route;
            foreach (var wp in navRouteList.Waypoints)
            {
                wp.missionids = GetSystemMissionIds(wp.systemName);
            }
            firstUnvisitedWaypoint = navRouteList.Waypoints.FirstOrDefault(w => !w.visited);
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.set.ToString(), firstUnvisitedWaypoint?.systemName, firstUnvisitedWaypoint?.systemName == system ? station : null, navRouteList, navRouteList.Waypoints.Count, firstUnvisitedWaypoint?.missionids ?? new List<long>());
        }

        /// <summary> Route to the star system where missions shall expire first </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetExpiringMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system  
            string searchSystem = null;
            long expiringSeconds = 0;
            var navRouteList = new NavWaypointCollection();

            if (missions.Count > 0)
            {
                foreach (Mission mission in missions
                             .Where(m => m.statusEDName == "Active" 
                                         && m.expiry >= DateTime.UtcNow
                                         && !string.IsNullOrEmpty(m.destinationsystem))
                             .ToList())
                {
                    if (expiringSeconds == 0 || mission.expiryseconds < expiringSeconds)
                    {
                        expiringSeconds = mission.expiryseconds ?? 0;
                        searchSystem = mission.destinationsystem;
                    }
                }

                if (!string.IsNullOrEmpty(searchSystem))
                {
                    var curr = EDDI.Instance?.CurrentStarSystem;
                    var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem); // Destination star system

                    navRouteList.Waypoints.Add(new NavWaypoint(curr) { visited = true });
                    if (curr?.systemname != dest?.systemname)
                    {
                        navRouteList.Waypoints.Add(new NavWaypoint(dest) { visited = dest?.systemname == curr?.systemname });
                    }
                }

                // Get mission IDs for 'expiring' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.expiring.ToString(), searchSystem, null, navRouteList, expiringSeconds, missionids);
        }

        /// <summary> Route to the star system furthest from the current star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetFarthestMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            var navRouteList = new NavWaypointCollection();

            if (missions.Count > 0)
            {
                var curr = EDDI.Instance?.CurrentStarSystem;
                var farthestList = new SortedList<decimal, NavWaypoint>();
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems != null && mission.destinationsystems.Any())
                    {
                        foreach (var system in mission.destinationsystems)
                        {
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName); // Destination star system
                            decimal distance = CalculateDistance(curr, dest);
                            if (!farthestList.ContainsKey(distance))
                            {
                                farthestList.Add(distance, system);

                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem); // Destination star system
                        decimal distance = CalculateDistance(curr, dest);
                        if (!farthestList.ContainsKey(distance))
                        {
                            farthestList.Add(distance, new NavWaypoint(dest) {visited = dest.systemname == curr?.systemname});
                        }
                    }
                }
                // Farthest system is last in the list
                searchSystem = farthestList.Values.LastOrDefault()?.systemName;

                navRouteList.Waypoints.Add(new NavWaypoint(curr) { visited = true });
                if (curr?.systemname != farthestList.Values.LastOrDefault()?.systemName)
                {
                    navRouteList.Waypoints.Add(farthestList.Values.LastOrDefault());
                }

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.farthest.ToString(), searchSystem, null, navRouteList, missionids.Count, missionids);
        }

        /// <summary> Route that provides the shortest total travel path to complete all missions using the 'Repetitive Nearest Neighbor' Algorithm (RNNA) </summary>
        /// <param name="homeSystem"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetRNNAMissionRoute(string homeSystem = null)
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var sortedRoute = new List<NavWaypoint>();
            var navRouteList = new NavWaypointCollection();
            string searchSystem = null;
            int routeCount = 0;

            var systems = new List<string>();      // List of eligible mission destination systems
            var missionids = new List<long>();       // List of mission IDs for the next system

            var homeStarSystem = dataProviderService.GetSystemData(homeSystem, true, false, false, false, false);
            NavWaypoint homeSystemWaypoint = null;
            if (homeStarSystem != null)
            {
                homeSystemWaypoint = new NavWaypoint(homeStarSystem);
            }

            if (missions.Count > 0)
            {
                // Add current star system first
                var curr = EDDI.Instance?.CurrentStarSystem;
                systems.Add(curr?.systemname);

                // Add origin systems for 'return to origin' missions to the 'systems' list
                foreach (Mission mission in missions.Where(m => m.statusEDName != "Failed").ToList())
                {
                    if (mission.originreturn && !systems.Contains(mission.originsystem))
                    {
                        systems.Add(mission.originsystem);
                    }
                }

                // Add destination systems for applicable mission types to the 'systems' list
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    foreach (var type in mission.edTags)
                    {
                        var exitLoop = false;
                        switch (type.ToLowerInvariant())
                        {
                            case "assassinate":
                            case "courier":
                            case "delivery":
                            case "disable":
                            case "hack":
                            case "massacre":
                            case "passengerbulk":
                            case "passengervip":
                            case "rescue":
                            case "salvage":
                            case "scan":
                            case "sightseeing":
                            case "smuggle":
                                {
                                    if (!(mission.destinationsystems?.Any() ?? false))
                                    {
                                        if (!string.IsNullOrEmpty(mission.destinationsystem) && !systems.Contains(mission.destinationsystem))
                                        {
                                            systems.Add(mission.destinationsystem);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var system in mission.destinationsystems)
                                        {
                                            if (!systems.Contains(system.systemName))
                                            {
                                                systems.Add(system.systemName);
                                            }
                                        }
                                    }
                                    exitLoop = true;
                                    break;
                                }
                        }
                        if (exitLoop) { break; }
                    }
                }

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithm (RNNA)
                var navWaypoints = dataProviderService.GetSystemsData(systems.ToArray(), true, false, false, false, false).Select(s => new NavWaypoint(s)).ToList();
                if (CalculateRNNA(navWaypoints, missions, out sortedRoute, homeSystemWaypoint))
                {
                    // Prepend our current system to the route if it is not already present
                    if (EDDI.Instance?.CurrentStarSystem != null 
                        && sortedRoute.FirstOrDefault()?.systemAddress != (EDDI.Instance.CurrentStarSystem.systemAddress ?? 0))
                    {
                        sortedRoute = sortedRoute.Prepend(new NavWaypoint(EDDI.Instance.CurrentStarSystem)).ToList();
                        sortedRoute[0].visited = true;
                    }

                    navRouteList = new NavWaypointCollection(sortedRoute);
                    searchSystem = navRouteList.Waypoints.FirstOrDefault(w => !w.visited)?.systemName;
                    routeCount = navRouteList.Waypoints.Count;

                    Logging.Debug("Calculated Route Selected = " + string.Join(", ", sortedRoute.Select(w => w.systemName)) + ", Total Distance = " + navRouteList.RouteDistance);

                    // Get mission IDs for 'search' system
                    missionids = GetSystemMissionIds(searchSystem);
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.route.ToString(), searchSystem, null, navRouteList, routeCount, missionids);
        }

        private bool CalculateRNNA(List<NavWaypoint> inputSystems, List<Mission> missions, out List<NavWaypoint> outputRoute, NavWaypoint homeSystem = null)
        {
            var found = false;
            outputRoute = new List<NavWaypoint>();

            var numSystems = inputSystems.Count;
            if (numSystems > 1)
            {
                var bestRoute = new List<NavWaypoint>();
                var bestDistance = 0M;

                // Pre-load all system distances
                if (homeSystem != null)
                {
                    inputSystems.Add(homeSystem);
                }
                var distMatrix = new decimal[inputSystems.Count][];
                for (int i = 0; i < inputSystems.Count; i++)
                {
                    distMatrix[i] = new decimal[inputSystems.Count];
                }
                for (int i = 0; i < inputSystems.Count - 1; i++)
                {
                    var curr = inputSystems.Find(s => s.systemName == inputSystems[i].systemName);
                    for (int j = i + 1; j < inputSystems.Count; j++)
                    {
                        var dest = inputSystems.Find(s => s.systemName == inputSystems[j].systemName);
                        var distance = Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0;
                        distMatrix[i][j] = distance;
                        distMatrix[j][i] = distance;
                    }
                }

                // Repetitive Nearest Neighbor Algorithm (RNNA)
                // Iterate through all possible routes by changing the starting system
                for (int i = 0; i < numSystems; i++)
                {
                    // If starting system is a destination for a 'return to origin' mission, then not a viable route
                    if (DestinationOriginReturn(inputSystems[i].systemName, missions)) { continue; }

                    var route = new List<NavWaypoint>();
                    var totalDistance = 0M;
                    int currIndex = i;

                    // Repeat until all systems (except starting system) are in the route
                    while (route.Count < numSystems - 1)
                    {
                        var nearestList = new SortedList<decimal, int>();

                        // Iterate through the remaining systems to find nearest neighbor
                        for (int j = 1; j < numSystems; j++)
                        {
                            // Wrap around the list
                            int destIndex = i + j < numSystems ? i + j : i + j - numSystems;
                            if (homeSystem != null && destIndex == 0) { destIndex = numSystems; }

                            // Check if destination system previously added to the route
                            if (route.IndexOf(inputSystems[destIndex]) == -1)
                            {
                                decimal distance = distMatrix[currIndex][destIndex];
                                if (!nearestList.ContainsKey(distance))
                                {
                                    nearestList.Add(distance, destIndex);
                                }
                            }
                        }
                        // Set the 'Nearest' system as the new 'current' system
                        currIndex = nearestList.Values.FirstOrDefault();

                        // Add 'nearest' system to the route list and add its distance to total distance traveled
                        route.Add(inputSystems[currIndex]);
                        totalDistance += nearestList.Keys.FirstOrDefault();
                    }

                    // Add 'starting system' to complete the route & add its distance to total distance traveled
                    int startIndex = homeSystem != null && i == 0 ? numSystems : i;
                    route.Add(inputSystems[startIndex]);
                    if (currIndex == numSystems) { currIndex = 0; }
                    totalDistance += distMatrix[currIndex][startIndex];
                    Logging.Debug("Build Route Iteration #" + i + " - Route = " + string.Join("_", route) + ", Total Distance = " + totalDistance);

                    // Use this route if total distance traveled is less than previous iterations
                    if (bestDistance == 0 || totalDistance < bestDistance)
                    {
                        bestRoute.Clear();
                        int homeIndex = route.IndexOf(inputSystems[homeSystem != null ? numSystems : 0]);
                        if (homeIndex < route.Count - 1)
                        {
                            // Rotate list to place homesystem at the end
                            bestRoute = route.Skip(homeIndex + 1)
                                .Concat(route.Take(homeIndex + 1))
                                .ToList();
                        }
                        else
                        {
                            bestRoute = route.ToList();
                        }
                    }
                }

                if (bestRoute.Count == numSystems)
                {
                    // Filter any repetitive systems in the route
                    outputRoute = bestRoute
                        .GroupBy(r => r.systemAddress)
                        .Select(r => r.First())
                        .ToList();
                    found = true;
                }
            }
            return found;
        }

        /// <summary> Route to the star system that provides the most active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMostMissionRoute(string targetSystemName)
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            long mostCount = 0;
            var navRouteList = new NavWaypointCollection();

            if (missions.Count > 0)
            {
                // Determine the number of missions per individual system
                List<string> systems = new List<string>();  // Mission systems
                List<int> systemsCount = new List<int>();   // Count of missions per system
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems?.Any() ?? false)
                    {
                        foreach (var system in mission.destinationsystems)
                        {
                            int index = systems.IndexOf(system.systemName);
                            if (index == -1)
                            {
                                systems.Add(system.systemName);
                                systemsCount.Add(1);
                            }
                            else
                            {
                                systemsCount[index] += 1;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        int index = systems.IndexOf(mission.destinationsystem);
                        if (index == -1)
                        {
                            systems.Add(mission.destinationsystem);
                            systemsCount.Add(1);
                        }
                        else
                        {
                            systemsCount[index] += 1;
                        }
                    }
                }

                // Sort the 'most' systems by distance
                var mostList = new SortedList<decimal, StarSystem>();   // List of 'most' systems, sorted by distance
                mostCount = systemsCount.Max(); 
                var curr = !string.IsNullOrEmpty(targetSystemName) 
                    ? StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(targetSystemName, true, false) 
                    : EDDI.Instance?.CurrentStarSystem;
                for (int i = 0; i < systems.Count; i++)
                {
                    if (systemsCount[i] == mostCount)
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systems[i]); // Destination star system
                        if (dest?.x != null)
                        {
                            mostList.Add(CalculateDistance(curr, dest), dest);
                        }
                    }
                }

                // Nearest 'most' system is first in the list
                searchSystem = mostList.Values.FirstOrDefault()?.systemname;

                navRouteList.Waypoints.Add(new NavWaypoint(curr) { visited = true });
                if (curr?.systemname != mostList.Values.FirstOrDefault()?.systemname)
                {
                    navRouteList.Waypoints.Add(new NavWaypoint(mostList.Values.FirstOrDefault()) { visited = mostList.Values.FirstOrDefault()?.systemname == curr?.systemname });
                }

                // Get mission IDs for 'most' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.most.ToString(), searchSystem, null, navRouteList, mostCount, missionids);
        }

        /// <summary> Route to the nearest star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            var navRouteList = new NavWaypointCollection();

            var curr = EDDI.Instance?.CurrentStarSystem;     // Current star system
            if (missions.Count > 0)
            {
                var nearestList = new SortedList<decimal, StarSystem>();
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems != null && mission.destinationsystems.Any())
                    {
                        foreach (var system in mission.destinationsystems)
                        {
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName); // Destination star system
                            decimal distance = CalculateDistance(curr, dest);
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, dest);

                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem); // Destination star system
                        decimal distance = CalculateDistance(curr, dest);
                        if (!nearestList.ContainsKey(distance))
                        {
                            nearestList.Add(distance, dest);
                        }
                    }
                }
                // Nearest system is first in the list
                searchSystem = nearestList.Values.FirstOrDefault()?.systemname;

                navRouteList.Waypoints.Add(new NavWaypoint(curr) { visited = true });
                if (curr?.systemname != nearestList.Values.FirstOrDefault()?.systemname)
                {
                    navRouteList.Waypoints.Add(new NavWaypoint(nearestList.Values.FirstOrDefault()) { visited = nearestList.Values.FirstOrDefault()?.systemname == curr?.systemname });
                }

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.nearest.ToString(), searchSystem, null, navRouteList, missionids.Count, missionids);
        }

        /// <summary> Route to the nearest star system that is eligible for fuel scoop refueling </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestScoopSystem(decimal? searchRadius = null)
        {
            if (searchRadius is null)
            {
                searchRadius = EDDI.Instance.CurrentShip?.JumpDetails("total")?.distance ?? 100;
            }

            string searchSystem = null;
            int searchCount = 0;
            int searchIncrement = (int)Math.Ceiling(Math.Min((decimal)searchRadius, 100) / 4);
            var navRouteList = new NavWaypointCollection();

            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                if (currentSystem.scoopable)
                {
                    searchSystem = currentSystem.systemname;
                    navRouteList.Waypoints.Add(new NavWaypoint(currentSystem) { visited = true });
                    searchCount = 1;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        int startRadius = i * searchIncrement;
                        var endRadius = (i + 1) * searchIncrement;
                        var sphereSystems = edsmService.GetStarMapSystemsSphere(currentSystem.systemname, startRadius, endRadius);
                        sphereSystems = sphereSystems.Where(kvp => (kvp["system"] as StarSystem)?.scoopable ?? false).ToList();
                        searchCount = sphereSystems.Count;
                        if (searchCount > 0)
                        {
                            var nearestList = new SortedList<decimal, StarSystem>();
                            foreach (Dictionary<string, object> system in sphereSystems)
                            {
                                decimal distance = (decimal)system["distance"];
                                if (!nearestList.ContainsKey(distance))
                                {
                                    nearestList.Add(distance, system["system"] as StarSystem);
                                }
                            }

                            // Nearest 'scoopable' system
                            searchSystem = nearestList.Values.FirstOrDefault()?.systemname;

                            // Update the navRouteList
                            navRouteList.Waypoints.Add(new NavWaypoint(currentSystem) { visited = true });
                            if (currentSystem.systemname != nearestList.Values.FirstOrDefault()?.systemname)
                            {
                                navRouteList.Waypoints.Add(new NavWaypoint(nearestList.Values.FirstOrDefault()) { visited = nearestList.Values.FirstOrDefault()?.systemname == currentSystem.systemname });
                            }

                            break;
                        }
                    }
                }
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.scoop.ToString(), searchSystem, null, navRouteList, searchCount, null);
        }

        /// <summary> Obtains a neutron star route between the current star system and a named star system </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNeutronRoute(string targetSystemName, bool is_supercharged = false, bool use_supercharge = true, bool use_injections = false, bool exclude_secondary = false)
        {
            var plottedRouteList = new NavWaypointCollection();
                var currentSystemName = EDDI.Instance.CurrentStarSystem.systemname;
            if (EDDI.Instance.CurrentStarSystem == null)
            {
                Logging.Debug("Neutron route plotting is not available, current star system is unknown.");
            }
            else if (string.IsNullOrEmpty(targetSystemName))
            {
                Logging.Debug("Neutron route plotting is not available, target star system is unknown.");
            }
            else if (targetSystemName == currentSystemName)
            {
                Logging.Debug("Neutron route plotting is not available, the target star system name matches the current star system.");
            }
            else
            {
                var cargoCarriedTons = ConfigService.Instance.cargoMonitorConfiguration.cargocarried;
                var shipID = ConfigService.Instance.shipMonitorConfiguration.currentshipid;
                var ship = ConfigService.Instance.shipMonitorConfiguration.shipyard.FirstOrDefault(s =>
                    s.LocalId == shipID);
                var spanshService = new SpanshService();
                plottedRouteList = spanshService.GetGalaxyRoute(currentSystemName, targetSystemName, ship, cargoCarriedTons,
                    is_supercharged, use_supercharge, use_injections, exclude_secondary);
            }

            if (plottedRouteList == null || plottedRouteList.Waypoints.Count <= 1) { return null; }

            // Sanity check - if we're already navigating to the plotted route destination then the number of jumps
            // must be equal or less then the already plotted route and the total route distance must be less also.
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if (plottedRouteList.Waypoints.LastOrDefault()?.systemAddress ==
                config.navRouteList.Waypoints.LastOrDefault()?.systemAddress
                && (plottedRouteList.Waypoints.Count >= config.navRouteList.Waypoints.Count))
            {
                plottedRouteList = config.navRouteList;
            }

            plottedRouteList.Waypoints.First().visited = true;
            var searchSystem = plottedRouteList.Waypoints[1].systemName;

            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.neutron.ToString(), searchSystem, null, plottedRouteList, plottedRouteList.Waypoints.Count, null);
        }

        /// <summary> Route to the nearest star system that offers a specific service </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetServiceSystem(QueryType serviceQuery, int? maxDistanceOverride = null, bool? prioritizeOrbitalStationsOverride = null)
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            int maxStationDistance = maxDistanceOverride ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            bool prioritizeOrbitalStations = prioritizeOrbitalStationsOverride ?? navConfig.prioritizeOrbitalStations;

            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                var shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                if (ServiceFilters.TryGetValue(serviceQuery, out var filter))
                {
                    var ServiceStarSystem =
                        GetServiceSystem(serviceQuery, maxStationDistance, prioritizeOrbitalStations);
                    if (ServiceStarSystem != null)
                    {
                        var searchSystem = ServiceStarSystem;

                        // Filter stations which meet the game version and landing pad size requirements
                        var ServiceStations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons
                            ? ServiceStarSystem.stations
                            : ServiceStarSystem.orbitalstations
                                .Where(s => s.stationservices.Count > 0).ToList();
                        ServiceStations = ServiceStations.Where(s => s.distancefromstar <= maxStationDistance).ToList();
                        if (serviceQuery == QueryType.facilitator)
                        {
                            ServiceStations = ServiceStations.Where(s => s.LandingPadCheck(shipSize)).ToList();
                        }

                        ServiceStations = ServiceStations.Where(s => s.stationServices.Contains(filter.service))
                            .ToList();

                        // Build list to find the station nearest to the main star
                        var nearestList = new SortedList<decimal, string>();
                        foreach (var station in ServiceStations)
                        {
                            if (!nearestList.ContainsKey(station.distancefromstar ?? 0))
                            {
                                nearestList.Add(station.distancefromstar ?? 0, station.name);
                            }
                        }

                        // Station is nearest to the main star which meets the service query
                        var searchStation = nearestList.Values.FirstOrDefault();

                        // Update the navRouteList
                        var navRouteList = new NavWaypointCollection();
                        navRouteList.Waypoints.Add(new NavWaypoint(currentSystem) { visited = true });
                        if (currentSystem.systemname != searchSystem.systemname)
                        {
                            navRouteList.Waypoints.Add(new NavWaypoint(searchSystem)
                            {
                                visited = searchSystem.systemname == currentSystem.systemname,
                                stationName = searchStation
                            });
                        }

                        // Get mission IDs for 'service' system 
                        var missionids = GetSystemMissionIds(searchSystem.systemname);

                        return new RouteDetailsEvent(DateTime.UtcNow, serviceQuery.ToString(), searchSystem.systemname, searchStation, navRouteList, missionids.Count, missionids);
                    }
                }
                else
                {
                    Logging.Error($"No navigation query filter found for query type {serviceQuery}.");
                }
            }
            else
            {
                Logging.Warn("Unable to obtain navigation service result - current star system is not known.");
            }
            return null;
        }

        public StarSystem GetServiceSystem(QueryType serviceQuery, int maxStationDistance, bool prioritizeOrbitalStations)
        {
            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the filter parameters
                var shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                if (ServiceFilters.TryGetValue(serviceQuery, out ServiceFilter filter))
                {
                    int cubeLy = filter.cubeLy;

                    //
                    var checkedSystems = new List<string>();
                    var maxTries = 5;

                    while (maxTries > 0)
                    {
                        var cubeSystems =
                            edsmService.GetStarMapSystemsCube(currentSystem.systemname, cubeLy);
                        if (cubeSystems?.Any() ?? false)
                        {
                            // Filter systems using search parameters
                            cubeSystems = cubeSystems.Where(s => s.population >= filter.population).ToList();
                            cubeSystems = cubeSystems
                                .Where(s => filter.security.Contains(s.securityLevel.invariantName)).ToList();
                            if (serviceQuery != QueryType.facilitator)
                            {
                                cubeSystems = cubeSystems
                                    .Where(s => filter.econ.Contains(s.Economies
                                        .FirstOrDefault(e => e.invariantName != "None")?.invariantName))
                                    .ToList();
                            }

                            // Retrieve systems in current radius which have not been previously checked
                            var systemNames =
                                cubeSystems.Select(s => s.systemname).Except(checkedSystems).ToList();
                            if (systemNames.Count > 0)
                            {
                                var StarSystems =
                                    StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames.ToArray(),
                                        true, false);
                                checkedSystems.AddRange(systemNames);

                                var nearestList = new SortedList<decimal, string>();
                                foreach (var starsystem in StarSystems)
                                {
                                    // Filter stations within the system which meet the station type prioritization,
                                    // max distance from the main star, game version, and landing pad size requirements
                                    var stations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons
                                        ? starsystem.stations
                                        : starsystem.orbitalstations
                                            .Where(s => s.stationservices.Count > 0).ToList();
                                    stations = stations.Where(s => s.distancefromstar <= maxStationDistance).ToList();
                                    if (serviceQuery == QueryType.facilitator)
                                    {
                                        stations = stations.Where(s => s.LandingPadCheck(shipSize)).ToList();
                                    }

                                    int stationCount = stations.Count(s => s.stationServices.Contains(filter.service));

                                    // Build list to find the 'service' system nearest to the current system, meeting station requirements
                                    if (stationCount > 0)
                                    {
                                        decimal distance = CalculateDistance(currentSystem, starsystem);
                                        if (!nearestList.ContainsKey(distance))
                                        {
                                            nearestList.Add(distance, starsystem.systemname);
                                        }
                                    }
                                }

                                // Nearest 'service' system
                                var ServiceSystem = nearestList.Values.FirstOrDefault();
                                if (ServiceSystem != null)
                                {
                                    return StarSystems.FirstOrDefault(s => s.systemname == ServiceSystem);
                                }
                            }
                        }

                        // Increase search radius in 10 Ly increments (up to 50 Ly)
                        // until the required 'service' is found
                        cubeLy += 10;
                        maxTries--;
                    }
                }
            }
            return null;
        }

        /// <summary> Route to the nearest star system that can be used to source active mission cargo </summary>
        /// <param name="system"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMissionCargoSourceRoute(string system = null)
        {
            var cargoConfig = ConfigService.Instance.cargoMonitorConfiguration;
            var inventory = cargoConfig.cargo.ToList();
            var missionsCount = inventory.Sum(c => c.haulageData.Count);
            StarSystem searchSystem = null;
            int systemsCount = 0;
            var missionids = new List<long>();       // List of mission IDs for the next system
            var sourceList = new SortedList<long, StarSystem>();
            var navRouteList = new NavWaypointCollection();

            if (missionsCount > 0)
            {
                var curr = EDDI.Instance?.CurrentStarSystem;
                var currentSystem = curr?.systemname;
                var fromHere = system == currentSystem;

                if (curr != null)
                {
                    foreach (Cargo cargo in inventory.Where(c => c.haulageData.Any()).ToList())
                    {
                        foreach (Haulage haulage in cargo.haulageData
                                     .Where(h => h.status == "Active" && h.sourcesystem != null).ToList())
                        {
                            if (fromHere && haulage.originsystem != currentSystem)
                            {
                                break;
                            }

                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(haulage.sourcesystem);
                            long distance = (long)(Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0 * 100);
                            if (!sourceList.TryGetValue(distance, out var _))
                            {
                                sourceList.Add(distance, dest);
                            }

                            missionids.Add(haulage.missionid);
                        }
                    }

                    searchSystem = sourceList.Values.FirstOrDefault();
                    systemsCount = sourceList.Count;

                    // Update the navRouteList
                    navRouteList.Waypoints.Add(new NavWaypoint(curr) { visited = true });
                    if (curr.systemname != searchSystem?.systemname)
                    {
                        navRouteList.Waypoints.Add(new NavWaypoint(searchSystem) { visited = searchSystem?.systemname == curr.systemname });
                    }
                }
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.source.ToString(), searchSystem?.systemname, null, navRouteList, systemsCount, missionids);
        }

        /// <summary> Repeat the last mission query and return an updated result if different from the prior result, either relative to your current location or to a named system </summary>
        /// <returns> The star system result from the repeated query </returns>
        private RouteDetailsEvent RefreshLastNavigationQuery()
        {
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if (!config.plottedRouteList.GuidanceEnabled) { return null; }

            if (LastQuery.Group() == QueryGroup.missions)
            {
                var missionsList = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
                if (missionsList
                    .Where(m => m.statusDef == MissionStatus.FromEDName("Active"))
                    .Any(m => m.destinationsystem == EDDI.Instance.CurrentStarSystem?.systemname))
                {
                    // We still have active missions at the current location
                    return null;
                }
            }

            var currentPlottedRoute = ConfigService.Instance.navigationMonitorConfiguration.plottedRouteList;
            if (currentPlottedRoute.Waypoints.All(w => w.visited))
            {
                // The current route has already been completed
                return null;
            }

            var currentStarSystem = EDDI.Instance.CurrentStarSystem;
            var currentWaypoint = currentPlottedRoute.Waypoints.FirstOrDefault(w => w.systemAddress == currentStarSystem?.systemAddress);
            var nextWaypoint = currentWaypoint is null ? null : currentPlottedRoute.Waypoints.FirstOrDefault(w => !w.visited && w.index > currentWaypoint.index);
            if (nextWaypoint != null)
            {
                // We're still following the plotted route
                return new RouteDetailsEvent(DateTime.UtcNow, QueryType.update.ToString(), nextWaypoint.systemName, nextWaypoint.stationName, currentPlottedRoute, currentPlottedRoute.Waypoints.Count, nextWaypoint.missionids);
            }

            // Recalculate the route
            Enum.TryParse(config.searchQuery, out QueryType lastQuery);
            var @event = NavQuery(lastQuery, config.searchQuerySystemArg, config.searchQuerySystemArg, config.maxSearchDistanceFromStarLs, config.prioritizeOrbitalStations);
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.UtcNow, QueryType.recalculating.ToString(), @event.system, @event.station, @event.Route, @event.count, @event.missionids));
            return new RouteDetailsEvent(DateTime.UtcNow, config.searchQuery, @event.system, @event.station, @event.Route, @event.count, @event.missionids);
        }

        private decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            if (curr is null || dest is null) { return 0; }
            return Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0;
        }

        private bool DestinationOriginReturn(string destination, List<Mission> missions)
        {
            foreach (Mission mission in missions.Where(m => m.originreturn).ToList())
            {
                if (mission.destinationsystems == null)
                {
                    if (mission.destinationsystem == destination)
                    {
                        return true;
                    }
                }
                else
                {
                    var system = mission.destinationsystems.FirstOrDefault(ds => ds.systemName == destination);
                    if (system != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private List<long> GetSystemMissionIds(string system)
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the system

            if (system != null)
            {
                // Get mission IDs associated with the system
                foreach (Mission mission in missions.Where(m => m.destinationsystem == system
                    || (m.originreturn && m.originsystem == system)).ToList())
                {
                    missionids.Add(mission.missionid);
                }
            }
            return missionids;
        }

        private void UpdateSearchData(string searchSystem, string searchStation)
        {
            // Update search system data
            if (!string.IsNullOrEmpty(searchSystem))
            {
                StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem);

                //Ignore null & empty systems
                if (system != null)
                {
                    if (system.systemname != SearchStarSystem?.systemname)
                    {
                        Logging.Debug("Search star system is " + system.systemname);
                        SearchStarSystem = system;
                    }
                    // Update search system distance
                    SearchDistanceLy = CalculateDistance(EDDI.Instance.CurrentStarSystem, system);
                }
            }
            else
            {
                SearchStarSystem = null;
            }

            // Update search station data
            if (!string.IsNullOrEmpty(searchStation) && SearchStarSystem?.stations != null)
            {
                string searchStationName = searchStation.Trim();
                Station station = SearchStarSystem.stations.FirstOrDefault(s => s.name == searchStationName);
                if (station != null)
                {
                    if (station.name != SearchStation?.name)
                    {
                        Logging.Debug("Search station is " + station.name);
                        SearchStation = station;
                    }
                }
            }
            else
            {
                SearchStation = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
