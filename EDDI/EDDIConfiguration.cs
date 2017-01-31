using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace Eddi
{
    /// <summary>Configuration for EDDI</summary>
    public class EDDIConfiguration
    {
        [JsonProperty("homeSystem")]
        public string HomeSystem { get; set; }
        [JsonProperty("homeStation")]
        public string HomeStation { get; set; }
        [JsonProperty("debug")]
        public bool Debug { get; set; }
        [JsonProperty("beta")]
        public bool Beta { get; set; }
        [JsonProperty("insurance")]
        public decimal Insurance { get; set; }
        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }

        [JsonIgnore]
        private string dataPath;

        public EDDIConfiguration()
        {
            Debug = false;
            Beta = false;
            Insurance = 5;
            Plugins = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\eddi.json is used
        /// </summary>
        public static EDDIConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\eddi.json";
            }

            EDDIConfiguration configuration = new EDDIConfiguration();
            try
            {
                configuration = JsonConvert.DeserializeObject<EDDIConfiguration>(File.ReadAllText(filename));
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read EDDI configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new EDDIConfiguration();
            }

            configuration.dataPath = filename;
            if (configuration.Plugins == null)
            {
                configuration.Plugins = new Dictionary<string, bool>();
            }

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\eddi.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\eddi.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
