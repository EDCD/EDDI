using System;
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

        [JsonIgnore]
        static readonly object fileLock = new object();

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
                string configData = Files.Read(filename);
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
            lock (fileLock)
            {
                Files.Write(filename, json);
            }
        }
    }
}
