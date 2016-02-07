using RestSharp;
using RestSharp.Deserializers;
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
            request.AddParameter("dateVisited", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var clientResponse = client.Execute<StarMapResponse>(request);
            StarMapResponse response = clientResponse.Data;
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

            var clientResponse = client.Execute<StarMapResponse>(request);
            StarMapResponse response = clientResponse.Data;
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
            var logClientResponse = client.Execute<StarMapResponse>(logRequest);
            StarMapResponse logResponse = logClientResponse.Data;
            // TODO check response

            // Also grab any comment that might be present
            var commentRequest = new RestRequest("api-logs-v1/get-comment");
            commentRequest.AddParameter("apiKey", apiKey);
            commentRequest.AddParameter("commanderName", commanderName);
            commentRequest.AddParameter("systemName", systemName);
            var commentClientResponse = client.Execute<StarMapResponse>(commentRequest);
            StarMapResponse commentResponse = commentClientResponse.Data;
            // TODO check response

            return new StarMapInfo(logResponse.logs.Count, logResponse.lastUpdate, commentResponse.comment);
        }


        public void sendStarMapDistances(string systemName, decimal distanceToSol, decimal distanceToMaia, decimal distanceToRobigo, decimal distanceTo17Draconis)
        {
            StarMapData data = new StarMapData(commanderName, systemName, distanceToSol, distanceToMaia, distanceToRobigo, distanceTo17Draconis);
            StarMapSubmission submission = new StarMapSubmission(data);
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

    public class StarMapDistance
    {
        public string systemName { get; set; }
        public decimal distance { get; set; }

        public StarMapDistance(string systemName, decimal distance)
        {
            this.systemName = systemName;
            this.distance = distance;
        }
    }

    // response from the Star Map API
    class StarMapResponse
    {
        public int msgnum { get; set; }
        public string msg { get; set; }
        public string comment { get; set; }
        public DateTime? lastUpdate { get; set; }
        public List<StarMapResponseLogEntry> logs { get; set; }
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
        private StarMapData data { get; set; }

        public StarMapSubmission(StarMapData data)
        {
            this.data = data;
        }
    }

    class Reference
    {
        private string name { get; set; }
        private decimal? distance { get; set; }

        public Reference(string name)
        {
            this.name = name;
        }
        public Reference(string name, decimal distance)
        {
            this.name = name;
            this.distance = distance;
        }
    }

    class StarMapData
    {
        private string commander { get; set; }
        private string fromSoftware { get; set; }
        private string fromSoftwareVersion { get; set; }
        private Reference p0 { get; set; }
        private List<Reference> refs { get; set; }

        public StarMapData(string commanderName, string systemName, decimal distanceToSol, decimal distanceToMaia, decimal distanceToRobigo, decimal distanceTo17Draconis)
        {
            this.commander = commanderName;
            this.fromSoftware = "EDDI";
            this.fromSoftwareVersion = "0.7.2";
            this.p0 = new Reference(systemName);
            this.refs = new List<Reference>();
            this.refs.Add(new Reference("Sol", distanceToSol));
            this.refs.Add(new Reference("Maia", distanceToMaia));
            this.refs.Add(new Reference("Robigo", distanceToRobigo));
            this.refs.Add(new Reference("17 Draconis", distanceTo17Draconis));
        }
    }
}
