using Newtonsoft.Json;
using System.ComponentModel;

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
        [JsonProperty("default")]
        private bool isDefault;

        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value;  OnPropertyChanged("Enabled"); }
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
            private set { responder = value; OnPropertyChanged("Responder"); }
        }

        [JsonIgnore]
        public bool Default
        {
            get { return isDefault; }
            set { isDefault = value; OnPropertyChanged("Default"); }
        }

        [JsonIgnore]
        public bool IsDeleteable
        {
            get { return !responder;  }
        }

        [JsonIgnore]
        public string Value
        {
            get { return script; }
            set { script = value; OnPropertyChanged("Value"); }
        }

        [JsonIgnore]
        public bool HasValue
        {
            get { return script != null; }
        }

        public Script(string name, string description, bool responder, string script, int priority = 3, bool Default = false)
        {
            Name = name;
            Description = description;
            this.responder = responder;
            Value = script;
            Priority = priority;
            Enabled = true;
            this.Default = Default;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
