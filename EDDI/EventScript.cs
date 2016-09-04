using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDDI
{
    public class EventScript
    {
        [JsonProperty("eventname")]
        public string EventName { get; private set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("defaultscript")]
        private string defaultScript;
        [JsonProperty("customscript")]
        private string customScript;

        [JsonIgnore]
        public string Value
        {
            get { return customScript == null ? defaultScript : customScript; }
            set { if (value == null || value == defaultScript) { customScript = null; } else { customScript = value; } }
        }
 
        public bool isDefault()
        {
            return customScript != null;
        }

        public EventScript(string name, string script)
        {
            EventName = name;
            defaultScript = script;
            Enabled = true;
        }
    }
}
