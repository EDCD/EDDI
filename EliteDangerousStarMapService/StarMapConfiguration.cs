using Newtonsoft.Json;
using System;
using System.IO;

namespace EliteDangerousStarMapService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access EDSM</summary>
    public class StarMapConfiguration
    {
        [JsonProperty("apiKey")]
        public String apiKey { get; set; }
        [JsonProperty("commanderName")]
        public String commanderName { get; set; }

        [JsonIgnore]
        private String dataPath;

        /// <summary>
        /// Obtain credentials from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\edsm.json is used
        /// </summary>
        public static StarMapConfiguration FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\edsm.json";
            }

            StarMapConfiguration credentials;
            try
            {
                String credentialsData = File.ReadAllText(filename);
                credentials = JsonConvert.DeserializeObject<StarMapConfiguration>(credentialsData);
            }
            catch
            {
                credentials = new StarMapConfiguration();
            }

            credentials.dataPath = filename;
            return credentials;
        }

        /// <summary>
        /// Clear the information held by credentials.
        /// </summary>
        public void Clear()
        {
            apiKey = null;
            commanderName = null;
        }

        /// <summary>
        /// Obtain credentials to a file.  If the filename is not supplied then the path used
        /// when reading in the credentials will be used, or the default path of 
        /// %APPDATA%\EDDI\credentials.json will be used
        /// </summary>
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                filename = dataPath;
            }
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\edsm.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
