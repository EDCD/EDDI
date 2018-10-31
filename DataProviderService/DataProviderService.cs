using EddiEddbService;
using EddiDataDefinitions;
using EddiStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Utilities;
using System.Threading.Tasks;

namespace EddiDataProviderService
{
    /// <summary>Access data services<summary>
    public class DataProviderService
    {
        // EDDB data service
        public static StarSystem GetEddbFullSystemData (string system)
        {
            if (system == null) { return null; }

            StarSystem starSystem = EddbService.System(system);

            List<Body> bodies = EddbService.Bodies(system);
            foreach (Body body in bodies)
            {
                // Add missing data from our system information
                if (body.rings?.Count() > 0)
                {
                    body.reserveLevel = starSystem.Reserve;
                }
                body.systemname = starSystem.name;
                starSystem.bodies.Add(body);
            }

            if (starSystem?.population > 0)
            {
                List<Station> stations = EddbService.Stations(system);
                foreach (Station station in stations)
                {
                    // Add missing data from our system information and EDDP server
                    station.systemname = starSystem.name;
                    JObject response = LegacyEddpService.GetData(system);
                    LegacyEddpService.SetCommoditiesData(station, starSystem, response);
                    starSystem.stations.Add(station);
                }
            }

            return starSystem;
        }

        // EDSM data service
        public static StarSystem GetEdsmFullSystemData(string system)
        {
            if (system == null) { return null; }

            StarSystem starSystem = StarMapService.GetStarMapSystem(system);
            if (starSystem != null)
            {
                List<Body> bodies = StarMapService.GetStarMapBodies(system);
                starSystem.bodies = bodies;

                if (starSystem?.population > 0)
                {
                    List<Faction> factions = StarMapService.GetStarMapFactions(system);
                    List<Station> stations = StarMapService.GetStarMapStations(system);

                    starSystem.stations = SetStationFactionData(stations, factions);

                    starSystem.factions = factions;
                    starSystem.stations = stations;
                }

                starSystem = LegacyEddpService.SetEdsmData(starSystem);
            }
            return starSystem ?? new StarSystem() { name = system };
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
                List<StarSystem> syncSystems = new List<StarSystem>();
                foreach (string system in systems.Keys)
                {
                    StarSystem CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, false);
                    CurrentStarSystem.visits = systems[system].visits;
                    CurrentStarSystem.lastvisit = systems[system].lastVisit;
                    if (comments.ContainsKey(system))
                    {
                        CurrentStarSystem.comment = comments[system];
                    }
                    syncSystems.Add(CurrentStarSystem);

                    if (syncSystems.Count == StarMapService.syncBatchSize)
                    {
                        saveFromStarMapService(syncSystems);
                        syncSystems.Clear();
                    }
                }
                if (syncSystems.Count > 0)
                {
                    saveFromStarMapService(syncSystems);
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
