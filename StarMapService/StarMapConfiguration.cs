using Newtonsoft.Json;
using System;
using System.IO;
using Utilities;

namespace EddiStarMapService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access EDSM</summary>
    public class StarMapConfiguration
    {
        [JsonProperty("apiKey")]
        public string apiKey { get; set; }
        [JsonProperty("commanderName")]
        public string commanderName { get; set; }

        [JsonIgnore]
        private string dataPath;

        /// <summary>
        /// Obtain credentials from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\edsm.json is used
        /// </summary>
        public static StarMapConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\edsm.json";
            }

            StarMapConfiguration configuration = new StarMapConfiguration();
            try
            {
                string credentialsData = File.ReadAllText(filename);
                configuration = JsonConvert.DeserializeObject<StarMapConfiguration>(credentialsData);
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read starmap configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new StarMapConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Clear the information held by credentials.
        /// </summary>
        public void Clear()
        {
            apiKey = null;
            commanderName = null;
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
                filename = Constants.DATA_DIR + @"\edsm.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
