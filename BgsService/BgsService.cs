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
        // This API only returns data for the "live" galaxy, game version 4.0 or later.
        private static readonly System.Version minGameVersion = new System.Version(4, 0);
        private static System.Version currentGameVersion { get; set; }

        public readonly IBgsRestClient bgsRestClient;
        public readonly IBgsRestClient eddbRestClient;

        private const string bgsBaseUrl = "https://elitebgs.app/api/ebgs/";
        private const string eddbBaseUrl = "https://eddbapi.elitebgs.app/api/";

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

        public BgsService(IBgsRestClient bgsRestClient = null, IBgsRestClient eddbRestClient = null)
        {
            this.bgsRestClient = bgsRestClient ?? new BgsRestClient(bgsBaseUrl);
            this.eddbRestClient = eddbRestClient ?? new BgsRestClient(eddbBaseUrl);
        }

        /// <summary> Specify the endpoint (e.g. EddiBgsService.Endpoint.factions) and a list of queries as KeyValuePairs </summary>
        public List<object> GetData(IBgsRestClient restClient, string endpoint, List<KeyValuePair<string, object>> queries)
        {
            if (!(queries?.Any() ?? false)) { return null; }
            if (currentGameVersion != null && currentGameVersion < minGameVersion) { return null; }

            var docs = new List<object>();
            var currentPage = 1;

            RestRequest request = new RestRequest(endpoint, Method.GET);
            foreach (KeyValuePair<string, object> query in queries)
            {
                request.AddParameter(query.Key, query.Value);
            }

            // Make our initial request
            PageResponse response = PageRequest(restClient, request, currentPage);
            if (response != null)
            {
                docs.AddRange(response.docs);
                var totalPages = response.pages;

                // Make additional requests as needed
                while (currentPage < totalPages)
                {
                    PageResponse pageResponse = PageRequest(restClient, request, ++currentPage);
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

        public static void SetGameVersion(System.Version version)
        {
            currentGameVersion = version;
            if (currentGameVersion != null && currentGameVersion < minGameVersion)
            {
                Logging.Warn($"Service disabled. Game version is {currentGameVersion}, service returns data for version {minGameVersion} or later.");
            }
        }

        private PageResponse PageRequest(IBgsRestClient restClient, RestRequest request, int page)
        {
            request.AddOrUpdateParameter("page", page);

            Logging.Debug($"Query: {JsonConvert.SerializeObject(request.Parameters)}. Sending request to {request.Resource}");
            var clientResponse = (RestResponse<RestRequest>)restClient.Execute<RestRequest>(request);
            Logging.Debug("Response received: ", clientResponse);
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
                Logging.Debug($"EliteBGS data error: Error obtaining data from {request.Resource}.", clientResponse);
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
