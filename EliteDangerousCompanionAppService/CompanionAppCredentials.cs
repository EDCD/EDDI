using Newtonsoft.Json;
using System;
using System.IO;

namespace EliteDangerousCompanionAppService
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access the Companion App</summary>
    public class CompanionAppCredentials
    {
        [JsonProperty("email")]
        public String email { get; set; }
        [JsonProperty("password")]
        private string encPassword;

        [JsonIgnore]
        public String password {
            get
            {
                return encPassword == null ? null : rijndaelHelper.Decrypt(encPassword);
            }
            set
            {
                encPassword = value == null ? null : rijndaelHelper.Encrypt(value);
            }
        }
        [JsonProperty("CompanionApp")]
        public String appId { get; set; }
        [JsonProperty("mid")]
        public String machineId { get; set; }
        [JsonProperty("mtk")]
        public String machineToken { get; set; }

        [JsonIgnore]
        private String dataPath;

        private static byte[] key = { 251, 9, 67, 117, 237, 158, 138, 150, 255, 97, 103, 128, 183, 65, 76, 161, 7, 79, 244, 225, 146, 180, 51, 123, 118, 167, 45, 10, 184, 181, 202, 190 };
        private static byte[] vector = { 214, 11, 221, 108, 210, 71, 14, 15, 151, 57, 241, 174, 177, 142, 115, 137 };
        private RijndaelHelper rijndaelHelper = new RijndaelHelper(key, vector);

        /// <summary>
        /// Obtain credentials from a file.  If the file name is not supplied the the default
        /// path of %APPDATA%\EDDI\credentials.json is used
        /// </summary>
        public static CompanionAppCredentials FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\credentials.json";
            }

            CompanionAppCredentials credentials;
            try
            {
                String credentialsData = File.ReadAllText(filename);
                credentials = JsonConvert.DeserializeObject<CompanionAppCredentials>(credentialsData);
            }
            catch
            {
                credentials = new CompanionAppCredentials();
            }

            credentials.dataPath = filename;
            return credentials;
        }

        /// <summary>
        /// Clear the information held by credentials.
        /// </summary>
        public void Clear()
        {
            appId = null;
            machineId = null;
            machineToken = null;
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
                filename = dataDir + "\\credentials.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
