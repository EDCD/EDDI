using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliteDangerousSpeechResponder
{
    /// <summary>Scritps for the speech responder</summary>
    public class ScriptsConfiguration
    {
        [JsonProperty("scripts")]
        public Dictionary<string, Script> Scripts { get; set; }

        [JsonIgnore]
        private String dataPath;

        public ScriptsConfiguration()
        {
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\scripts.json is used
        /// </summary>
        public static ScriptsConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\scripts.json";
            }

            ScriptsConfiguration configuration;
            try
            {
                configuration = JsonConvert.DeserializeObject<ScriptsConfiguration>(File.ReadAllText(filename));
            }
            catch
            {
                configuration = new ScriptsConfiguration();
            }
            configuration.dataPath = filename;

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// %APPDATA%\EDDI\scripts.json will be used
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
                filename = dataDir + "\\scripts.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
