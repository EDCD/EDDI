using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class ProfileEndpoint : Endpoint
    {
        private const string PROFILE_URL = "/profile";

        // We cache the profile to avoid spamming the service
        private JObject cachedProfileJson;
        private DateTime cachedProfileTimeStamp;
        private DateTime cachedProfileExpires => cachedProfileTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public event EndpointEventHandler ProfileUpdatedEvent;

        /// <summary>
        /// Contains information about the player's profile and commander
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public async Task<JObject> GetProfileAsync(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedProfileExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{PROFILE_URL} endpoint queried too often. Returning cached data: ", cachedProfileJson);
                return cachedProfileJson;
            }

            try
            {
                Logging.Debug($"Getting {PROFILE_URL} data");
                var result = await GetEndpointAsync(PROFILE_URL);

                if ( result is null )
                {
                    return null;
                }
                if (!JToken.DeepEquals(result, cachedProfileJson))
                {
                    cachedProfileJson = result;
                    cachedProfileTimeStamp = result["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    Logging.Debug($"{PROFILE_URL} returned: ", cachedProfileJson);
                    ProfileUpdatedEvent?.Invoke(this, new CompanionApiEndpointEventArgs( cachedProfileJson, null, null, null));
                }
                else
                {
                    Logging.Debug($"{PROFILE_URL} returned, no change from prior cached data.");
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Warn(ex.Message);

                // Reset the timestamp so that we wait for a cooldown before we try again
                cachedProfileTimeStamp = DateTime.UtcNow;
            }
            return cachedProfileJson;
        }
    }
}