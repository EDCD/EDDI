using System;
using System.Collections.Generic;
using System.Linq;
using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using RestSharp;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        /// <summary> Partial of system name is required. </summary>
        public List<string> GetTypeAheadStarSystems(string partialSystemName)
        {
            if (string.IsNullOrEmpty(partialSystemName)) { return new List<string>(); }

            var request = new RestRequest("typeahead/systems/query/" + partialSystemName, Method.POST);
            var clientResponse = restClient.Execute<List<JObject>>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JArray responses)
                {
                    List<string> starSystems = responses
                        .AsParallel()
                        .Select(s => ParseTypeAheadSystem(s.ToObject<JObject>()))
                        .Where(s => s != null)
                        .ToList();
                    return starSystems;
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<string>();
        }

        public string ParseTypeAheadSystem(JObject response)
        {
            return (string)response["value"];
        }
    }
}