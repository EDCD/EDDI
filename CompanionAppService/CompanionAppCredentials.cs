using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access the Companion App</summary>
    public class CompanionAppCredentials
    {
        private const string fileName = "CompanionAPI.json";

        [JsonProperty]
        public string accessToken { get; set; }

        [JsonProperty]
        public string refreshToken { get; set; }

        [JsonProperty]
        public DateTime tokenExpiry { get; set; }

        static string defaultPath => $"{Constants.DATA_DIR}\\{fileName}";

        [JsonIgnore]
        private string dataPath = defaultPath;

        static readonly object fileLock = new object();

        /// <summary>
        /// Obtain credentials from a file. If filepath is not supplied then defaultPath is used.
        /// </summary>
        public static CompanionAppCredentials Load(string filepath=null)
        {
            CompanionAppCredentials credentials = null;
            filepath = filepath ?? defaultPath;

            string data = null;
            if (System.IO.File.Exists(filepath))
            {
                data = Files.Read(filepath);
            }
            if (data != null)
            {
                try
                {
                    credentials = JsonConvert.DeserializeObject<CompanionAppCredentials>(data);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read companion app credentials", ex);
                    credentials = null;
                }
            }

            if (credentials == null)
            {
                credentials = new CompanionAppCredentials() {dataPath = filepath};
                credentials.Save();
            }

            credentials.dataPath = filepath;
            return credentials;
        }

        /// <summary>
        /// Clear the information held by credentials.
        /// </summary>
        public void Clear()
        {
            accessToken = null;
            refreshToken = null;
        }

        /// <summary>
        /// Write credentials to a file. If the filename is not supplied then the object's
        /// dataPath will be used, failing that, the class's defaultPath.
        /// </summary>
        public void Save(string filename=null)
        {
            filename = filename ?? dataPath ?? defaultPath;

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }
    }
}
