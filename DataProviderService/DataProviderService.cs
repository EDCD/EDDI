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
            SetBodyReserves(starSystem, bodies);
            starSystem.bodies = bodies;

            if (starSystem?.population > 0)
            {
                List<Station> stations = EddbService.Stations(system);
                SetStationSystemName(starSystem);
                SetCommodityData(system, stations);
                starSystem.stations = stations;
            }

            return starSystem;
        }

        public static StarSystem GetEddbSystemData(string system)
        {
            if (system == null) { return null; }
            return EddbService.System(system);
        }

        private static void SetStationSystemName(StarSystem starSystem)
        {
            // Assign the system name to each stations. Only needed for EDDB data. 
            foreach (Station station in starSystem.stations)
            {
                station.systemname = starSystem.name;
            }
        }

        private static void SetBodyReserves(StarSystem starSystem, List<Body> bodies)
        {
            // Sets body reserve levels. Only needed for EDDB data.
            foreach (Body body in bodies)
            {
                if (body.rings?.Count() > 0)
                {
                    body.reserveLevel = starSystem.Reserve;
                }
                body.systemname = starSystem.name;
            }
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

                    SetStationFactionData(stations, factions);
                    SetCommodityData(system, stations);

                    starSystem.factions = factions;
                    starSystem.stations = stations;
                }
            }

            return starSystem ?? new StarSystem() { name = system };
        }

        public static StarSystem GetEdsmSystemData(string system)
        {
            if (system == null) { return null; }
            return StarMapService.GetStarMapSystem(system);
        }

        private static void SetStationFactionData(List<Station> stations, List<Faction> factions)
        {
            // EDSM doesn't provide FactionState information from the stations endpoint data, we need to add it from the factions endpoint data
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
        }

        // Legacy EDDP data service
        private static void SetCommodityData (string system, List<Station> stations)
        {
            foreach (Station station in stations)
            {
                station.systemname = system;
                LegacyEddpService.GetCommoditiesData(station.name, system, out List<CommodityMarketQuote> commodities, out long? commoditiesupdatedat);
                station.commodities = commodities;
                station.commoditiesupdatedat = commoditiesupdatedat;
            }
        }

        // EDSM flight log synchronization
        public void syncFromStarMapService (StarMapService starMapService, StarMapConfiguration starMapCredentials)
        {
            Logging.Info("Syncing from EDSM");
            try
            {
                Dictionary<string, StarMapLogInfo> systems = starMapService.getStarMapLog(starMapCredentials.lastSync);
                Dictionary<string, string> comments = starMapService.getStarMapComments();
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
