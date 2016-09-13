using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>Scripts for the speech responder</summary>
    public class ScriptsConfiguration
    {
        [JsonIgnore]
        private static Dictionary<string, Script> defaultScripts;

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
                string filename = dir.FullName + "\\defaultscripts.json";
                defaultScripts = JsonConvert.DeserializeObject<Dictionary<string, Script>>(File.ReadAllText(filename));
                Logging.Info("*******************************************************************************************");
                Logging.Info("Default scripts are " + JsonConvert.SerializeObject(defaultScripts));
                Logging.Info("*******************************************************************************************");
            }
            catch
            {
                defaultScripts = new Dictionary<string, Script>();
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

            ScriptsConfiguration configuration = null;
            try
            {
                configuration = JsonConvert.DeserializeObject<ScriptsConfiguration>(File.ReadAllText(filename));
            }
            catch
            {
            }
            
            if (configuration == null)
            {
                configuration = new ScriptsConfiguration();
            }
            configuration.dataPath = filename;
            if (configuration.Scripts == null)
            {
                configuration.Scripts = new Dictionary<string, Script>();
            }

            // Set default scripts appropriately
            foreach (KeyValuePair<string, Script> defaultScript in defaultScripts)
            {
                Logging.Info("Looking for existing details for " + defaultScript.Key);
                Script existingScript;
                configuration.Scripts.TryGetValue(defaultScript.Key, out existingScript);
                Script newScript = new Script(defaultScript.Value.Name, defaultScript.Value.Description, defaultScript.Value.Value);

                Logging.Info("Existing script is " + (existingScript == null ? null : JsonConvert.SerializeObject(existingScript)));
                Logging.Info("New script is " + (newScript == null ? null : JsonConvert.SerializeObject(newScript)));
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
