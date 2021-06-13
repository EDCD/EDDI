using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        // The maximum batch size we will use for syncing flight logs before we write systems to our sql database
        public const int syncBatchSize = 50;

        // The default timeout for requests to EDSM. Requests can override this by setting `RestRequest.Timeout`. Both are in milliseconds.
        private const int DefaultTimeoutMilliseconds = 10000;

        // The timeout for journal events is lengthened to 30 seconds (as recommended by Anthor in private message)
        private const int JournalTimeoutMilliseconds = 30000;

        // Pause a short time to allow any initial events to build in the queue before our first EDSM responder event sync
        private const int startupDelayMilliSeconds = 1000 * 10; // 10 seconds

        // The minimum interval between EDSM responder event syncs
        private const int syncIntervalMilliSeconds = 60000; // 1 minute

        public static string inGameCommanderName { get; set; }
        private string commanderName { get; set; }
        private string apiKey { get; set; }
        private readonly IEdsmRestClient restClient;
        private BlockingCollection<IDictionary<string, object>> queuedEvents;
        private static CancellationTokenSource syncCancellationTS; // This must be static so that it is visible to child threads and tasks

        // For normal use, the EDSM API base URL is https://www.edsm.net/.
        // If you need to do some testing on EDSM's API, please use the https://beta.edsm.net/ endpoint for sending data.
        private const string baseUrl = "https://www.edsm.net/";

        private class EdsmRestClient : IEdsmRestClient
        {
            private readonly RestClient restClient;

            public EdsmRestClient(string baseUrl)
            {
                restClient = new RestClient(baseUrl)
                {
                    Timeout = DefaultTimeoutMilliseconds
                };
            }

            public Uri BuildUri(IRestRequest request) => restClient.BuildUri(request);
            IRestResponse<T> IEdsmRestClient.Execute<T>(IRestRequest request) => restClient.Execute<T>(request);
        }

        public StarMapService(IEdsmRestClient restClient = null)
        {
            this.restClient = restClient ?? new EdsmRestClient(baseUrl);

            // Set up EDSM API credentials
            SetEdsmCredentials();
        }

        public void SetEdsmCredentials()
        {
            StarMapConfiguration starMapCredentials = StarMapConfiguration.FromFile();
            if (!string.IsNullOrEmpty(starMapCredentials?.apiKey))
            {
                // Commander name might come from EDSM credentials or from the game and companion app
                string cmdrName = starMapCredentials.commanderName ?? inGameCommanderName;
                if (!string.IsNullOrEmpty(cmdrName))
                {
                    apiKey = starMapCredentials.apiKey?.Trim();
                    commanderName = cmdrName?.Trim();
                }
                else
                {
                    Logging.Warn("EDSM Responder not configured: Commander name not set.");
                }
            }
            else
            {
                Logging.Warn("EDSM Responder not configured: API key not set.");
            }
        }

        public bool EdsmCredentialsSet()
        {
            return !string.IsNullOrEmpty(commanderName) && !string.IsNullOrEmpty(apiKey);
        }

        public void StartJournalSync()
        {
            if (syncCancellationTS is null || syncCancellationTS.IsCancellationRequested)
            {
                Logging.Debug("Enabling EDSM Responder event sync.");
                Task.Run(() => BackgroundJournalSync()).ConfigureAwait(false);
            }
        }

        public void StopJournalSync()
        {
            if (syncCancellationTS != null && !syncCancellationTS.IsCancellationRequested)
            {
                Logging.Debug("Stopping EDSM Responder event sync.");
                syncCancellationTS.Cancel();
                // Clean up by sending anything left in the queue.
                SendEvents(queuedEvents.ToList());
            }
        }

        private async void BackgroundJournalSync()
        {
            queuedEvents = new BlockingCollection<IDictionary<string, object>>();
            using (syncCancellationTS = new CancellationTokenSource())
            {
                try
                {

                    // Pause a short time to allow any initial events to build in the queue before our first sync
                    await Task.Delay(startupDelayMilliSeconds, syncCancellationTS.Token).ConfigureAwait(false);
                    await Task.Run(async () =>
                    {
                        // The `GetConsumingEnumerable` method blocks the thread while the underlying collection is empty
                        // If we haven't extracted events to send to EDSM, this will wait / pause background sync until `queuedEvents` is no longer empty.
                        var holdingQueue = new List<IDictionary<string, object>>();
                        try
                        {
                            foreach (var pendingEvent in queuedEvents.GetConsumingEnumerable(syncCancellationTS.Token))
                            {
                                holdingQueue.Add(pendingEvent);

                                if (queuedEvents.Count == 0)
                                {
                                    // Once we hit zero queued events, wait a couple more seconds for any concurrent events to register
                                    await Task.Delay(2000, syncCancellationTS.Token).ConfigureAwait(false);
                                    if (queuedEvents.Count > 0) { continue; }
                                    // No additional events registered, send any events we have in our holding queue
                                    if (holdingQueue.Count > 0)
                                    {
                                        var sendingQueue = holdingQueue.Copy();
                                        holdingQueue = new List<IDictionary<string, object>>();
                                        await Task.Run(() => SendEvents(sendingQueue), syncCancellationTS.Token).ConfigureAwait(false);
                                        await Task.Delay(syncIntervalMilliSeconds, syncCancellationTS.Token).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Operation was cancelled. Return any events we've extracted back to the primary queue.
                            foreach (var pendingEvent in holdingQueue)
                            {
                                queuedEvents.Add(pendingEvent);
                            }
                        }
                    }).ConfigureAwait(false);
                }
                catch (TaskCanceledException)
                {
                    // Task cancelled. Nothing to do here.
                }
            }
        }

        public void EnqueueEvent(IDictionary<string, object> eventObject)
        {
            if (eventObject is null) { return; }
            queuedEvents.Add(eventObject);
        }

        private void ReEnqueueEvents(IEnumerable<IDictionary<string, object>> eventData)
        {
            if (eventData is null) { return; }
            // Re-enqueue the data so we can send it again later (preserving order as best we can).
            var newQueue = eventData.ToList();
            while (queuedEvents.TryTake(out var eventObject))
            {
                newQueue.Add(eventObject);
            }
            foreach (var eventObject in newQueue)
            {
                queuedEvents.Add(eventObject);
            }
        }

        private void SendEvents(List<IDictionary<string, object>> queue)
        {
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            SendEventBatch(queue, starMapConfiguration);
        }

        private void SendEventBatch(List<IDictionary<string, object>> eventData, StarMapConfiguration starMapConfiguration)
        {
            if (!EdsmCredentialsSet()) { return; }

            // Filter any stale data
            eventData = eventData
                .Where(e => JsonParsing.getDateTime("timestamp", e) > starMapConfiguration.lastJournalSync)
                .ToList();
            if (eventData.Count == 0) { return; }

            // The EDSM responder has a `gameIsBeta` flag that it checks prior to sending data via this method.  
            var request = new RestRequest("api-journal-v1", Method.POST);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);
            request.AddParameter("message", JsonConvert.SerializeObject(eventData).Normalize());
            request.Timeout = JournalTimeoutMilliseconds;

            try
            {
                Logging.Debug("Sending message to EDSM: " + restClient.BuildUri(request).AbsoluteUri);
                var clientResponse = restClient.Execute<StarMapLogResponse>(request);
                StarMapLogResponse response = clientResponse.Data;

                if (response is null)
                {
                    Logging.Warn(clientResponse.ErrorMessage);
                    ReEnqueueEvents(eventData);
                }
                else if (response.msgnum >= 100 && response.msgnum <= 103)
                {
                    // 100 -  Everything went fine! 
                    // 101 -  The journal message was already processed in our database. 
                    // 102 -  The journal message was already in a newer version in our database. 
                    // 103 -  Duplicate event request (already reported from another software client). 
                    starMapConfiguration.lastJournalSync = eventData
                        .Select(e => JsonParsing.getDateTime("timestamp", e))
                        .Max();
                    starMapConfiguration.ToFile();
                }
                if (response?.msgnum != 100)
                {
                    if (!string.IsNullOrEmpty(response?.msg))
                    {
                        Logging.Warn("EDSM responded with: " + response.msg);
                    }
                    else
                    {
                        Logging.Warn("EDSM responded with: " + JsonConvert.SerializeObject(response));
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to send event to EDSM", ex);
            }
        }

        public void sendStarMapComment(string systemName, string comment)
        {
            if (!EdsmCredentialsSet()) { return; }

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
                        Logging.Warn("EDSM responded with " + response?.msg ?? clientResponse.ErrorMessage);
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
            return response;
        }

        public Dictionary<string, string> getStarMapComments()
        {
            if (!EdsmCredentialsSet()) { return new Dictionary<string, string>(); }

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
                    if (!string.IsNullOrEmpty(entry.comment))
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
            if (!EdsmCredentialsSet()) { return new List<StarMapResponseLogEntry>(); }

            var request = new RestRequest("api-logs-v1/get-logs", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("showId", 1); // Obtain EDSM IDs
            if (systemNames?.Count() == 1)
            {
                // When a single system name is provided, the api responds with 
                // the complete flight logs for that star system
                request.AddParameter("systemName", systemNames[0]);
            }
            else
            {
                if (since.HasValue)
                {
                    request.AddParameter("startdatetime", Dates.FromDateTimeToString(since.Value));
                }
                else
                {
                    // Though not documented in the api, Anthor from EDSM has confirmed that this 
                    // unpublished parameter is valid and overrides "startdatetime" and "enddatetime".
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
                if (response.logs == null) { return null; }

                if (systemNames?.Length > 0)
                {
                    response.logs.RemoveAll(s => !systemNames.Contains(s.system));
                }
                return response.logs;
            }
            Logging.Debug("No response received.");
            throw new EDSMException("No response received."); // not for localization
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
        public T Deserialize<T>(IRestResponse response)
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
