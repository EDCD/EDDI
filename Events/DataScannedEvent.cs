using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class DataScannedEvent : Event
    {
        public const string NAME = "Data scanned";
        public const string DESCRIPTION = "Triggered when scanning some types of data links";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"DataScanned\", \"Type\":\"DataLink\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DataScannedEvent()
        {
            VARIABLES.Add("type", "The type of Data Link scanned");
        }

        [JsonProperty("type")]
        public string type { get; private set; }

        public DataScannedEvent(DateTime timestamp, DataScan type) : base(timestamp, NAME)
        {
            this.type = type.name;
        }
    }
}
