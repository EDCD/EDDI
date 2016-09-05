using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousJournalMonitor
{
    public class JournalEntry
    {
        [JsonProperty("timestamp")]
        public DateTime timestamp { get; set; }

        [JsonProperty("type")]
        public String type { get; set; }

        [JsonProperty("refetchprofile")]
        public bool refetchProfile { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, dynamic> data { get; set;  }

        [JsonProperty("stringdata")]
        public Dictionary<string, string> stringData { get; set; }

        [JsonProperty("intdata")]
        public Dictionary<string, int> intData { get; set; }

        [JsonProperty("booldata")]
        public Dictionary<string, bool> boolData { get; set; }

        [JsonProperty("datetimedata")]
        public Dictionary<string, DateTime> datetimeData { get; set; }

        [JsonProperty("decimaldata")]
        public Dictionary<string, decimal> decimalData { get; set; }

        public JournalEntry()
        {
            data = new Dictionary<string, dynamic>();
            stringData = new Dictionary<string, string>();
            intData = new Dictionary<string, int>();
            boolData = new Dictionary<string, bool>();
            datetimeData = new Dictionary<string, DateTime>();
            decimalData = new Dictionary<string, decimal>();
        }
    }
}
