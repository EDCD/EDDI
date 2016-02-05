using Newtonsoft.Json;
using System;
using System.IO;

namespace EliteDangerousNetLogMonitor
{
    /// <summary>Configuration for the NetLog service</summary>
    public class NetLogConfiguration
    {
        [JsonProperty("path")]
        public String path { get; set; }

        [JsonIgnore]
        private String dataPath;

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\netlog.json is used
        /// </summary>
        public static NetLogConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\netlog.json";
            }

            NetLogConfiguration configuration;
            try
            {
                String configurationData = File.ReadAllText(filename);
                configuration = JsonConvert.DeserializeObject<NetLogConfiguration>(configurationData);
                configuration.dataPath = filename;
            }
            catch
            {
                configuration = new NetLogConfiguration();
                configuration.dataPath = filename;
                // See if there was old information present
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                string oldFilename = dataDir + "\\productpath";
                try
                {
                    string path = File.ReadAllText(oldFilename);
                    if (path != null)
                    {
                        configuration.path = path;
                        configuration.ToFile();
                        File.Delete(oldFilename);
                    }
                }
                catch { }
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
        /// %APPDATA%\EDDI\netlog.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\netlog.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
