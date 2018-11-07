using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddbService
{
    public partial class EddbService
    {
        // Data currently solely available from EDDB:
        // - Station: Import commodities
        // - Station: Export commodities
        // - Station: Prohibited commodities 
        // - Responses to complex multi-parameter queries

        // Since EDDB doesn't have an official API, we make use of the "unofficial" API
        // This API is high latency - reserve for targeted queries and data not available from any other source.
        private const string baseUrl = "https://elitebgs.kodeblox.com/api/eddb/";
        private static RestClient client = new RestClient(baseUrl);
        
        /// <summary> Specify the endpoint (e.g. EddiEddbService.Endpoint.bodies) and a list of queries as KeyValuePairs </summary>
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
            DateTime requestTime = DateTime.UtcNow;

            PageResponse response = PageRequest(request, currentPage);
            if (response != null)
            {
                docs.AddRange(response.docs);
                totalPages = response.pages;

                // Make additional requests as needed
                while (currentPage < totalPages && (DateTime.UtcNow - requestTime).TotalMilliseconds > 5)
                {
                    requestTime = DateTime.UtcNow;
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
                Logging.Info("Page response latency (ms): " + (DateTime.UtcNow - startTime).Milliseconds);

                string json = clientResponse.Content;
                var pageResponse = JsonConvert.DeserializeObject<PageResponse>(json);

                if (pageResponse != null && pageResponse.docs.Any())
                {
                    return pageResponse;
                }
            }
            else
            {
                Logging.Debug("Eddb data error: Error obtaining data from " + request.Resource + ". Query: " + request.Parameters.ToArray());
            }
            return null; // No results
        }
    }

    public class Endpoint
    {
        public const string bodies = "v3/bodies?";
        public const string factions = "v3/factions?";
        public const string populatedSystems = "v3/populatedsystems?";
        public const string stations = "v3/stations?";
        public const string systems = "v3/systems?";
    }

    public class PageResponse
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

    public class SystemQuery
    {
        /// <summary> EDDB ID. </summary>
        public const string eddbId = "eddbid";

        /// <summary> System name. </summary>
        public const string systemName = "name";

        /// <summary> Name of the allegiance. </summary>
        public const string allegiance = "allegiancename";

        /// <summary> Name of the government type. </summary>
        public const string government = "governmentname";

        /// <summary> State the system is in. </summary>
        public const string state = "statename";

        /// <summary> The primary economy of the system. </summary>
        public const string primaryEconomy = "primaryeconomyname";

        /// <summary> Comma seperated names of powers in influence in the system. </summary>
        public const string power = "power";

        /// <summary> Comma seperated states of the powers in influence in the system. </summary>
        public const string powerState = "powerstatename";

        /// <summary> Whether the system is permit locked. </summary>
        public const string permit = "permit";

        /// <summary> The name of the security status in the system. </summary>
        public const string security = "securityname";

        /// <summary> Name of a faction present in the system. </summary>
        public const string factionName = "factionname";

        /// <summary> Presence type of the faction. </summary>
        public const string presenceType = "presencetype";
    }

    public class FactionQuery
    {
        /// <summary> EDDB ID. </summary>
        public const string eddbId = "eddbid";

        /// <summary> Faction name. </summary>
        public const string factionName = "name";

        /// <summary> Name of the allegiance. </summary>
        public const string allegiance = "allegiancename";

        /// <summary> Name of the government type. </summary>
        public const string government = "governmentname";

        /// <summary> State the faction is in. </summary>
        public const string state = "statename";

        /// <summary> Whether the faction is a player faction. (value must be boolean) </summary>
        public const string playerFaction = "playerfaction";

        /// <summary> Name of the power in influence in a system the faction is in. </summary>
        public const string power = "power";

        /// <summary> Name of the home system of the faction. </summary>
        public const string homeSystem = "homesystemname";
    }

    public class StationQuery
    {
        /// <summary> EDDB ID. </summary>
        public const string eddbId = "eddbid";

        /// <summary> Station name. </summary>
        public const string stationName = "name";

        /// <summary> Comma seperated names of ships sold. </summary>
        public const string shipsSold = "ships";

        /// <summary> Comma seperated ids of modules sold. </summary>
        public const string moduleIds = "moduleid";

        /// <summary> Name of the controlling minor faction. </summary>
        public const string controllingFaction = "controllingfactionname";

        /// <summary> Name of the allegiance. </summary>
        public const string allegiance = "allegiancename";

        /// <summary> Name of the government type. </summary>
        public const string government = "governmentname";

        /// <summary> Minimum landing pad size available. </summary>
        public const string minimumLandingPad = "minlandingpad";

        /// <summary> Maximum distance from the star. </summary>
        public const string distanceFromStar = "distancestar";

        /// <summary> Comma seperated names of facilities available in the station. </summary>
        public const string facilities = "facilities";

        /// <summary> Comma seperated names of commodities available. </summary>
        public const string commodities = "commodities";

        /// <summary> Comma seperated types of station. </summary>
        public const string stationType = "stationtypename";

        /// <summary> Whether the station is planetary.	(value must be a boolean) </summary>
        public const string planetary = "planetary";

        /// <summary> The economy of the station. </summary>
        public const string economy = "economyname";

        /// <summary> Whether the system where the station exists is permit locked.	</summary>
        public const string permit = "permit";

        /// <summary> Comma seperated names of powers in influence in the system the station is in. </summary>
        public const string power = "power";

        /// <summary> Comma seperated states of the powers in influence in the system the station is in. </summary>
        public const string powerState = "powerstatename";

        /// <summary> System name. </summary>
        public const string systemName = "systemname";
    }

    public class BodyQuery
    {
        /// <summary> EDDB ID. </summary>
        public const string eddbId = "eddbid";

        /// <summary> Body name. </summary>
        public const string bodyName = "name";

        /// <summary> Comma seperated material names. </summary>
        public const string materials = "materials";

        /// <summary> System name. </summary>
        public const string systemName = "systemname";

        /// <summary> Reserve type of the system. </summary>
        public const string reserve = "reservetypename";

        /// <summary> Whether the system is populated. </summary>
        public const string isPopulated = "ispopulated";

        /// <summary> Name of the power in influence in the system. </summary>
        public const string power = "power";

        /// <summary> Name of type of ring. </summary>
        public const string ringComposition = "ringtypename";

        /// <summary> Comma seperated names of group of body. </summary>
        public const string bodyGroupName = "bodygroupname";

        /// <summary> Whether the body has rings. </summary>
        public const string hasRings = "hasrings";

        /// <summary> Comma seperated names of type of body. </summary>
        public const string bodyTypeName = "bodytypename";

        /// <summary> Distance to arrival of the body. </summary>
        public const string distanceToArrival = "distancearrival";

        /// <summary> Whether the star is a main star. </summary>
        public const string isMainStar = "ismainstar";

        /// <summary> Comma seperated specular classes of the star. </summary>
        public const string spectralClasses = "specclass";

        /// <summary> Comma seperated luminosity classes of the star. </summary>
        public const string luminosityClasses = "lumoclass";

        /// <summary> Whether the body is landable. </summary>
        public const string landable = "landable";
    }
}
