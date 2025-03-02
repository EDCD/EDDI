using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class CombinedStationEndpoints : Endpoint
    {
        private const string MARKET_URL = "/market";
        private const string SHIPYARD_URL = "/shipyard";

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
        public async Task<JObject> GetCombinedStationAsync(string expectedCommanderName, string expectedStarSystemName, string expectedStationName, bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedStationExpires > DateTime.UtcNow)
            {
                // return the cached version
                return cachedStationJson;
            }

            var profileJson = await CompanionAppService.Instance.ProfileEndpoint.GetProfileAsync();
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
                    try
                    {
                        // If we're docked, the lastStation information should be located within the lastSystem identified by the profile
                        // Make sure the profile is caught up to the game state
                        var marketJson = await GetMarketAsync();
                        var shipyardJson = await GetShipyardAsync();
                        if ( ( docked || onFoot ) &&
                             !string.IsNullOrEmpty( profileSystemName ) &&
                             expectedStarSystemName == profileSystemName &&
                             profileStationName == expectedStationName &&
                             marketJson != null && profileStationName == marketJson[ "name" ]?.ToString() &&
                             shipyardJson != null && profileStationName == shipyardJson[ "name" ]?.ToString() )
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
                            }.OrderByDescending( d => d ).FirstOrDefault() ?? DateTime.MinValue;
                            StationUpdatedEvent?.Invoke( this, new CompanionApiEndpointEventArgs( profileJson, marketJson, shipyardJson, null ) );
                            return cachedStationJson;
                        }
                    }
                    catch ( EliteDangerousCompanionAppException ex )
                    {
                        // not Logging.Error as telemetry is getting spammed when the server is down
                        Logging.Warn( ex.Message );

                        // Reset the timestamp so that we wait for a cooldown before we try again
                        cachedStationTimeStamp = DateTime.UtcNow;
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
            return await GetCombinedStationAsync(expectedCommanderName, expectedStarSystemName, expectedStationName);
        }

        private async Task<JObject> GetMarketAsync ()
        {
            Logging.Debug( $"Getting {MARKET_URL} data" );
            var result = await GetEndpointAsync( MARKET_URL );
            Logging.Debug( $"{MARKET_URL} returned: ", result );

            return result;
        }

        private async Task<JObject> GetShipyardAsync ()
        {
            Logging.Debug( $"Getting {SHIPYARD_URL} data" );
            var result = await GetEndpointAsync( SHIPYARD_URL );
            Logging.Debug( $"{SHIPYARD_URL} returned: ", result );

            return result;
        }
    }
}
