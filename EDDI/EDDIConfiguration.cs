using EddiDataDefinitions;
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
        [JsonIgnore]
        private string _HomeSystem = null;
        [JsonProperty("homeSystem")]
        public string HomeSystem
        {
            get { return _HomeSystem; }
            set { _HomeSystem = value; }
        }

        [JsonIgnore]
        private string _HomeStation = null;
        [JsonProperty("homeStation")]
        public string HomeStation
        {
            get { return _HomeStation; }
            set { _HomeStation = value; }
        }

        [JsonIgnore]
        private string _SquadronName = null;
        [JsonProperty("squadronName")]
        public string SquadronName
        {
            get { return _SquadronName; }
            set { _SquadronName = value; }
        }

        [JsonIgnore]
        private string _SquadronID = null;
        [JsonProperty("squadronID")]
        public string SquadronID
        {
            get { return _SquadronID; }
            set { _SquadronID = value; }
        }

        [JsonProperty("squadronRank")]
        public string squadronRank
        {

            get => SquadronRank?.edname ?? "None";
            set
            {
                SquadronRank srDef = SquadronRank.FromEDName(value);
                this.SquadronRank = srDef;
            }
        }
        [JsonIgnore]
        private SquadronRank _SquadronRank = SquadronRank.None;
        [JsonIgnore]
        public SquadronRank SquadronRank
        {
            get { return _SquadronRank; }
            set { _SquadronRank = value; }
        }

        [JsonProperty("squadronAllegiance")]
        public string squadronAllegiance
        {

            get => SquadronAllegiance?.edname ?? Superpower.None.edname;
            set
            {
                Superpower saDef = Superpower.FromEDName(value);
                this.SquadronAllegiance = saDef;
            }
        }
        [JsonIgnore]
        private Superpower _SquadronAllegiance = Superpower.None;
        [JsonIgnore]
        public Superpower SquadronAllegiance
        {
            get { return _SquadronAllegiance; }
            set { _SquadronAllegiance = value; }
        }

        [JsonProperty("squadronPower")]
        public string squadronPower
        {

            get => SquadronPower?.edname ?? Power.None.edname;
            set
            {
                Power spDef = Power.FromEDName(value);
                this.SquadronPower = spDef;
            }
        }
        [JsonIgnore]
        private Power _SquadronPower = Power.None;
        [JsonIgnore]
        public Power SquadronPower
        {
            get { return _SquadronPower; }
            set { _SquadronPower = value; }
        }

        [JsonIgnore]
        private string _SquadronSystem = null;
        [JsonProperty("squadronSystem")]
        public string SquadronSystem
        {
            get { return _SquadronSystem; }
            set { _SquadronSystem = value; }
        }

        [JsonIgnore]
        private string _SquadronFaction = null;
        [JsonProperty("squadronFaction")]
        public string SquadronFaction
        {
            get { return _SquadronFaction; }
            set { _SquadronFaction = value; }
        }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("beta")]
        public bool Beta { get; set; }

        [JsonProperty("plugins")]
        public IDictionary<string, bool> Plugins { get; set; }

        [JsonProperty("Gender")]
        public string Gender { get; set; } = "Male";

        /// <summary>the current export target for the shipyard</summary>
        [JsonProperty("exporttarget")]
        public string exporttarget { get; set; } = "Coriolis";

        /// <summary> Administrative values </summary>
        public bool validHomeSystem { get; set; }
        public bool validHomeStation { get; set; }
        public bool validSquadronSystem { get; set; }

        [JsonIgnore]
        private string dataPath;

        [JsonIgnore]
        static readonly object fileLock = new object();

        public EDDIConfiguration()
        {
            Debug = false;
            Beta = false;
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
