using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Utilities;

namespace EddiCrimeMonitor
{
    /// <summary>Storage for configuration of criminal record details</summary>
    public class CrimeMonitorConfiguration
    {
        public ObservableCollection<FactionRecord> criminalrecord { get; set; }
        public Dictionary<string, string> homeSystems { get; set; }

        public long claims { get; set; }
        public long fines { get; set; }
        public long bounties { get; set; }
        public int? maxStationDistanceFromStarLs { get; set; }
        public bool prioritizeOrbitalStations { get; set; }
        public DateTime updatedat { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public CrimeMonitorConfiguration()
        {
            criminalrecord = new ObservableCollection<FactionRecord>();
            homeSystems = new Dictionary<string, string>();
        }

        /// <summary>
        /// Obtain criminal record configuration from json.
        /// </summary>
        public static CrimeMonitorConfiguration FromJson(dynamic json)
        {
            CrimeMonitorConfiguration configuration = new CrimeMonitorConfiguration();
            if (json != null)
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<CrimeMonitorConfiguration>(json);
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to obtain cargo configuration", ex);
                }
            }
            return configuration;
        }

        /// <summary>
        /// Obtain criminal record configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\crimemonitor.json is used
        /// </summary>
        public static CrimeMonitorConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\crimemonitor.json";
            }

            CrimeMonitorConfiguration configuration = new CrimeMonitorConfiguration();
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
                        Logging.Debug("Failed to read crime configuration", ex);
                    }
                }
            }
            if (configuration == null)
            {
                configuration = new CrimeMonitorConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        public static CrimeMonitorConfiguration FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<CrimeMonitorConfiguration>(json);
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\crimemonitor.json will be used
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\crimemonitor.json";
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