using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Utilities;

namespace EddiCompanionAppService
{
    /// <summary>
    /// Storage for the Commander Configs
    /// </summary>
    public class CommanderConfiguration
    {
        [JsonProperty("phoneticName")]
        public string PhoneticName { get; set; }

        [JsonIgnore]
        private string dataPath;

        /// <summary>
        /// Obtain commander config from a file. If  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\commander.json is used
        /// </summary>
        /// <param name="filename"></param>
        public static CommanderConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\commander.json";
            }

            CommanderConfiguration configuration = new CommanderConfiguration();
            try
            {
                string configData = File.ReadAllText(filename);
                configuration = JsonConvert.DeserializeObject<CommanderConfiguration>(configData);
            }
            catch (Exception ex)
            {
                Logging.Debug("Failed to read commander configuration", ex);
            }
            if (configuration == null)
            {
                configuration = new CommanderConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Clear the information
        /// </summary>
        public void Clear()
        {
            PhoneticName = null;
        }

        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\commander.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
