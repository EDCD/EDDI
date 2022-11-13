using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class ShipyardEndpoint : Endpoint
    {
        private const string SHIPYARD_URL = "/shipyard";

        // We cache the market to avoid spamming the service
        private JObject cachedShipyardJson;
        private DateTime cachedShipyardTimeStamp;
        private DateTime cachedShipyardExpires => cachedShipyardTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public event EventHandler ShipyardUpdatedEvent;

        /// <summary>
        /// Contains information about shipyard and outfitting at the last station visited
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public JObject GetShipyard(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedShipyardExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{SHIPYARD_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedShipyardJson));
                return cachedShipyardJson;
            }

            try
            {
                Logging.Debug($"Getting {SHIPYARD_URL} data");
                var result = GetEndpoint(SHIPYARD_URL);

                if (!result?.DeepEquals(cachedShipyardJson) ?? false)
                {
                    cachedShipyardJson = result;
                    cachedShipyardTimeStamp = result["timestamp"]?.ToObject<DateTime>() ?? DateTime.MinValue;
                    Logging.Debug($"{SHIPYARD_URL} returned " + JsonConvert.SerializeObject(cachedShipyardJson));
                    ShipyardUpdatedEvent?.Invoke(cachedShipyardJson, new CompanionApiEventArgs(cachedShipyardJson));
                }
                else
                {
                    Logging.Debug($"{SHIPYARD_URL} returned, no change from prior cached data.");
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return cachedShipyardJson;
        }
    }
}