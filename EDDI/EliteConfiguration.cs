using Newtonsoft.Json;
using System;
using Utilities;

namespace Eddi
{
    /// <summary>Configuration for Elite</summary>
    public class EliteConfiguration
    {
        [JsonProperty("beta")]
        public bool Beta { get; set; }

        [JsonIgnore]
        private string dataPath;

        public EliteConfiguration()
        {
            Beta = false;
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\elite.json is used
        /// </summary>
        public static EliteConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\elite.json";
            }

            EliteConfiguration configuration = new EliteConfiguration();
            string data = Files.Read(filename);
            if (data != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<EliteConfiguration>(data);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read Elite configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new EliteConfiguration();
            }

            configuration.dataPath = filename;

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\elite.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\elite.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
