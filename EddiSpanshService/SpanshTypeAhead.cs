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
        /// <summary> Partial of system name is required. </summary>
        public List<string> GetTypeAheadStarSystems(string partialSystemName)
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<string>(); }

            var request = TypeAheadRequest(partialSystemName);
            var clientResponse = spanshRestClient.Get(request);

            if (string.IsNullOrEmpty(clientResponse.Content))
            {
                Logging.Warn("Spansh API is not responding");
                return null;
            }

            if (clientResponse.IsSuccessful)
            {
                Logging.Debug("Spansh responded with " + clientResponse.Content);
                var response = JToken.Parse(clientResponse.Content);
                if (response is JObject responses && 
                    responses.ContainsKey("values") && 
                    responses["values"] is JArray systemList)
                {
                    List<string> starSystems = ParseTypeAheadSystems(systemList);
                    return starSystems
                        .OrderByDescending(s => s.StartsWith(partialSystemName, StringComparison.InvariantCultureIgnoreCase))
                        .ThenBy(s => s)
                        .ToList();
                }
            }
            else
            {
                Logging.Debug("Spansh responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<string>();
        }

        private IRestRequest TypeAheadRequest(string partialSystemName)
        {
            var request = new RestRequest("systems/field_values/system_names");
            request.AddParameter("q", partialSystemName);
            return request;
        }

        private List<string> ParseTypeAheadSystems(JToken responses)
        {
            return responses
                .AsParallel()
                .Where(s => s != null)
                .Select(s => s.ToString())
                .ToList();
        }
    }
}