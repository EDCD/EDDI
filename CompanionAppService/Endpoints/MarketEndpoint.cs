using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class MarketEndpoint : Endpoint
    {
        private const string MARKET_URL = "/market";

        // We cache the market to avoid spamming the service
        private static JObject cachedMarketJson;
        private static DateTime cachedMarketTimeStamp;
        private static DateTime cachedMarketExpires => cachedMarketTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public static event EventHandler MarketUpdatedEvent;

        /// <summary>
        /// Contains information about the market at the last station visited
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public Tuple<JObject, DateTime> GetMarket(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedMarketExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{MARKET_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedMarketJson));
                return new Tuple<JObject, DateTime>(cachedMarketJson, cachedMarketTimeStamp);
            }

            try
            {
                Logging.Debug($"Getting {MARKET_URL} data");
                var result = GetEndpoint(MARKET_URL);

                if (!result.Item1?.DeepEquals(cachedMarketJson) ?? false)
                {
                    cachedMarketJson = result.Item1;
                    cachedMarketTimeStamp = result.Item2;
                    Logging.Debug($"{MARKET_URL} returned " + JsonConvert.SerializeObject(cachedMarketJson));
                    MarketUpdatedEvent?.Invoke(cachedMarketJson, EventArgs.Empty);
                }
                else
                {
                    Logging.Debug($"{MARKET_URL} returned, no change from prior cached data.");
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return new Tuple<JObject, DateTime>(cachedMarketJson, cachedMarketTimeStamp);
        }
    }
}