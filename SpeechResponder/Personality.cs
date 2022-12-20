using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// A personality is a combination of scripts used to respond to specific events
    /// </summary>
    public class Personality : INotifyPropertyChanged
    {
        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            private set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("description")]
        public string Description
        {
            get => _description;
            private set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("scripts")]
        public Dictionary<string, Script> Scripts
        {
            get => _scripts;
            private set
            {
                if (_scripts != value)
                {
                    _scripts = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool IsCustom
        {
            get => _isCustom;
            set
            {
                _isCustom = value;
                OnPropertyChanged();
            }
        }
        
        [JsonIgnore]
        private string _name;

        [JsonIgnore]
        private string _description;

        [JsonIgnore]
        private Dictionary<string, Script> _scripts;

        [JsonIgnore]
        private bool _isCustom;        

        [JsonIgnore]
        private string dataPath;

        private static readonly string[] obsoleteScriptKeys = 
        {
            "Jumping", // Replaced by "FSD engaged" script
            "Crew member role change", // This name is mismatched to the key (should be "changed"), so EDDI couldn't match the script name to the .json key correctly. The default script has been corrected.
            "Ship low fuel", // Accidental duplicate. The real event is called 'Low fuel'
            "Modification applied", // Event deprecated by FDev, no longer written. 
            "List launchbays", // Replaced by "Launchbay report" script
            "Combat promotion", // Replaced by "Commander promotion" script
            "Empire promotion", // Replaced by "Commander promotion" script
            "Exploration promotion", // Replaced by "Commander promotion" script
            "Federation promotion", // Replaced by "Commander promotion" script
            "Trade promotion", // Replaced by "Commander promotion" script
            "Ship repurchased" // Replaced by "Respawned" script
        };

        private static readonly string[] ignoredEventKeys =
        {
            // Shares updates with monitors / responders but are not intended to be user facing
            "Cargo",
            "Fleet carrier materials",
            "Market",
            "Outfitting",
            "Shipyard",
            "Squadron startup",
            "Stored ships",
            "Stored modules",
            "Unhandled event"
        };
        private static readonly string DIRECTORYPATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string DEFAULT_PATH = new DirectoryInfo(DIRECTORYPATH).FullName + @"\" + Properties.SpeechResponder.default_personality_script_filename;
        private static readonly string DEFAULT_USER_PATH = Constants.DATA_DIR + @"\personalities\" + Properties.SpeechResponder.default_personality_script_filename;

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
            return FromFile(DEFAULT_PATH, true);
        }

        /// <summary>
        /// Obtain personality from a file.
        /// </summary>
        public static Personality FromFile(string filename = null, bool isDefault = false)
        {
            if (filename == null)
            {
                filename = DEFAULT_USER_PATH;
                isDefault = true;
            }

            Personality personality = null;
            string data = Files.Read(filename);
            if (data != null)
            {
                try
                {
                    personality = JsonConvert.DeserializeObject<Personality>(data);
                }
                catch (Exception e)
                {
                    if (!isDefault)
                    {
                        // malformed JSON for some reason: rename so that the user can examine and fix it.
                        string newFileName = filename + ".malformed";
                        if (File.Exists(newFileName))
                        {
                            // no point keeping a history: only the latest is likely to be useful. Pro users will be using version control anyway.
                            File.Delete(newFileName);
                        }
                        File.Move(filename, newFileName);

                        Logging.Error($"Could not parse \"{filename}\": moved to \"{newFileName}\". Error was \"{e.Message}\"");
                    }
                    else
                    {
                        throw new FormatException("Could not parse default personality (eddi.json)");
                    }
                }
            }

            if (personality != null)
            {
                personality.dataPath = filename;
                personality.IsCustom = !isDefault;
                fixPersonalityInfo(personality);
            }

            return personality;
        }

        /// <summary>
        /// Write personality to a file.
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = DEFAULT_USER_PATH;
            }

            if (filename != DEFAULT_PATH)
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                Files.Write(filename, json);
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
            // Default personality for reference scripts
            var defaultPersonality = !personality.IsCustom ? null : Default();

            var fixedScripts = new Dictionary<string, Script>();

            // First, iterate through our default scripts. Ensure that every required event script is present.
            List<string> missingScripts = new List<string>();
            foreach (var defaultEvent in Events.DESCRIPTIONS)
            {
                personality.Scripts.TryGetValue(defaultEvent.Key, out Script script);
                Script defaultScript = null;
                defaultPersonality?.Scripts?.TryGetValue(defaultEvent.Key, out defaultScript);
                script = UpgradeScript(script, defaultScript);
                if (!obsoleteScriptKeys.Contains(defaultEvent.Key))
                {
                    if (script == null && !ignoredEventKeys.Contains(defaultEvent.Key))
                    {
                        missingScripts.Add(defaultEvent.Key);
                    }
                    else if (script != null)
                    {
                        script.PersonalityIsCustom = personality.IsCustom;
                        fixedScripts.Add(defaultEvent.Key, script);
                    }
                }
            }
            // Report missing scripts for events from the events list, except those we have specifically named
            if (missingScripts.Count > 0)
            {
                Logging.Info("Failed to find scripts" + string.Join(";", missingScripts));
            }
            // Also add any secondary scripts present in the default personality but which aren't present in the events list
            if (defaultPersonality?.Scripts != null)
            {
                foreach (var kv in defaultPersonality.Scripts)
                {
                    if (!fixedScripts.ContainsKey(kv.Key) && !obsoleteScriptKeys.Contains(kv.Key))
                    {
                        Script defaultScript = null;
                        defaultPersonality.Scripts?.TryGetValue(kv.Key, out defaultScript);
                        var script = UpgradeScript(kv.Value, defaultScript);
                        script.PersonalityIsCustom = personality.IsCustom;
                        fixedScripts.Add(kv.Key, script);
                    }
                }
            }

            // Next, iterate through the personality's scripts and add any secondary scripts from the personality.
            foreach (var kv in personality.Scripts)
            {
                // Add non-event scripts from the personality for which we do not have a default
                if (!fixedScripts.ContainsKey(kv.Key) && !obsoleteScriptKeys.Contains(kv.Key))
                {
                    var script = kv.Value;
                    script.PersonalityIsCustom = personality.IsCustom;
                    fixedScripts.Add(kv.Key, script);
                }
            }

            // Sort scripts and save to file
            personality.Scripts = fixedScripts.OrderBy(s => s.Key).ToDictionary(s => s.Key, s => s.Value);
            personality.ToFile();
        }

        public static Script UpgradeScript(Script personalityScript, Script defaultScript)
        {
            Script script = personalityScript ?? defaultScript;
            if (script != null)
            {
                if (defaultScript != null)
                {
                    if (script.Default)
                    {
                        // This is a default script so take the latest value
                        script.Value = defaultScript.Value;
                    }

                    // Set the default value of our script
                    script.defaultValue = defaultScript.Value;

                    if (defaultScript.Responder)
                    {
                        // This is a responder script so update applicable parameters
                        script.Description = defaultScript.Description;
                        script.Responder = defaultScript.Responder;
                    }
                }
            }

            return script;
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
