using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="cancellationToken">A task cancellation token.</param>
        /// <returns>A list of basic system waypoints (with just system name, system address, and coordinates) ordered by match with the provided system name</returns>
        public async Task<List<NavWaypoint>> GetWaypointsBySystemNameAsync (string partialSystemName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<NavWaypoint>(); }

            try
            {
                var requestUri = PrepareRequest( partialSystemName );
                var clientResponse = await spanshHttpClient.GetAsync( requestUri, cancellationToken ).ConfigureAwait(false);
                clientResponse.EnsureSuccessStatusCode();
                var responseJson = await clientResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Unable to handle server response." );
                    return [ ];
                }

                Logging.Debug( "Spansh responded with " + responseJson );
                var response = JToken.Parse( responseJson );
                if ( response is JObject responses && responses.ContainsKey( "values" ) )
                {
                    var starSystems = ParseTypeAheadSystems( responses );
                    return starSystems
                        .OrderByDescending( s =>
                            s.systemName.Equals( partialSystemName, StringComparison.InvariantCultureIgnoreCase ) )
                        .ToList();
                }
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }
            catch ( HttpRequestException he )
            {
                Logging.Warn( he.Message, he );
            }

            return [ ];
        }

        private static string PrepareRequest (string partialSystemName)
        {
            var requestUri = $"systems/field_values/system_names?q={partialSystemName}";
            return requestUri;
        }

        private List<NavWaypoint> ParseTypeAheadSystems ( JToken responses )
        {
            return responses[ "min_max" ]?
                .Select( ParseTypeAheadSystem ).RemoveNulls().ToList();
        }

        private NavWaypoint ParseTypeAheadSystem ( JToken r )
        {
            try
            {
                return new NavWaypoint( r[ "name" ].ToString(), r[ "id64" ].Value<ulong>(), r[ "x" ].Value<decimal>(), r[ "y" ].Value<decimal>(), r[ "z" ].Value<decimal>() );
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse star system name data: {e.Message}", e );
                return null;
            }
        }
    }
}