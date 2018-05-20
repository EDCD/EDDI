using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiMaterialMonitor
{
    /// <summary>Storage for configuration of material amounts</summary>
    public class MaterialMonitorConfiguration
    {
        public ObservableCollection<MaterialAmount> materials { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public MaterialMonitorConfiguration()
        {
            materials = new ObservableCollection<MaterialAmount>();
        }

        /// <summary>
        /// Obtain materials configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\materialmonitor.json is used
        /// </summary>
        public static MaterialMonitorConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\materialmonitor.json";
            }

            MaterialMonitorConfiguration configuration = new MaterialMonitorConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string json = Files.Read(filename);
                    if (json != null)
                    {
                        configuration = FromJsonString(json);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read materials configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new MaterialMonitorConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        public static MaterialMonitorConfiguration FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<MaterialMonitorConfiguration>(json);
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\materialmonitor.json will be used
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
                filename = Constants.DATA_DIR + @"\materialmonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }
    }
}
