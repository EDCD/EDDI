using System;
using System.IO;
using Newtonsoft.Json;
using Utilities;

namespace EddiSpeechService
{
    /// <summary>
    /// Storage for the Text-to-Speech Configs
    /// </summary>
    public class SpeechServiceConfiguration
    {
        [JsonProperty("standardVoice")]
        public string StandardVoice { get; set; }

        [JsonProperty("volume")]
        public int Volume { get; set; } = 100;

        [JsonProperty("effectsLevel")]
        public int EffectsLevel { get; set; } = 50;

        [JsonProperty("distortOnDamage")]
        public bool DistortOnDamage { get; set; } = true;

        [JsonProperty("rate")]
        public int Rate{ get; set; } = 0;

        [JsonProperty("disablessml")]
        public bool DisableSsml { get; set; } = false;

        [JsonProperty("enableicao")]
        public bool EnableIcao { get; set; } = false;

        [JsonIgnore]
        private string dataPath;

        /// <summary>
        /// Obtain speech config from a file. If  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\speech.json is used
        /// </summary>
        /// <param name="filename"></param>
        public static SpeechServiceConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\speech.json";
            }

            SpeechServiceConfiguration configuration = new SpeechServiceConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        configuration = JsonConvert.DeserializeObject<SpeechServiceConfiguration>(data);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read speech service configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new SpeechServiceConfiguration();
            }

            configuration.dataPath = filename;
            return configuration;
        }

        /// <summary>
        /// Clear the information held by speech
        /// </summary>
        public void Clear()
        {
            StandardVoice = null;
            Volume = 100;
            EffectsLevel = 50;
            DistortOnDamage = true;
            DisableSsml = false;
            EnableIcao = false;
        }

        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\speech.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
