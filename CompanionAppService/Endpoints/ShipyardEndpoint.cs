using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;

namespace EddiCompanionAppService.Endpoints
{
    public class ShipyardEndpoint : Endpoint
    {
        private const string SHIPYARD_URL = "/shipyard";

        public JObject GetShipyard()
        {
            JObject result = null;
            try
            {
                Logging.Debug($"Getting {SHIPYARD_URL} data");
                result = GetEndpoint(SHIPYARD_URL);
                Logging.Debug($"{SHIPYARD_URL} returned " + JsonConvert.SerializeObject(result));
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return result;
        }
    }
}