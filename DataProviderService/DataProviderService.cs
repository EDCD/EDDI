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
    /// <summary>Access to EDDP data<summary>
    public class DataProviderService
    {
        public static StarSystem GetFullSystemData(string system)
        {
            if (system == null) { return null; }

            // Retrieve starSystem and station details concurrently.

            Task<StarSystem> getSystem = Task.Run(() => EddbService.System(system));
            StarSystem starSystem = getSystem.Result;

            Task<List<Station>> getStations = Task.Run(() => setStationSystemData(system));

            // Wait until the starSystem has been retrieved, then set reserve levels from starSystem data

            Task<List<Body>> getBodies = getSystem.ContinueWith(s => setBodySystemData(system, s.Result.Reserve));

            starSystem.stations = getStations.Result;
            starSystem.bodies = getBodies.Result;

            return starSystem;
        }

        public static StarSystem GetSystemData(string system)
        {
            if (system == null) { return null; }
            return EddbService.System(system);
        }

        private static List<Station> setStationSystemData(string system)
        {
            List<Station> stations = EddbService.Stations(system);
            foreach (Station station in stations)
            {
                station.systemname = system;
            }
            return stations;
        }

        private static List<Body> setBodySystemData(string system, SystemReserveLevel reserve)
        {
            List<Body> bodies = EddbService.Bodies(system);
            foreach (Body body in bodies)
            {
                if (body.rings?.Count() > 0)
                {
                    body.reserveLevel = reserve;
                }
                body.systemname = system;
            }
            return bodies;
        }


        public void syncFromStarMapService(StarMapService starMapService, StarMapConfiguration starMapCredentials)
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

        public static void saveFromStarMapService(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
}
