using EddiCompanionAppService.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    public abstract class Endpoint
    {
        public delegate void EndpointEventHandler(object sender, CompanionApiEndpointEventArgs e);

        protected JObject GetEndpoint(string endpointURL)
        {
            JObject newJson = null;
            try
            {
                var result = CompanionAppService.Instance.obtainData(CompanionAppService.Instance.ServerURL() + endpointURL);
                if (result is null) { return null; }
                var data = result.Item1;
                var timestamp = result.Item2;

                if (data == null || !data.StartsWith("{"))
                {
                    Logging.Debug($"{endpointURL} endpoint returned no data");
                    return null;
                }

                try
                {
                    Logging.Debug($"{endpointURL} endpoint returned " + data);
                    newJson = JObject.Parse(data);
                    newJson.Add("timestamp", timestamp);
                }
                catch (JsonException ex)
                {
                    Logging.Error($"Failed to parse Frontier server {endpointURL} data", ex);
                    newJson = null;
                }

            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as telemetry is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            return newJson;
        }
    }

    public class CompanionApiEndpointEventArgs : EventArgs
    {
        public readonly JObject profileJson;

        public readonly JObject marketJson;

        public readonly JObject shipyardJson;

        public readonly JObject fleetCarrierJson;

        public CompanionApiEndpointEventArgs(JObject profileJson, JObject marketJson, JObject shipyardJson, JObject fleetCarrierJson)
        {
            this.profileJson = profileJson;
            this.marketJson = marketJson;
            this.shipyardJson = shipyardJson;
            this.fleetCarrierJson = fleetCarrierJson;
        }
    }
}