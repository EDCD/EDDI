﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Utilities;

namespace EddiStarMapService
{
    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public partial class StarMapService
    {
        // Set the maximum batch size we will use for syncing before we write systems to our sql database
        public const int syncBatchSize = 100;

        // For normal use, the EDSM API base URL is https://www.edsm.net/.
        // If you need to do some testing on EDSM's API, please use the https://beta.edsm.net/ endpoint.
        private static string sendUrl => ShouldUseTestEndpoints() ? "https://beta.edsm.net/" : "https://www.edsm.net/";
        private static string getUrl => "https://www.edsm.net/";

        private static string commanderName;
        private static string apiKey;

        public StarMapService(string apikey, string commandername)
        {
            apiKey = apikey;
            commanderName = commandername;
        }

        private static bool ShouldUseTestEndpoints()
        {
#if DEBUG
            return true;
#else
            // use test endpoints if the game is in beta or EDDI is not release candidate or final
            return EDDI.Instance.inBeta || (Constants.EDDI_VERSION.phase < Utilities.Version.TestPhase.rc);
#endif
        }

        public void sendEvent(string eventData)
        {
            var client = new RestClient(sendUrl);
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
                        Logging.Warn("EDSM responded with " + response.msg);
                    }
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
            var client = new RestClient(sendUrl);
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
                    if (response.msgnum != 100)
                    {
                        Logging.Warn("EDSM responded with " + response.msg);
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
            var client = new RestClient(getUrl);
            var request = new RestRequest("api-journal-v1/discard", Method.POST);
            var clientResponse = client.Execute<List<string>>(request);
            List<string> response = clientResponse.Data;
            return response ?? null;
        }

        public string getStarMapComment(string systemName)
        {
            var client = new RestClient(getUrl);
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
            var client = new RestClient(getUrl);

            // First fetch the data itself
            var logRequest = new RestRequest("api-logs-v1/get-logs", Method.POST);
            logRequest.AddParameter("apiKey", apiKey);
            logRequest.AddParameter("commanderName", commanderName);
            logRequest.AddParameter("systemName", systemName);
            var logClientResponse = client.Execute<StarMapLogResponse>(logRequest);
            StarMapLogResponse logResponse = logClientResponse.Data;
            if (logResponse.msgnum != 100)
            {
                Logging.Warn("EDSM responded with " + logResponse.msg);
            }

            // Also grab any comment that might be present
            var commentRequest = new RestRequest("api-logs-v1/get-comment", Method.POST);
            commentRequest.AddParameter("apiKey", apiKey);
            commentRequest.AddParameter("commanderName", commanderName);
            commentRequest.AddParameter("systemName", systemName);
            var commentClientResponse = client.Execute<StarMapLogResponse>(commentRequest);
            StarMapLogResponse commentResponse = commentClientResponse.Data;
            if (commentResponse.msgnum != 100)
            {
                Logging.Warn("EDSM responded with " + commentResponse.msg);
            }

            int visits = (logResponse != null && logResponse.logs != null) ? logResponse.logs.Count : 1;
            DateTime lastUpdate = (logResponse != null && logResponse.lastUpdate != null) ? (DateTime)logResponse.lastUpdate : new DateTime();
            string comment = commentResponse?.comment;

            return new StarMapInfo(visits, lastUpdate, comment);
        }

        public Dictionary<string, string> getStarMapComments()
        {
            var client = new RestClient(getUrl);
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

        public Dictionary<string, StarMapLogInfo> getStarMapLog(DateTime? since = null)
        {
            var client = new RestClient(getUrl);
            var request = new RestRequest("api-logs-v1/get-logs", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
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

            Dictionary<string, StarMapLogInfo> vals = new Dictionary<string, StarMapLogInfo>();
            if (response != null && response.logs != null)
            {
                foreach (StarMapResponseLogEntry entry in response.logs)
                {
                    if (vals.ContainsKey(entry.system))
                    {
                        vals[entry.system].visits = vals[entry.system].visits + 1;
                        if (entry.date > vals[entry.system].lastVisit)
                        {
                            vals[entry.system].previousVisit = vals[entry.system].lastVisit;
                            vals[entry.system].lastVisit = entry.date;
                        }
                        else if (vals[entry.system].previousVisit == null || entry.date > vals[entry.system].previousVisit)
                        {
                            vals[entry.system].previousVisit = entry.date;
                        }
                    }
                    else
                    {
                        vals[entry.system] = new StarMapLogInfo
                        {
                            system = entry.system,
                            visits = 1,
                            lastVisit = entry.date
                        };
                    }
                }
            }
            return vals;
        }
    }
   
    // response from the Star Map log API
    class StarMapResponse
    {
        public string content{ get; set; }
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

    public class StarMapLogInfo
    {
        public string system { get; set; }
        public int visits { get; set; }
        public DateTime lastVisit { get; set; }
        public DateTime? previousVisit { get; set; }
    }

    class StarMapResponseLogEntry
    {
        public string system { get; set; }
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
