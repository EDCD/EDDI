using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using EddiStatusMonitor;
using Utilities;

namespace EddiNavigationService
{
    public class NavigationService
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
        private Status currentStatus;

        // Search variables
        public StarSystem SearchStarSystem { get; private set; }
        public Station SearchStation { get; private set; }
        public decimal SearchDistanceLy { get; set; }

        // Last mission query variables
        private QueryType LastMissionQuery { get; set; }
        private dynamic[] LastMissionQueryArgs { get; set; }
        private StarSystem LastUpdateMissionStarSystem { get; set; }
        private Station LastUpdateMissionStation { get; set; }

        public NavigationService(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);

            // Remember our last query
            var configuration = ConfigService.Instance.navigationMonitorConfiguration;
            if (Enum.TryParse(configuration.searchQuery, true, out QueryType queryType))
            {
                switch (queryType)
                {
                    case QueryType.expiring:
                    case QueryType.farthest:
                    case QueryType.most:
                    case QueryType.nearest:
                    case QueryType.route:
                    case QueryType.source:
                    {
                        LastMissionQuery = queryType;
                        LastMissionQueryArgs = configuration.searchQueryArgs;
                        break;
                    }
                }
            }

            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
        }

        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                LockManager.GetLock(nameof(currentStatus), () => { currentStatus = status; });
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
        /// <param name="args">The query arguments</param>
        /// <returns>The query result</returns>
        public RouteDetailsEvent NavQuery(QueryType queryType, dynamic[] args = null)
        {
            try
            {
                // Keep track of the last mission query (excluding `update` queries)
                switch (queryType)
                {
                    case QueryType.expiring:
                    case QueryType.farthest:
                    case QueryType.most:
                    case QueryType.nearest:
                    case QueryType.route:
                    case QueryType.source:
                    {
                        LastMissionQuery = queryType;
                        LastMissionQueryArgs = args;
                        break;
                    }
                }

                // Resolve the current search query
                switch (queryType)
                {
                    case QueryType.cancel:
                    {
                        return CancelRoute();
                    }
                    case QueryType.encoded:
                    {
                        return GetServiceSystem(QueryType.encoded);
                    }
                    case QueryType.expiring:
                    {
                        var result = GetExpiringMissionRoute();
                        return result;
                    }
                    case QueryType.facilitator:
                    {
                        return GetServiceSystem(QueryType.facilitator);
                    }
                    case QueryType.farthest:
                    {
                        var result = GetFarthestMissionRoute();
                        return result;
                    }
                    case QueryType.guardian:
                    {
                        return GetServiceSystem(QueryType.guardian);
                    }
                    case QueryType.human:
                    {
                        return GetServiceSystem(QueryType.human);
                    }
                    case QueryType.manufactured:
                    {
                        return GetServiceSystem(QueryType.manufactured);
                    }
                    case QueryType.most:
                    {
                        var result = GetMostMissionRoute();
                        return result;
                    }
                    case QueryType.nearest:
                    {
                        var result = GetNearestMissionRoute();
                        return result;
                    }
                    case QueryType.raw:
                    {
                        return GetServiceSystem(QueryType.raw);
                    }
                    case QueryType.route:
                    {
                        string system = null;
                        if (!(args?[0] is null))
                        {
                            system = args[0].ToString();
                        }

                        var result = GetRNNAMissionRoute(string.IsNullOrEmpty(system) ? null : system);
                        return result;
                    }
                    case QueryType.scoop:
                    {
                        decimal? distance;
                        if (args?[0] != null && (decimal?)args[0] > 0)
                        {
                            distance = (decimal)(args[0] ?? 0);
                        }
                        else
                        {
                            distance = JumpCalcs.JumpDetails("total", EDDI.Instance.CurrentShip,
                                currentStatus.fuelInTanks,
                                ConfigService.Instance.cargoMonitorConfiguration.cargocarried)?.distance;
                        }

                        return GetNearestScoopSystem(distance ?? 100);
                    }
                    case QueryType.set:
                    {
                        string system = null;
                        if (!(args?[0] is null))
                        {
                            system = args[0].ToString();
                        }

                        string station = null;
                        if (!(args?[1] is null))
                        {
                            station = args[1].ToString();
                        }

                        return SetRoute(system, station);
                    }
                    case QueryType.source:
                    {
                        string system = null;
                        if (!(args?[0] is null))
                        {
                            system = args[0].ToString();
                        }

                        var result = GetMissionCargoSourceRoute(string.IsNullOrEmpty(system) ? null : system);
                        return result;
                    }
                    case QueryType.update:
                    {
                        return RefreshLastNavigationQuery();
                    }
                    case QueryType.None:
                    {
                        return null;
                    }
                    default:
                    {
                        throw new ArgumentException($"{queryType} has not been configured in NavigationService.cs");
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("Nav query failed", e);
                return null;
            }
        }

        private RouteDetailsEvent CancelRoute()
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            // Save updated route data to the configuration
            navConfig.plottedRouteList = null;
            navConfig.routeGuidanceEnabled = false;
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;

            // Update Voice Attack & Cottle variables
            EDDI.Instance.updateDestinationSystem(null);
            EDDI.Instance.DestinationDistanceLy = 0;

            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.cancel.ToString(), null, null, null, 0, 0, 0, null);
        }

        private RouteDetailsEvent SetRoute(string system, string station = null)
        {
            // Get up-to-date configuration data
            decimal distance = 0;

            if (system != null)
            {
                var curr = EDDI.Instance?.CurrentStarSystem;
                var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, true);
                if (curr?.x != null && dest?.x != null)
                {
                    if (system != curr.systemname) { distance = CalculateDistance(curr, dest); }
                }
                else
                {
                    system = null;
                    station = null;
                }
            }

            // Get mission IDs for 'set' system
            List<long> missionids = GetSystemMissionIds(system);

            ConfigService.Instance.navigationMonitorConfiguration.routeGuidanceEnabled = true;
            var routeList = ConfigService.Instance.navigationMonitorConfiguration.plottedRouteList ?? new List<NavWaypoint>();
            var routeDistance = ConfigService.Instance.navigationMonitorConfiguration.plottedRouteDistance;
            var count = routeList.Count;

            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.set.ToString(), system, station, routeList, count, distance, routeDistance, missionids);
        }

        /// <summary> Obtains the star system where missions shall expire first </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetExpiringMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system  
            string searchSystem = null;
            decimal searchDistance = 0;
            long expiringSeconds = 0;
            var navRouteList = new List<NavWaypoint>();

            if (missions.Count > 0)
            {
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active" && !string.IsNullOrEmpty(m.destinationsystem)).ToList())
                {
                    if (expiringSeconds == 0 || mission.expiryseconds < expiringSeconds)
                    {
                        expiringSeconds = mission.expiryseconds ?? 0;
                        searchSystem = mission.destinationsystem;
                    }
                }

                var curr = EDDI.Instance?.CurrentStarSystem;
                var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true); // Destination star system
                searchDistance = CalculateDistance(curr, dest);

                // Save the missions route data
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.expiring.ToString();
                navConfig.searchQueryArgs = null;
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                navRouteList.Add(new NavWaypoint(curr) {visited = true}); 
                navRouteList.Add(new NavWaypoint(dest));

                // Get mission IDs for 'expiring' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.expiring.ToString(), searchSystem, null, navRouteList, expiringSeconds, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the star system furthest from the current star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetFarthestMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;
            var navRouteList = new List<NavWaypoint>();

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
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName, true); // Destination star system
                            decimal distance = CalculateDistance(curr, dest);
                            if (!farthestList.ContainsKey(distance))
                            {
                                farthestList.Add(distance, system);

                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem, true); // Destination star system
                        decimal distance = CalculateDistance(curr, dest);
                        if (!farthestList.ContainsKey(distance))
                        {
                            farthestList.Add(distance, new NavWaypoint(dest));
                        }
                    }
                }
                // Farthest system is last in the list
                searchSystem = farthestList.Values.LastOrDefault()?.systemName;
                searchDistance = farthestList.Keys.LastOrDefault();

                // Save the route data to the configuration
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.farthest.ToString();
                navConfig.searchQueryArgs = null;
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                navRouteList.Add(new NavWaypoint(curr) { visited = true });
                navRouteList.Add(farthestList.Values.LastOrDefault());

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.farthest.ToString(), searchSystem, null, navRouteList, missionids.Count, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the star system that provides the shortest total travel path to complete all missions using the 'Repetitive Nearest Neighbor' Algorithm (RNNA) </summary>
        /// <param name="homeSystem"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetRNNAMissionRoute(string homeSystem = null)
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var systemsRoute = new List<NavWaypoint>();
            decimal routeDistance = 0;
            string searchSystem = null;
            decimal searchDistance = 0;
            int routeCount = 0;

            var systems = new List<string>();      // List of eligible mission destination systems
            var missionids = new List<long>();       // List of mission IDs for the next system

            var homeStarSystem = new NavWaypoint(dataProviderService.GetSystemData(homeSystem, true, false, false, false, false));

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
                if (CalculateRNNA(navWaypoints, homeStarSystem, missions, ref systemsRoute, ref routeDistance))
                {
                    searchSystem = systemsRoute[0].systemName;
                    StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                    searchDistance = CalculateDistance(curr, dest);
                    routeCount = systemsRoute.Count;

                    Logging.Debug("Calculated Route Selected = " + systemsRoute + ", Total Distance = " + routeDistance);

                    // Save the route data to the configuration
                    var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                    navConfig.searchQuery = QueryType.route.ToString();
                    navConfig.searchQueryArgs = new dynamic[]{ homeSystem };
                    navConfig.searchSystem = searchSystem;
                    navConfig.searchStation = null;
                    navConfig.searchDistance = searchDistance;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    UpdateSearchData(searchSystem, null, searchDistance);

                    // Get mission IDs for 'search' system
                    missionids = GetSystemMissionIds(searchSystem);
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.route.ToString(), searchSystem, null, systemsRoute, routeCount, searchDistance, routeDistance, missionids);
        }

        private bool CalculateRNNA(List<NavWaypoint> systems, NavWaypoint homeSystem, List<Mission> missions, ref List<NavWaypoint> systemsRoute, ref decimal routeDistance)
        {
            bool found = false;

            int numSystems = systems.Count();
            if (numSystems > 1)
            {
                var bestRoute = new List<NavWaypoint>();
                decimal bestDistance = 0;

                // Pre-load all system distances
                if (homeSystem != null)
                {
                    systems.Add(homeSystem);
                }
                decimal[][] distMatrix = new decimal[systems.Count][];
                for (int i = 0; i < systems.Count; i++)
                {
                    distMatrix[i] = new decimal[systems.Count];
                }
                for (int i = 0; i < systems.Count - 1; i++)
                {
                    var curr = systems.Find(s => s.systemName == systems[i].systemName);
                    for (int j = i + 1; j < systems.Count; j++)
                    {
                        var dest = systems.Find(s => s.systemName == systems[j].systemName);
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
                    if (DestinationOriginReturn(systems[i].systemName, missions)) { continue; }

                    var route = new List<NavWaypoint>();
                    decimal totalDistance = 0;
                    int currIndex = i;

                    // Repeat until all systems (except starting system) are in the route
                    while (route.Count < numSystems - 1)
                    {
                        SortedList<decimal, int> nearestList = new SortedList<decimal, int>();

                        // Iterate through the remaining systems to find nearest neighbor
                        for (int j = 1; j < numSystems; j++)
                        {
                            // Wrap around the list
                            int destIndex = i + j < numSystems ? i + j : i + j - numSystems;
                            if (homeSystem != null && destIndex == 0) { destIndex = numSystems; }

                            // Check if destination system previously added to the route
                            if (route.IndexOf(systems[destIndex]) == -1)
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
                        route.Add(systems[currIndex]);
                        totalDistance += nearestList.Keys.FirstOrDefault();
                    }

                    // Add 'starting system' to complete the route & add its distance to total distance traveled
                    int startIndex = homeSystem != null && i == 0 ? numSystems : i;
                    route.Add(systems[startIndex]);
                    if (currIndex == numSystems) { currIndex = 0; }
                    totalDistance += distMatrix[currIndex][startIndex];
                    Logging.Debug("Build Route Iteration #" + i + " - Route = " + string.Join("_", route) + ", Total Distance = " + totalDistance);

                    // Use this route if total distance traveled is less than previous iterations
                    if (bestDistance == 0 || totalDistance < bestDistance)
                    {
                        bestRoute.Clear();
                        int homeIndex = route.IndexOf(systems[homeSystem != null ? numSystems : 0]);
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
                        bestDistance = totalDistance;
                    }
                }

                if (bestRoute.Count == numSystems)
                {
                    systemsRoute = bestRoute;
                    routeDistance = bestDistance;
                    found = true;
                }
            }
            return found;
        }

        /// <summary> Obtains the star system that provides the most active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMostMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;
            long mostCount = 0;
            var navRouteList = new List<NavWaypoint>();

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
                var curr = EDDI.Instance?.CurrentStarSystem;
                for (int i = 0; i < systems.Count(); i++)
                {
                    if (systemsCount[i] == mostCount)
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systems[i], true); // Destination star system
                        if (dest?.x != null)
                        {
                            mostList.Add(CalculateDistance(curr, dest), dest);
                        }
                    }
                }

                // Nearest 'most' system is first in the list
                searchSystem = mostList.Values.FirstOrDefault()?.systemname;
                searchDistance = mostList.Keys.FirstOrDefault();

                // Save the route data to the configuration
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.most.ToString();
                navConfig.searchQueryArgs = null;
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                navRouteList.Add(new NavWaypoint(curr) { visited = true });
                navRouteList.Add(new NavWaypoint(mostList.Values.FirstOrDefault()));

                // Get mission IDs for 'most' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.most.ToString(), searchSystem, null, navRouteList, mostCount, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the nearest star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestMissionRoute()
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;
            var navRouteList = new List<NavWaypoint>();

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
                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.systemName, true); // Destination star system
                            decimal distance = CalculateDistance(curr, dest);
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, dest);

                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(mission.destinationsystem))
                    {
                        var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem, true); // Destination star system
                        decimal distance = CalculateDistance(curr, dest);
                        if (!nearestList.ContainsKey(distance))
                        {
                            nearestList.Add(distance, dest);
                        }
                    }
                }
                // Nearest system is first in the list
                searchSystem = nearestList.Values.FirstOrDefault()?.systemname;
                searchDistance = nearestList.Keys.FirstOrDefault();

                // Save the route data to the configuration
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.nearest.ToString();
                navConfig.searchQueryArgs = null;
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                navRouteList.Add(new NavWaypoint(curr) { visited = true });
                navRouteList.Add(new NavWaypoint(nearestList.Values.FirstOrDefault()));

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.nearest.ToString(), searchSystem, null, navRouteList, missionids.Count, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the nearest star system that is eligible for fuel scoop refueling </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestScoopSystem(decimal searchRadius)
        {
            string searchSystem = null;
            decimal searchDistance = 0;
            int searchCount = 0;
            int searchIncrement = (int)Math.Ceiling(Math.Min(searchRadius, 100) / 4);
            int endRadius = 0;
            var navRouteList = new List<NavWaypoint>();

            var currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    int startRadius = i * searchIncrement;
                    endRadius = (i + 1) * searchIncrement;
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
                        searchDistance = nearestList.Keys.FirstOrDefault();

                        // Update the navRouteList
                        navRouteList.Add(new NavWaypoint(currentSystem) { visited = true });
                        navRouteList.Add(new NavWaypoint(nearestList.Values.FirstOrDefault()));

                        break;
                    }
                }

                // Save the route data to the configuration
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.scoop.ToString();
                navConfig.searchQueryArgs = new dynamic[] { searchRadius };
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.scoop.ToString(), searchSystem, null, navRouteList, searchCount, searchDistance, endRadius, null);
        }

        /// <summary> Obtains the nearest star system that offers a specific service </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetServiceSystem(QueryType serviceQuery, int? maxDistanceOverride = null, bool? prioritizeOrbitalStationsOverride = null)
        {
            // Get up-to-date configuration data
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            int maxStationDistance = maxDistanceOverride ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            bool prioritizeOrbitalStations = prioritizeOrbitalStationsOverride ?? navConfig.prioritizeOrbitalStations;
            var navRouteList = new List<NavWaypoint>();

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                if (ServiceFilters.TryGetValue(serviceQuery, out ServiceFilter filter))
                {
                    StarSystem ServiceStarSystem =
                        GetServiceSystem(serviceQuery, maxStationDistance, prioritizeOrbitalStations);
                    if (ServiceStarSystem != null)
                    {
                        var searchSystem = ServiceStarSystem;
                        var searchDistance = CalculateDistance(currentSystem, ServiceStarSystem);

                        // Filter stations which meet the game version and landing pad size requirements
                        List<Station> ServiceStations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons
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
                        SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                        foreach (Station station in ServiceStations)
                        {
                            if (!nearestList.ContainsKey(station.distancefromstar ?? 0))
                            {
                                nearestList.Add(station.distancefromstar ?? 0, station.name);
                            }
                        }

                        // Station is nearest to the main star which meets the service query
                        var searchStation = nearestList.Values.FirstOrDefault();

                        // Update the navRouteList
                        navRouteList.Add(new NavWaypoint(currentSystem) { visited = true });
                        navRouteList.Add(new NavWaypoint(searchSystem));

                        // Save the route data to the configuration
                        navConfig.searchQuery = serviceQuery.ToString();
                        navConfig.searchQueryArgs = null;
                        navConfig.searchSystem = searchSystem.systemname;
                        navConfig.searchStation = searchStation;
                        navConfig.searchDistance = searchDistance;
                        ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                        UpdateSearchData(searchSystem.systemname, searchStation, searchDistance);

                        // Get mission IDs for 'service' system 
                        var missionids = GetSystemMissionIds(searchSystem.systemname);

                        return new RouteDetailsEvent(DateTime.UtcNow, serviceQuery.ToString(), searchSystem.systemname, searchStation, navRouteList, missionids.Count, searchDistance, searchDistance, missionids);
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

        private StarSystem GetServiceSystem(QueryType serviceQuery, int maxStationDistance, bool prioritizeOrbitalStations)
        {
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the filter parameters
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                if (ServiceFilters.TryGetValue(serviceQuery, out ServiceFilter filter))
                {
                    int cubeLy = filter.cubeLy;

                    //
                    var checkedSystems = new List<string>();
                    var maxTries = 5;

                    while (maxTries > 0)
                    {
                        List<StarSystem> cubeSystems =
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
                            List<string> systemNames =
                                cubeSystems.Select(s => s.systemname).Except(checkedSystems).ToList();
                            if (systemNames.Count > 0)
                            {
                                List<StarSystem> StarSystems =
                                    StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames.ToArray(),
                                        true, false);
                                checkedSystems.AddRange(systemNames);

                                SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                                foreach (StarSystem starsystem in StarSystems)
                                {
                                    // Filter stations within the system which meet the station type prioritization,
                                    // max distance from the main star, game version, and landing pad size requirements
                                    List<Station> stations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons
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

        /// <summary> Obtains the nearest star system that can be used to source active mission cargo </summary>
        /// <param name="system"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMissionCargoSourceRoute(string system = null)
        {
            var cargoConfig = ConfigService.Instance.cargoMonitorConfiguration;
            var inventory = cargoConfig.cargo.ToList();
            var missionsCount = inventory.Sum(c => c.haulageData.Count);
            StarSystem searchSystem = null;
            decimal searchDistance = 0;
            int systemsCount = 0;
            var missionids = new List<long>();       // List of mission IDs for the next system
            var sourceList = new SortedList<long, StarSystem>();
            var navRouteList = new List<NavWaypoint>();

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

                            var dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(haulage.sourcesystem, true);
                            long distance = (long)(Functions.StellarDistanceLy(curr.x, curr.y, curr.z, dest.x, dest.y, dest.z) ?? 0 * 100);
                            if (!sourceList.TryGetValue(distance, out var _))
                            {
                                sourceList.Add(distance, dest);
                            }

                            missionids.Add(haulage.missionid);
                        }
                    }
                }

                searchSystem = sourceList.Values.FirstOrDefault();
                searchDistance = (decimal)sourceList.Keys.FirstOrDefault() / 100;
                systemsCount = sourceList.Count;

                // Update the navRouteList
                navRouteList.Add(new NavWaypoint(curr) { visited = true });
                navRouteList.Add(new NavWaypoint(searchSystem));

                // Save the route data to the configuration
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                navConfig.searchQuery = QueryType.source.ToString();
                navConfig.searchQueryArgs = new dynamic[] { system };
                navConfig.searchSystem = searchSystem?.systemname;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem?.systemname, null, searchDistance);
            }
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.source.ToString(), searchSystem?.systemname, null, navRouteList, systemsCount, searchDistance, 0, missionids);
        }

        /// <summary> Repeat the last mission query and return an updated result if different from the prior result, either relative to your current location or to a named system </summary>
        /// <returns> The star system result from the repeated query </returns>
        private RouteDetailsEvent RefreshLastNavigationQuery()
        {
            var missionsList = ConfigService.Instance.missionMonitorConfiguration.missions.ToList();
            if (missionsList
                .Where(m => m.statusDef == MissionStatus.FromEDName("Active"))
                .Any(m => m.destinationsystem == EDDI.Instance.CurrentStarSystem?.systemname))
            {
                // We still have active missions at the current location
                LastUpdateMissionStarSystem = null;
                LastUpdateMissionStation = null;
                return null;
            }
            if (LastMissionQuery is QueryType.None) { LastMissionQuery = QueryType.route; }
            var @event = NavQuery(LastMissionQuery, LastMissionQueryArgs);
            if (LastUpdateMissionStarSystem?.systemname == SearchStarSystem?.systemname
                && LastUpdateMissionStation?.name == SearchStation?.name)
            {
                // Same result as last time. Suppress the repetition.
                return null;
            }
            LastUpdateMissionStarSystem = SearchStarSystem;
            LastUpdateMissionStation = SearchStation;
            return new RouteDetailsEvent(DateTime.UtcNow, QueryType.update.ToString(), @event.system, @event.station, @event.Route, @event.count, @event.distance, @event.routedistance, @event.missionids);
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

        private void UpdateSearchData(string searchSystem, string searchStation, decimal searchDistance)
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

            // Update search system distance
            SearchDistanceLy = searchDistance;
        }
    }
}
