using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiShipMonitor;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiNavigationService
{
    public class NavigationService
    {
        private CargoMonitorConfiguration cargoConfig => ConfigService.Instance.cargoMonitorConfiguration;
        private MissionMonitorConfiguration missionsConfig => ConfigService.Instance.missionMonitorConfiguration;
        private NavigationMonitorConfiguration navConfig => ConfigService.Instance.navigationMonitorConfiguration;

        private static readonly Dictionary<string, dynamic> ServiceFilter = new Dictionary<string, dynamic>()
        {
            { "encoded", new {
                econ = new List<string>() {"High Tech", "Military"},
                population = 1000000,
                security = new List<string>() {"Medium", "High"},
                service = StationService.FromName("Material Trader"),
                cubeLy = 40}
            },
            { "facilitator", new {
                econ = new List<string>(),
                population = 0,
                security = new List<string>() {"Low"},
                service = StationService.FromName("Interstellar Factors Contact"),
                cubeLy = 25}
            },
            { "manufactured", new {
                econ = new List<string>() {"Industrial"},
                population = 1000000,
                security = new List<string>() {"Medium", "High"},
                service = StationService.FromName("Material Trader"),
                cubeLy = 40}
            },
            { "raw", new {
                econ = new List<string>() {"Extraction", "Refinery"},
                population = 1000000,
                security = new List<string>() {"Medium", "High"},
                service = StationService.FromName("Material Trader"),
                cubeLy = 40}
            },
            { "guardian", new {
                econ = new List<string>() {"High Tech"},
                population = 10000000,
                security = new List<string>()  {"High"},
                service = StationService.FromName("Technology Broker"),
                cubeLy = 80}
            },
            { "human", new {
                econ = new List<string>() {"Industrial"},
                population = 10000000,
                security = new List<string>() {"High"},
                service = StationService.FromName("Technology Broker"),
                cubeLy = 80}
            }
        };

        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;
        private static NavigationService instance;
        private static readonly object instanceLock = new object();

        // Search variables
        private QueryTypes SearchQuery { get; set; }
        private dynamic[] SearchQueryArgs { get; set; }
        public StarSystem SearchStarSystem { get; private set; }
        public Station SearchStation { get; private set; }
        public decimal SearchDistanceLy { get; set; }

        // Last mission query variables
        private QueryTypes LastMissionQuery { get; set; }
        private dynamic[] LastMissionQueryArgs { get; set; }
        private StarSystem LastMissionStarSystem { get; set; }
        private Station LastMissionStation { get; set; }

        public NavigationService(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);
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
        public RouteDetailsEvent NavQuery(QueryTypes queryType, dynamic[] args = null)
        {
            try
            {
                // Keep track of the last mission query (excluding `update` queries)
                switch (queryType)
                {
                    case QueryTypes.expiring:
                    case QueryTypes.farthest:
                    case QueryTypes.most:
                    case QueryTypes.nearest:
                    case QueryTypes.route:
                    case QueryTypes.source:
                    {
                        LastMissionQuery = queryType;
                        LastMissionQueryArgs = args;
                        break;
                    }
                }
                
                // Note the current search query
                SearchQuery = queryType;
                SearchQueryArgs = args;

                // Resolve the current search query
                switch (queryType)
                {
                    case QueryTypes.encoded:
                    {
                        return GetServiceSystem("encoded");
                    }
                    case QueryTypes.expiring:
                    {
                        var result = GetExpiringMissionSystem();
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.facilitator:
                    {
                        return GetServiceSystem("facilitator");
                    }
                    case QueryTypes.farthest:
                    {
                        var result = GetFarthestMissionSystem();
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.guardian:
                    {
                        return GetServiceSystem("guardian");
                    }
                    case QueryTypes.human:
                    {
                        return GetServiceSystem("human");
                    }
                    case QueryTypes.manufactured:
                    {
                        return GetServiceSystem("manufactured");
                    }
                    case QueryTypes.most:
                    {
                        var result = GetMostMissionSystem();
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.nearest:
                    {
                        var result = GetNearestMissionSystem();
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.raw:
                    {
                        return GetServiceSystem("raw");
                    }
                    case QueryTypes.route:
                    {
                        string system = null;
                        if (args?.Length > 0)
                        {
                            system = args[0].ToString();
                        }
                        var result = GetShortestPathMissionSystem(system);
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.scoop:
                    {
                        decimal? distance;
                        if (args?.Length > 0 && (decimal)args[0] > 0)
                        {
                            distance = (decimal)args[0];
                        }
                        else
                        {
                            distance = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.JumpDetails("total")?.distance;
                        }
                        return GetNearestScoopSystem(distance ?? 100);
                    }
                    case QueryTypes.source:
                    {
                        string system = null;
                        if (args?.Length > 0)
                        {
                            system = args[0].ToString();
                        }
                        var result = GetMissionCargoSource(string.IsNullOrEmpty(system) ? null : system);
                        LastMissionStarSystem = SearchStarSystem;
                        LastMissionStation = SearchStation;
                        return result;
                    }
                    case QueryTypes.update:
                    {
                        return RefreshLastMissionQuery();
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

        /// <summary> Obtains the star system where missions shall expire first </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetExpiringMissionSystem()
        {
            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system  
            string searchSystem = null;
            decimal searchDistance = 0;
            long expiringSeconds = 0;

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = new StarSystem();             // Destination star system

                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (expiringSeconds == 0 || mission.expiryseconds < expiringSeconds)
                    {
                        expiringSeconds = mission.expiryseconds ?? 0;
                        searchSystem = mission.destinationsystem;
                    }
                }
                dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                searchDistance = CalculateDistance(curr, dest);

                // Save the missions route data
                navConfig.searchQuery = "expiring";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'expiring' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.Now, "expiring", searchSystem, null, searchSystem, expiringSeconds, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the star system furthest from the current star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetFarthestMissionSystem()
        {
            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = new StarSystem();             // Destination star system

                SortedList<decimal, string> farthestList = new SortedList<decimal, string>();
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems != null && mission.destinationsystems.Any())
                    {
                        foreach (DestinationSystem system in mission.destinationsystems)
                        {
                            dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.name, true);
                            decimal distance = CalculateDistance(curr, dest);
                            if (!farthestList.ContainsKey(distance))
                            {
                                farthestList.Add(distance, system.name);

                            }
                        }
                    }
                    else if (mission.destinationsystem != string.Empty)
                    {
                        dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem, true);
                        decimal distance = CalculateDistance(curr, dest);
                        if (!farthestList.ContainsKey(distance))
                        {
                            farthestList.Add(distance, mission.destinationsystem);
                        }
                    }
                }
                // Farthest system is last in the list
                searchSystem = farthestList.Values.LastOrDefault();
                searchDistance = farthestList.Keys.LastOrDefault();

                // Save the route data to the configuration
                navConfig.searchQuery = "farthest";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.Now, "farthest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the star system that provides the shortest total travel path to complete all missions using the 'Repetitive Nearest Neighbor' Algorithm (RNNA) </summary>
        /// <param name="homeSystem"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetShortestPathMissionSystem(string homeSystem = null)
        {
            var missions = missionsConfig.missions.ToList();
            var systemsRoute = new List<string>();
            decimal routeDistance = 0;
            string searchSystem = null;
            decimal searchDistance = 0;
            int routeCount = 0;

            List<string> systems = new List<string>();      // List of eligible mission destination systems
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missions.Count > 0)
            {
                // Add current star system first
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
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
                                        if (!systems.Contains(mission.destinationsystem))
                                        {
                                            systems.Add(mission.destinationsystem);
                                        }
                                    }
                                    else
                                    {
                                        foreach (DestinationSystem system in mission.destinationsystems)
                                        {
                                            if (!systems.Contains(system.name))
                                            {
                                                systems.Add(system.name);
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
                if (CalculateRNNA(systems, homeSystem, missions, ref systemsRoute, ref routeDistance))
                {
                    searchSystem = systemsRoute[0];
                    StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                    searchDistance = CalculateDistance(curr, dest);
                    routeCount = systemsRoute.Count;

                    Logging.Debug("Calculated Route Selected = " + systemsRoute + ", Total Distance = " + routeDistance);

                    // Save the route data to the configuration
                    navConfig.searchQuery = "route";
                    navConfig.searchSystem = searchSystem;
                    navConfig.searchStation = null;
                    navConfig.searchDistance = searchDistance;
                    navConfig.ToFile();
                    UpdateSearchData(searchSystem, null, searchDistance);

                    // Get mission IDs for 'search' system
                    missionids = GetSystemMissionIds(searchSystem);
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
            }
            return new RouteDetailsEvent(DateTime.Now, "route", searchSystem, null, string.Join("_", systemsRoute), routeCount, searchDistance, routeDistance, missionids);
        }

        private bool CalculateRNNA(List<string> systems, string homeSystem, List<Mission> missions, ref List<string> systemsRoute, ref decimal routeDistance)
        {
            bool found = false;
            systemsRoute = null;
            routeDistance = 0;

            int numSystems = systems.Count();
            if (numSystems > 1)
            {
                List<string> bestRoute = new List<string>();
                decimal bestDistance = 0;

                // Pre-load all system distances
                if (homeSystem != null)
                {
                    systems.Add(homeSystem);
                }
                List<StarSystem> starsystems = dataProviderService.GetSystemsData(systems.ToArray(), true, false, false, false, false);
                decimal[][] distMatrix = new decimal[systems.Count][];
                for (int i = 0; i < systems.Count; i++)
                {
                    distMatrix[i] = new decimal[systems.Count];
                }
                for (int i = 0; i < systems.Count - 1; i++)
                {
                    StarSystem curr = starsystems.Find(s => s.systemname == systems[i]);
                    for (int j = i + 1; j < systems.Count; j++)
                    {
                        StarSystem dest = starsystems.Find(s => s.systemname == systems[j]);
                        decimal distance = CalculateDistance(curr, dest);
                        distMatrix[i][j] = distance;
                        distMatrix[j][i] = distance;
                    }
                }

                // Repetitive Nearest Neighbor Algorithm (RNNA)
                // Iterate through all possible routes by changing the starting system
                for (int i = 0; i < numSystems; i++)
                {
                    // If starting system is a destination for a 'return to origin' mission, then not a viable route
                    if (DestinationOriginReturn(systems[i], missions)) { continue; }

                    List<string> route = new List<string>();
                    decimal totalDistance = 0;
                    int currIndex = i;

                    // Repeat until all systems (except starting system) are in the route
                    while (route.Count() < numSystems - 1)
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
        private RouteDetailsEvent GetMostMissionSystem()
        {
            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;
            long mostCount = 0;

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = new StarSystem();             // Destination star system

                // Determine the number of missions per individual system
                List<string> systems = new List<string>();  // Mission systems
                List<int> systemsCount = new List<int>();   // Count of missions per system
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems?.Any() ?? false)
                    {
                        foreach (DestinationSystem system in mission.destinationsystems)
                        {
                            int index = systems.IndexOf(system.name);
                            if (index == -1)
                            {
                                systems.Add(system.name);
                                systemsCount.Add(1);
                            }
                            else
                            {
                                systemsCount[index] += 1;
                            }
                        }
                    }
                    else if (mission.destinationsystem != string.Empty)
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
                SortedList<decimal, string> mostList = new SortedList<decimal, string>();   // List of 'most' systems, sorted by distance
                mostCount = systemsCount.Max();
                for (int i = 0; i < systems.Count(); i++)
                {
                    if (systemsCount[i] == mostCount)
                    {
                        dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systems[i], true);
                        if (dest?.x != null)
                        {
                            mostList.Add(CalculateDistance(curr, dest), systems[i]);
                        }
                    }
                }

                // Nearest 'most' system is first in the list
                searchSystem = mostList.Values.FirstOrDefault();
                searchDistance = mostList.Keys.FirstOrDefault();

                // Save the route data to the configuration
                navConfig.searchQuery = "most";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'most' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.Now, "most", searchSystem, null, searchSystem, mostCount, searchDistance, searchDistance, missionids);
        }

        /// <summary> Obtains the nearest star system with active missions </summary>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetNearestMissionSystem()
        {
            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string searchSystem = null;
            decimal searchDistance = 0;

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;     // Current star system
                StarSystem dest = new StarSystem();                     // Destination star system

                SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems != null && mission.destinationsystems.Any())
                    {
                        foreach (DestinationSystem system in mission.destinationsystems)
                        {
                            dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system.name, true);
                            decimal distance = CalculateDistance(curr, dest);
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, system.name);

                            }
                        }
                    }
                    else if (mission.destinationsystem != string.Empty)
                    {
                        dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(mission.destinationsystem, true);
                        decimal distance = CalculateDistance(curr, dest);
                        if (!nearestList.ContainsKey(distance))
                        {
                            nearestList.Add(distance, mission.destinationsystem);
                        }
                    }
                }
                // Nearest system is first in the list
                searchSystem = nearestList.Values.FirstOrDefault();
                searchDistance = nearestList.Keys.FirstOrDefault();

                // Save the route data to the configuration
                navConfig.searchQuery = "nearest";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            return new RouteDetailsEvent(DateTime.Now, "nearest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids);
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

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    int startRadius = i * searchIncrement;
                    endRadius = (i + 1) * searchIncrement;
                    List<Dictionary<string, object>> sphereSystems = edsmService.GetStarMapSystemsSphere(currentSystem.systemname, startRadius, endRadius);
                    sphereSystems = sphereSystems.Where(kvp => (kvp["system"] as StarSystem)?.scoopable ?? false).ToList();
                    searchCount = sphereSystems.Count;
                    if (searchCount > 0)
                    {
                        SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                        foreach (Dictionary<string, object> system in sphereSystems)
                        {
                            decimal distance = (decimal)system["distance"];
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, (system["system"] as StarSystem)?.systemname);
                            }
                        }

                        // Nearest 'scoopable' system
                        searchSystem = nearestList.Values.FirstOrDefault();
                        searchDistance = nearestList.Keys.FirstOrDefault();

                        break;
                    }
                }

                // Save the route data to the configuration
                navConfig.searchQuery = "scoop";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);
            }
            return new RouteDetailsEvent(DateTime.Now, "scoop", searchSystem, null, searchSystem, searchCount, searchDistance, endRadius, null);
        }

        /// <summary> Obtains the nearest star system that offers a specific service </summary>
        /// <returns> The query result </returns>
        public RouteDetailsEvent GetServiceSystem(string serviceQuery, int? maxDistance = null)
        {
            // Get up-to-date configuration data
            int maxStationDistance = maxDistance ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            bool prioritizeOrbitalStations = navConfig.prioritizeOrbitalStations;

            string searchSystem = null;
            string searchStation = null;
            decimal searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                ServiceFilter.TryGetValue(serviceQuery, out dynamic filter);

                StarSystem ServiceStarSystem = GetServiceSystem(serviceQuery, maxStationDistance, prioritizeOrbitalStations);
                if (ServiceStarSystem != null)
                {
                    searchSystem = ServiceStarSystem.systemname;
                    searchDistance = CalculateDistance(currentSystem, ServiceStarSystem);

                    // Filter stations which meet the game version and landing pad size requirements
                    List<Station> ServiceStations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons ? ServiceStarSystem.stations : ServiceStarSystem.orbitalstations
                        .Where(s => s.stationservices.Count > 0).ToList();
                    ServiceStations = ServiceStations.Where(s => s.distancefromstar <= maxStationDistance).ToList();
                    if (serviceQuery == "facilitator") { ServiceStations = ServiceStations.Where(s => s.LandingPadCheck(shipSize)).ToList(); }
                    ServiceStations = ServiceStations.Where(s => s.stationServices.Contains(filter.service)).ToList();

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
                    searchStation = nearestList.Values.FirstOrDefault();

                    // Save the route data to the configuration
                    navConfig.searchQuery = serviceQuery;
                    navConfig.searchSystem = searchSystem;
                    navConfig.searchStation = searchStation;
                    navConfig.searchDistance = searchDistance;
                    navConfig.ToFile();
                    UpdateSearchData(searchSystem, searchStation, searchDistance);

                    // Get mission IDs for 'service' system
                    missionids = GetSystemMissionIds(searchSystem);
                }
            }
            return new RouteDetailsEvent(DateTime.Now, serviceQuery, searchSystem, searchStation, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids);
        }

        private StarSystem GetServiceSystem(string serviceQuery, int maxStationDistance, bool prioritizeOrbitalStations)
        {
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the filter parameters
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.Size ?? LandingPadSize.Large;
                ServiceFilter.TryGetValue(serviceQuery, out dynamic filter);
                int cubeLy = filter.cubeLy;

                //
                List<string> checkedSystems = new List<string>();
                string ServiceSystem = null;
                int maxTries = 5;

                while (maxTries > 0)
                {
                    List<StarSystem> cubeSystems = edsmService.GetStarMapSystemsCube(currentSystem.systemname, cubeLy);
                    if (cubeSystems?.Any() ?? false)
                    {
                        // Filter systems using search parameters
                        cubeSystems = cubeSystems.Where(s => s.population >= filter.population).ToList();
                        cubeSystems = cubeSystems.Where(s => filter.security.Contains(s.securityLevel.invariantName)).ToList();
                        if (serviceQuery != "facilitator")
                        {
                            cubeSystems = cubeSystems
                                .Where(s => filter.econ.Contains(s.Economies.FirstOrDefault(e => e.invariantName != "None")?.invariantName))
                                .ToList();
                        }

                        // Retrieve systems in current radius which have not been previously checked
                        List<string> systemNames = cubeSystems.Select(s => s.systemname).Except(checkedSystems).ToList();
                        if (systemNames.Count > 0)
                        {
                            List<StarSystem> StarSystems = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames.ToArray(), true, false);
                            checkedSystems.AddRange(systemNames);

                            SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                            foreach (StarSystem starsystem in StarSystems)
                            {
                                // Filter stations within the system which meet the station type prioritization,
                                // max distance from the main star, game version, and landing pad size requirements
                                List<Station> stations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons ? starsystem.stations : starsystem.orbitalstations
                                     .Where(s => s.stationservices.Count > 0).ToList();
                                stations = stations.Where(s => s.distancefromstar <= maxStationDistance).ToList();
                                if (serviceQuery == "facilitator") { stations = stations.Where(s => s.LandingPadCheck(shipSize)).ToList(); }
                                int stationCount = stations.Where(s => s.stationServices.Contains(filter.service)).Count();

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
                            ServiceSystem = nearestList.Values.FirstOrDefault();
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
            return null;
        }

        /// <summary> Obtains the nearest star system that can be used to source active mission cargo </summary>
        /// <param name="system"> (Optional) If set, calculate relative to the named starting system rather than the current system </param>
        /// <returns> The query result </returns>
        private RouteDetailsEvent GetMissionCargoSource(string system = null)
        {
            List<Cargo> inventory = cargoConfig.cargo.ToList();
            int missionsCount = inventory.Sum(c => c.haulageData.Count());
            string searchSystem = null;
            decimal searchDistance = 0;
            string sourceSystems = null;
            int systemsCount = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missionsCount > 0)
            {
                var sourceList = new SortedList<long, string>();
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                string currentSystem = curr?.systemname;
                bool fromHere = system == currentSystem;

                foreach (Cargo cargo in inventory.Where(c => c.haulageData.Any()).ToList())
                {
                    foreach (Haulage haulage in cargo.haulageData.Where(h => h.status == "Active" && h.sourcesystem != null).ToList())
                    {
                        if (fromHere && haulage.originsystem != currentSystem)
                        {
                            break;
                        }

                        StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(haulage.sourcesystem, true);
                        long distance = (long)((CalculateDistance(curr, dest)) * 100);
                        if (!sourceList.TryGetValue(distance, out string val))
                        {
                            sourceList.Add(distance, haulage.sourcesystem);
                        }
                        missionids.Add(haulage.missionid);
                    }
                }

                searchSystem = sourceList.Values.FirstOrDefault();
                searchDistance = (decimal)sourceList.Keys.FirstOrDefault() / 100;
                sourceSystems = string.Join("_", sourceList.Values);
                systemsCount = sourceList.Count;

                // Save the route data to the configuration
                navConfig.searchQuery = "source";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.ToFile();
                UpdateSearchData(searchSystem, null, searchDistance);
            }
            return new RouteDetailsEvent(DateTime.Now, "source", searchSystem, null, sourceSystems, systemsCount, searchDistance, 0, missionids);
        }

        /// <summary> Repeat the last mission query and return an updated result if different from the prior result, either relative to your current location or to a named system </summary>
        /// <returns> The star system result from the repeated query </returns>
        private RouteDetailsEvent RefreshLastMissionQuery()
        {
            var @event = NavQuery(LastMissionQuery, LastMissionQueryArgs);
            if (LastMissionStarSystem?.systemname == SearchStarSystem?.systemname)
            {
                // Same star system as last result
                if (LastMissionStation?.name == SearchStation?.name)
                {
                    // No station or same station as last result
                    return null;
                }
            }
            return new RouteDetailsEvent(DateTime.Now, "update", @event.system, @event.station, @event.route, @event.count, @event.distance, @event.routedistance, @event.missionids);
        }

        private decimal CalculateDistance(string currentSystem, string destinationSystem)
        {
            StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(currentSystem, true);
            StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(destinationSystem, true);
            return CalculateDistance(curr, dest);
        }

        private decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
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
                    DestinationSystem system = mission.destinationsystems.FirstOrDefault(ds => ds.name == destination);
                    if (system != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<long> GetSystemMissionIds(string system)
        {
            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the system

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

        public void UpdateSearchData(string searchSystem, string searchStation, decimal searchDistance)
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

        public void UpdateSearchDistance(string starSystem, DateTime updateDat)
        {
            if (SearchStarSystem is null) { return; }

            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(starSystem);
            SearchDistanceLy = CalculateDistance(system, SearchStarSystem);
            navConfig.searchDistance = SearchDistanceLy;
            navConfig.updatedat = updateDat;
            navConfig.ToFile();

            Logging.Debug("Distance from search system is " + SearchDistanceLy);
        }
    }
}
