using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Generic;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        public enum QueryGroup
        {
            bodies, 
            stations, 
            systems
        }

        [CanBeNull]
        public JToken Query ( QueryGroup queryGroup, [NotNull] Dictionary<string, object> searchFilters, int? maxResults = 500, int? pageId = 0 )
        {
            var request = GetRestRequest( queryGroup, searchFilters, maxResults, pageId );
            var response = spanshRestClient.Post( request );
            return response is null ? null : JToken.Parse( response.Content );
        }

        [ CanBeNull ]
        public JToken DistanceOrderedQuery ( QueryGroup queryGroup, decimal fromX, decimal fromY, decimal fromZ, [NotNull] Dictionary<string, object> searchFilters )
        {
            var request = GetDistanceOrderedRestRequest( queryGroup, fromX, fromY, fromZ, searchFilters );
            var response = spanshRestClient.Post( request );
            return response is null ? null : JToken.Parse( response.Content );
        }

        // TODO: Handle multi-page responses (see BgsService for example)
        private static IRestRequest GetRestRequest ( QueryGroup queryGroup, [NotNull] Dictionary<string, object> filter, int? maxResults, int? pageId )
        {
            var request = new RestRequest($@"{queryGroup}/search") { Method = Method.POST };
            var jsonObject = new
            {
                filters = filter,
                size = maxResults,
                page = pageId
            };
            request.AddJsonBody( JsonConvert.SerializeObject( jsonObject ) );
            return request;
        }

        private static IRestRequest GetDistanceOrderedRestRequest ( QueryGroup queryGroup, decimal fromX, decimal fromY, decimal fromZ, [NotNull] Dictionary<string, object> filter )
        {
            var request = new RestRequest($@"{queryGroup}/search") { Method = Method.POST };
            var jsonObject = new
            {
                filters = filter,
                sort = new List<object>
                {
                    new { distance = new { direction = "asc" } },
                    new { distance_to_arrival = new { direction = "asc" } }
                },
                size = 10,
                page = 0,
                reference_coords = new Dictionary<string, object>
                {
                    { "x", fromX }, { "y", fromY }, { "z", fromZ }
                }
            };
            request.AddJsonBody( JsonConvert.SerializeObject( jsonObject ) );
            return request;
        }
    }
}