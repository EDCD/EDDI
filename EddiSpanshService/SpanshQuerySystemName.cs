using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        /// <returns>A list of basic system waypoints (with just system name, system address, and coordinates) ordered by match with the provided system name</returns>
        public async Task<List<NavWaypoint>> GetWaypointsBySystemNameAsync (string partialSystemName)
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<NavWaypoint>(); }

            try
            {
                var requestUri = PrepareRequest( partialSystemName );
                var clientResponse = await spanshHttpClient.GetAsync( requestUri ).ConfigureAwait( false );
                clientResponse.EnsureSuccessStatusCode();
                var responseJson = await clientResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Unable to handle server response." );
                    return new List<NavWaypoint>();
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
            catch ( HttpRequestException he )
            {
                Logging.Error( he.Message, he );
                return null;
            }

            return new List<NavWaypoint>();
        }

        private string PrepareRequest(string partialSystemName)
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
                return new NavWaypoint( r[ "name" ].ToString(), r[ "id64" ].ToObject<ulong>(), r[ "x" ].ToObject<decimal>(), r[ "y" ].ToObject<decimal>(), r[ "z" ].ToObject<decimal>() );
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse star system name data: {e.Message}", e );
                return null;
            }
        }
    }
}