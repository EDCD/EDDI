using Eddi;
using EddiConfigService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using EddiCore;
using Utilities;

namespace EddiNavigationService
{
    public class NavigationService
    {
        private static Dictionary<string, dynamic> ServiceFilter = new Dictionary<string, dynamic>()
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

        private CargoMonitorConfiguration cargoConfig = new CargoMonitorConfiguration();
        private MissionMonitorConfiguration missionsConfig = new MissionMonitorConfiguration();
        private NavigationMonitorConfiguration navConfig = new NavigationMonitorConfiguration();

        private readonly IEdsmService edsmService;
        private readonly DataProviderService dataProviderService;
        private static NavigationService instance;
        private static readonly object instanceLock = new object();

        // Search variables
        public StarSystem SearchStarSystem { get; private set; }
        public Station SearchStation { get; private set; }
        public decimal SearchDistanceLy { get; set; }

        public NavigationService(IEdsmService edsmService)
        {
            this.edsmService = edsmService;
            dataProviderService = new DataProviderService(edsmService);
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
        }

        private static NavigationService instance;
        private static readonly object instanceLock = new object();

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

        public void CancelRoute()
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            string destination = navConfig.navDestination;

            // Save the route data to the configuration
            navConfig.searchQuery = "cancel";
            navConfig.searchSystem = null;
            navConfig.searchStation = null;
            navConfig.searchDistance = 0;
            navConfig.missionsRouteList = null;
            navConfig.missionsRouteDistance = 0;
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;

            // Update Voice Attack & Cottle variables
            UpdateSearchData(null, null, 0);
            EDDI.Instance.updateDestinationSystem(null);
            EDDI.Instance.DestinationDistanceLy = 0;


            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "cancel", destination, null, null, 0, 0, 0, null));
        }

        public string GetExpiringRoute()
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

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

                // Save the misisons route data
                navConfig.searchQuery = "expiring";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                navConfig.missionsRouteList = null;
                navConfig.missionsRouteDistance = 0;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'expiring' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "expiring", searchSystem, null, searchSystem, expiringSeconds, searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetFarthestRoute()
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

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
                navConfig.missionsRouteList = null;
                navConfig.missionsRouteDistance = 0;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "farthest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetMissionsRoute(string homeSystem = null)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

            List<Mission> missions = missionsConfig.missions.ToList();
            string missionsRouteList = null;
            decimal missionsRouteDistance = 0;
            string searchSystem = null;
            decimal searchDistance = 0;
            int routeCount = 0;

            List<string> systems = new List<string>();      // List of eligible mission destintaion systems
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
                    string type = mission.typeEDName.ToLowerInvariant();
                    switch (type)
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
                            }
                            break;
                    }
                }

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithim (RNNA)
                if (CalculateRNNA(systems, homeSystem, missions, ref missionsRouteList, ref missionsRouteDistance))
                {
                    searchSystem = missionsRouteList?.Split('_')[0];
                    StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                    searchDistance = CalculateDistance(curr, dest);
                    routeCount = missionsRouteList.Split('_').Count();

                    Logging.Debug("Calculated Route Selected = " + missionsRouteList + ", Total Distance = " + missionsRouteDistance);

                    // Save the route data to the configuration
                    navConfig.missionsRouteList = missionsRouteList;
                    navConfig.missionsRouteDistance = missionsRouteDistance;
                    navConfig.searchQuery = "route";
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
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "route", searchSystem, null, missionsRouteList, routeCount, searchDistance, missionsRouteDistance, missionids));
            return searchSystem;
        }

        public bool CalculateRNNA(List<string> systems, string homeSystem, List<Mission> missions, ref string missionsRouteList, ref decimal missionsRouteDistance)
        {
            bool found = false;
            missionsRouteList = null;
            missionsRouteDistance = 0;

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
                    missionsRouteList = string.Join("_", bestRoute);
                    missionsRouteDistance = bestDistance;
                    found = true;
                }
            }
            return found;
        }
        public string GetMostRoute(string homeSystem = null)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

            List<Mission> missions = missionsConfig.missions.ToList();
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string missionsRouteList = null;
            decimal missionsRouteDistance = 0;
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

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithim (RNNA)
                mostList.Add(0, curr?.systemname);
                if (CalculateRNNA(mostList.Values.ToList(), homeSystem, missions, ref missionsRouteList, ref missionsRouteDistance))
                {
                    Logging.Debug("Calculated Route Selected = " + missionsRouteList + ", Total Distance = " + missionsRouteDistance);
                    if (homeSystem != null)
                    {
                        searchSystem = missionsRouteList?.Split('_')[0];
                        searchDistance = mostList.Keys[mostList.Values.ToList().IndexOf(searchSystem)];
                    }
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }

                // Save the route data to the configuration
                navConfig.missionsRouteList = missionsRouteList;
                navConfig.missionsRouteDistance = missionsRouteDistance;
                navConfig.searchQuery = "most";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'most' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "most", searchSystem, null, missionsRouteList, mostCount, searchDistance, missionsRouteDistance, missionids));
            return searchSystem;
        }

        public string GetNearestRoute()
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

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
                navConfig.missionsRouteList = null;
                navConfig.missionsRouteDistance = 0;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'farthest' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "nearest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetScoopRoute(decimal searchRadius)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

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
                    sphereSystems = sphereSystems.Where(kvp => (kvp["system"] as StarSystem).scoopable).ToList();
                    searchCount = sphereSystems.Count;
                    if (searchCount > 0)
                    {
                        SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                        foreach (Dictionary<string, object> system in sphereSystems)
                        {
                            decimal distance = (decimal)system["distance"];
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, (system["system"] as StarSystem).systemname);
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
                navConfig.missionsRouteList = null;
                navConfig.missionsRouteDistance = 0;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "scoop", searchSystem, null, searchSystem, searchCount, searchDistance, endRadius, null));
            return searchSystem;
        }

        public string GetServiceRoute(string serviceQuery, int? maxDistance = null)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            int maxStationDistance = maxDistance ?? navConfig.maxSearchDistanceFromStarLs ?? 10000;
            bool prioritizeOrbitalStations = navConfig.prioritizeOrbitalStations;

            string searchSystem = null;
            string searchStation = null;
            decimal searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.size ?? LandingPadSize.Large;
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
                    navConfig.missionsRouteList = null;
                    navConfig.missionsRouteDistance = 0;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    UpdateSearchData(searchSystem, searchStation, searchDistance);

                    // Get mission IDs for 'service' system
                    missionids = GetSystemMissionIds(searchSystem);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, serviceQuery, searchSystem, searchStation, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public StarSystem GetServiceSystem(string serviceQuery, int maxStationDistance, bool prioritizeOrbitalStations)
        {
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the filter parameters
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.size ?? LandingPadSize.Large;
                ServiceFilter.TryGetValue(serviceQuery, out dynamic filter);
                int cubeLy = filter.cubeLy;

                //
                List<string> checkedSystems = new List<string>();
                string ServiceSystem = null;
                int maxTries = 5;

                while (ServiceSystem == null && maxTries > 0)
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

                        // Retreive systems in current radius which have not been previously checked
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

        public string GetSourceRoute(string system = null)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            cargoConfig = ConfigService.Instance.cargoMonitorConfiguration;

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
                        long distance = (long)(CalculateDistance(curr, dest) * 100);
                        if (!sourceList.TryGetValue(distance, out string val))
                        {
                            sourceList.Add(distance, haulage.sourcesystem);
                        }
                        missionids.Add(haulage.missionid);
                    }
                }

                if (sourceList != null)
                {
                    searchSystem = sourceList.Values.FirstOrDefault();
                    searchDistance = (decimal)sourceList.Keys.FirstOrDefault() / 100;
                    sourceSystems = string.Join("_", sourceList.Values);
                    systemsCount = sourceList.Count;

                    // Save the route data to the configuration
                    navConfig.searchQuery = "source";
                    navConfig.searchSystem = searchSystem;
                    navConfig.searchStation = null;
                    navConfig.searchDistance = searchDistance;
                    navConfig.missionsRouteList = sourceSystems;
                    navConfig.missionsRouteDistance = 0;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    UpdateSearchData(searchSystem, null, searchDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "source", searchSystem, null, sourceSystems, systemsCount, searchDistance, 0, missionids));
            return searchSystem;
        }

        public string GetNextInRoute()
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            string missionsRouteList = navConfig.missionsRouteList;
            decimal missionsRouteDistance = navConfig.missionsRouteDistance;
            string searchSystem = missionsRouteList?.Split('_')[0];
            string searchStation = null;
            decimal searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            int count = 0;
            if (searchSystem != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                searchDistance = CalculateDistance(curr, dest);
                count = missionsRouteList.Split('_').Count();

                // Save the route data to the configuration
                navConfig.searchQuery = "next";
                navConfig.searchSystem = searchSystem;
                navConfig.searchStation = null;
                navConfig.searchDistance = searchDistance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                UpdateSearchData(searchSystem, null, searchDistance);

                // Get mission IDs for 'next' system
                missionids = GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "next", searchSystem, searchStation, missionsRouteList, count, searchDistance, missionsRouteDistance, missionids));
            return searchSystem;
        }

        public string SetRoute(string system, string station = null)
        {
            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            decimal distance = 0;

            if (system != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, true);
                if (curr?.x != null && dest?.x != null && system != curr.systemname)
                {
                    distance = CalculateDistance(curr, dest);
                }
                else
                {
                    system = null;
                    station = null;
                    distance = 0;
                }

                // Update the search data to the configuration
                navConfig.searchQuery = "set";
                navConfig.searchSystem = system;
                navConfig.searchStation = station;
                navConfig.searchDistance = distance;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
            UpdateSearchData(system, station, distance);

            // Get mission IDs for 'set' system
            List<long> missionids = GetSystemMissionIds(system);

            string routeList = navConfig.missionsRouteList;
            decimal routeDistance = navConfig.missionsRouteDistance;
            int count = routeList?.Split('_').Count() ?? 0;
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "set", system, station, routeList, count, distance, routeDistance, missionids));
            return system;
        }

        public string UpdateRoute(string updateSystem = null)
        {
            bool update;

            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;

            string missionsRouteList = navConfig.missionsRouteList;
            decimal missionsRouteDistance = navConfig.missionsRouteDistance;
            List<Mission> missions = missionsConfig.missions.ToList();
            string searchSystem = null;
            decimal searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            string currentSystem = EDDI.Instance?.CurrentStarSystem?.systemname;
            List<string> route = missionsRouteList?.Split('_').ToList() ?? new List<string>();

            if (route.Count == 0) { update = false; }
            else if (updateSystem == null)
            {
                updateSystem = route[0];

                // Determine if the 'update' system in the missions route list is the current system & has no pending missions
                update = currentSystem == updateSystem ? !SystemPendingMissions(updateSystem, missions) : false;
            }
            else { update = route.Contains(updateSystem); }

            // Remove 'update' system from the missions route list
            if (update)
            {
                if (RemoveSystemFromRoute(updateSystem, missions, ref missionsRouteList, ref missionsRouteDistance))
                {
                    searchSystem = missionsRouteList?.Split('_')[0];
                    if (searchSystem != null)
                    {
                        searchDistance = CalculateDistance(currentSystem, searchSystem);

                        // Get mission IDs for 'next' system
                        missionids = GetSystemMissionIds(searchSystem);
                    }
                    Logging.Debug("Route Updated = " + missionsRouteList + ", Total Distance = " + missionsRouteDistance);

                    // Save the route data to the configuration
                    navConfig.searchQuery = "update";
                    navConfig.searchSystem = searchSystem;
                    navConfig.searchStation = null;
                    navConfig.searchDistance = searchDistance;
                    navConfig.missionsRouteList = missionsRouteList;
                    navConfig.missionsRouteDistance = missionsRouteDistance;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    UpdateSearchData(searchSystem, null, searchDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "update", searchSystem, null, missionsRouteList, route.Count, searchDistance, missionsRouteDistance, missionids));
            return searchSystem;
        }

        public decimal CalculateDistance(string currentSystem, string destinationSystem)
        {
            StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(currentSystem, true);
            StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(destinationSystem, true);
            return CalculateDistance(curr, dest);
        }

        public decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            double square(double x) => x * x;
            decimal distance = 0;
            if (curr?.x != null && dest?.x != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(square((double)(curr.x - dest.x))
                            + square((double)(curr.y - dest.y))
                            + square((double)(curr.z - dest.z))), 2);
            }
            return distance;
        }

        private decimal CalculateRouteDistance(string missionsRouteList)
        {
            List<string> route = missionsRouteList?.Split('_').ToList();
            decimal distance = 0;

            if (route.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;

                // Get all the route coordinates from EDSM in one request
                List<StarSystem> starsystems = dataProviderService.GetSystemsData(route.ToArray(), true, false, false, false, false);

                // Get distance to the next system
                StarSystem dest = starsystems.Find(s => s.systemname == route[0]);
                distance = CalculateDistance(curr, dest);

                // Calculate remaining route distance
                for (int i = 0; i < route.Count() - 1; i++)
                {
                    curr = starsystems.Find(s => s.systemname == route[i]);
                    dest = starsystems.Find(s => s.systemname == route[i + 1]);
                    distance += CalculateDistance(curr, dest);
                }
            }
            return distance;
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
            missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
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

        private bool RemoveSystemFromRoute(string system, List<Mission> missions, ref string missionsRouteList, ref decimal missionsRouteDistance)
        {
            List<string> route = missionsRouteList?.Split('_').ToList();
            if (route.Count == 0) { return false; }

            int index = route.IndexOf(system);
            if (index > -1)
            {
                // Do not remove the 'home' system unless last in list
                if (route.Count > 1 && index == route.Count - 1) { return false; }

                route.RemoveAt(index);
                if (route.Count > 0)
                {
                    // If other than 'next' system removed, recalculate the route
                    if (route.Count > 2 && index > 0)
                    {
                        // Use copy to keep the original intact.
                        List<string> systems = new List<string>(route);

                        // Build systems list
                        string homeSystem = systems.Last();
                        systems.RemoveAt(systems.Count - 1);
                        systems.Insert(0, EDDI.Instance?.CurrentStarSystem?.systemname);

                        if (CalculateRNNA(systems, homeSystem, missions, ref missionsRouteList, ref missionsRouteDistance)) { return true; }
                    }
                    missionsRouteList = string.Join("_", route);
                    missionsRouteDistance = CalculateRouteDistance(missionsRouteList);
                }
                else
                {
                    missionsRouteList = null;
                    missionsRouteDistance = 0;
                }
                return true;
            }
            return false;
        }

        private bool SystemPendingMissions(string system, List<Mission> missions)
        {
            foreach (Mission mission in missions.Where(m => m.statusEDName != "Fail").ToList())
            {
                string type = mission.typeEDName.ToLowerInvariant();
                switch (type)
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
                            // Check if the system is origin system for 'Active' and 'Complete' missions
                            if (mission.originsystem == system) { return true; }

                            // Check if the system is destination system for 'Active' missions
                            else if (mission.statusEDName == "Active")
                            {
                                if (mission.destinationsystems?.Any() ?? false)
                                {
                                    if (mission.destinationsystems.Where(d => d.name == system).Any()) { return true; }
                                }
                                else if (mission.destinationsystem == system) { return true; }
                            }
                        }
                        break;
                }
            }
            return false;
        }

        public void UpdateSearchData(string searchSystem, string searchStation, decimal searchDistance)
        {
            // Update search system data
            if (searchSystem != null)
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
            if (searchStation != null && SearchStarSystem?.stations != null)
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

            // Get up-to-date configuration data
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(starSystem);
            SearchDistanceLy = CalculateDistance(system, SearchStarSystem);
            navConfig.searchDistance = SearchDistanceLy;
            navConfig.updatedat = updateDat;
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;

            Logging.Debug("Distance from search system is " + SearchDistanceLy);
        }


    }
}
