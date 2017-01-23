using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEddpMonitor
{
    /// <summary>
    /// The parameters to match EDDP messages
    /// </summary>
    public class Watch
    {
        [JsonProperty("name")]
        public string name { get; private set; }
        [JsonProperty("system")]
        public string system { get; private set; }
        [JsonProperty("station")]
        public string station { get; private set; }
        [JsonProperty("faction")]
        public string faction { get; private set; }
        [JsonProperty("state")]
        public string state { get; private set; }
        [JsonProperty("maxdistancefromship")]
        public decimal? maxdistancefromship { get; private set; }
        [JsonProperty("maxdistancefromhome")]
        public decimal? maxdistancefromhome { get; private set; }

        public Watch() { }
    }
}
