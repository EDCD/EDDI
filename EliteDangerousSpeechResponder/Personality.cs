using EliteDangerousEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousSpeechResponder
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

        public Personality(string name, string description, Dictionary<string, Script> scripts)
        {
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
        /// default of %APPDATA%\EDDI\personalities is used
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
            return FromFile(Constants.DATA_DIR + @"\personalities\" + name + ".json");
        }

        /// <summary>
        /// Obtain the default personality
        /// </summary>
        public static Personality Default()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            return FromFile(dir.FullName + "\\personality.json");
        }

        /// <summary>
        /// Obtain personality from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\personalities\eddi.json is used
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
                filename = Constants.DATA_DIR + @"\personalities\eddi.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        /// <summary>
        /// Create a copy of this file, altering the datapath appropriately
        /// </summary>
        public Personality Copy(string name, string description)
        {
            // Save a copy of this personality
            string iname = name.ToLowerInvariant();
            string copyPath = Constants.DATA_DIR + @"\personalities\" + iname + ".json";
            ToFile(copyPath);
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
                    script = new Script(defaultEvent.Key, defaultEvent.Value, true, script.Value);
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
            personality.Scripts = fixedScripts;
        }
    }
}
