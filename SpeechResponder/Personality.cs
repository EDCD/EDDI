using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
                if ( _isCustom == value ) { return; }

                _isCustom = value;
                ApplyScriptPersonalityState();
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
        internal string dataPath;

        [ JsonIgnore ] 
        private static Personality _defaultPersonality;

        private static readonly string[] obsoleteScriptKeys =
        [
            "Combat promotion", // Replaced by "Commander promotion" script
            "Crew member role change", // This name is mismatched to the key (should be "changed"), so EDDI couldn't match the script name to the .json key correctly. The default script has been corrected.
            "Empire promotion", // Replaced by "Commander promotion" script
            "Entered signal source", // Replaced by "Destination arrived" script
            "Exploration promotion", // Replaced by "Commander promotion" script
            "Federation promotion", // Replaced by "Commander promotion" script
            "Fighter docked", // Replaced by "Vessel docked"
            "Fighter launched", // Replaced by "Vessel launched"
            "Jumping", // Replaced by "FSD engaged" script
            "List launchbays", // Replaced by "Launchbay report" script
            "Modification applied", // Event deprecated by FDev, no longer written. 
            "Power commodity fast tracked", // Made obsolete in Powerplay 2.0 which no longer allows fast tracking commodity allocations
            "Power defected", // Made obsolete in Powerplay 2.0 which no longer includes a defection mechanic.
            "Power expansion vote cast", // Made obsolete in Powerplay 2.0 which no longer includes a voting system
            "Power preparation vote cast", // Made obsolete in Powerplay 2.0 which no longer includes a voting system
            "Power salary claimed", // Made obsolete in Powerplay 2.0 which no longer includes a weekly salary
            "Ship low fuel", // Accidental duplicate. The real event is called 'Low fuel'
            "Ship repurchased", // Replaced by "Respawned" script
            "SRV docked", // Replaced by "Vessel docked"
            "SRV launched", // Replaced by "Vessel launched"
            "Trade promotion", // Replaced by "Commander promotion" script
            "Vehicle destroyed", // Replaced by "Vessel destroyed"
        ];

        private static readonly string[] ignoredEventKeys =
        [
            // Shares updates with monitors / responders but are not intended to be user facing
            CargoEvent.NAME,
            FleetCarrierMaterialsEvent.NAME,
            MarketEvent.NAME,
            OutfittingEvent.NAME,
            ShipyardEvent.NAME,
            SquadronStartupEvent.NAME,
            StoredShipsEvent.NAME,
            StoredModulesEvent.NAME,
            UnhandledEvent.NAME
        ];

        private static readonly string DIRECTORYPATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string DEFAULT_PATH =>
            Path.Combine(
                new DirectoryInfo( DIRECTORYPATH ).FullName,
                Properties.SpeechResponder.default_personality_script_filename );
        private static string DEFAULT_USER_PATH =>
            Path.Combine(
                Constants.DATA_DIR,
                "personalities",
                Properties.SpeechResponder.default_personality_script_filename );
        
        private static readonly List<string> upgradedPersonalities = [ ];

        public Personality(string name, string description, Dictionary<string, Script> scripts)
        {
            // Ensure that the name doesn't have any illegal characters
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex($"[{Regex.Escape(regexSearch)}]");
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
            List<Personality> personalities = [ ];
            if (directory == null)
            {
                directory = Constants.DATA_DIR + @"\personalities";
                Directory.CreateDirectory(directory);
            }
            foreach (var file in new DirectoryInfo(directory).GetFiles("*.json", SearchOption.AllDirectories))
            {
                var personality = FromFile(file.FullName);
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
            return  _defaultPersonality ??= FromFile( DEFAULT_PATH, true ) ;
        }

        /// <summary>
        ///  Language changes currently require a restart. This reset assists with unit testing.
        /// </summary>
        public static void ResetDefault ()
        {
            _defaultPersonality = null;
        }

        /// <summary>
        /// Obtain personality from a file.
        /// </summary>
        public static Personality FromFile(string filename = null, bool isDefault = false)
        {
            if ( isDefault )
            {
                Logging.Debug( $"Loading default SpeechResponder personality from '{DEFAULT_PATH}' " +
                               $"for UI culture '{CultureInfo.CurrentUICulture.Name}'." );
            }
            
            if (filename == null)
            {
                filename = DEFAULT_USER_PATH;
                isDefault = true;
            }

            Personality personality = null;
            var data = Files.Read(filename);
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
                        var newFileName = filename + ".malformed";
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
                if ( !isDefault )
                {
                    fixPersonalityInfo( personality );
                }
                else
                {
                    foreach ( var script in personality.Scripts.Values )
                    {
                        script.defaultValue = script.Value;
                    }
                }
            }

            return personality;
        }

        /// <summary>
        /// Write personality to a file.
        /// </summary>
        public void ToFile(string filename = null)
        {
            filename ??= dataPath;
            filename ??= DEFAULT_USER_PATH;

            if (filename != DEFAULT_PATH)
            {
                Files.Write(filename, ToJson());
            }
        }

        internal string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static void incrementPersonalityBackups(Personality personality)
        {
            if (!personality.IsCustom) { return; }
            if (Files.unitTesting) { return; }

            var filesToMove = new Dictionary<string, string>(); // Key = FROM, Value = TO
            var filesToDelete = new List<string>();

            // Obtain files, sorting by last write time to ensure that older files are incremented prior to newer files
            var personalityDirInfo = new FileInfo(personality.dataPath).Directory;
            if (personalityDirInfo is null) { return; }
            foreach (var file in personalityDirInfo.GetFiles()
                .Where(f =>
                    f.Name.StartsWith(personality.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    f.Name.EndsWith(".bak", StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(f => f.LastWriteTimeUtc)
                .ToList())
            {
                var personalityName = file.FullName
                    .Replace( $@"{personality.dataPath}", "" )
                    .Replace( ".bak", "" )
                    .Replace( ".", "" );
                var i = 0;
                if ( string.IsNullOrEmpty( personalityName ) || int.TryParse( personalityName, out i ) )
                {
                    ++i; // Increment our index number

                    if ( i >= 10 )
                    {
                        filesToDelete.Add( file.FullName );
                    }
                    else
                    {
                        filesToMove.Add( file.FullName, $@"{personality.dataPath}.{i}.bak" );
                    }
                }
            }
            try
            {
                LockManager.GetLock(nameof(personality.Name), () =>
                {
                    foreach (var deleteFilePath in filesToDelete)
                    {
                        File.Delete(deleteFilePath);
                    }
                    foreach (var moveFilePath in filesToMove)
                    {
                        File.Move(moveFilePath.Key, moveFilePath.Value);
                    }
                });
            }
            catch (Exception)
            {
                // Someone may have had the file open when this code executed? Nothing to do, we'll try again on the next run
            }

            // Save the most recent backup
            var backupPath = $"{personality.dataPath}.bak";
            if (File.Exists(personality.dataPath))
            {
                File.Copy(personality.dataPath, backupPath, true);
            }
            else
            {
                personality.ToFile(backupPath);
            }
        }

        /// <summary>
        /// Create a copy of this file, altering the datapath appropriately
        /// </summary>
        public Personality Copy(string name, string description)
        {
            // Tidy the name up to avoid bad characters
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            name = r.Replace(name, "");

            // Save a copy of this personality
            var iname = name.ToLowerInvariant();
            var copyPath = Constants.DATA_DIR + @"\personalities\" + iname + ".json";
            var scripts = Scripts.ToDictionary(kv => kv.Key, kv => kv.Value.Copy());
            var newPersonality = new Personality(name, description, scripts)
            {
                dataPath = copyPath,
                IsCustom = true
            };

            newPersonality.ToFile();
            return newPersonality;
        }

        public void RemoveFile()
        {
            File.Delete(dataPath);
        }

        internal void ApplyScriptPersonalityState()
        {
            if ( Scripts is null ) { return; }

            foreach ( var script in Scripts.Values )
            {
                script.PersonalityIsCustom = IsCustom;
            }
        }

        /// <summary>
        /// Fix up the personality information to ensure that it contains the correct event
        /// </summary>
        private static void fixPersonalityInfo(Personality personality)
        {
            if (upgradedPersonalities.Contains(personality.dataPath)) { return; }

            // Create or update a simple backup before we begin
            incrementPersonalityBackups(personality);

            // Default personality for reference scripts
            var defaultPersonality = Default();
            var fixedScripts = new Dictionary<string, Script>();
            var upgradeableScripts = new Dictionary<string, Script>();

            // First, iterate through our event keys, less any we have chosen to omit.
            // Ensure that every required event script is present.
            // Identify as "responder" scripts.
            SortScripts( Events.TYPES.Keys.Except(ignoredEventKeys), true );

            // Next, iterate through remaining default scripts which are not tied to an event.
            // Ensure that every required non-event script is present.
            // Identify as "non-responder" scripts.
            SortScripts( defaultPersonality.Scripts.Keys.Where( key =>
                !upgradeableScripts.ContainsKey( key ) &&
                !fixedScripts.ContainsKey( key ) ), false );

            // Next, try to upgrade each personality script referencing a matching script in the default personality.
            foreach ( var kv in upgradeableScripts )
            {
                if ( defaultPersonality.Scripts.TryGetValue( kv.Key, out var defaultScript ) && defaultScript != null )
                {
                    var script = UpgradeScript(kv.Value, defaultScript);
                    fixedScripts.Add( kv.Key, script );
                }
            }

            // Finally, iterate through the personality's scripts and add any
            // non-default secondary scripts from the personality which do not
            // have a match in the default personality. Preserve customized
            // obsolete scripts under a recovery name so user edits are not lost.
            foreach ( var kv in personality.Scripts )
            {
                if ( fixedScripts.ContainsKey( kv.Key ) )
                {
                    continue;
                }

                if ( obsoleteScriptKeys.Contains( kv.Key ) )
                {
                    if ( kv.Value.Default )
                    {
                        continue;
                    }

                    var recoveryKey = GetObsoleteScriptRecoveryKey( kv.Key );
                    if ( fixedScripts.ContainsKey( recoveryKey ) )
                    {
                        continue;
                    }

                    var scriptToRecover = personality.Scripts.TryGetValue( recoveryKey, out var existingRecoveryScript ) &&
                                          !existingRecoveryScript.Default
                        ? existingRecoveryScript
                        : kv.Value;
                    fixedScripts.Add( recoveryKey, PrepareObsoleteScriptForRecovery( recoveryKey, scriptToRecover ) );
                    continue;
                }

                if ( !kv.Value.Default )
                {
                    fixedScripts.Add( kv.Key, PrepareSecondaryScriptForRetention( kv.Key, kv.Value ) );
                }
            }

            // Set the `PersonalityIsCustom` property.
            foreach ( var kv in fixedScripts )
            {
                kv.Value.PersonalityIsCustom = personality.IsCustom;
            }

            // Sort scripts and save to file. 
            personality.Scripts = fixedScripts
                .OrderBy(s => s.Key)
                .ToDictionary(s => s.Key, s => s.Value);
            personality.ToFile();
            upgradedPersonalities.Add(personality.dataPath);
            Logging.Info( $"Upgraded custom personality '{personality.Name}' using default personality '{DEFAULT_PATH}'." );
            return;

            void SortScripts ( IEnumerable<string> keys, bool isResponderScripts )
            {
                foreach ( var key in keys )
                {
                    // If the script is present in the target personality, upgrade the script from the target personality
                    if ( personality.Scripts.TryGetValue( key, out var personalityScript ) )
                    {
                        personalityScript.Responder = isResponderScripts;
                        upgradeableScripts.Add( key, personalityScript );
                    }
                    else
                    {
                        // If the script is not present in the target personality then add the default script to the output
                        if ( defaultPersonality.Scripts.TryGetValue( key, out var defaultScript ) )
                        {
                            var script = defaultScript.Copy();
                            script.Responder = isResponderScripts;
                            fixedScripts.Add( key, script );
                        }
                    }
                }
            }
        }

        private static string GetObsoleteScriptRecoveryKey ( string scriptKey )
        {
            return $"(Obsolete) {scriptKey}";
        }

        private static Script PrepareObsoleteScriptForRecovery ( string scriptKey, Script script )
        {
            var recoveredScript = PrepareSecondaryScriptForRetention( scriptKey, script );
            recoveredScript.Name = scriptKey;
            return recoveredScript;
        }

        private static Script PrepareSecondaryScriptForRetention ( string scriptKey, Script script )
        {
            var retainedScript = script.Copy();
            // Make sure that these scripts don't carry metadata that can lead to miscategorization.
            retainedScript.Name = scriptKey;
            retainedScript.Responder = false;
            retainedScript.defaultValue = null;
            return retainedScript;
        }

        public static Script UpgradeScript(Script personalityScript, Script defaultScript)
        {
            var script = personalityScript ?? defaultScript?.Copy();
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
