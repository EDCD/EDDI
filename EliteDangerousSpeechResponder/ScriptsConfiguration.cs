using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>Scritps for the speech responder</summary>
    public class ScriptsConfiguration
    {
        [JsonIgnore]
        private static Dictionary<string, Script> defaultConfiguration;

        [JsonProperty("scripts")]
        public Dictionary<string, Script> Scripts { get; set; }

        [JsonIgnore]
        private string dataPath;

        static ScriptsConfiguration()
        {
            // Load default scripts
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                string filename = dir.Name + "\\defaultscripts.json";
                defaultConfiguration = JsonConvert.DeserializeObject<Dictionary<string, Script>>(File.ReadAllText(filename));
            }
            catch
            {
                Logging.Warn("Failed to load default scripts");
                defaultConfiguration = new Dictionary<string, Script>();
            }
        }

        public ScriptsConfiguration()
        {
        }

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\scripts.json is used
        /// </summary>
        public static ScriptsConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                string dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\scripts.json";
            }

            ScriptsConfiguration configuration;
            try
            {
                configuration = JsonConvert.DeserializeObject<ScriptsConfiguration>(File.ReadAllText(filename));
            }
            catch
            {
                configuration = new ScriptsConfiguration();
            }
            configuration.dataPath = filename;

            // Set default scripts appropriately
            foreach (KeyValuePair<string, Script> defaultScript in defaultConfiguration)
            {
                Script existingScript;
                configuration.Scripts.TryGetValue(defaultScript.Key, out existingScript);
                Script newScript = new Script(defaultScript.Value.Name, defaultScript.Value.Description, defaultScript.Value.Value);

                if (existingScript != null)
                {
                    newScript.Enabled = existingScript.Enabled;
                    if (!newScript.isDefault())
                    {
                        newScript.Value = existingScript.Value;
                    }
                }
                configuration.Scripts.Remove(defaultScript.Key);
                configuration.Scripts.Add(defaultScript.Key, newScript);
            }

            Logging.Info("Scripts are " + JsonConvert.SerializeObject(configuration));
            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// %APPDATA%\EDDI\scripts.json will be used
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                string dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\scripts.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
