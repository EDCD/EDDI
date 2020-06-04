using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiMissionMonitor
{
    /// <summary>Storage for configuration of mission details</summary>
    public class MissionMonitorConfiguration
    {
        public ObservableCollection<Mission> missions { get; set; }

        public DateTime updatedat { get; set; }
        public int goalsCount { get; set; }
        public int missionsCount { get; set; }
        public int? missionWarning { get; set; }
        public string missionsRouteList { get; set; }
        public decimal missionsRouteDistance { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public MissionMonitorConfiguration()
        {
            missions = new ObservableCollection<Mission>();
        }

        /// <summary>
        /// Obtain cargo configuration from a json.
        /// </summary>
        public static MissionMonitorConfiguration FromJson(dynamic json)
        {
            MissionMonitorConfiguration configuration = new MissionMonitorConfiguration();
            if (json != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<MissionMonitorConfiguration>(json);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to obtain missions configuration", ex);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Obtain cargo configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\cargomonitor.json is used
        /// </summary>
        public static MissionMonitorConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\missionmonitor.json";
            }

            MissionMonitorConfiguration configuration = new MissionMonitorConfiguration();
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
                        Logging.Debug("Failed to read missions configuration", ex);
                    }
                }
            }
            if (configuration == null)
            {
                configuration = new MissionMonitorConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        public static MissionMonitorConfiguration FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<MissionMonitorConfiguration>(json);
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\missionmonitor.json will be used
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
                filename = Constants.DATA_DIR + @"\missionmonitor.json";
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
