using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace EliteDangerousStarMapService
{
    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public class StarMapService
    {
        private string commanderName;
        private string apiKey;
        private string baseUrl;

        public StarMapService(string apiKey, string commanderName, string baseUrl="http://www.edsm.net/")
        {
            this.apiKey = apiKey;
            this.commanderName = commanderName;
            this.baseUrl = baseUrl;
        }

        public void sendStarMapLog(string systemName)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/set-log");
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("systemName", systemName);
            request.AddParameter("dateVisited", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));

            var clientResponse = client.Execute<StarMapLogResponse>(request);
            StarMapLogResponse response = clientResponse.Data;
            // TODO check response
        }

        public void sendStarMapComment(string systemName, string comment)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/set-comment");
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            request.AddParameter("systemName", systemName);
            request.AddParameter("comment", comment);

            var clientResponse = client.Execute<StarMapLogResponse>(request);
            StarMapLogResponse response = clientResponse.Data;
            // TODO check response
        }

        public StarMapInfo getStarMapInfo(string systemName)
        {
            var client = new RestClient(baseUrl);

            // First fetch the data itself
            var logRequest = new RestRequest("api-logs-v1/get-logs");
            logRequest.AddParameter("apiKey", apiKey);
            logRequest.AddParameter("commanderName", commanderName);
            logRequest.AddParameter("systemName", systemName);
            var logClientResponse = client.Execute<StarMapLogResponse>(logRequest);
            StarMapLogResponse logResponse = logClientResponse.Data;
            // TODO check response

            // Also grab any comment that might be present
            var commentRequest = new RestRequest("api-logs-v1/get-comment");
            commentRequest.AddParameter("apiKey", apiKey);
            commentRequest.AddParameter("commanderName", commanderName);
            commentRequest.AddParameter("systemName", systemName);
            var commentClientResponse = client.Execute<StarMapLogResponse>(commentRequest);
            StarMapLogResponse commentResponse = commentClientResponse.Data;
            // TODO check response

            return new StarMapInfo(logResponse.logs.Count, logResponse.lastUpdate, commentResponse.comment);
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

        public Dictionary<string, StarMapLogInfo> getStarMapLog(DateTime? since = null)
        {
            var client = new RestClient(baseUrl);
            var request = new RestRequest("api-logs-v1/get-logs");
            request.AddParameter("apiKey", apiKey);
            request.AddParameter("commanderName", commanderName);
            if (since.HasValue)
            {
                request.AddParameter("startdatetime", since.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            var starMapLogResponse = client.Execute<StarMapLogResponse>(request);
            StarMapLogResponse response = starMapLogResponse.Data;

            Dictionary<string, StarMapLogInfo> vals = new Dictionary<string, StarMapLogInfo>();
            if (response != null)
            {
                foreach (StarMapResponseLogEntry entry in response.logs)
                {
                    Console.WriteLine("Entry found for " + entry.system);
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

        public static string ObtainApiKey()
        {
            String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
            Directory.CreateDirectory(dataDir);
            string filename = dataDir + "\\edsmapikey";
            try
            {
                return File.ReadAllText(filename);
            }
            catch
            {
                return null;
            }
        }

        public static void WritePath(string path)
        {
            String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
            Directory.CreateDirectory(dataDir);
            string filename = dataDir + "\\edsmapikey";
            File.WriteAllText(filename, path);
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

    //class StarMapLog
    //{
    //    private string apiKey;
    //    private string commanderName;
    //    private string systemName;
    //    private DateTime dateVisited;

    //    public StarMapLog(string apiKey, string commanderName, string systemName)
    //    {
    //        this.apiKey = apiKey;
    //        this.commanderName = commanderName;
    //        this.systemName = systemName;
    //    }
    //}

    //class StarMapComment
    //{
    //    private string apiKey;
    //    private string commanderName;
    //    private string systemName;
    //    private string comment;

    //    public StarMapComment(string apiKey, string commanderName, string systemName, string comment)
    //    {
    //        this.apiKey = apiKey;
    //        this.commanderName = commanderName;
    //        this.systemName = systemName;
    //        this.comment = comment;
    //    }
    //}

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
            this.fromSoftware = "EDDI";
            this.fromSoftwareVersion = "1.1.0";
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

        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        public T Deserialize<T>(RestSharp.IRestResponse response)
        {
            var content = response.Content;

            using (var stringReader = new StringReader(content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return serializer.Deserialize<T>(jsonTextReader);
                }
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
