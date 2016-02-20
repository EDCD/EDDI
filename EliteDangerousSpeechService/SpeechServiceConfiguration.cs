using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EliteDangerousSpeechService
{
    /// <summary>
    /// Storage for the Text-to-Speech Configs
    /// </summary>
    public class SpeechServiceConfiguration
    {
        [JsonProperty("StandardVoice")]
        public String StandardVoice { get; set; }

        [JsonIgnore]
        private String dataPath;

        /// <summary>
        /// Obtain speech config from a file. If  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\edsm.json is used
        /// </summary>
        /// <param name="filename"></param>
        public static SpeechServiceConfiguration FromFile(string filename = null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\speech.json";
            }

            SpeechServiceConfiguration speech;
            try
            {
                String configData = File.ReadAllText(filename);
                speech = JsonConvert.DeserializeObject<SpeechServiceConfiguration>(configData);
            }
            catch
            {
                speech = new SpeechServiceConfiguration();
            }

            speech.dataPath = filename;
            return speech;
        }

        /// <summary>
        /// Clear the information held by speech
        /// </summary>
        public void Clear()
        {
            StandardVoice = null;
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
                filename = dataDir + "\\speech.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
