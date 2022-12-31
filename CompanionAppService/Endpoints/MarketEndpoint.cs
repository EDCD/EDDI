using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class MarketEndpoint : Endpoint
    {
        private const string MARKET_URL = "/market";

        public JObject GetMarket()
        {
            JObject result = null;
            try
            {
                Logging.Debug($"Getting {MARKET_URL} data");
                result = GetEndpoint(MARKET_URL);
                Logging.Debug($"{MARKET_URL} returned: ", result);

            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Warn(ex.Message);
            }

            return result;
        }
    }
}