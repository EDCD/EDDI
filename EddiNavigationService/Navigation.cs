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

        public string GetFacillitatorRoute()
        {
            string IFSystem = null;
            decimal IFDistance = 0;
            List<long> missionids = new List<long>();       // List of mission IDs for the next system

            Station IFStation = GetInterstellarFactorsStation();
            if (IFStation != null)
            {
                StarSystem curr = EDDI.Instance?.CurrentStarSystem;
                StarSystem dest = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(IFStation.systemname, true);
                if (dest != null)
                {
                    IFSystem = dest.name;
                    IFDistance = CalculateDistance(curr, dest);
                    missionids = ((MissionMonitor)EDDI.Instance.ObtainMonitor("Mission monitor"))?.GetSystemMissionIds(IFSystem);
                }

                // Set destination variables
                UpdateDestinationData(IFSystem, IFStation.name, IFDistance);
            }

            EDDI.Instance.enqueueEvent(new RouteDetailsEvent(DateTime.Now, "facillitator", IFSystem, IFSystem, missionids.Count(), IFDistance, IFDistance, missionids));
            return IFSystem;
        }

        public Station GetInterstellarFactorsStation(int cubeLy = 20)
        {
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem == null) { return null; }

            // Get the nearest Interstellar Factors system and station
            List<StarSystem> cubeSystems = StarMapService.GetStarMapSystemsCube(currentSystem.name, cubeLy);
            if (cubeSystems != null && cubeSystems.Any())
            {
                SortedList<decimal, string> nearestList = new SortedList<decimal, string>();

                string shipSize = EDDI.Instance?.CurrentShip?.size ?? "Large";
                SecurityLevel securityLevel = SecurityLevel.FromName("Low");
                StationService service = StationService.FromEDName("InterstellarFactorsContact");

                // Find the low security level systems which may contain IF contacts
                List<string> systemNames = cubeSystems.Where(s => s.securityLevel == securityLevel).Select(s => s.name).ToList();
                List<StarSystem> IFSystems = DataProviderService.GetSystemsData(systemNames.ToArray(), true, true, true, true, true);

                foreach (StarSystem starsystem in IFSystems)
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
                string nearestSystem = nearestList.Values.FirstOrDefault();
                if (nearestSystem == null) { return null; }
                StarSystem IFSystem = IFSystems.FirstOrDefault(s => s.name == nearestSystem);

                // Filter stations within the IF system which meet the game version and landing pad size requirements
                List<Station> IFStations = EDDI.Instance.inHorizons ? IFSystem.stations : IFSystem.orbitalstations
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

                // Interstellar Factors station nearest to the main star
                string nearestStation = nearestList.Values.FirstOrDefault();
                return IFSystem.stations.FirstOrDefault(s => s.name == nearestStation);
            }
            return null;
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

        private void UpdateDestinationData(string system, string station, decimal distance)
        {
            EDDI.Instance.updateDestinationSystem(system);
            EDDI.Instance.DestinationDistance = distance;
            EDDI.Instance.updateDestinationStation(station);
        }

    }
}
