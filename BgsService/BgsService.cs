using JetBrains.Annotations;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiBgsService
{
    // This API is high latency - reserve for targeted queries and data not available from any other source.
    public interface IBgsRestClient
    {
        Uri BuildUri(IRestRequest request);
        IRestResponse<T> Execute<T>(IRestRequest request) where T : new();
    }

    public partial class BgsService : IBgsService
    {
        private readonly IBgsRestClient bgsRestClient;

        private const string bgsBaseUrl = "https://elitebgs.app/api/ebgs/";

        private class BgsRestClient : IBgsRestClient
        {
            private readonly RestClient restClient;

            public BgsRestClient(string baseUrl)
            {
                restClient = new RestClient(baseUrl);
            }

            public Uri BuildUri(IRestRequest request) => restClient.BuildUri(request);
            IRestResponse<T> IBgsRestClient.Execute<T>(IRestRequest request) => restClient.Execute<T>(request);
        }

        public BgsService(IBgsRestClient bgsRestClient = null)
        {
            this.bgsRestClient = bgsRestClient ?? new BgsRestClient(bgsBaseUrl);
        }

        /// <summary> Specify the endpoint (e.g. EddiBgsService.Endpoint.factions) and a list of queries as KeyValuePairs </summary>
        public List<object> GetData( [NotNull] IBgsRestClient restClient, [NotNull] string endpoint, [NotNull] List<KeyValuePair<string, object>> queries)
        {
            if ( !queries.Any() || queries.Any( q => 
                    string.IsNullOrEmpty( q.Key ) || 
                    string.IsNullOrEmpty( q.Value.ToString() ) ) ) 
            { return null; }

            var docs = new List<object>();
            var currentPage = 1;

            var request = new RestRequest(endpoint, Method.GET);
            foreach (KeyValuePair<string, object> query in queries)
            {
                request.AddParameter(query.Key, query.Value);
            }

            // Make our initial request
            var response = PageRequest(restClient, request, currentPage);
            if (response != null)
            {
                docs.AddRange(response.docs);
                var totalPages = response.pages;

                // Make additional requests as needed
                while (currentPage < totalPages)
                {
                    var pageResponse = PageRequest(restClient, request, ++currentPage);
                    if (pageResponse != null)
                    {
                        docs.AddRange(pageResponse.docs);
                    }
                }

                Logging.Debug($"Query: {JsonConvert.SerializeObject(request.Parameters)}. {endpoint} returned response: ", docs);
                return docs;
            }
            return null;
        }

        private PageResponse PageRequest(IBgsRestClient restClient, RestRequest request, int page)
        {
            request.AddOrUpdateParameter("page", page);
            var clientResponse = (RestResponse<RestRequest>)restClient.Execute<RestRequest>(request);
            if (clientResponse.IsSuccessful)
            {
                string json = clientResponse.Content;
                var pageResponse = JsonConvert.DeserializeObject<PageResponse>(json);

                if (pageResponse != null && pageResponse.docs.Any())
                {
                    return pageResponse;
                }
            }
            else
            {
                Logging.Warn( $"EliteBGS data error: Error obtaining data from {request.Resource}.",
                    new Dictionary<string, object> { { "request", request }, { "response", clientResponse } } );
            }
            return null; // No results
        }
    }

    class PageResponse
    {
        [JsonProperty("page")]
        public int page { get; set; }

        [JsonProperty("pages")]
        public int pages { get; set; }

        [JsonProperty("limit")]
        public int limit { get; set; }

        [JsonProperty("docs")]
        public IEnumerable<object> docs { get; set; }
    }
}
