using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        /// <summary>
        /// Star system waypoints ordered by system name, for use in type-ahead functions or for obtaining just the system address and coordinates of a named system.
        /// </summary>
        /// <param name="partialSystemName">At least a partial system name is required.</param>
        /// <param name="cancellationToken">A task cancellation token</param>
        /// <returns>A list of basic system waypoints (with just system name, system address, and coordinates) ordered by match with the provided system name</returns>
        public async Task<List<string>> GetTypeAheadSystemNamesAsync ( string partialSystemName, CancellationToken cancellationToken )
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<string>(); }

            try
            {
                var requestUri = $"systems?q={partialSystemName}";
                var clientResponse = await spanshHttpClient.GetAsync( requestUri, cancellationToken ).ConfigureAwait(false);
                clientResponse.EnsureSuccessStatusCode();
                var responseJson = await clientResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Unable to handle server response." );
                    return new List<string>();
                }
                return JsonConvert.DeserializeObject<List<string>>(responseJson);
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }
            catch ( HttpRequestException he )
            {
                Logging.Warn( he.Message, he );
            }

            return null;
        }
    }
}