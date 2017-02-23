using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
using System.Collections.ObjectModel;
=======
>>>>>>> Add material monitor.
using System.IO;
using System.Linq;
using Utilities;

namespace EddiMaterialMonitor
{
    /// <summary>Storage for configuration of material amounts</summary>
    public class MaterialMonitorConfiguration
    {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        public ObservableCollection<MaterialAmount> materials { get; set; }
=======
        public IDictionary<string, Limits> limits { get; set; }
>>>>>>> Add material monitor.

        [JsonIgnore]
        private string dataPath;

        public MaterialMonitorConfiguration()
        {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
            materials = new ObservableCollection<MaterialAmount>();
=======
            limits = new Dictionary<string, Limits>();
>>>>>>> Add material monitor.
        }

        /// <summary>
        /// Obtain materials configuration from a file.  If the file name is not supplied the the default
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        /// path of Constants.Data_DIR\materialmonitor.json is used
=======
        /// path of Constants.Data_DIR\materialsmonitor.json is used
>>>>>>> Add material monitor.
        /// </summary>
        public static MaterialMonitorConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
                filename = Constants.DATA_DIR + @"\materialmonitor.json";
=======
                filename = Constants.DATA_DIR + @"\materialsmonitor.json";
>>>>>>> Add material monitor.
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

<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
            //// We fully populate the list with all known materials
            //foreach (Material material in Material.MATERIALS)
            //{
            //    Limits cur;
            //    if (!configuration.limits.TryGetValue(material.EDName, out cur))
            //    {
            //        configuration.limits[material.EDName] = new Limits(null, null, null);
            //    }
            //}
=======
            // We fully populate the list with all known materials
            foreach (Material material in Material.MATERIALS)
            {
                Limits cur;
                if (!configuration.limits.TryGetValue(material.EDName, out cur))
                {
                    configuration.limits[material.EDName] = new Limits(null, null, null);
                }
            }
>>>>>>> Add material monitor.

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
        /// Constants.Data_DIR\materialmonitor.json will be used
=======
        /// Constants.Data_DIR\materialsmonitor.json will be used
>>>>>>> Add material monitor.
        /// </summary>
        public void ToFile(string filename=null)
        {
            // Remove any items that are all NULL
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
            //limits = limits.Where(x => x.Value.minimum.HasValue || x.Value.desired.HasValue || x.Value.maximum.HasValue).ToDictionary(x => x.Key, x => x.Value);
=======
            limits = limits.Where(x => x.Value.minimum.HasValue || x.Value.desired.HasValue || x.Value.maximum.HasValue).ToDictionary(x => x.Key, x => x.Value);
>>>>>>> Add material monitor.

            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
<<<<<<< 7cbcc8de1cbcbf428f25b18ac4a1658a1f0ba9cf
                filename = Constants.DATA_DIR + @"\materialmonitor.json";
=======
                filename = Constants.DATA_DIR + @"\materialsmonitor.json";
>>>>>>> Add material monitor.
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
