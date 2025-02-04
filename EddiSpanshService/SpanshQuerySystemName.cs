using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<NavWaypoint> GetWaypointsBySystemName (string partialSystemName)
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<NavWaypoint>(); }

            var request = PrepareRequest(partialSystemName);
            var clientResponse = spanshRestClient.Get(request);

            if (clientResponse.IsSuccessful)
            {
                if ( string.IsNullOrEmpty( clientResponse.Content ) )
                {
                    Logging.Warn( "Unable to handle server response." );
                    return new List<NavWaypoint>();
                }

                Logging.Debug("Spansh responded with " + clientResponse.Content);
                var response = JToken.Parse(clientResponse.Content);
                if (response is JObject responses && 
                    responses.ContainsKey("values"))
                {
                    var starSystems = ParseTypeAheadSystems(responses);
                    return starSystems
                        .OrderByDescending(s => s.systemName.Equals( partialSystemName, StringComparison.InvariantCultureIgnoreCase ) )
                        .ToList();
                }
            }
            else
            {
                Logging.Warn("Spansh responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<NavWaypoint>();
        }

        private IRestRequest PrepareRequest(string partialSystemName)
        {
            var request = new RestRequest("systems/field_values/system_names");
            request.AddParameter("q", partialSystemName);
            return request;
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