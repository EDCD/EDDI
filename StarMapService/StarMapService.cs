using EddiDataDefinitions;
using EddiDataProviderService;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Utilities;

namespace EddiStarMapService
{
    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public class StarMapService
    {
        // Set the maximum batch size we will use for syncing before we write systems to our sql database
        public const int syncBatchSize = 100;

        // Use en-US everywhere to ensure that we don't use , rather than . for our separator
        private static CultureInfo EN_US_CULTURE = new CultureInfo("en-US");

        private string commanderName;
        private string apiKey;
        private string baseUrl;

        public StarMapService(string apiKey, string commanderName, string baseUrl="https://www.edsm.net/")
        {
            this.apiKey = apiKey;
            this.commanderName = commanderName;
            this.baseUrl = baseUrl;
        }

        public void sendStarMapLog(DateTime timestamp, string systemName, decimal? x, decimal? y, decimal? z)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/set-log", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("systemName", systemName);
            request.AddParameter("dateVisited", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);
            if (x.HasValue)
            {
                request.AddParameter("x", ((decimal)x).ToString("0.000", EN_US_CULTURE));
            }
            if (y.HasValue)
            {
                request.AddParameter("y", ((decimal)y).ToString("0.000", EN_US_CULTURE));
            }
            if (z.HasValue)
            {
                request.AddParameter("z", ((decimal)z).ToString("0.000", EN_US_CULTURE));
            }

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send starmap log";
            thread.Start();
        }

        public void sendCredits(decimal credits, decimal loan)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/set-credits", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("balance", credits);
            request.AddParameter("loan", loan);
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send credits";
            thread.Start();
        }

        public void sendRanks(int combat, int combatProgress,
            int trade, int tradeProgress,
            int exploration, int explorationProgress,
            int cqc, int cqcProgress,
            int federation, int federationProgress,
            int empire, int empireProgress)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/set-ranks", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("Combat", combat + ";" + combatProgress);
            request.AddParameter("Trade", trade + ";" + tradeProgress);
            request.AddParameter("Explore", exploration + ";" + explorationProgress);
            request.AddParameter("CQC", cqc + ";" + cqcProgress);
            request.AddParameter("Federation", federation + ";" + federationProgress);
            request.AddParameter("Empire", empire + ";" + empireProgress);
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send ranks";
            thread.Start();
        }

        public void sendMaterials(Dictionary<string, int> materials)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/set-materials", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("type", "materials");
            request.AddParameter("values", JsonConvert.SerializeObject(materials));
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send materials";
            thread.Start();
        }

        public void sendData(Dictionary<string, int> data)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/set-materials", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("type", "data");
            request.AddParameter("values", JsonConvert.SerializeObject(data));
            request.AddParameter("fromSoftware", Constants.EDDI_NAME);
            request.AddParameter("fromSoftwareVersion", Constants.EDDI_VERSION);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send data";
            thread.Start();
        }

        public void sendShip(Ship ship)
        {
            if (ship == null)
            {
                return;
            }

            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/update-ship", Method.POST);
            string coriolis_uri = ship.CoriolisUri();
            string edshipyard_uri = ship.EDShipyardUri();

            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("shipId", ship.LocalId);
            request.AddParameter("shipName", ship.name);
            request.AddParameter("shipIdent", ship.ident);
            request.AddParameter("type", ship.EDName);
            request.AddParameter("paintJob", ship.paintjob);
            request.AddParameter("cargoQty", ship.cargocarried);
            request.AddParameter("cargoCapacity", ship.cargocapacity);
            request.AddParameter("linkToEDShipyard", edshipyard_uri);
            request.AddParameter("linkToCoriolis", coriolis_uri);
            
            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending ship data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    Logging.Debug("Data sent to EDSM");
                    StarMapLogResponse response = clientResponse.Data;
                    if (response == null)
                    {
                        Logging.Warn($"EDSM rejected ship data with {clientResponse.ErrorMessage}");
                        return;
                    }
                    else
                    {
                        Logging.Debug($"EDSM response {response.msgnum}: " + response.msg);
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send ship";
            thread.Start();
        }

        public void sendShipSwapped(int shipId)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/set-ship-id", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("shipId", shipId);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send ship ID";
            thread.Start();
        }

        public void sendShipSold(int shipId)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-commander-v1/sell-ship", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("shipId", shipId);

            Thread thread = new Thread(() =>
            {
                try
                {
                    Logging.Debug("Sending data to EDSM: " + client.BuildUri(request).AbsoluteUri);
                    var clientResponse = client.Execute<StarMapLogResponse>(request);
                    StarMapLogResponse response = clientResponse.Data;
                    Logging.Debug("Data sent to EDSM");
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
                    Logging.Warn("Failed to send data to EDSM", ex);
                }
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService sell ship";
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
            });
            thread.IsBackground = true;
            thread.Name = "StarMapService send starmap comment";
            thread.Start();
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
            return (commentResponse != null) ? commentResponse.comment : null;
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
            string comment = (commentResponse != null) ? commentResponse.comment : null;

            return new StarMapInfo(visits, lastUpdate, comment);
        }


        public void sendStarMapDistance(string systemName, string remoteSystemName, decimal distance)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest(Method.POST);
            request.Resource = "api-v1/submit-distances";

            StarMapData data = new StarMapData(commanderName, systemName, remoteSystemName, distance);
            StarMapSubmission submission = new StarMapSubmission(data);

            request.JsonSerializer = NewtonsoftJsonSerializer.Default;
            request.RequestFormat = DataFormat.Json;
            request.AddBody(submission);

            var clientResponse = client.Execute<StarMapDistanceResponse>(request);
            StarMapDistanceResponse response = clientResponse.Data;
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

        public Dictionary<string, StarMapLogInfo> getStarMapLog(DateTime? since = null)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/get-logs", Method.POST);
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            if (since.HasValue)
            {
                request.AddParameter("startdatetime", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else
            {
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
                        vals[entry.system] = new StarMapLogInfo();
                        vals[entry.system].system = entry.system;
                        vals[entry.system].visits = 1;
                        vals[entry.system].lastVisit = entry.date;
                    }
                }
            }
            return vals;
        }

        public void Sync(DateTime? since = null)
        {
            Logging.Info("Syncing with EDSM");
            try
            {
                Dictionary<string, StarMapLogInfo> systems = getStarMapLog(since);
                Dictionary<string, string> comments = getStarMapComments();
                List<StarSystem> syncSystems = new List<StarSystem>();
                foreach (string system in systems.Keys)
                {
                    StarSystem CurrentStarSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, false);
                    CurrentStarSystem.visits = systems[system].visits;
                    CurrentStarSystem.lastvisit = systems[system].lastVisit;
                    if (comments.ContainsKey(system))
                    {
                        CurrentStarSystem.comment = comments[system];
                    }
                    syncSystems.Add(CurrentStarSystem);

                    if (syncSystems.Count == syncBatchSize)
                    {
                        saveStarSystems(syncSystems);
                        syncSystems.Clear();
                    }
                }
                if (syncSystems.Count > 0)
                {
                    saveStarSystems(syncSystems);
                }
                Logging.Info("EDSM sync completed");
            }
            catch (EDSMException edsme)
            {
                Logging.Debug("EDSM error received: " + edsme.Message);
            }
            catch (ThreadAbortException e)
            {
                Logging.Debug("EDSM update stopped by user: " + e.Message);
            }
        }

        public static void saveStarSystems(List<StarSystem> syncSystems)
        {
            StarSystemSqLiteRepository.Instance.SaveStarSystems(syncSystems);
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            starMapConfiguration.lastSync = DateTime.UtcNow;
            starMapConfiguration.ToFile();
        }
    }
    
    // response from the Star Map distance API
    class StarMapDistanceResponse
    {

    }

    // response from the Star Map log API
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

    class StarMapSubmission
    {
        public StarMapData data { get; set; }

        public StarMapSubmission(StarMapData data)
        {
            this.data = data;
        }
    }

    public class StarMapDistance
    {
        [JsonProperty("name")]
        public string systemName { get; set; }
        [JsonProperty("dist")]
        public decimal? distance { get; set; }

        public StarMapDistance(string systemName)
        {
            this.systemName = systemName;
        }

        public StarMapDistance(string systemName, decimal distance)
        {
            this.systemName = systemName;
            this.distance = distance;
        }
    }

    public class StarMapData
    {
        public string commander { get; set; }
        public string fromSoftware { get; set; }
        public string fromSoftwareVersion { get; set; }
        public StarMapDistance p0 { get; set; }
        public List<StarMapDistance> refs { get; set; }

        public StarMapData(string commanderName, string systemName, string remoteSystemName, decimal distance)
        {
            this.commander = commanderName;
            this.fromSoftware = Constants.EDDI_NAME;
            this.fromSoftwareVersion = Constants.EDDI_VERSION;
            this.p0 = new StarMapDistance(systemName);
            this.refs = new List<StarMapDistance>();
            this.refs.Add(new StarMapDistance(remoteSystemName, distance));
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
