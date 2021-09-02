using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiConfigService
{
    /// <summary>Storage for configuration of mission details</summary>
    public class MissionMonitorConfiguration
    {
        public ObservableCollection<Mission> missions { get; set; }

        public DateTime updatedat { get; set; }
        public int goalsCount { get; set; }
        public int missionsCount { get; set; }
        public int? missionWarning { get; set; } = 60;

        [JsonIgnore]
        private string dataPath;

        public MissionMonitorConfiguration()
        {
            missions = new ObservableCollection<Mission>();
        }

        /// <summary>
        /// Obtain configuration from a json.
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
        /// Obtain configuration from a file.  If the file name is not supplied the the default path is used
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
        /// when reading in the configuration will be used, unless it too is null, for example
        /// a test config purely based on JSON, in which case nothing will be written.
        /// </summary>
        public void ToFile(string filename = null)
        {
            filename = filename ?? dataPath;
            if (filename == null) { return; }

            string json = null;
            LockManager.GetLock(nameof(MissionMonitorConfiguration), () =>
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
