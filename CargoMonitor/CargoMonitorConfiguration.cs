using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Utilities;

namespace EddiCargoMonitor
{
    /// <summary>Storage for configuration of cargo details</summary>
    public class CargoMonitorConfiguration
    {
        public ObservableCollection<Cargo> cargo { get; set; }

        [JsonIgnore]
        private string dataPath;

        public CargoMonitorConfiguration()
        {
            cargo = new ObservableCollection<Cargo>();
        }

        /// <summary>
        /// Obtain cargo configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\cargomonitor.json is used
        /// </summary>
        public static CargoMonitorConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\cargomonitor.json";
            }

            CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
            if (File.Exists(filename))
            {
                string data = Files.Read(filename);
                if (data != null)
                {
                    try
                    {
                        configuration = JsonConvert.DeserializeObject<CargoMonitorConfiguration>(data);
                    }
                    catch (Exception ex)
                    {
                        Logging.Debug("Failed to read cargo configuration", ex);
                    }
                }
            }
            if (configuration == null)
            {
                configuration = new CargoMonitorConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\cargomonitor.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            // Remove any items that are all NULL
            //limits = limits.Where(x => x.Value.minimum.HasValue || x.Value.desired.HasValue || x.Value.maximum.HasValue).ToDictionary(x => x.Key, x => x.Value);

            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\cargomonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
