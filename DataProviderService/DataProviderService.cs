using EddiDataDefinitions;
using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access data services<summary>
    public class DataProviderService
    {
        // Uses the EDSM data service and legacy EDDP data
        public static StarSystem GetSystemData(string system, bool showCoordinates = true, bool showSystemInformation = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (system == null) { return null; }

            StarSystem starSystem = StarMapService.GetStarMapSystem(system, showCoordinates, showSystemInformation);
            starSystem = GetSystemExtras(starSystem, showSystemInformation, showBodies, showStations, showFactions) ?? new StarSystem() { name = system };
            return starSystem;
        }

        public static List<StarSystem> GetSystemsData(string[] systemNames, bool showCoordinates = true, bool showSystemInformation = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (systemNames == null || systemNames.Length == 0) { return null; }

            List<StarSystem> starSystems = StarMapService.GetStarMapSystems(systemNames, showCoordinates, showSystemInformation);
            List<StarSystem> fullStarSystems = new List<StarSystem>();
            foreach (string systemName in systemNames)
            {
                fullStarSystems.Add(GetSystemExtras(starSystems.Find(s => s.name == systemName), showSystemInformation, showBodies, showStations, showFactions) ?? new StarSystem() { name = systemName } );
            }
            return starSystems;
        }

        private static StarSystem GetSystemExtras(StarSystem starSystem, bool showInformation, bool showBodies, bool showStations, bool showFactions)
        {
            if (starSystem != null)
            {
                if (showBodies)
                {
                    List<Body> bodies = StarMapService.GetStarMapBodies(starSystem.name) ?? new List<Body>();
                    foreach (Body body in bodies)
                    {
                        body.systemname = starSystem.name;
                        body.systemAddress = starSystem.systemAddress;
                        body.systemEDDBID = starSystem.EDDBID;
                        starSystem.bodies.Add(body);
                    }
                }

                if (starSystem?.population > 0)
                {
                    List<Faction> factions = new List<Faction>();
                    if (showFactions || showStations)
                    {
                        factions = StarMapService.GetStarMapFactions(starSystem.name);
                        starSystem.factions = factions;
                    }
                    if (showStations)
                    {
                        List<Station> stations = StarMapService.GetStarMapStations(starSystem.name);
                        starSystem.stations = SetStationFactionData(stations, factions);
                        starSystem.stations = stations;
                    }
                }

                starSystem = LegacyEddpService.SetLegacyData(starSystem, showInformation, showBodies, showStations);
            }

            return starSystem;
        }

        private static List<Station> SetStationFactionData(List<Station> stations, List<Faction> factions)
        {
            // EDSM doesn't provide full faction information (like faction state) from the stations endpoint data
            // so we add it from the factions endpoint data
            foreach (Station station in stations)
            {
                foreach (Faction faction in factions)
                {
                    if (station.Faction.name == faction.name)
                    {
                        station.Faction = faction;
                    }
                }
            }
            return stations;
        }

        // EDSM flight log synchronization
        public static void syncFromStarMapService (bool forceSyncAll = false)
        {
            Logging.Info("Syncing from EDSM");

            try
            {
                StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
                Dictionary<string, StarMapLogInfo> systems = StarMapService.Instance.getStarMapLog(forceSyncAll ? null : (DateTime?)starMapCredentials.lastSync);
                Dictionary<string, string> comments = StarMapService.Instance.getStarMapComments();

                int total = systems.Count;
                int i = 0;

                string[] systemNames = systems.Keys.ToArray();
                while (i < total)
                {
                    int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                    string[] batchNames = systemNames.Skip(i).Take(batchSize).ToArray();
                    List<StarSystem> batchSystems = new List<StarSystem>();

                    List<StarSystem> starSystems = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystems(batchNames, true);
                    foreach (string system in batchNames)
                    {
                        StarSystem CurrentStarSystem = starSystems.FirstOrDefault(s => s.name == system);
                        if (CurrentStarSystem == null) { continue; }
                        CurrentStarSystem.visits = systems[system].visits;
                        CurrentStarSystem.lastvisit = systems[system].lastVisit;
                        if (comments.ContainsKey(system))
                        {
                            CurrentStarSystem.comment = comments[system];
                        }
                        batchSystems.Add(CurrentStarSystem);
                    }
                    saveFromStarMapService(batchSystems);
                    i = i + batchSize;
                }
                Logging.Info("EDSM sync completed");
            }
            catch (EDSMException edsme)
            {
                Logging.Debug("EDSM error received: " + edsme.Message);
            }
            catch (ThreadAbortException e)
            {
                Logging.Debug("EDSM update stopped by user: " + e.Message);
            }
        }

        public static void saveFromStarMapService (List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
}
