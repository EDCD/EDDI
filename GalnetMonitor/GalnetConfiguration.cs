using Newtonsoft.Json;
using System;
using System.IO;
using Utilities;

namespace GalnetMonitor
{
    /// <summary>Configuration for the Galnet monitor</summary>
    public class GalnetConfiguration
    {
        [JsonIgnore]
        public string path { get; set; }

        public string lastuuid { get; set; }

        public string language { get; set; } = "English";

        public bool galnetAlwaysOn { get; set; } = false;

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\galnetmonitor.json is used
        /// </summary>
        public static GalnetConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\galnetmonitor.json";
            }

            GalnetConfiguration configuration = new GalnetConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        configuration = JsonConvert.DeserializeObject<GalnetConfiguration>(data);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read Galnet monitor configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new GalnetConfiguration();
            }

            configuration.path = filename;

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\galnetmonitor.json will be used
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
                filename = Constants.DATA_DIR + @"\galnetmonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
