using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of cargo details</summary>
    public class CargoMonitorConfiguration
    {
        public ObservableCollection<Cargo> cargo { get; set; }

        public int cargocarried { get; set; }

        public DateTime updatedat { get; set; }

        [JsonIgnore]
        private string dataPath;

        public CargoMonitorConfiguration()
        {
            cargo = new ObservableCollection<Cargo>();
        }

        /// <summary>
        /// Obtain configuration from a json.
        /// </summary>
        public static CargoMonitorConfiguration FromJson(dynamic json)
        {
            CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
            if (json != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<CargoMonitorConfiguration>(json);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to obtain cargo configuration", ex);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default path is used
        /// </summary>
        public static CargoMonitorConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\cargomonitor.json";
            }

            CargoMonitorConfiguration configuration = new CargoMonitorConfiguration();
            if (File.Exists(filename))
            {
                string json = Files.Read(filename);
                if (json != null)
                {
                    try
                    {
                        configuration = FromJsonString(json);
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

        public static CargoMonitorConfiguration FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<CargoMonitorConfiguration>(json);
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, unless it too is null, for example
        /// a test config purely based on JSON, in which case nothing will be written.
        /// </summary>
        public void ToFile(string filename = null)
        {
            filename = filename ?? dataPath;
            if (filename == null) { return; }

            string json = null;
            LockManager.GetLock(nameof(CargoMonitorConfiguration), () =>
            {
                json = JsonConvert.SerializeObject(this, Formatting.Indented);
            });
            if (!string.IsNullOrEmpty(json))
            {
                Logging.Debug("Configuration to file: " + json);
                Files.Write(filename, json);
            }
        }
    }
}
