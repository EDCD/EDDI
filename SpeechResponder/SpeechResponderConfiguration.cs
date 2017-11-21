using Newtonsoft.Json;
using System;
using System.IO;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>Configuration for the speech responder</summary>
    public class SpeechResponderConfiguration
    {
        [JsonProperty("personality")]
        public string Personality { get; set; }

        [JsonProperty("subtitles")]
        public bool Subtitles { get; set; }

        [JsonProperty("subtitlesonly")]
        public bool SubtitlesOnly { get; set; }

        [JsonIgnore]
        private string dataPath;

        /// <summary>
        /// Obtain configuration from a file.  If the file name is not supplied the the default
        /// path of Constants.Data_DIR\speechresponder.json is used
        /// </summary>
        public static SpeechResponderConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\speechresponder.json";
            }

            SpeechResponderConfiguration configuration = new SpeechResponderConfiguration();
            if (File.Exists(filename))
            {
                try
                {
                    string data = Files.Read(filename);
                    if (data != null)
                    {
                        configuration = JsonConvert.DeserializeObject<SpeechResponderConfiguration>(data);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Debug("Failed to read speech responder configuration", ex);
                }
            }
            if (configuration == null)
            {
                configuration = new SpeechResponderConfiguration();
            }

            if (configuration.Personality == null)
            {
                configuration.Personality = "EDDI";
                configuration.ToFile();
            }
            configuration.dataPath = filename;

            return configuration;
        }

        /// <summary>
        /// Write configuration to a file.  If the filename is not supplied then the path used
        /// when reading in the configuration will be used, or the default path of 
        /// Constants.Data_DIR\speechresponder.json will be used
        /// </summary>
        public void ToFile(string filename = null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                filename = Constants.DATA_DIR + @"\speechresponder.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            Files.Write(filename, json);
        }
    }
}
