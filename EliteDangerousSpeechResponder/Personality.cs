using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// A personality is a combination of scripts used to respond to specific events
    /// </summary>
    class Personality
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("scripts")]
        public Dictionary<string, Script> Scripts { get; private set; }

        [JsonIgnore]
        private string dataPath;

        public Personality(string name, string description, Dictionary<string, Script> scripts)
        {
            Name = name;
            Description = description;
            Scripts = scripts;
        }

        /// <summary>
        /// Obtain all personalities from a directory.  If the directory name is not supplied the
        /// default of %APPDATA%\EDDI\personalities is used
        /// </summary>
        public static List<Personality> AllFromDirectory(string directory = null)
        {
            List<Personality> personalities = new List<Personality>();
            if (directory == null)
            {
                directory = Environment.GetEnvironmentVariable("AppData") + "\\EDDI\\personalities";
            }
            foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
            {
                Personality personality = FromFile(file.FullName);
                if (personality != null)
                {
                    personalities.Add(personality);
                }
            }

            return personalities;
        }

        public static Personality FromName(string name)
        {
            return FromFile(Environment.GetEnvironmentVariable("AppData") + "\\EDDI\\personalities\\" + name + ".json");
        }

        /// <summary>
        /// Obtain personality from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\personalities\eddi.json is used
        /// </summary>
        public static Personality FromFile(string filename = null)
        {
            if (filename == null)
            {
                string dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI\\personalities";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\eddi.json";
            }

            Personality personality = null;
            try
            {
                personality = JsonConvert.DeserializeObject<Personality>(File.ReadAllText(filename));
                Logging.Debug("Personality is " + JsonConvert.SerializeObject(personality));
            }
            catch (Exception e)
            {
                if (filename == null)
                {
                    Logging.Warn("Failed to access default personality: " + e.Message);
                }
                else
                {
                    Logging.Warn("Failed to access personality: " + e.Message);
                    personality = FromFile(null);
                }
            }

            if (personality != null)
            {
                personality.dataPath = filename;
            }

            return personality;
        }

        /// <summary>
        /// Write personality to a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\personalities\eddi.json is used
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                string dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI\\personalities";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\eddi.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
