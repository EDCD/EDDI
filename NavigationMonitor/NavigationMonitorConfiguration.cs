using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiNavigationMonitor
{
    /// <summary>Storage for configuration of cargo details</summary>
    public class NavigationMonitorConfiguration
    {
        public ObservableCollection<Bookmark> bookmarks { get; set; }

        public DateTime updatedat { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public NavigationMonitorConfiguration()
        {
            bookmarks = new ObservableCollection<Bookmark>();
        }

        /// <summary>
        /// Obtain cargo configuration from a json.
        /// </summary>
        public static NavigationMonitorConfiguration FromJson(dynamic json)
        {
            NavigationMonitorConfiguration configuration = new NavigationMonitorConfiguration();
            if (json != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<NavigationMonitorConfiguration>(json);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to obtain bookmark configuration", ex);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Obtain cargo configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\cargomonitor.json is used
        /// </summary>
        public static NavigationMonitorConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\navigationmonitor.json";
            }

            NavigationMonitorConfiguration configuration = new NavigationMonitorConfiguration();
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
                        Logging.Debug("Failed to read bookmark configuration", ex);
                    }
                }
            }
            if (configuration == null)
            {
                configuration = new NavigationMonitorConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        public static NavigationMonitorConfiguration FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<NavigationMonitorConfiguration>(json);
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\cargomonitor.json will be used
        /// </summary>
        public void ToFile(string filename = null)
        {
            // Remove any items that are all NULL
            //limits = limits.Where(x => x.Value.minimum.HasValue || x.Value.desired.HasValue || x.Value.maximum.HasValue).ToDictionary(x => x.Key, x => x.Value);

            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\navigationmonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Logging.Debug("Configuration to file: " + json);
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }
    }
}
