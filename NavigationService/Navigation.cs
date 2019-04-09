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

        public string routeList;
        public decimal routeDistance;

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

        public void CancelRoute()
        {
            routeList = null;
            routeDistance = 0;
            missionMonitor?.writeMissions();

            // Clear destination variables
            UpdateDestinationData(null, null, 0);

            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "cancel", null, routeList, 0, 0, routeDistance, null));
        }

        public string GetExpiringRoute()
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
            string expiringSystem = null;
            decimal expiringDistance = 0;
            long expiringSeconds = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missions.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = new StarSystem();             // Destination star system

                foreach (Mission mission in missions.Where(m => m.statusEDName == "Active").ToList())
                {
                    if (expiringSeconds == 0 || mission.expiryseconds < expiringSeconds)
                    {
                        expiringSeconds = mission.expiryseconds ?? 0;
                        expiringSystem = mission.destinationsystem;
                    }
                }

                dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(expiringSystem, true);
                expiringDistance = CalculateDistance(curr, dest);
                routeList = expiringSystem;
                routeDistance = expiringDistance;
                missionMonitor?.writeMissions();

                // Get mission IDs for 'expiring' system
                missionids = missionMonitor?.GetSystemMissionIds(expiringSystem);

                // Set destination variables
                UpdateDestinationData(expiringSystem, null, expiringDistance);

            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "expiring", expiringSystem, routeList, expiringSeconds, expiringDistance, routeDistance, missionids));
            return expiringSystem;
        }

        public string GetFacilitatorRoute()
        {
            string IFSystem = null;
            string IFStation = null;
            decimal IFDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                // Get the nearest Interstellar Factors system and station
                List<StarSystem> cubeSystems = StarMapService.GetStarMapSystemsCube(currentSystem.name, 25);
                if (cubeSystems != null && cubeSystems.Any())
                {
                    SortedList<decimal, string> nearestList = new SortedList<decimal, string>();

                    string shipSize = EDDI.Instance?.CurrentShip?.size ?? "Large";
                    SecurityLevel securityLevel = SecurityLevel.FromName("Low");
                    StationService service = StationService.FromEDName("InterstellarFactorsContact");

                    // Find the low security level systems which may contain IF contacts
                    List<string> systemNames = cubeSystems.Where(s => s.securityLevel == securityLevel).Select(s => s.name).ToList();
                    List<StarSystem> IFStarSystems = DataProviderService.GetSystemsData(systemNames.ToArray(), true, true, true, true, true);

                    foreach (StarSystem starsystem in IFStarSystems)
                    {
                        // Filter stations which meet the game version and landing pad size requirements
                        int stationCount = (EDDI.Instance.inHorizons ? starsystem.stations : starsystem.orbitalstations)
                            .Where(s => s.stationServices.Contains(service) && s.LandingPadCheck(shipSize))
                            .Count();

                        // Build list to find the IF system nearest to the current system
                        if (stationCount > 0)
                        {
                            decimal distance = CalculateDistance(currentSystem, starsystem);
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, starsystem.name);
                            }
                        }
                    }

                    // Nearest Interstellar Factors system
                    IFSystem = nearestList.Values.FirstOrDefault();
                    if (IFSystem != null)
                    {
                        StarSystem IFStarSystem = IFStarSystems.FirstOrDefault(s => s.name == IFSystem);
                        IFDistance = nearestList.Keys.FirstOrDefault();

                        // Filter stations within the IF system which meet the game version and landing pad size requirements
                        List<Station> IFStations = EDDI.Instance.inHorizons ? IFStarSystem.stations : IFStarSystem.orbitalstations
                            .Where(s => s.stationServices.Contains(service) && s.LandingPadCheck(shipSize)).ToList();

                        // Build list to find the IF station nearest to the main star
                        nearestList.Clear();

                        foreach (Station station in IFStations)
                        {
                            if (!nearestList.ContainsKey(station.distancefromstar ?? 0))
                            {
                                nearestList.Add(station.distancefromstar ?? 0, station.name);
                            }
                        }

                        // Interstellar Factors station is nearest to the main star
                        IFStation = nearestList.Values.FirstOrDefault();
                    }
                }
                routeList = IFSystem;
                routeDistance = IFDistance;
                missionMonitor?.writeMissions();

                // Get mission IDs for 'insterstallar factors' system
                missionids = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.GetSystemMissionIds(IFSystem);

                // Set destination variables
                UpdateDestinationData(IFSystem, IFStation, IFDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "facillitator", IFSystem, IFSystem, missionids.Count(), IFDistance, IFDistance, missionids));
            return IFSystem;
        }

        public string GetFarthestRoute()
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
            string farthestSystem = null;
            decimal farthestDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

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
                            dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system.name, true);
                            decimal distance = CalculateDistance(curr, dest);
                            if (!farthestList.ContainsKey(distance))
                            {
                                farthestList.Add(distance, system.name);

                            }
                        }
                    }
                    else if (mission.destinationsystem != string.Empty)
                    {
                        dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(mission.destinationsystem, true);
                        decimal distance = CalculateDistance(curr, dest);
                        if (!farthestList.ContainsKey(distance))
                        {
                            farthestList.Add(distance, mission.destinationsystem);
                        }
                    }
                }

                // Farthest system is last in the list
                farthestSystem = farthestList.Values.LastOrDefault();
                farthestDistance = farthestList.Keys.LastOrDefault();
                routeList = farthestSystem;
                routeDistance = farthestDistance;
                missionMonitor?.writeMissions();

                // Get mission IDs for 'farthest' system
                missionids = missionMonitor?.GetSystemMissionIds(farthestSystem);

                // Set destination variables
                UpdateDestinationData(farthestSystem, null, farthestDistance);

            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "farthest", farthestSystem, routeList, missionids.Count(), farthestDistance, routeDistance, missionids));
            return farthestSystem;
        }

        public string GetMostRoute(string homeSystem = null)
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
            string mostSystem = null;
            decimal mostDistance = 0;
            long mostCount = 0;
            List<long> missionids = new List<long>();   // List of mission IDs for the next system

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
                        dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(systems[i], true);
                        if (dest != null)
                        {
                            mostList.Add(CalculateDistance(curr, dest), systems[i]);
                        }
                    }
                }

                // Nearest 'most' system is first in the list
                mostSystem = mostList.Values.FirstOrDefault();
                mostDistance = mostList.Keys.FirstOrDefault();

                // Calculate the missions route using the 'Repetitive Nearest Neighbor' Algorithim (RNNA)
                mostList.Add(0, curr?.name);
                if (CalculateRNNA(mostList.Values.ToList(), homeSystem))
                {
                    Logging.Debug("Calculated Route Selected = " + routeList + ", Total Distance = " + routeDistance);
                    if (homeSystem != null)
                    {
                        mostSystem = routeList?.Split('_')[0];
                        mostDistance = mostList.Keys[mostList.Values.ToList().IndexOf(mostSystem)];
                    }
                }
                else
                {
                    routeList = mostSystem;
                    routeDistance = mostDistance;
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
                missionMonitor?.writeMissions();

                // Get mission IDs for 'most' system
                missionids = missionMonitor?.GetSystemMissionIds(mostSystem);

                // Set destination variables
                UpdateDestinationData(mostSystem, null, mostDistance);

            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "most", mostSystem, routeList, mostCount, mostDistance, routeDistance, missionids));
            return mostSystem;
        }

        public string GetMissionsRoute(string homeSystem = null)
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
            string nextSystem = null;
            decimal nextDistance = 0;
            int routeCount = 0;

            List<string> systems = new List<string>();      // List of eligible mission destintaion systems
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missions.Count > 0)
            {
                // Add current star system first
                string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
                systems.Add(currentSystem);

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
                                if (mission.destinationsystems == null || !mission.destinationsystems.Any())
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
                if (CalculateRNNA(systems, homeSystem))
                {
                    nextSystem = routeList?.Split('_')[0];
                    nextDistance = CalculateDistance(currentSystem, nextSystem);
                    routeCount = routeList.Split('_').Count();

                    Logging.Debug("Calculated Route Selected = " + routeList + ", Total Distance = " + routeDistance);
                    missionMonitor?.writeMissions();

                    // Get mission IDs for 'next' system
                    missionids = missionMonitor?.GetSystemMissionIds(nextSystem);

                    // Set destination variables
                    UpdateDestinationData(nextSystem, null, nextDistance);
                }
                else
                {
                    Logging.Debug("Unable to meet missions route calculation criteria");
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "route", nextSystem, routeList, routeCount, nextDistance, routeDistance, missionids));
            return nextSystem;
        }

        private bool CalculateRNNA(List<string> systems, string homeSystem)
        {
            // Clear route list & distance
            routeList = null;
            routeDistance = 0;
            bool found = false;

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
                List<StarSystem> starsystems = DataProviderService.GetSystemsData(systems.ToArray(), true, false, false, false, false);
                decimal[][] distMatrix = new decimal[systems.Count][];
                for (int i = 0; i < systems.Count; i++)
                {
                    distMatrix[i] = new decimal[systems.Count];
                }
                for (int i = 0; i < systems.Count - 1; i++)
                {
                    StarSystem curr = starsystems.Find(s => s.name == systems[i]);
                    for (int j = i + 1; j < systems.Count; j++)
                    {
                        StarSystem dest = starsystems.Find(s => s.name == systems[j]);
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
                    if (DestinationOriginReturn(systems[i])) { continue; }

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
                                nearestList.Add(distMatrix[currIndex][destIndex], destIndex);
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
                    routeList = string.Join("_", bestRoute);
                    routeDistance = bestDistance;
                    found = true;
                }
            }
            return found;
        }

        public string GetNearestRoute()
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
            string nearestSystem = null;
            decimal nearestDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

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
                            dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system.name, true);
                            decimal distance = CalculateDistance(curr, dest);
                            if (!nearestList.ContainsKey(distance))
                            {
                                nearestList.Add(distance, system.name);

                            }
                        }
                    }
                    else if (mission.destinationsystem != string.Empty)
                    {
                        dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(mission.destinationsystem, true);
                        decimal distance = CalculateDistance(curr, dest);
                        if (!nearestList.ContainsKey(distance))
                        {
                            nearestList.Add(distance, mission.destinationsystem);
                        }
                    }
                }

                // Nearest system is first in the list
                nearestSystem = nearestList.Values.FirstOrDefault();
                nearestDistance = nearestList.Keys.FirstOrDefault();
                routeList = nearestSystem;
                routeDistance = nearestDistance;
                missionMonitor?.writeMissions();

                // Get mission IDs for 'nearest' system
                missionids = missionMonitor?.GetSystemMissionIds(nearestSystem);

                // Set destination variables
                UpdateDestinationData(nearestSystem, null, nearestDistance);

            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "nearest", nearestSystem, routeList, missionids.Count(), nearestDistance, routeDistance, missionids));
            return nearestSystem;
        }

        public string GetSourceRoute(string system = null)
        {
            List<Cargo> inventory = cargoMonitor?.inventory.ToList();
            int missionsCount = inventory.Sum(c => c.haulageData.Count());

            // Missions Route Event variables
            decimal sourceDistance = 0;
            string sourceSystem = null;
            string sourceSystems = null;
            int systemsCount = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (missionsCount > 0)
            {
                var sourceList = new SortedList<long, string>();
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                string currentSystem = curr?.name;
                bool fromHere = system == currentSystem;

                foreach (Cargo cargo in inventory.Where(c => c.haulageData.Any()).ToList())
                {
                    foreach (Haulage haulage in cargo.haulageData.Where(h => h.status == "Active" && h.sourcesystem != null).ToList())
                    {
                        if (fromHere && haulage.originsystem != currentSystem)
                        {
                            break;
                        }

                        StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(haulage.sourcesystem, true);
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
                    sourceSystem = sourceList.Values.FirstOrDefault();
                    sourceDistance = (decimal)sourceList.Keys.FirstOrDefault() / 100;
                    sourceSystems = string.Join("_", sourceList.Values);
                    systemsCount = sourceList.Count;

                    // Set destination variables
                    EDDI.Instance.updateDestinationSystem(sourceSystem);
                    EDDI.Instance.DestinationDistance = sourceDistance;
                    EDDI.Instance.updateDestinationStation(null);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "source", sourceSystem, sourceSystems, systemsCount, sourceDistance, 0, missionids));
            return sourceSystem;
        }

        public string SetNextRoute()
        {
            string nextSystem = routeList?.Split('_')[0];
            decimal nextDistance = 0;
            int count = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (nextSystem != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(nextSystem, true);

                if (dest != null && nextSystem != curr.name)
                {
                    nextDistance = CalculateDistance(curr, dest);
                }
                count = routeList.Split('_').Count();

                // Get mission IDs for 'next' system
                missionids = missionMonitor?.GetSystemMissionIds(nextSystem);

                // Set destination variables
                UpdateDestinationData(nextSystem, null,  nextDistance);
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "next", nextSystem, routeList, count, nextDistance, routeDistance, missionids));
            return nextSystem;
        }

        public string SetRoute(string system)
        {
            string destination = null;
            decimal distance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            if (system != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, true);

                if (dest != null && system != curr.name)
                {
                    distance = CalculateDistance(curr, dest);
                    destination = dest.name;
                }
                routeList = destination;
                routeDistance = distance;
                missionMonitor?.writeMissions();

                // Get mission IDs for 'next' system
                missionids = missionMonitor?.GetSystemMissionIds(destination);

                // Set destination variables
                UpdateDestinationData(destination, null, distance);

            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "set", destination, routeList, 1, distance, routeDistance, missionids));
            return destination;
        }

        public string UpdateRoute(string updateSystem = null)
        {
            bool update;
            string nextSystem = null;
            decimal nextDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            List<string> route = routeList?.Split('_').ToList();

            if (route.Count == 0) { update = false; }
            else if (updateSystem == null)
            {
                updateSystem = route[0];

                // Determine if the 'update' system in the missions route list is the current system & has no pending missions
                update = currentSystem == updateSystem ? !SystemPendingMissions(updateSystem) : false;
            }
            else { update = route.Contains(updateSystem); }

            // Remove 'update' system from the missions route list
            if (update)
            {
                if (RemoveSystemFromRoute(updateSystem))
                {
                    nextSystem = routeList?.Split('_')[0];
                    if (nextSystem != null)
                    {
                        nextDistance = CalculateDistance(currentSystem, nextSystem);

                        // Get mission IDs for 'next' system
                        missionids = missionMonitor?.GetSystemMissionIds(nextSystem);
                    }
                    Logging.Debug("Route Updated = " + routeList + ", Total Distance = " + routeDistance);
                    missionMonitor?.writeMissions();

                    // Set destination variables
                    UpdateDestinationData(nextSystem, null, nextDistance);
                }
            }
            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "update", nextSystem, routeList, route.Count, nextDistance, routeDistance, missionids));
            return nextSystem;
        }

        public decimal CalculateDistance(string currentSystem, string destinationSystem)
        {
            StarSystem curr = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(currentSystem, true);
            StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(destinationSystem, true);
            return CalculateDistance(curr, dest);
        }

        private decimal CalculateDistance(StarSystem curr, StarSystem dest)
        {
            decimal distance = -1;
            if (curr != null && dest != null)
            {
                distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(curr.x - dest.x), 2)
                    + Math.Pow((double)(curr.y - dest.y), 2)
                    + Math.Pow((double)(curr.z - dest.z), 2)), 2);

            }
            return distance;
        }

        private decimal CalculateRouteDistance()
        {
            List<string> route = routeList?.Split('_').ToList();
            decimal distance = 0;

            if (route.Count > 0)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;

                // Get all the route coordinates from EDSM in one request
                List<StarSystem> starsystems = DataProviderService.GetSystemsData(route.ToArray(), true, false, false, false, false);

                // Get distance to the next system
                StarSystem dest = starsystems.Find(s => s.name == route[0]);
                distance = CalculateDistance(curr, dest);

                // Calculate remaining route distance
                for (int i = 0; i < route.Count() - 1; i++)
                {
                    curr = starsystems.Find(s => s.name == route[i]);
                    dest = starsystems.Find(s => s.name == route[i + 1]);
                    distance += CalculateDistance(curr, dest);
                }
            }
            return distance;
        }

        private bool DestinationOriginReturn(string destination)
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
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

        private bool RemoveSystemFromRoute(string system)
        {
            List<string> route = routeList?.Split('_').ToList();
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
                        systems.Insert(0, EDDI.Instance?.CurrentStarSystem?.name);

                        if (CalculateRNNA(systems, homeSystem)) { return true; }
                    }
                    routeList = string.Join("_", route);
                    routeDistance = CalculateRouteDistance();
                }
                else
                {
                    routeList = null;
                    routeDistance = 0;
                }
                return true;
            }
            return false;
        }

        private bool SystemPendingMissions(string system)
        {
            List<Mission> missions = missionMonitor?.missions.ToList();
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
                                if (mission.destinationsystems != null && mission.destinationsystems.Any())
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
        private void UpdateDestinationData(string system, string station, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistance = distance;
            EDDI.Instance.updateDestinationStation(station);
        }
    }
}
