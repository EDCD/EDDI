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
    public static class DataProviderService
    {
        // Uses the EDSM data service and legacy EDDP data
        public static StarSystem GetSystemData(string system, bool showCoordinates = true, bool showSystemInformation = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (system == null || string.IsNullOrEmpty(system)) { return null; }

            StarSystem starSystem = StarMapService.GetStarMapSystem(system, showCoordinates, showSystemInformation);
            starSystem = GetSystemExtras(starSystem, showSystemInformation, showBodies, showStations, showFactions);
            starSystem = SyncEdsmLogsAndComments(new List<StarSystem>() { starSystem })?.FirstOrDefault();
            return starSystem ?? new StarSystem() { systemname = system };
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
                    fullStarSystems.Add(GetSystemExtras(starSystems.Find(s => s?.systemname == systemName), showSystemInformation, showBodies, showStations, showFactions) ?? new StarSystem() { systemname = systemName });
                }
            }
            fullStarSystems = SyncEdsmLogsAndComments(fullStarSystems);
            return fullStarSystems;
        }

        private static StarSystem GetSystemExtras(StarSystem starSystem, bool showInformation, bool showBodies, bool showStations, bool showFactions)
        {
            if (starSystem != null)
            {
                if (showBodies)
                {
                    List<Body> bodies = StarMapService.GetStarMapBodies(starSystem.systemname) ?? new List<Body>();
                    foreach (Body body in bodies)
                    {
                        body.systemname = starSystem.systemname;
                        body.systemAddress = starSystem.systemAddress;
                        body.systemEDDBID = starSystem.EDDBID;
                        starSystem.bodies.Add(body);
                    }
                    starSystem.bodies = starSystem.bodies.OrderBy(b => b.bodyId).ToList();
                }

                if (starSystem?.population > 0)
                {
                    List<Faction> factions = new List<Faction>();
                    if (showFactions || showStations)
                    {
                        factions = StarMapService.GetStarMapFactions(starSystem.systemname);
                        starSystem.factions = factions;
                    }
                    if (showStations)
                    {
                        List<Station> stations = StarMapService.GetStarMapStations(starSystem.systemname);
                        starSystem.stations = SetStationFactionData(stations, factions);
                        starSystem.stations = stations;
                    }
                }

                starSystem = LegacyEddpService.SetLegacyData(starSystem, showInformation, showBodies, showStations);
            }
            return starSystem;
        }

        private static List<StarSystem> SyncEdsmLogsAndComments(List<StarSystem> starSystems)
        {
            // Identify systems that need updated flight logs from EDSM
            List<StarSystem> syncSystems = new List<StarSystem>();
            foreach (StarSystem starSystem in starSystems)
            {
                if (starSystem?.visitLog == null && starSystem?.lastvisit != null)
                {
                    syncSystems.Add(starSystem);
                }
            }

            if (syncSystems.Count > 0)
            {
                // Synchronize EDSM flight logs and comments for applicable star systems
                syncSystems = syncFromStarMapService(syncSystems);
                foreach (StarSystem starSystem in syncSystems)
                {
                    StarSystem systemToUpdate = starSystems.FirstOrDefault(s => s.systemname == starSystem.systemname);
                    systemToUpdate = starSystem;
                }
            }

            return starSystems;
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
            if (string.IsNullOrEmpty(factionName)) { return null; }

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

        public static Traffic GetSystemTraffic(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return StarMapService.GetStarMapTraffic(systemName, edsmId);
        }

        public static Traffic GetSystemDeaths(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return StarMapService.GetStarMapDeaths(systemName, edsmId);
        }

        public static Traffic GetSystemHostility(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return StarMapService.GetStarMapHostility(systemName, edsmId);
        }

        // EDSM flight log synchronization
        public static void syncFromStarMapService(DateTime? lastSync = null, IProgress<string> progress = null)
        {
            if (StarMapService.Instance != null)
            {
                Logging.Info("Syncing from EDSM");

                try
                {
                    List<StarMapResponseLogEntry> flightLogs = StarMapService.Instance.getStarMapLog(lastSync);
                    Dictionary<string, string> comments = StarMapService.Instance.getStarMapComments();
                    int total = flightLogs.Count;
                    int i = 0;

                    while (i < total)
                    {
                        int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                        List<StarMapResponseLogEntry> flightLogBatch = flightLogs.Skip(i).Take(batchSize).ToList();
                        string[] batchNames = flightLogBatch.Select(x => x.system).ToArray();
                        List<StarSystem> batchsystems = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystems(batchNames, false);
                        syncEdsmLogBatch(batchsystems, flightLogBatch, comments);
                        i += batchSize;
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
        }

        // EDSM flight log synchronization (named star systems)
        public static List<StarSystem> syncFromStarMapService(List<StarSystem> starSystems)
        {
            if (StarMapService.Instance != null && starSystems.Count > 0)
            {
                try
                {
                    List<StarMapResponseLogEntry> flightLogs = StarMapService.Instance.getStarMapLog(null, starSystems.Select(s => s.systemname).ToArray());

                    foreach (StarSystem starSystem in starSystems)
                    {
                        if (starSystem?.systemname != null)
                        {
                            Logging.Debug("Syncing star system " + starSystem.systemname + " from EDSM.");
                            foreach (StarMapResponseLogEntry flightLog in flightLogs)
                            {
                                if (flightLog.system == starSystem.systemname)
                                {
                                    starSystem.visitLog.Add(flightLog.date);
                                }
                            }
                            starSystem.comment = StarMapService.Instance.getStarMapComment(starSystem.systemname);
                        }
                    }
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
            return starSystems;
        }

        public static void syncEdsmLogBatch(List<StarSystem> batchSystems, List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments)
        {
            List<StarSystem> syncedSystems = new List<StarSystem>();
            foreach (StarMapResponseLogEntry flightLog in flightLogBatch)
            {
                StarSystem CurrentStarSystem = batchSystems.FirstOrDefault(s => s.systemname == flightLog.system);
                if (CurrentStarSystem != null)
                {
                    CurrentStarSystem.visitLog.Add(flightLog.date);
                    if (comments.ContainsKey(flightLog.system))
                    {
                        CurrentStarSystem.comment = comments[flightLog.system];
                    }
                    syncedSystems.Add(CurrentStarSystem);
                }
            }
            saveFromStarMapService(syncedSystems);
        }

        public static void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
}
