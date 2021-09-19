using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of navigation details</summary>
    public class NavigationMonitorConfiguration
    {
        public ObservableCollection<NavBookmark> bookmarks { get; set; }

        public DateTime updatedat { get; set; }

        // Search parameters
        public int? maxSearchDistanceFromStarLs { get; set; } = 10000;
        public bool prioritizeOrbitalStations { get; set; } = true;

        // Missions route data
        public string missionsRouteList { get; set; }
        public decimal missionsRouteDistance { get; set; }

        // Search data
        public string searchQuery { get; set; }
        public string searchSystem { get; set; }
        public string searchStation { get; set; }
        public decimal searchDistance { get; set; }

        // Ship touchdown data
        public decimal? tdLat { get; set; }
        public decimal? tdLong { get; set; }
        public string tdPOI { get; set; }

        // Guidance system
        public bool guidanceSystemEnabled { get; set; }


        [JsonIgnore]
        private string dataPath;

        public NavigationMonitorConfiguration()
        {
            bookmarks = new ObservableCollection<NavBookmark>();
        }

        /// <summary>
        /// Obtain configuration from a json.
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
        /// Obtain configuration from a file.  If the file name is not supplied the the default path is used
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
        /// when reading in the configuration will be used, unless it too is null, for example
        /// a test config purely based on JSON, in which case nothing will be written.
        /// </summary>
        public void ToFile(string filename = null)
        {
            filename = filename ?? dataPath;
            if (filename == null) { return; }

            string json = null;
            LockManager.GetLock(nameof(NavigationMonitorConfiguration), () =>
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
