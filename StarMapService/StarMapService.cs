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
    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public partial class StarMapService
    {
        // Set the maximum batch size we will use for syncing before we write systems to our sql database
        public const int syncBatchSize = 50;

        public static string commanderFrontierApiName { get; set; }

        private string commanderName { get; set; }
        private string apiKey { get; set; }
        private static string baseUrl = "https://www.edsm.net/";

        // For normal use, the EDSM API base URL is https://www.edsm.net/.
        // If you need to do some testing on EDSM's API, please use the https://beta.edsm.net/ endpoint for sending data.
        public StarMapService(string apikey, string commandername, string baseURL = "https://www.edsm.net/")
        {
            apiKey = apikey?.Trim();
            commanderName = commandername?.Trim();
            baseUrl = baseURL;
        }

        private static readonly object instanceLock = new object();
        private static StarMapService instance;
        public static StarMapService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No StarMapService instance: creating one");

                            // Set up the star map service
                            StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
                            if (!string.IsNullOrEmpty(starMapCredentials?.apiKey))
                            {
                                // Commander name might come from EDSM credentials or from the game and companion app
                                string commanderName = starMapCredentials?.commanderName ?? commanderFrontierApiName;
                                if (!string.IsNullOrEmpty(commanderName))
                                {
                                    instance = new StarMapService(starMapCredentials.apiKey, commanderName);
                                    Logging.Info("Configuring EDDI access to EDSM profile data");
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
                    }
                }
                return instance;
            }
        }

        public void sendEvent(string eventData)
        {
            // The EDSM responder has a `inBeta` flag that it checks prior to sending data via this method.  
            var client = new RestClient(baseUrl);
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
                    Logging.Debug("Sending event to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/set-comment", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("systemName", systemName);
            request.AddParameter("comment", comment);

            Thread thread = new Thread(() =>
            {
                try
                {
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
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
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-journal-v1/discard", Method.POST);
            var clientResponse = client.Execute<List<string>>(request);
            List<string> response = clientResponse.Data;
            return response ?? null;
        }

        public string getStarMapComment(string systemName)
        {
            var client = new RestClient(baseUrl);
            var commentRequest = new RestRequest("api-logs-v1/get-comment", Method.POST);
            commentRequest.AddParameter("apiKey", apiKey);
            commentRequest.AddParameter("commanderName", commanderName);
            commentRequest.AddParameter("systemName", systemName);
            var commentClientResponse = client.Execute<StarMapLogResponse>(commentRequest);
            StarMapLogResponse commentResponse = commentClientResponse.Data;
            return commentResponse?.comment;
        }

        public StarMapInfo getStarMapInfo(string systemName)
        {
            var client = new RestClient(baseUrl);

            // First fetch the data itself
            var logRequest = new RestRequest("api-logs-v1/get-logs", Method.POST);
            logRequest.AddParameter("apiKey", apiKey);
            logRequest.AddParameter("commanderName", commanderName);
            logRequest.AddParameter("systemName", systemName);
            var logClientResponse = client.Execute<StarMapLogResponse>(logRequest);
            StarMapLogResponse logResponse = logClientResponse.Data;
            if (logResponse?.msgnum != 100)
            {
                Logging.Warn("EDSM responded with " + logResponse.msg ?? logClientResponse.ErrorMessage);
            }

            // Also grab any comment that might be present
            var commentRequest = new RestRequest("api-logs-v1/get-comment", Method.POST);
            commentRequest.AddParameter("apiKey", apiKey);
            commentRequest.AddParameter("commanderName", commanderName);
            commentRequest.AddParameter("systemName", systemName);
            var commentClientResponse = client.Execute<StarMapLogResponse>(commentRequest);
            StarMapLogResponse commentResponse = commentClientResponse.Data;
            if (commentResponse?.msgnum != 100)
            {
                Logging.Warn("EDSM responded with " + commentResponse.msg ?? commentClientResponse.ErrorMessage);
            }

            int visits = (logResponse != null && logResponse.logs != null) ? logResponse.logs.Count : 1;
            DateTime lastUpdate = (logResponse != null && logResponse.lastUpdate != null) ? (DateTime)logResponse.lastUpdate : new DateTime();
            string comment = commentResponse?.comment;

            return new StarMapInfo(visits, lastUpdate, comment);
        }

        public Dictionary<string, string> getStarMapComments()
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/get-comments", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            var starMapCommentResponse = client.Execute<StarMapCommentResponse>(request);
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
            var client = new RestClient(baseUrl);
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
            var starMapLogResponse = client.Execute<StarMapLogResponse>(request);
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
        private Newtonsoft.Json.JsonSerializer serializer;

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
