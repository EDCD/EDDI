using EddiCompanionAppService.Exceptions;
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
        private JObject cachedMarketJson;
        private DateTime cachedMarketTimeStamp;
        private DateTime cachedMarketExpires => cachedMarketTimeStamp.AddSeconds(30);

        // Set up an event handler for data changes
        public event EventHandler MarketUpdatedEvent;

        /// <summary>
        /// Contains information about the market at the last station visited
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <returns></returns>
        public JObject GetMarket(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedMarketExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug($"{MARKET_URL} endpoint queried too often. Returning cached data " + JsonConvert.SerializeObject(cachedMarketJson));
                return cachedMarketJson;
            }

            try
            {
                Logging.Debug($"Getting {MARKET_URL} data");
                var result = GetEndpoint(MARKET_URL);

                if (!result?.DeepEquals(cachedMarketJson) ?? false)
                {
                    cachedMarketJson = result;
                    cachedMarketTimeStamp = result["timestamp"]?.ToObject<DateTime?>() ?? DateTime.MinValue;
                    Logging.Debug($"{MARKET_URL} returned " + JsonConvert.SerializeObject(cachedMarketJson));
                    MarketUpdatedEvent?.Invoke(cachedMarketJson, new CompanionApiEventArgs(cachedMarketJson));
                }
                else
                {
                    Logging.Debug($"{MARKET_URL} returned, no change from prior cached data.");
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return cachedMarketJson;
        }
    }
}