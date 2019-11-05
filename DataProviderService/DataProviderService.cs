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
        private readonly IEdsmService edsmService;

        public DataProviderService(IEdsmService edsmService = null)
        {
            this.edsmService = edsmService ?? new StarMapService();
        }

        // Uses the EDSM data service and legacy EDDP data
        public StarSystem GetSystemData(string system, bool showCoordinates = true, bool showSystemInformation = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (system == null || string.IsNullOrEmpty(system)) { return null; }

            StarSystem starSystem = edsmService.GetStarMapSystem(system, showCoordinates, showSystemInformation);
            starSystem = GetSystemExtras(starSystem, showSystemInformation, showBodies, showStations, showFactions);
            return starSystem ?? new StarSystem() { systemname = system };
        }

        public List<StarSystem> GetSystemsData(string[] systemNames, bool showCoordinates = true, bool showSystemInformation = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (systemNames == null || systemNames.Length == 0) { return null; }

            List<StarSystem> starSystems = edsmService.GetStarMapSystems(systemNames, showCoordinates, showSystemInformation);
            List<StarSystem> fullStarSystems = new List<StarSystem>();
            foreach (string systemName in systemNames)
            {
                if (!string.IsNullOrEmpty(systemName))
                {
                    fullStarSystems.Add(GetSystemExtras(starSystems.Find(s => s?.systemname == systemName), showSystemInformation, showBodies, showStations, showFactions) ?? new StarSystem() { systemname = systemName });
                }
            }
            return fullStarSystems;
        }

        private StarSystem GetSystemExtras(StarSystem starSystem, bool showInformation, bool showBodies, bool showStations, bool showFactions)
        {
            if (starSystem != null)
            {
                if (showBodies)
                {
                    List<Body> bodies = edsmService.GetStarMapBodies(starSystem.systemname) ?? new List<Body>();
                    foreach (Body body in bodies)
                    {
                        body.systemname = starSystem.systemname;
                        body.systemAddress = starSystem.systemAddress;
                        body.systemEDDBID = starSystem.EDDBID;
                    }
                    starSystem.AddOrUpdateBodies(bodies);
                }

                if (starSystem?.population > 0)
                {
                    List<Faction> factions = new List<Faction>();
                    if (showFactions || showStations)
                    {
                        factions = edsmService.GetStarMapFactions(starSystem.systemname);
                        starSystem.factions = factions;
                    }
                    if (showStations)
                    {
                        List<Station> stations = edsmService.GetStarMapStations(starSystem.systemname);
                        starSystem.stations = SetStationFactionData(stations, factions);
                        starSystem.stations = stations;
                    }
                }

                starSystem = LegacyEddpService.SetLegacyData(starSystem, showInformation, showBodies, showStations);
            }
            return starSystem;
        }

        private List<Station> SetStationFactionData(List<Station> stations, List<Faction> factions)
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

        public Traffic GetSystemTraffic(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapTraffic(systemName, edsmId);
        }

        public Traffic GetSystemDeaths(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapDeaths(systemName, edsmId);
        }

        public Traffic GetSystemHostility(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapHostility(systemName, edsmId);
        }

        // EDSM flight log synchronization
        public void syncFromStarMapService(DateTime? lastSync = null)
        {
            if (edsmService != null)
            {
                try
                {
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog(lastSync);
                    if (flightLogs.Count > 0)
                    {
                        Logging.Debug("Syncing from EDSM");
                        Dictionary<string, string> comments = edsmService.getStarMapComments();
                        int total = flightLogs.Count;
                        int i = 0;

                        while (i < total)
                        {
                            int batchSize = Math.Min(total, StarMapService.syncBatchSize);
                            List<StarMapResponseLogEntry> flightLogBatch = flightLogs.Skip(i).Take(batchSize).ToList();
                            syncEdsmLogBatch(flightLogBatch, comments);
                            i += batchSize;
                        }
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
        public List<StarSystem> syncFromStarMapService(List<StarSystem> starSystems)
        {
            if (edsmService != null && starSystems.Count > 0)
            {
                try
                {
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog(null, starSystems.Select(s => s.systemname).ToArray());
                    Dictionary<string, string> comments = edsmService.getStarMapComments();

                    foreach (StarSystem starSystem in starSystems)
                    {
                        if (starSystem?.systemname != null)
                        {
                            Logging.Debug("Syncing star system " + starSystem.systemname + " from EDSM.");
                            foreach (StarMapResponseLogEntry flightLog in flightLogs)
                            {
                                if (flightLog.system == starSystem.systemname)
                                {
                                    if (starSystem.EDSMID == null)
                                    {
                                        starSystem.EDSMID = flightLog.systemId;
                                    }
                                    else
                                    {
                                        if (starSystem.EDSMID != flightLog.systemId)
                                        {
                                            continue;
                                        }
                                    }
                                    starSystem.visitLog.Add(flightLog.date);
                                }
                            }
                            var comment = comments.FirstOrDefault(s => s.Key == starSystem.systemname);
                            if (!string.IsNullOrEmpty(comment.Value))
                            {
                                starSystem.comment = comment.Value;
                            }
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

        public void syncEdsmLogBatch(List<StarMapResponseLogEntry> flightLogBatch, Dictionary<string, string> comments)
        {
            List<StarSystem> syncedSystems = new List<StarSystem>();
            string[] systemNames = flightLogBatch.Select(x => x.system).Distinct().ToArray();
            List<StarSystem> batchSystems = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystems(systemNames, false, false);
            foreach (StarSystem starSystem in batchSystems)
            {
                if (starSystem != null)
                {
                    foreach (StarMapResponseLogEntry flightLog in flightLogBatch.Where(log => log.system == starSystem.systemname))
                    {
                        if (starSystem.EDSMID == null)
                        {
                            starSystem.EDSMID = flightLog.systemId;
                        }
                        else
                        {
                            if (starSystem.EDSMID != flightLog.systemId)
                            {
                                continue;
                            }
                        }
                        starSystem.visitLog.Add(flightLog.date);
                        if (comments.ContainsKey(flightLog.system))
                        {
                            starSystem.comment = comments[flightLog.system];
                        }
                    }
                }
                syncedSystems.Add(starSystem);
            }
            saveFromStarMapService(syncedSystems);
        }

        public void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
}
