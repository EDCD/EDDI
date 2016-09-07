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

        [JsonProperty("data")]
        public Dictionary<string, dynamic> data { get; set;  }

        public JournalEntry()
        {
            data = new Dictionary<string, dynamic>();
        }
    }
}
