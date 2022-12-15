using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
        public JObject GetProfile(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedProfileExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{PROFILE_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedProfileJson));
                return cachedProfileJson;
            }

            try
            {
                Logging.Debug($"Getting {PROFILE_URL} data");
                var result = GetEndpoint(PROFILE_URL);

                if (!result?.DeepEquals(cachedProfileJson) ?? false)
                {
                    cachedProfileJson = result;
                    cachedProfileTimeStamp = result["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    Logging.Debug($"{PROFILE_URL} returned " + JsonConvert.SerializeObject(cachedProfileJson));
                    ProfileUpdatedEvent?.Invoke(this, new CompanionApiEndpointEventArgs(CompanionAppService.Instance.ServerURL(), cachedProfileJson, null, null, null));
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
            }
            return cachedProfileJson;
        }
    }
}