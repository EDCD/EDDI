using EddiSpeechService;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace EddiSpeechResponder
{
    public class Script : INotifyPropertyChanged
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("enabled")]
        private bool enabled;
        [JsonProperty("priority")]
        private int priority = 3;
        [JsonProperty("responder")]
        private bool responder;
        [JsonProperty("script")]
        private string script;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; OnPropertyChanged("Enabled"); }
        }

        [JsonIgnore]
        public int Priority
        {
            get { return priority; }
            set { priority = value; OnPropertyChanged("Priority"); }
        }

        [JsonIgnore]
        public bool Responder
        {
            get { return responder; }
            set { responder = value; OnPropertyChanged("Responder"); }
        }

        [JsonProperty("default")]
        public bool Default => Value == defaultValue;

        [JsonIgnore]
        public bool IsResettableOrDeletable
        {
            get { resettableOrDeletable = !Default || (!Responder && string.IsNullOrWhiteSpace(defaultValue)); return resettableOrDeletable; }
            set { resettableOrDeletable = value; OnPropertyChanged("IsResettableOrDeletable"); }
        }
        [JsonIgnore]
        private bool resettableOrDeletable;

        [JsonIgnore]
        public bool IsResettable => Responder || (!Responder && !string.IsNullOrWhiteSpace(defaultValue));

        [JsonIgnore]
        public string Value
        {
            get { return script; }
            set { script = value; OnPropertyChanged("Value"); }
        }

        [JsonProperty("defaultValue")]
        public string defaultValue { get; set; }

        [JsonIgnore]
        public bool HasValue
        {
            get { return script != null; }
        }

        [JsonIgnore]
        public IList<int> Priorities
        {
            get { return priorities; }
            set { if (priorities != value) { priorities = value; OnPropertyChanged("Priorities"); }; }
        }

        [JsonIgnore]
        private IList<int> priorities;

        public Script(string name, string description, bool responder, string script, int priority = 3, string defaultScript = null)
        {
            Name = name;
            Description = description;
            this.responder = responder;
            Value = script;
            Priority = priority;
            Enabled = true;
            defaultValue = defaultScript;

            Priorities = new List<int>();
            for (int i = 1; i <= SpeechService.Instance.speechQueue.priorityQueues.Count - 1; i++)
            {
                if (i > 0) { Priorities.Add(i); }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // Convert from legacy personalities which did not store the default value.
            if (additionalJsonData != null)
            {
                additionalJsonData.TryGetValue("default", out JToken defaultVal);
                if (defaultVal != null)
                {
                    bool defaultScript = (bool?)defaultVal ?? false;
                    if (defaultScript)
                    {
                        defaultValue = Value;
                    }
                    additionalJsonData = null;
                }
            }
        }
    }
}
