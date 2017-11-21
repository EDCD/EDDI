using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access the Companion App</summary>
    public class CompanionAppCredentials
    {
        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("CompanionApp")]
        public string appId { get; set; }
        [JsonProperty("mid")]
        public string machineId { get; set; }
        [JsonProperty("mtk")]
        public string machineToken { get; set; }

        [JsonIgnore]
        private string dataPath;

        /// <summary>
        /// Obtain credentials from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\credentials.json is used
        /// </summary>
        public static CompanionAppCredentials FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\credentials.json";
            }

            CompanionAppCredentials credentials =null;
            string data = Files.Read(filename);
            if (data != null)
            {
            try
            {
                credentials = JsonConvert.DeserializeObject<CompanionAppCredentials>(data);
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read companion app credentials", ex);
            }
            }
            if (credentials == null)
            {
                credentials = new CompanionAppCredentials();
            }

            credentials.dataPath = filename;
            return credentials;
        }

        /// <summary>
        /// Clear the information held by credentials.
        /// </summary>
        public void Clear()
        {
            appId = null;
            machineId = null;
            machineToken = null;
        }

        /// <summary>
        /// Obtain credentials to a file.  If the filename is not supplied then the path used
        /// when reading in the credentials will be used, or the default path of 
        /// Constants.Data_DIR\credentials.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\credentials.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
