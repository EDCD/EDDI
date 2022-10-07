using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class FleetCarrierEndpoint : Endpoint
    {
        private const string FLEETCARRIER_URL = "/fleetcarrier";

        // We cache data to avoid spamming the service
        private JObject cachedFleetCarrierJson;
        private DateTime cachedFleetCarrierTimeStamp;
        private DateTime cachedFleetCarrierExpires => cachedFleetCarrierTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public event EventHandler FleetCarrierUpdatedEvent;

        /// <summary>
        /// Contains information about the player's fleet carrier (if any)
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public Tuple<JObject, DateTime> GetFleetCarrier(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedFleetCarrierExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{FLEETCARRIER_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedFleetCarrierJson));
                return new Tuple<JObject, DateTime>(cachedFleetCarrierJson, cachedFleetCarrierTimeStamp);
            }

            Logging.Debug($"Getting {FLEETCARRIER_URL} data");
            var result = GetEndpoint(FLEETCARRIER_URL);

            if (!result.Item1?.DeepEquals(cachedFleetCarrierJson) ?? false)
            {
                cachedFleetCarrierJson = result.Item1;
                cachedFleetCarrierTimeStamp = result.Item2;
                Logging.Debug($"{FLEETCARRIER_URL} returned " + JsonConvert.SerializeObject(cachedFleetCarrierJson));
                FleetCarrierUpdatedEvent?.Invoke(this, new CompanionApiEventArgs(cachedFleetCarrierJson, cachedFleetCarrierTimeStamp));
            }
            else
            {
                Logging.Debug($"{FLEETCARRIER_URL} returned, no change from prior cached data.");
            }

            return result;
        }
    }
}