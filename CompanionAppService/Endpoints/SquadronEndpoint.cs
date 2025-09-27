using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class SquadronEndpoint : Endpoint
    {
        private const string SQUADRON_URL = "/squadron";

        // We cache data to avoid spamming the service
        private JObject cachedSquadronJson;
        private DateTime cachedSquadronTimeStamp;
        private DateTime cachedSquadronExpires => cachedSquadronTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public event EndpointEventHandler SquadronUpdatedEvent;

        /// <summary>
        /// Contains information about the player's squadron (if any)
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public async Task<JObject> GetSquadronAsync(bool forceRefresh = false)
        {
            if (!forceRefresh && cachedSquadronExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{SQUADRON_URL} endpoint queried too often. Returning cached data: ", cachedSquadronJson);
                return cachedSquadronJson;
            }

            try
            {
                Logging.Debug($"Getting {SQUADRON_URL} data");
                var result = await GetEndpointAsync(SQUADRON_URL).ConfigureAwait(false);

                if ( result is null )
                {
                    return null;
                }
                if (!JToken.DeepEquals(result, cachedSquadronJson))
                {
                    cachedSquadronJson = result;
                    cachedSquadronTimeStamp = result["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    Logging.Debug($"{SQUADRON_URL} returned: ", cachedSquadronJson);
                    SquadronUpdatedEvent?.Invoke(this, new CompanionApiEndpointEventArgs( null, null, null, cachedSquadronJson));
                }
                else
                {
                    Logging.Debug($"{SQUADRON_URL} returned, no change from prior cached data.");
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Warn(ex.Message);

                // Reset the timestamp so that we wait for a cooldown before we try again
                cachedSquadronTimeStamp = DateTime.UtcNow;
            }
            return cachedSquadronJson;
        }
    }
}