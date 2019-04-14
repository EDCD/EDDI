using EddiBgsService;
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
            if (system == null || string.IsNullOrEmpty(system)) { return null; }

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
                if (!string.IsNullOrEmpty(systemName))
                {
                    fullStarSystems.Add(GetSystemExtras(starSystems.Find(s => s.name == systemName), showSystemInformation, showBodies, showStations, showFactions) ?? new StarSystem() { name = systemName });
                }
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

        ///<summary> Faction data from EliteBGS (allows search by faction name - EDSM can only search by system name). 
        /// If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system </summary>
        public static Faction GetFactionByName(string factionName, string systemName = null)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(BgsService.FactionParameters.factionName, factionName)
            };
            List<Faction> factions = BgsService.GetFactions(BgsService.factionEndpoint, queryList);

            // If a systemName is provided, we can filter factions that share a name according to whether they have a presence in a known system
            if (systemName != null && factions.Count > 1)
            {
                foreach (Faction faction in factions)
                {
                    faction.presences = faction.presences.Where(f => f.systemName == systemName)?.ToList();
                }
            }

            return factions?.FirstOrDefault() ?? new Faction()
            {
                name = factionName
            };
        }

        // EDSM flight log synchronization
        public static void syncFromStarMapService (DateTime? lastSync = null, IProgress<string> progress = null)
        {
            Logging.Info("Syncing from EDSM");

            try
            {
                Dictionary<string, StarMapLogInfo> systems = StarMapService.Instance.getStarMapLog(lastSync);
                Dictionary<string, string> comments = StarMapService.Instance.getStarMapComments();

                int total = systems.Count;
                int i = 0;

                while (i < total)
                {
                    int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                    syncEdsmLogBatch(systems.Skip(i).Take(batchSize).ToDictionary(x => x.Key, x => x.Value), comments);
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

        public static void syncEdsmLogBatch(Dictionary<string, StarMapLogInfo> systems, Dictionary<string, string> comments)
        {
            List<StarSystem> batchSystems = new List<StarSystem>();
            string[] batchNames = systems.Select(x => x.Key).ToArray();
            List<StarSystem> starSystems = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystems(batchNames, false);
            foreach (string name in batchNames)
            {
                StarSystem CurrentStarSystem = starSystems.FirstOrDefault(s => s.name == name);
                if (CurrentStarSystem == null) { continue; }
                CurrentStarSystem.visits = systems[name].visits;
                CurrentStarSystem.lastvisit = systems[name].lastVisit;
                if (comments.ContainsKey(name))
                {
                    CurrentStarSystem.comment = comments[name];
                }
                batchSystems.Add(CurrentStarSystem);
            }
            saveFromStarMapService(batchSystems);
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
