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
        [JsonProperty("description")]
        public string Description { get; private set; }
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

        [JsonIgnore]
        public string DefaultValue
        {
            get { return defaultScript; }
        }

        public bool isDefault()
        {
            return customScript != null;
        }

        public EventScript(string name, string description, string script)
        {
            EventName = name;
            Description = description;
            defaultScript = script;
            Enabled = true;
        }
    }
}
