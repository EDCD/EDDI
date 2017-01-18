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
        public string path { get; set; }

        [JsonIgnore]
        public List<Watch> watches { get; private set; }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\systemmonitor.json is used
        /// </summary>
        public static EddpConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\netlog.json";
            }

            EddpConfiguration configuration = new EddpConfiguration();
            try
            {
                string configurationData = File.ReadAllText(filename);
                configuration = JsonConvert.DeserializeObject<EddpConfiguration>(configurationData);
            }
            catch
            {
                configuration = new EddpConfiguration();
            }

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
        /// Constants.Data_DIR\system.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                Directory.CreateDirectory(Constants.DATA_DIR);
                filename = Constants.DATA_DIR + @"\system.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
