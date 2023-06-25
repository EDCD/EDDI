using EddiConfigService;
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
        public StarSystem GetSystemData(string system, bool showCoordinates = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (system == null || string.IsNullOrEmpty(system)) { return null; }

            StarSystem starSystem = edsmService.GetStarMapSystem(system, showCoordinates);
            starSystem = GetSystemExtras(starSystem, showBodies, showStations, showFactions);
            return starSystem ?? new StarSystem() { systemname = system };
        }

        public List<StarSystem> GetSystemsData(string[] systemNames, bool showCoordinates = true, bool showBodies = true, bool showStations = true, bool showFactions = true)
        {
            if (systemNames == null || systemNames.Length == 0) { return new List<StarSystem>(); }

            List<StarSystem> starSystems = edsmService.GetStarMapSystems(systemNames, showCoordinates);
            if (starSystems == null) { return new List<StarSystem>(); }

            List<StarSystem> fullStarSystems = new List<StarSystem>();
            foreach (string systemName in systemNames)
            {
                if (!string.IsNullOrEmpty(systemName))
                {
                    fullStarSystems.Add(GetSystemExtras(starSystems.Find(s => s?.systemname == systemName), showBodies, showStations, showFactions) ?? new StarSystem() { systemname = systemName });
                }
            }
            return fullStarSystems;
        }

        private StarSystem GetSystemExtras(StarSystem starSystem, bool showBodies, bool showStations, bool showFactions)
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
                    }
                    starSystem.AddOrUpdateBodies(bodies);
                }

                if (starSystem.population > 0)
                {
                    List<Faction> factions = new List<Faction>();
                    List<Station> stations = new List<Station>();
                    if (showFactions)
                    {
                        factions = edsmService.GetStarMapFactions(starSystem.systemname) ?? factions;
                        starSystem.factions = factions;
                    }
                    if (showStations)
                    {
                        stations = edsmService.GetStarMapStations(starSystem.systemname) ?? stations;
                        starSystem.stations = showFactions 
                            ? SetStationFactionData( stations, factions ) 
                            : stations;
                    }
                }
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
            return edsmService.GetStarMapTraffic(systemName, edsmId) ?? new Traffic();
        }

        public Traffic GetSystemDeaths(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapDeaths(systemName, edsmId) ?? new Traffic();
        }

        public Traffic GetSystemHostility(string systemName, long? edsmId = null)
        {
            if (string.IsNullOrEmpty(systemName)) { return null; }
            return edsmService.GetStarMapHostility(systemName, edsmId) ?? new Traffic();
        }

        // EDSM flight log synchronization
        public void syncFromStarMapService(DateTime? lastSync = null)
        {
            if (edsmService != null)
            {
                try
                {
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog(lastSync);
                    if (flightLogs?.Count > 0)
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
            if (edsmService != null && edsmService.EdsmCredentialsSet() && starSystems.Count > 0)
            {
                try
                {
                    List<StarMapResponseLogEntry> flightLogs = edsmService.getStarMapLog(null, starSystems.Select(s => s.systemname).ToArray());
                    Dictionary<string, string> comments = edsmService.getStarMapComments();

                    if (flightLogs != null)
                    {
                        foreach (StarSystem starSystem in starSystems)
                        {
                            if (starSystem?.systemname != null)
                            {
                                Logging.Debug("Syncing star system " + starSystem.systemname + " from EDSM.");
                                foreach (StarMapResponseLogEntry flightLog in flightLogs)
                                {
                                    if (flightLog.system == starSystem.systemname)
                                    {
                                        starSystem.EDSMID = starSystem.EDSMID ?? flightLog.systemId;
                                        if (starSystem.EDSMID != flightLog.systemId) { continue; }
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
            var syncedSystems = new List<StarSystem>();
            var systemNames = flightLogBatch.Select(x => x.system).Distinct().ToArray();
            var batchSystems = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystems(systemNames, true, false, false, false, false);
            foreach (StarSystem starSystem in batchSystems)
            {
                if (starSystem != null)
                {
                    foreach (var flightLog in flightLogBatch.Where(log => log.system == starSystem.systemname))
                    {
                        // Fill missing SystemAddresses
                        if ( starSystem.systemAddress == 0 && flightLog.systemId64 > 0)
                        {
                            starSystem.systemAddress = flightLog.systemId64;
                            var bodies = starSystem.bodies.Where(b => b.systemAddress is null).ToList();
                            bodies.AsParallel().ForAll( b =>
                            {
                                b.systemAddress = flightLog.systemId64;
                            } );
                            foreach ( var body in starSystem.bodies )
                            {
                                body.systemAddress = flightLog.systemId64;
                            }
                            starSystem.AddOrUpdateBodies( bodies );
                        }

                        // Fill missing EDSMIDs
                        if ( starSystem.EDSMID == null )
                        {
                            starSystem.EDSMID = flightLog.systemId;
                        }

                        // Update Comments
                        if ( comments.TryGetValue( flightLog.system, out var comment ) )
                        {
                            starSystem.comment = comment;
                        }

                        // Update Visit Log
                        starSystem.visitLog.Add(flightLog.date);
                    }
                }
                syncedSystems.Add(starSystem);
            }
            saveFromStarMapService(syncedSystems);
        }

        private void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            var starMapConfiguration = ConfigService.Instance.edsmConfiguration;
            starMapConfiguration.lastFlightLogSync = DateTime.UtcNow;
            ConfigService.Instance.edsmConfiguration = starMapConfiguration;
        }
    }
}
