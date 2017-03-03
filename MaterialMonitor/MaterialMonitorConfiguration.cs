using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Utilities;

namespace EddiMaterialMonitor
{
    /// <summary>Storage for configuration of material amounts</summary>
    public class MaterialMonitorConfiguration
    {
        public ObservableCollection<MaterialAmount> materials { get; set; }

        [JsonIgnore]
        private string dataPath;

        public MaterialMonitorConfiguration()
        {
            materials = new ObservableCollection<MaterialAmount>();
        }

        /// <summary>
        /// Obtain materials configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\materialsmonitor.json is used
        /// </summary>
        public static MaterialMonitorConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\materialsmonitor.json";
            }

            MaterialMonitorConfiguration configuration = new MaterialMonitorConfiguration();
            try
            {
                configuration = JsonConvert.DeserializeObject<MaterialMonitorConfiguration>(File.ReadAllText(filename));
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read materials configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new MaterialMonitorConfiguration();
            }

            //// We fully populate the list with all known materials
            //foreach (Material material in Material.MATERIALS)
            //{
            //    Limits cur;
            //    if (!configuration.limits.TryGetValue(material.EDName, out cur))
            //    {
            //        configuration.limits[material.EDName] = new Limits(null, null, null);
            //    }
            //}

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\materialsmonitor.json will be used
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
                filename = Constants.DATA_DIR + @"\materialsmonitor.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
