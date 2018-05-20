using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Utilities;

namespace Eddi
{
    /// <summary>Configuration for EDDI</summary>
    public class EDDIConfiguration : INotifyPropertyChanged
    {
        [JsonProperty("homeSystem")]
        public string HomeSystem
        {
            get { return _HomeSystem; }
            set { _HomeSystem = value; }
        }
        [JsonProperty("homeStation")]
        public string HomeStation
        {
            get { return _HomeStation; }
            set { _HomeStation = value; }
        }
        [JsonProperty("debug")]
        public bool Debug { get; set; }
        [JsonProperty("beta")]
        public bool Beta { get; set; }
        [JsonProperty("insurance")]
        public decimal Insurance { get; set; }
        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }
        [JsonProperty("Gender")]
        public string Gender { get; set; } = "Male";

        /// <summary>the current export target for the shipyard</summary>
        [JsonProperty("exporttarget")]
        public string exporttarget { get; set; } = "Coriolis";

        /// <summary> Administrative values </summary>
        public bool validSystem { get; set; }
        public bool validStation { get; set; }

        private string _HomeSystem;
        private string _HomeStation;

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public EDDIConfiguration()
        {
            Debug = false;
            Beta = false;
            Insurance = 5;
            Plugins = new Dictionary<string, bool>();
            exporttarget = "Coriolis";
            Gender = "Male";

            // Default the galnet monitor to 'off'
            Plugins.Add("Galnet monitor", false);
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\eddi.json is used
        /// </summary>
        public static EDDIConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\eddi.json";
            }

            EDDIConfiguration configuration = new EDDIConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        configuration = JsonConvert.DeserializeObject<EDDIConfiguration>(data);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("EDDI configuration file could not be read", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new EDDIConfiguration();
                Files.ignoreMissing = true; // This is our first run - we don't need to report missing config files for this session
            }

            configuration.dataPath = filename;
            if (configuration.Plugins == null)
            {
                configuration.Plugins = new Dictionary<string, bool>();
            }

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\eddi.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\eddi.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
