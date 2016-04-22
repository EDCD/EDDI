using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EliteDangerousCompanionAppService
{
    /// <summary>
    /// Storage for the Commander Configs
    /// </summary>
    public class CommanderConfiguration
    {
        [JsonProperty("phoneticName")]
        public string PhoneticName { get; set; }

        [JsonIgnore]
        private String dataPath;

        /// <summary>
        /// Obtain commander config from a file. If  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\commander.json is used
        /// </summary>
        /// <param name="filename"></param>
        public static CommanderConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\commander.json";
            }

            CommanderConfiguration speech;
            try
            {
                String configData = File.ReadAllText(filename);
                speech = JsonConvert.DeserializeObject<CommanderConfiguration>(configData);
            }
            catch
            {
                speech = new CommanderConfiguration();
            }

            speech.dataPath = filename;
            return speech;
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
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\commander.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
