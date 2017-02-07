using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilities;

namespace EddiCompanionAppService
{
    /// <summary>Storage of configuration for Elite: Dangerous ships as obtained by the Companion App</summary>
    public class ShipsConfiguration
    {
        [JsonProperty("ships")]
        public List<Ship> Ships;

        [JsonIgnore]
        private string dataPath;

        public ShipsConfiguration()
        {
            Ships = new List<Ship>();
        }

        /// <summary>
        /// Obtain ships configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\ships.json is used
        /// </summary>
        public static ShipsConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\ships.json";
            }

            ShipsConfiguration configuration = new ShipsConfiguration();
            try
            {
                configuration = JsonConvert.DeserializeObject<ShipsConfiguration>(File.ReadAllText(filename));
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read ships configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new ShipsConfiguration();
            }

            // There was a bug that caused null entries to be written to the ships configuration; remove these if present
            configuration.Ships = configuration.Ships.Where(x => x.role != null).ToList();

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\ships.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            // We only write to the file if there is at least one ship present in the list
            if (Ships.Count == 0)
            {
                return;
            }

            // There was a bug that caused null entries to be written to the ships configuration; remove these if present
            Ships = Ships.Where(x => x.role != null).ToList();

            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\ships.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
