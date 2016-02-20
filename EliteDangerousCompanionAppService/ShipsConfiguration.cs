using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliteDangerousCompanionAppService
{
    /// <summary>Storage of configuration for Elite: Dangerous ships as obtained by the Companion App</summary>
    public class ShipsConfiguration
    {
        [JsonProperty("ships")]
        public List<Ship> Ships;

        [JsonIgnore]
        private String dataPath;

        public ShipsConfiguration()
        {
            Ships = new List<Ship>();
        }

        /// <summary>
        /// Obtain ships configuration from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\ships.json is used
        /// </summary>
        public static ShipsConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\ships.json";
            }

            ShipsConfiguration configuration;
            try
            {
                configuration = JsonConvert.DeserializeObject<ShipsConfiguration>(File.ReadAllText(filename));
            }
            catch
            {
                configuration = new ShipsConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// %APPDATA%\EDDI\ships.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            // We only write to the file if there is at least one ship present in the list
            if (Ships.Count == 0)
            {
                return;
            }

            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\ships.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
