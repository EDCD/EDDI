using Newtonsoft.Json;
using System;
using System.IO;
using Utilities;

namespace EddiInaraService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access EDSM</summary>
    public class InaraConfiguration
    {
        [JsonProperty("apiKey")]
        public string apiKey { get; set; }

        [JsonProperty("commanderName")]
        public string commanderName { get; set; }

        [JsonProperty("commanderFrontierID")]
        public string commanderFrontierID { get; set; }

        [JsonProperty("lastSync")]
        DateTime lastSync { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        /// <summary>
        /// Obtain credentials from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\inara.json is used
        /// </summary>
        public static InaraConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\inara.json";
            }

            InaraConfiguration configuration = new InaraConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        configuration = JsonConvert.DeserializeObject<InaraConfiguration>(data);

                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read Inara configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new InaraConfiguration();
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
        /// Constants.Data_DIR\inara.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\inara.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }
    }
}
