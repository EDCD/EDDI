using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class CombinedStationEndpoints : Endpoint
    {
        // We cache the profile to avoid spamming the service
        private JObject cachedStationJson;
        private DateTime cachedStationTimeStamp;
        public DateTime cachedStationExpires => cachedStationTimeStamp.AddSeconds(30);

        private static int retries;

        // Set up an event handler for data changes
        public event EndpointEventHandler StationUpdatedEvent;

        /// <summary>
        /// Returns combined profile, market, and shipyard data (verifying that the data is synchronized to expected values between endpoints)
        /// </summary>
        public JObject GetCombinedStation(string expectedCommanderName, string expectedStarSystemName, string expectedStationName, bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedStationExpires > DateTime.UtcNow)
            {
                // return the cached version
                return cachedStationJson;
            }

            var profileJson = CompanionAppService.Instance.ProfileEndpoint.GetProfile();
            if (profileJson != null)
            {
                var profileCmdrName = profileJson["commander"]?["name"]?.ToString();
                var docked = profileJson["commander"]?["docked"]?.ToObject<bool?>() ?? false;
                var onFoot = profileJson["commander"]?["onfoot"]?.ToObject<bool?>() ?? false;

                var profileSystemName = profileJson["lastSystem"]?["name"]?.ToString();
                var profileStationName = (profileJson["lastStarport"]?["name"])?.ToString().ReplaceEnd('+');

                // Make sure that the Frontier API is configured to return data for the correct commander
                if (string.IsNullOrEmpty(profileCmdrName) || profileCmdrName == expectedCommanderName)
                {
                    // If we're docked, the lastStation information should be located within the lastSystem identified by the profile
                    // Make sure the profile is caught up to the game state
                    var marketJson = new MarketEndpoint().GetMarket();
                    var shipyardJson = new ShipyardEndpoint().GetShipyard();
                    if ((docked || onFoot) &&
                        !string.IsNullOrEmpty(profileSystemName) &&
                        expectedStarSystemName == profileSystemName &&
                        profileStationName == expectedStationName &&
                        marketJson != null && profileStationName == marketJson["name"]?.ToString() &&
                        shipyardJson != null && profileStationName == shipyardJson["name"]?.ToString())
                    {
                        // Data is up to date, we can proceed
                        retries = 0;
                        cachedStationJson = new JObject
                        {
                            { "profileJson", profileJson },
                            { "marketJson", marketJson },
                            { "shipyardJson", shipyardJson }
                        };
                        cachedStationTimeStamp = new List<DateTime?>
                        {
                            profileJson["timestamp"]?.ToObject<DateTime?>(), 
                            marketJson["timestamp"]?.ToObject<DateTime?>(), 
                            shipyardJson["timestamp"]?.ToObject<DateTime?>()
                        }.OrderByDescending(d => d).FirstOrDefault() ?? DateTime.MinValue;
                        StationUpdatedEvent?.Invoke(this, new CompanionApiEndpointEventArgs(CompanionAppService.Instance.ServerURL(), profileJson, marketJson, shipyardJson, null));
                        return cachedStationJson;
                    }
                }
                else
                {
                    Logging.Warn("Frontier API incorrectly configured: Returning information for Commander " +
                                 $"'{profileCmdrName}' rather than for expected Commander '{expectedCommanderName}'. Disregarding incorrect information.");
                    return null;
                }
            }

            // Data acquisition not successful, delay and retry (up to 5 times)
            if (retries >= 5) { return null; }
            retries += 1;
            Thread.Sleep(TimeSpan.FromSeconds(10));
            return GetCombinedStation(expectedCommanderName, expectedStarSystemName, expectedStationName);
        }
    }
}
