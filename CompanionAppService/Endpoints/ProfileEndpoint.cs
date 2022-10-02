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
        public static event EventHandler ProfileUpdatedEvent;

        /// <summary>
        /// Contains information about the player's profile and commander
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public Tuple<JObject, DateTime> GetProfile(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedProfileExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{PROFILE_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedProfileJson));
                return new Tuple<JObject, DateTime>(cachedProfileJson, cachedProfileTimeStamp);
            }

            Logging.Debug($"Getting {PROFILE_URL} data");
            var result = GetEndpoint(PROFILE_URL);

            if (!result.Item1?.DeepEquals(cachedProfileJson) ?? false)
            {
                cachedProfileJson = result.Item1;
                cachedProfileTimeStamp = result.Item2;
                Logging.Debug($"{PROFILE_URL} returned " + JsonConvert.SerializeObject(cachedProfileJson));
                ProfileUpdatedEvent?.Invoke(this, new CompanionApiEventArgs(cachedProfileJson, cachedProfileTimeStamp));
            }
            else
            {
                Logging.Debug($"{PROFILE_URL} returned, no change from prior cached data.");
            }

            return result;
        }
    }
}