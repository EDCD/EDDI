using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// A personality is a combination of scripts used to respond to specific events
    /// </summary>
    public class Personality
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("scripts")]
        public Dictionary<string, Script> Scripts { get; private set; }

        [JsonIgnore]
        private string dataPath;

        private static readonly string DEFAULT_PATH = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName + @"\eddi.json";

        public Personality(string name, string description, Dictionary<string, Script> scripts)
        {
            // Ensure that the name doesn't have any illegal characters
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            Name = r.Replace(name, "");

            Name = name;
            Description = description;
            Scripts = scripts;
        }

        [JsonIgnore]
        public bool IsEditable
        {
            get { return (Name != "EDDI"); }
            set { }
        }

        /// <summary>
        /// Obtain all personalities from a directory.  If the directory name is not supplied the
        /// default of Constants.Data_DIR\personalities is used
        /// </summary>
        public static List<Personality> AllFromDirectory(string directory = null)
        {
            List<Personality> personalities = new List<Personality>();
            if (directory == null)
            {
                directory = Constants.DATA_DIR + @"\personalities";
                Directory.CreateDirectory(directory);
            }
            foreach (FileInfo file in new DirectoryInfo(directory).GetFiles("*.json", SearchOption.AllDirectories))
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
            if (name == "EDDI")
            {
                return Default();
            }
            else
            {
                return FromFile(Constants.DATA_DIR + @"\personalities\" + name.ToLowerInvariant() + ".json");
            }
        }

        /// <summary>
        /// Obtain the default personality
        /// </summary>
        public static Personality Default()
        {
            return FromFile(DEFAULT_PATH);
        }

        /// <summary>
        /// Obtain personality from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\personalities\eddi.json is used
        /// </summary>
        public static Personality FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\personalities\eddi.json";
            }

            Personality personality = null;
            try
            {
                personality = JsonConvert.DeserializeObject<Personality>(File.ReadAllText(filename));
            }
            catch (Exception e)
            {
                Logging.Warn("Failed to access personality at " + filename + ": " + e.Message);
            }

            if (personality != null)
            {
                personality.dataPath = filename;
                fixPersonalityInfo(personality);
            }

            return personality;
        }

        /// <summary>
        /// Write personality to a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\personalities\eddi.json is used
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\personalities\eddi.json";
            }

            if (filename != DEFAULT_PATH)
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filename, json);
            }
        }

        /// <summary>
        /// Create a copy of this file, altering the datapath appropriately
        /// </summary>
        public Personality Copy(string name, string description)
        {
            // Tidy the name up to avoid bad characters
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            name = r.Replace(name, "");

            // Save a copy of this personality
            string iname = name.ToLowerInvariant();
            string copyPath = Constants.DATA_DIR + @"\personalities\" + iname + ".json";
            // Ensure it doesn't exist
            if (!File.Exists(copyPath))
            {
                ToFile(copyPath);
            }
            // Load the personality back in
            Personality newPersonality = FromFile(copyPath);
            // Change its name and description and save it back out again
            newPersonality.Name = name;
            newPersonality.Description = description;
            newPersonality.ToFile();
            // And finally return it
            return newPersonality;
        }

        public void RemoveFile()
        {
            File.Delete(dataPath);
        }

        /// <summary>
        /// Fix up the personality information to ensure that it contains the correct event
        /// </summary>
        private static void fixPersonalityInfo(Personality personality)
        {
            Dictionary<string, Script> fixedScripts = new Dictionary<string, Script>();
            // Ensure that every required event is present
            foreach (KeyValuePair<string, string> defaultEvent in Events.DESCRIPTIONS)
            {
                Script script;
                personality.Scripts.TryGetValue(defaultEvent.Key, out script);
                if (script == null)
                {
                    // The personality doesn't have this event; create a default
                    string defaultScript = Events.DefaultByName(defaultEvent.Key);
                    script = new Script(defaultEvent.Key, defaultEvent.Value, true, defaultScript);
                }
                else if (script.Description != defaultEvent.Value)
                {
                    // The description has been updated
                    script = new Script(defaultEvent.Key, defaultEvent.Value, true, script.Value, script.Priority);
                }
                fixedScripts.Add(defaultEvent.Key, script);
            }
            foreach (KeyValuePair<string, Script> kv in personality.Scripts)
            {
                if (!fixedScripts.ContainsKey(kv.Key))
                {
                    fixedScripts.Add(kv.Key, kv.Value);
                }
            }
            if (personality.Name != "EDDI")
            {
                // Also add any secondary scripts in the default personality that aren't present in the list
                Personality defaultPersonality = Default();
                foreach (KeyValuePair<string, Script> kv in defaultPersonality.Scripts)
                {
                    if (!fixedScripts.ContainsKey(kv.Key))
                    {
                        fixedScripts.Add(kv.Key, kv.Value);
                    }
                }
            }

            personality.Scripts = fixedScripts;
        }
    }
}
