using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Utilities;

namespace EddiStarMapService
{
    public interface IEdsmRestClient
    {
        Uri BuildUri(IRestRequest request);
        IRestResponse<T> Execute<T>(IRestRequest request) where T : new();
    }

    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public partial class StarMapService : IEdsmService
    {
        // Set the maximum batch size we will use for syncing before we write systems to our sql database
        public const int syncBatchSize = 50;

        public static string commanderFrontierApiName { get; set; }

        private string commanderName { get; set; }
        private string apiKey { get; set; }
        private readonly IEdsmRestClient restClient;

        // For normal use, the EDSM API base URL is https://www.edsm.net/.
        // If you need to do some testing on EDSM's API, please use the https://beta.edsm.net/ endpoint for sending data.
        private const string baseUrl = "https://www.edsm.net/";

        private class EdsmRestClient : IEdsmRestClient
        {
            private readonly RestClient restClient;

            public EdsmRestClient(string baseUrl)
            {
                restClient = new RestClient(baseUrl);
            }

            public Uri BuildUri(IRestRequest request) => restClient.BuildUri(request);
            IRestResponse<T> IEdsmRestClient.Execute<T>(IRestRequest request) => restClient.Execute<T>(request);
        }

        public StarMapService(IEdsmRestClient restClient = null)
        {
            this.restClient = restClient ?? new EdsmRestClient(baseUrl);

            // Set up the star map service
            StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
            if (!string.IsNullOrEmpty(starMapCredentials?.apiKey))
            {
                // Commander name might come from EDSM credentials or from the game and companion app
                string cmdrName = starMapCredentials?.commanderName ?? commanderFrontierApiName;
                if (!string.IsNullOrEmpty(cmdrName))
                {
                    apiKey = starMapCredentials.apiKey?.Trim();
                    commanderName = cmdrName?.Trim();
                }
                else
                {
                    Logging.Warn("No StarMapService instance: Commander name not set.");
                }
            }
            else
            {
                Logging.Warn("No StarMapService instance: API key not set.");
            }
        }

        public void sendEvent(string eventData)
        {
            // The EDSM responder has a `inBeta` flag that it checks prior to sending data via this method.  
            var request = new RestRequest("api-journal-v1", Method.POST);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);
            request.AddParameter("message", eventData);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending event to EDSM: " + restClient.BuildUri(request).AbsoluteUri);
                    var clientResponse = restClient.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    if (response?.msgnum != 100)
                    {
                        if (response?.msg != null)
                        {
                            Logging.Warn("EDSM responded with " + response?.msg);
                        }
                        else
                        {
                            Logging.Warn(clientResponse?.ErrorMessage);
                        }
                    }
                    Thread.Sleep(200); // Add a buffer to prevent time outs.
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send event to EDSM", ex);
                }
            })
            {
                IsBackground = true,
                Name = "StarMapService send event"
            };
            thread.Start();
        }

        public void sendStarMapComment(string systemName, string comment)
        {

            var request = new RestRequest("api-logs-v1/set-comment", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("systemName", systemName);
            request.AddParameter("comment", comment);

            Thread thread = new Thread(() =>
            {
                try
                {
                    var clientResponse = restClient.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    if (response?.msgnum != 100)
                    {
                        Logging.Warn("EDSM responded with " + response.msg ?? clientResponse.ErrorMessage);
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send comment to EDSM", ex);
                }
            })
            {
                IsBackground = true,
                Name = "StarMapService send starmap comment"
            };
            thread.Start();
        }

        public List<string> getIgnoredEvents()
        {
            var request = new RestRequest("api-journal-v1/discard", Method.POST);
            var clientResponse = restClient.Execute<List<string>>(request);
            List<string> response = clientResponse.Data;
            return response ?? null;
        }

        public Dictionary<string, string> getStarMapComments()
        {
            var request = new RestRequest("api-logs-v1/get-comments", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            var starMapCommentResponse = restClient.Execute<StarMapCommentResponse>(request);
            StarMapCommentResponse response = starMapCommentResponse.Data;

            Dictionary<string, string> vals = new Dictionary<string, string>();
            if (response != null && response.comments != null)
            {
                foreach (StarMapResponseCommentEntry entry in response.comments)
                {
                    if (entry.comment != null && entry.comment != "")
                    {
                        Logging.Debug("Comment found for " + entry.system);
                        vals[entry.system] = entry.comment;
                    }
                }
            }
            return vals;
        }

        public List<StarMapResponseLogEntry> getStarMapLog(DateTime? since = null, string[] systemNames = null)
        {
            var request = new RestRequest("api-logs-v1/get-logs", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("showId", 1); // Obtain EDSM IDs
            if (systemNames?.Count() == 1)
            {
                /// When a single system name is provided, the api responds with 
                /// the complete flight logs for that star system
                request.AddParameter("systemName", systemNames[0]);
            }
            else
            {
                if (since.HasValue)
                {
                    request.AddParameter("startdatetime", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    /// Though not documented in the api, Anthor from EDSM has confirmed that this 
                    /// unpublished parameter is valid and overrides "startdatetime" and "enddatetime".
                    request.AddParameter("fullSync", 1);
                }
            }
            var starMapLogResponse = restClient.Execute<StarMapLogResponse>(request);
            StarMapLogResponse response = starMapLogResponse.Data;

            if (response != null)
            {
                Logging.Debug("Response for star map logs is " + JsonConvert.SerializeObject(response));
                if (response.msgnum != 100)
                {
                    // An error occurred
                    throw new EDSMException(response.msg);
                }
            }
            else
            {
                Logging.Debug("No response received.");
                throw new EDSMException();
            }

            if (response?.logs != null)
            {
                if (systemNames?.Count() > 0)
                {
                    response.logs.RemoveAll(s => !systemNames.Contains(s.system));
                }
                return response.logs;
            }
            else
            {
                return null;
            }
        }
    }

    // response from the Star Map log API
    class StarMapResponse
    {
        public string content { get; set; }
        public Dictionary<string, object> data { get; set; }
        public bool isSuccessful { get; set; }
        public string errorMessage { get; set; }
        public Exception errorException { get; set; }
    }

    class StarMapLogResponse
    {
        public int msgnum { get; set; }
        public string msg { get; set; }
        public string comment { get; set; }
        public DateTime? lastUpdate { get; set; }
        public List<StarMapResponseLogEntry> logs { get; set; }
    }

    public class StarMapResponseLogEntry
    {
        public string system { get; set; } // System name

        public long systemId { get; set; } // EDSM ID
        public DateTime date { get; set; }
    }

    // response from the Star Map comment API
    class StarMapCommentResponse
    {
        public int msgnum { get; set; }
        public string msg { get; set; }
        public string comment { get; set; }
        public DateTime? lastUpdate { get; set; }
        public List<StarMapResponseCommentEntry> comments { get; set; }
    }

    class StarMapResponseCommentEntry
    {
        public string system { get; set; }
        public string comment { get; set; }
    }

    // public consolidated version of star map log information
    public class StarMapInfo
    {
        public int Visits { get; set; }
        public DateTime? LastVisited { get; set; }
        public string Comment { get; set; }

        public StarMapInfo(int visits, DateTime? lastVisited, string comment)
        {
            this.Visits = visits;
            this.LastVisited = lastVisited;
            this.Comment = comment;
        }
    }

    // Custom serializer for REST requests
    public class NewtonsoftJsonSerializer : ISerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer;

        public NewtonsoftJsonSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public string ContentType
        {
            get { return "application/json"; } // Probably used for Serialization?
            set { }
        }

        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonTextWriter, obj);
                return stringWriter.ToString();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public T Deserialize<T>(RestSharp.IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static NewtonsoftJsonSerializer Default
        {
            get
            {
                return new NewtonsoftJsonSerializer(new Newtonsoft.Json.JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
            }
        }
    }
}
