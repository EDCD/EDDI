using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace EddiEddpMonitor
{
    /// <summary>Configuration for the EDDP monitor</summary>
    public class EddpConfiguration
    {
        [JsonIgnore]
        public string path { get; set; }

        public List<Watch> watches { get; private set; }

        public EddpConfiguration()
        {
            watches = new List<Watch>();
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\eddpmonitor.json is used
        /// </summary>
        public static EddpConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\eddpmonitor.json";
            }

            EddpConfiguration configuration = new EddpConfiguration();
            try
            {
                configuration = JsonConvert.DeserializeObject<EddpConfiguration>(File.ReadAllText(filename));
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read EDDP monitor configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new EddpConfiguration();
            }

            configuration.path = filename;

            return configuration;
        }

        /// <summary>
        /// Clear the information held by configuration
        /// </summary>
        public void Clear()
        {
            path = null;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\eddpmonitor.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = path;
            }
            if (filename == null)
            {
                Directory.CreateDirectory(Constants.DATA_DIR);
                filename = Constants.DATA_DIR + @"\eddpmonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
