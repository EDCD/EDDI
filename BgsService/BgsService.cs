using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiBgsService
{
    public partial class BgsService
    {
        // This API is high latency - reserve for targeted queries and data not available from any other source.
        private const string baseUrl = "https://elitebgs.app/api/ebgs/";
        private static RestClient client = new RestClient(baseUrl);

        /// <summary> Specify the endpoint (e.g. EddiBgsService.Endpoint.factions) and a list of queries as KeyValuePairs </summary>
        public static List<object> GetData(string endpoint, List<KeyValuePair<string, object>> queries)
        {
            if (queries == null) { return null; }

            var docs = new List<object>();
            int currentPage = 1;
            int totalPages = 0;

            RestRequest request = new RestRequest(endpoint, Method.GET);
            foreach (KeyValuePair<string, object> query in queries)
            {
                request.AddParameter(query.Key, query.Value);
            }

            // Make our initial request
            PageResponse response = PageRequest(request, currentPage);
            if (response != null)
            {
                docs.AddRange(response.docs);
                totalPages = response.pages;

                // Make additional requests as needed
                while (currentPage < totalPages)
                {
                    PageResponse pageResponse = PageRequest(request, ++currentPage);
                    if (pageResponse != null)
                    {
                        docs.AddRange(pageResponse.docs);
                    }
                }

                return docs;
            }
            return null;
        }

        private static PageResponse PageRequest(RestRequest request, int page)
        {
            request.AddOrUpdateParameter("page", page);

            DateTime startTime = DateTime.UtcNow;

            RestResponse<RestRequest> clientResponse = (RestResponse<RestRequest>)client.Execute<RestRequest>(request);
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
                Logging.Debug("EliteBGS data error: Error obtaining data from " + request.Resource + ". Query: " + request.Parameters.ToArray());
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
