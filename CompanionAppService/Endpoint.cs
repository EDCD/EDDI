using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Utilities;

namespace EddiCompanionAppService
{
    public abstract class Endpoint
    {
        public delegate void EndpointEventHandler(object sender, CompanionApiEndpointEventArgs e);

        protected static async Task<JObject> GetEndpointAsync(string endpointURL)
        {
            if ( CompanionAppService.unitTesting ) { return null; }

            JObject newJson;

            var result = await CompanionAppService.Instance.obtainDataAsync( CompanionAppService.Instance.ServerURL() + endpointURL ).ConfigureAwait(false);
            if ( result is null ) { return null; }

            var data = result.Item1;
            var timestamp = result.Item2;

            if ( data == null || !data.StartsWith( '{' ) )
            {
                Logging.Debug( $"{endpointURL} endpoint returned no data" );
                return null;
            }

            try
            {
                Logging.Debug( $"{endpointURL} endpoint returned " + data );
                newJson = JObject.Parse( data );
                newJson.Add( "timestamp", timestamp );
            }
            catch ( JsonException ex )
            {
                Logging.Error( $"Failed to parse Frontier server {endpointURL} data", ex );
                newJson = null;
            }

            return newJson;
        }
    }

    public class CompanionApiEndpointEventArgs (
        JObject profileJson,
        JObject marketJson,
        JObject shipyardJson,
        JObject fleetCarrierJson )
        : EventArgs
    {
        public readonly JObject profileJson = profileJson;

        public readonly JObject marketJson = marketJson;

        public readonly JObject shipyardJson = shipyardJson;

        public readonly JObject fleetCarrierJson = fleetCarrierJson;
    }
}