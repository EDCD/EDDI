using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProvider
{
    public class Credentials
    {
        [JsonProperty("CompanionApp")]
        public String appId { get; set; }
        [JsonProperty("mid")]
        public String machineId { get; set; }
        [JsonProperty("mtk")]
        public String machineToken { get; set; }

        private String dataPath;

        /**
         * Obtain credentials from a file.  If the file name is not supplied then use the default path
         */
        public static Credentials FromFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                System.IO.Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\credentials.json";
            }

            Credentials credentials;
            try
            {
                String credentialsData = File.ReadAllText(filename);
                credentials = JsonConvert.DeserializeObject<Credentials>(credentialsData);
            }
            catch
            {
                credentials = new Credentials();
            }

            credentials.dataPath = filename;
            return credentials;
        }

        // Clear credentials
        public void Clear()
        {
            appId = null;
            machineId = null;
            machineToken = null;
        }

        /**
         * Write credentials to the file
         */
        public void ToFile(string filename=null)
        {
            if (filename == null)
            {
                String dataDir = Environment.GetEnvironmentVariable("AppData") + "\\EDDI";
                System.IO.Directory.CreateDirectory(dataDir);
                filename = dataDir + "\\credentials.json";
            }

            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, json);
        }
    }
}
