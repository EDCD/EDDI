using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiCargoMonitor;
using EddiEvents;
using EddiMissionMonitor;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiNavigationService
{
    public class Navigation
    {
        private CargoMonitor cargoMonitor = (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
        private MissionMonitor missionMonitor = (MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor");

        private string searchSystem = null;
        private string searchStation = null;
        private decimal searchDistance = 0;

        private static Dictionary<string, dynamic> ServiceFilter = new Dictionary<string, dynamic>()
        {
            { "encoded", new {econ = new List<string>() {"High Tech", "Military"}, population = 1000000, security = new List<string>() {"Medium", "High"}, service = "Material Trader", cubeLy = 40} },
            { "facilitator", new {econ = new List<string>(), population = 0, security = new List<string>() {"Low"}, service = "Interstellar Factors Contact", cubeLy = 25} },
            { "manufactured", new {econ = new List<string>() {"Industrial"}, population = 1000000, security = new List<string>() {"Medium", "High"}, service = "Material Trader", cubeLy = 40} },
            { "raw", new {econ = new List<string>() {"Extraction", "Refinery"}, population = 1000000, security = new List<string>() {"Medium", "High"}, service = "Material Trader", cubeLy = 40} },
            { "guardian", new {econ = new List<string>() {"High Tech"}, population = 10000000, security = new List<string>()  {"High"}, service = "Technology Broker", cubeLy = 80} },
            { "human", new {econ = new List<string>() {"Industrial"}, population = 10000000, security = new List<string>() {"High"}, service = "Technology Broker", cubeLy = 80} }
        };

        private static Navigation instance;
        private static readonly object instanceLock = new object();

        public static Navigation Instance
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
                            instance = new Navigation();
                        }
                    }
                }
                return instance;
            }
        }

        public void CancelDestination()
        {
            // Clear missions route variables
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            missionMonitor.SetMissionsRouteData(null, 0);
            string destination = EDDI.Instance?.DestinationStarSystem?.systemname;
            UpdateDestinationData(null, null, 0);

            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "cancel", destination, null, null, 0, 0, 0, null));
        }

        public string GetExpiringRoute()
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            long expiringSeconds = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            List<Mission> missions = missionMonitor.missions.ToList();

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

                // Get mission IDs for 'expiring' system
                missionids = missionMonitor.GetSystemMissionIds(searchSystem);

                // Set missions route variables
                missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);


            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "expiring", searchSystem, null, searchSystem, expiringSeconds, searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetFarthestRoute()
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            List<Mission> missions = missionMonitor.missions.ToList();

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

                // Get mission IDs for 'farthest' system
                missionids = missionMonitor.GetSystemMissionIds(searchSystem);

                // Set missions route variables
                missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "farthest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetMissionsRoute(string homeSystem = null)
        {
            searchSystem = missionMonitor.GetMissionsRoute(homeSystem);
            searchStation = null;
            searchDistance = 0;

            if (searchSystem != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                searchDistance = CalculateDistance(curr, dest);
            }
            return searchSystem;
        }

        public string GetMostRoute(string homeSystem = null)
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            string routeList = null;
            decimal routeDistance = 0;
            long mostCount = 0;
            List<long> missionids = new List<long>();   // List of mission IDs for the next system
            List<Mission> missions = missionMonitor.missions.ToList();

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = new StarSystem();             // Destination star system

                // Determine the number of missions per individual system
                List<string> systems = new List<string>();  // Mission systems
                List<int> systemsCount = new List<int>();   // Count of missions per system
                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (mission.destinationsystems != null && mission.destinationsystems.Any())
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
                        if (dest != null && dest.x != null)
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
                if (missionMonitor.CalculateRNNA(mostList.Values.ToList(), homeSystem))
                {
                    routeList = missionMonitor.missionsRouteList;
                    routeDistance = missionMonitor.missionsRouteDistance;
                    Logging.Debug("Calculated Route Selected = " + routeList + ", Total Distance = " + routeDistance);
                    if (homeSystem != null)
                    {
                        searchSystem = routeList?.Split('_')[0];
                        searchDistance = mostList.Keys[mostList.Values.ToList().IndexOf(searchSystem)];
                    }
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");

                    // Set missions route variables
                    routeList = searchSystem;
                    routeDistance = searchDistance;
                    missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);
                }

                // Get mission IDs for 'most' system
                missionids = missionMonitor.GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "most", searchSystem, null, routeList, mostCount, searchDistance, routeDistance, missionids));
            return searchSystem;
        }

        public string GetNearestRoute()
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            List<Mission> missions = missionMonitor.missions.ToList();

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

                // Get mission IDs for 'nearest' system
                missionids = missionMonitor.GetSystemMissionIds(searchSystem);

                // Set missions route variables
                missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "nearest", searchSystem, null, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public string GetServiceRoute(string serviceType, int maxStationDistance, bool prioritizeOrbitalStations = false)
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.size ?? LandingPadSize.Large;
                ServiceFilter.TryGetValue(serviceType, out dynamic filter);

                StarSystem ServiceStarSystem = GetServiceSystem(serviceType, maxStationDistance, prioritizeOrbitalStations);
                if (ServiceStarSystem != null)
                {
                    searchSystem = ServiceStarSystem.systemname;
                    searchDistance = CalculateDistance(currentSystem, ServiceStarSystem);

                    // Filter stations which meet the game version and landing pad size requirements
                    List<Station> ServiceStations = !prioritizeOrbitalStations && EDDI.Instance.inHorizons ? ServiceStarSystem.stations : ServiceStarSystem.orbitalstations
                        .Where(s => s.stationservices.Count > 0).ToList();
                    ServiceStations = ServiceStations.Where(s => s.distancefromstar <= maxStationDistance).ToList();
                    if (serviceType == "facilitator") { ServiceStations = ServiceStations.Where(s => s.LandingPadCheck(shipSize)).ToList(); }
                    ServiceStations = ServiceStations.Where(s => s.stationservices.Contains(filter.service)).ToList();

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

                    // Get mission IDs for 'service' system
                    missionids = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.GetSystemMissionIds(searchSystem);

                    // Set missions route variables
                    missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, serviceType, searchSystem, searchStation, searchSystem, missionids.Count(), searchDistance, searchDistance, missionids));
            return searchSystem;
        }

        public StarSystem GetServiceSystem(string serviceType, int maxStationDistance, bool prioritizeOrbitalStations)
        {
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the filter parameters
                LandingPadSize shipSize = EDDI.Instance?.CurrentShip?.size ?? LandingPadSize.Large;
                ServiceFilter.TryGetValue(serviceType, out dynamic filter);
                int cubeLy = filter.cubeLy;

                //
                List<string> checkedSystems = new List<string>();
                string ServiceSystem = null;
                int maxTries = 5;

                while (ServiceSystem == null && maxTries > 0)
                {
                    List<StarSystem> cubeSystems = StarMapService.GetStarMapSystemsCube(currentSystem.systemname, cubeLy);
                    if (cubeSystems?.Any() ?? false)
                    {
                        // Filter systems using search parameters
                        cubeSystems = cubeSystems.Where(s => s.population >= filter.population).ToList();
                        cubeSystems = cubeSystems.Where(s => filter.security.Contains(s.security)).ToList();
                        if (serviceType != "facilitator")
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
                                if (serviceType == "facilitator") { stations = stations.Where(s => s.LandingPadCheck(shipSize)).ToList(); }
                                int stationCount = stations.Where(s => s.stationservices.Contains(filter.service)).Count();

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
            List<Cargo> inventory = cargoMonitor.inventory.ToList();
            int missionsCount = inventory.Sum(c => c.haulageData.Count());

            // Missions Route Event variables
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;
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

                    // Set missions route variables
                    missionMonitor.SetMissionsRouteData(searchSystem, searchDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "source", searchSystem, null, sourceSystems, systemsCount, searchDistance, 0, missionids));
            return searchSystem;
        }

        public string GetNextInRoute()
        {
            string routeList = missionMonitor.missionsRouteList;
            decimal routeDistance = missionMonitor.missionsRouteDistance;
            int count = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            searchSystem = routeList?.Split('_')[0];
            searchStation = null;
            searchDistance = 0;
            if (searchSystem != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(searchSystem, true);
                searchDistance = CalculateDistance(curr, dest);
                count = routeList.Split('_').Count();

                // Get mission IDs for 'next' system
                missionids = missionMonitor.GetSystemMissionIds(searchSystem);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "next", searchSystem, searchStation, routeList, count, searchDistance, routeDistance, missionids));
            return searchSystem;
        }

        public string SetDestination(string system = null, string station = null)
        {
            decimal distance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (system != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, true);
                if (curr?.x != null && dest?.x != null && system != curr.systemname)
                {
                    distance = CalculateDistance(curr, dest);
                    missionMonitor.SetMissionsRouteData(system, distance);
                }
                else
                {
                    system = null;
                    station = null;
                    missionMonitor.SetMissionsRouteData(null, 0);
                }
            }
            else if (searchSystem != null)
            {
                system = searchSystem;
                station = searchStation;
                distance = searchDistance;
            }

            // Get mission IDs for 'set' system
            missionids = missionMonitor.GetSystemMissionIds(system);

            // Set destination variables
            UpdateDestinationData(system, station, distance);

            // Clear 'search' variables
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;

            string routeList = missionMonitor.missionsRouteList;
            decimal routeDistance = missionMonitor.missionsRouteDistance;
            int count = routeList?.Split('_').Count() ?? 0;
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "set", system, station, routeList, count, distance, routeDistance, missionids));
            return searchSystem;
        }

        public string UpdateRoute(string updateSystem = null)
        {
            searchSystem = null;
            searchStation = null;
            searchDistance = 0;

            return missionMonitor.UpdateRoute(updateSystem);
        }

        public void UpdateDestinationData(string system, string station, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistanceLy = distance;
            EDDI.Instance.updateDestinationStation(station);
        }

        public decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            decimal distance = 0;
            if (curr?.x != null && dest?.x != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(curr.x - dest.x), 2)
                    + Math.Pow((double)(curr.y - dest.y), 2)
                    + Math.Pow((double)(curr.z - dest.z), 2)), 2);

            }
            return distance;
        }
    }
}
