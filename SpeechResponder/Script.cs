using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace EddiSpeechResponder
{
    public class Script : INotifyPropertyChanged
    {
        [JsonProperty("name")]
        public string Name
        {
            get => name;
            set
            {
                name = value; OnPropertyChanged();
            }
        }

        [JsonProperty("description")]
        public string Description
        {
            get => description;
            set
            {
                description = value; OnPropertyChanged();
            }
        }

        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => enabled;
            set { enabled = value; OnPropertyChanged(); }
        }

        [JsonProperty("priority")]
        public int? Priority
        {
            get => responder ? priority : null; // Invoked scripts have no independent priority
            set { priority = value; OnPropertyChanged(); }
        }

        [JsonProperty("responder")]
        public bool Responder
        {
            get => responder;
            set { responder = value; OnPropertyChanged(); }
        }

        [JsonProperty("script")]
        public string Value
        {
            get => script;
            set { script = value; OnPropertyChanged(); }
        }

        [JsonProperty("defaultValue")]

        public string defaultValue { get; set; }

        [JsonProperty("default")]
        // Determine whether the script matches the default, treating empty strings and null values as equal
        public bool Default => string.IsNullOrWhiteSpace(Value)
            ? string.IsNullOrWhiteSpace(defaultValue)
            : string.Equals(Value, defaultValue);

        [JsonIgnore]
        public bool IsResettableOrDeletable
        {
            get { resettableOrDeletable = !Default || (!Responder && string.IsNullOrWhiteSpace(defaultValue)); return resettableOrDeletable; }
            set { resettableOrDeletable = value; OnPropertyChanged(); }
        }
        [JsonIgnore]
        public bool IsResettable => Responder || (!Responder && !string.IsNullOrWhiteSpace(defaultValue));
        [JsonIgnore]
        public bool HasValue => script != null;
        [JsonIgnore]
        private string name;
        [JsonIgnore]
        private string description;
        [JsonIgnore]
        private bool enabled;
        [JsonIgnore]
        private int? priority = 3;
        [JsonIgnore]
        private bool responder;
        [JsonIgnore]
        private string script;
        [JsonIgnore]
        private bool resettableOrDeletable;
        
        public Script(string name, string description, bool responder, string script, int? priority = 3, string defaultScript = null)
        {
            Name = name;
            Description = description;
            this.responder = responder;
            Value = script;
            Priority = priority;
            Enabled = true;
            defaultValue = defaultScript;
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

        #region INotifyPropertyChanged
        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
